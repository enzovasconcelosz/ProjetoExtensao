namespace ProjetoExtensao.Application.Perfil;

/// <summary>
/// Onde a foto de perfil de cada usuario e guardada no aparelho.
///
/// A regra fica separada do serviço que le e grava o arquivo por um motivo
/// pratico: o aparelho pode ser compartilhado por mais de uma conta, e foi
/// justamente uma chave sem o Id do usuario que fez todas as contas exibirem a
/// mesma foto. A derivacao da chave e do nome do arquivo e o que garante a
/// separacao, entao precisa ser verificavel sozinha.
/// </summary>
public static class ArquivoImagemPerfil
{
    /// <summary>
    /// Chave usada antes de a foto passar a ser separada por conta. Continua
    /// sendo lida uma unica vez, para nao descartar a foto de quem ja usava o
    /// aplicativo.
    /// </summary>
    public const string ChaveAntiga = "PerfilImagem";

    private const string PrefixoChave = "PerfilImagem";
    private const string PrefixoArquivo = "perfil_";

    /// <summary>Chave da preferencia que guarda o caminho da foto do usuario.</summary>
    public static string Chave(long idUsuario) => $"{PrefixoChave}_{idUsuario}";

    /// <summary>
    /// Nome do arquivo da foto. O instante da gravacao entra no nome porque
    /// <c>ImageSource</c> guarda cache pelo caminho: reaproveitar o mesmo nome
    /// faria a tela continuar exibindo a foto anterior.
    /// </summary>
    public static string NomeArquivo(long idUsuario, long ticks) =>
        $"{PrefixoArquivo}{idUsuario}_{ticks}.img";
}
