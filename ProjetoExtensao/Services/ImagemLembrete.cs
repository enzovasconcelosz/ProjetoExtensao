namespace ProjetoExtensao;

/// <summary>
/// Imagem opcional de cada lembrete, exibida na tela inicial no lugar das
/// iniciais.
///
/// A tabela Lembrete nao tem coluna para o binario da imagem, entao o arquivo
/// fica no dispositivo, nomeado pelo Id do lembrete.
/// </summary>
public static class ImagemLembrete
{
    private const string Prefixo = "lembrete_";

    private static string CaminhoDe(long idLembrete) =>
        Path.Combine(FileSystem.AppDataDirectory, $"{Prefixo}{idLembrete}.img");

    public static bool Existe(long idLembrete) =>
        idLembrete > 0 && File.Exists(CaminhoDe(idLembrete));

    /// <summary>Imagem do lembrete, ou <c>null</c> quando nao houver.</summary>
    public static ImageSource? Obter(long idLembrete)
    {
        if (!Existe(idLembrete))
            return null;

        var caminho = CaminhoDe(idLembrete);

        // Por stream: FromFile guarda cache pelo caminho e manteria a imagem antiga
        return ImageSource.FromStream(() => File.OpenRead(caminho));
    }

    public static async Task SalvarAsync(long idLembrete, byte[] conteudo)
    {
        if (idLembrete <= 0)
            return;

        await File.WriteAllBytesAsync(CaminhoDe(idLembrete), conteudo);
    }

    /// <summary>Remove a imagem, usado ao excluir o lembrete.</summary>
    public static void Remover(long idLembrete)
    {
        try
        {
            if (Existe(idLembrete))
                File.Delete(CaminhoDe(idLembrete));
        }
        catch
        {
            // arquivo em uso: nao impede a exclusao do lembrete
        }
    }
}
