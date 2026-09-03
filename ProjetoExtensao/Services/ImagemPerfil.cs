using ProjetoExtensao.Application.Perfil;

namespace ProjetoExtensao;

/// <summary>
/// Foto de perfil do usuario, gravada no proprio dispositivo.
///
/// A tabela Imagem do banco nao tem coluna para o binario da foto, entao o
/// arquivo fica em <see cref="FileSystem.AppDataDirectory"/> e o caminho em
/// <see cref="Preferences"/>.
///
/// A chave e o nome do arquivo levam o Id do usuario: o aparelho pode ser
/// compartilhado por mais de uma conta, e cada uma precisa ver a propria foto.
///
/// A imagem gravada ja vem recortada em quadrado pela tela de ajuste, entao
/// os avatares apenas a exibem dentro de um Border circular.
/// </summary>
public static class ImagemPerfil
{
    private static string? ChaveDoUsuario()
    {
        var id = SessaoUsuario.Id;
        return id == null ? null : ArquivoImagemPerfil.Chave(id.Value);
    }

    public static string? Caminho
    {
        get
        {
            var chave = ChaveDoUsuario();

            if (chave == null)
                return null;

            var caminho = Preferences.Default.Get(chave, string.Empty);

            if (string.IsNullOrWhiteSpace(caminho))
                caminho = AdotarImagemAntiga(chave);

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
        var chave = ChaveDoUsuario();

        if (chave == null)
            return;

        var anterior = Caminho;

        var caminho = Path.Combine(
            FileSystem.AppDataDirectory,
            ArquivoImagemPerfil.NomeArquivo(SessaoUsuario.Id!.Value, DateTime.UtcNow.Ticks));

        await File.WriteAllBytesAsync(caminho, conteudo);

        Preferences.Default.Set(chave, caminho);

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

    /// <summary>
    /// Migracao da foto que existia quando a chave era unica para o aparelho.
    ///
    /// Ela e entregue a primeira conta que abrir o aplicativo depois da
    /// atualizacao e a chave antiga e apagada em seguida — assim a foto nao
    /// aparece tambem no perfil das outras contas.
    /// </summary>
    private static string AdotarImagemAntiga(string chaveDoUsuario)
    {
        var antigo = Preferences.Default.Get(ArquivoImagemPerfil.ChaveAntiga, string.Empty);

        Preferences.Default.Remove(ArquivoImagemPerfil.ChaveAntiga);

        if (string.IsNullOrWhiteSpace(antigo) || !File.Exists(antigo))
            return string.Empty;

        Preferences.Default.Set(chaveDoUsuario, antigo);

        return antigo;
    }
}
