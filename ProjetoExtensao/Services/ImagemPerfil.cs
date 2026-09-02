namespace ProjetoExtensao;

/// <summary>
/// Foto de perfil do usuario, gravada no proprio dispositivo.
///
/// A tabela Imagem do banco nao tem coluna para o binario da foto, entao o
/// arquivo fica em <see cref="FileSystem.AppDataDirectory"/> e o caminho em
/// <see cref="Preferences"/>, compartilhado por todas as telas.
///
/// A imagem gravada ja vem recortada em quadrado pela tela de ajuste, entao
/// os avatares apenas a exibem dentro de um Border circular.
/// </summary>
public static class ImagemPerfil
{
    private const string ChaveCaminho = "PerfilImagem";
    private const string Prefixo = "perfil_";

    public static string? Caminho
    {
        get
        {
            var caminho = Preferences.Default.Get(ChaveCaminho, string.Empty);
            return !string.IsNullOrWhiteSpace(caminho) && File.Exists(caminho) ? caminho : null;
        }
    }

    /// <summary>
    /// Imagem atual do usuario, ou o avatar padrao quando nenhuma foi escolhida.
    /// Le por stream de proposito: <c>ImageSource.FromFile</c> guarda a imagem
    /// em cache pelo caminho e continuaria mostrando a foto antiga.
    /// </summary>
    public static ImageSource Obter()
    {
        var caminho = Caminho;

        if (caminho == null)
            return ImageSource.FromFile("dotnet_bot.png");

        return ImageSource.FromStream(() => File.OpenRead(caminho));
    }

    /// <summary>
    /// Grava a nova foto com um nome inedito e descarta a anterior.
    /// </summary>
    public static async Task SalvarAsync(byte[] conteudo)
    {
        var anterior = Caminho;

        var caminho = Path.Combine(
            FileSystem.AppDataDirectory,
            $"{Prefixo}{DateTime.UtcNow.Ticks}.img");

        await File.WriteAllBytesAsync(caminho, conteudo);

        Preferences.Default.Set(ChaveCaminho, caminho);

        if (anterior != null && anterior != caminho)
        {
            try
            {
                File.Delete(anterior);
            }
            catch
            {
                // arquivo antigo em uso: sera sobrescrito em outra oportunidade
            }
        }
    }
}
