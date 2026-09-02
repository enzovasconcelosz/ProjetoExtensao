using System.Security.Cryptography;

namespace ProjetoExtensao;

/// <summary>
/// Geracao e conferencia da senha do usuario, em um unico lugar para que
/// cadastro, login e alteracao de senha usem sempre o mesmo formato.
///
/// Formato gravado: {salt}.{hash}.{iteracoes} — PBKDF2/SHA256, tudo em Base64.
/// </summary>
public static class SenhaHash
{
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const int Iteracoes = 100_000;

    public static string Gerar(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}.{Iteracoes}";
    }

    /// <summary>
    /// Confere a senha informada contra o valor gravado no banco.
    /// Senhas antigas, gravadas em texto puro, continuam sendo aceitas para
    /// nao invalidar cadastros existentes — veja <see cref="EmTextoPuro"/>.
    /// </summary>
    public static bool Conferir(string senhaInformada, string? senhaGravada)
    {
        if (string.IsNullOrEmpty(senhaGravada))
            return false;

        var partes = senhaGravada.Split('.');

        if (partes.Length != 3)
            return senhaInformada == senhaGravada;

        try
        {
            var salt = Convert.FromBase64String(partes[0]);
            var hashEsperado = Convert.FromBase64String(partes[1]);
            var iteracoes = int.Parse(partes[2]);

            var hash = Rfc2898DeriveBytes.Pbkdf2(senhaInformada, salt, iteracoes, HashAlgorithmName.SHA256, hashEsperado.Length);

            // Comparacao em tempo fixo
            return CryptographicOperations.FixedTimeEquals(hash, hashEsperado);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Indica que a senha gravada ainda nao passou pelo hash, permitindo
    /// atualiza-la silenciosamente no proximo login bem-sucedido.
    /// </summary>
    public static bool EmTextoPuro(string? senhaGravada)
    {
        return !string.IsNullOrEmpty(senhaGravada) && senhaGravada.Split('.').Length != 3;
    }
}
