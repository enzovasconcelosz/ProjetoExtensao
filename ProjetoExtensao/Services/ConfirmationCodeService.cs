using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ProjetoExtensao.Services;

/// <summary>
/// Codigos de acesso usados na recuperacao de senha.
///
/// Os codigos ficam apenas em memoria: valem enquanto o aplicativo estiver
/// aberto, expiram sozinhos e sao descartados apos o uso ou apos exceder o
/// numero de tentativas.
/// </summary>
public static class ConfirmationCodeService
{
    /// <summary>Tempo de validade do codigo.</summary>
    public static readonly TimeSpan Validade = TimeSpan.FromMinutes(10);

    /// <summary>Tentativas erradas antes de invalidar o codigo.</summary>
    public const int MaximoTentativas = 5;

    /// <summary>Intervalo minimo entre dois envios para o mesmo e-mail.</summary>
    public static readonly TimeSpan IntervaloReenvio = TimeSpan.FromSeconds(60);

    private sealed class Registro
    {
        public required string Codigo { get; init; }
        public required DateTime Gerado { get; init; }
        public int Tentativas { get; set; }

        public DateTime Expira => Gerado.Add(Validade);
        public bool Expirou => DateTime.UtcNow > Expira;
    }

    private static readonly ConcurrentDictionary<string, Registro> Codigos = new(StringComparer.OrdinalIgnoreCase);

    public enum ResultadoValidacao
    {
        Valido,
        Incorreto,
        Expirado,
        NaoSolicitado,
        TentativasExcedidas
    }

    /// <summary>
    /// Gera um codigo de 6 digitos para o e-mail, substituindo qualquer anterior.
    /// </summary>
    public static string GerarCodigoPara(string email)
    {
        // RandomNumberGenerator em vez de Random: o codigo autoriza a troca de senha
        var codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        Codigos[email] = new Registro
        {
            Codigo = codigo,
            Gerado = DateTime.UtcNow
        };

        return codigo;
    }

    /// <summary>
    /// Quanto falta para que um novo envio seja permitido. Zero se ja puder reenviar.
    /// </summary>
    public static TimeSpan EsperaParaReenvio(string email)
    {
        if (!Codigos.TryGetValue(email, out var registro))
            return TimeSpan.Zero;

        var decorrido = DateTime.UtcNow - registro.Gerado;
        var restante = IntervaloReenvio - decorrido;

        return restante > TimeSpan.Zero ? restante : TimeSpan.Zero;
    }

    public static ResultadoValidacao Validar(string email, string? codigoInformado)
    {
        if (string.IsNullOrWhiteSpace(codigoInformado))
            return ResultadoValidacao.Incorreto;

        if (!Codigos.TryGetValue(email, out var registro))
            return ResultadoValidacao.NaoSolicitado;

        if (registro.Expirou)
        {
            Codigos.TryRemove(email, out _);
            return ResultadoValidacao.Expirado;
        }

        if (registro.Codigo == codigoInformado.Trim())
        {
            // Codigo de uso unico
            Codigos.TryRemove(email, out _);
            return ResultadoValidacao.Valido;
        }

        registro.Tentativas++;

        if (registro.Tentativas >= MaximoTentativas)
        {
            Codigos.TryRemove(email, out _);
            return ResultadoValidacao.TentativasExcedidas;
        }

        return ResultadoValidacao.Incorreto;
    }

    /// <summary>
    /// Tentativas restantes antes de o codigo ser invalidado.
    /// </summary>
    public static int TentativasRestantes(string email)
    {
        return Codigos.TryGetValue(email, out var registro)
            ? Math.Max(0, MaximoTentativas - registro.Tentativas)
            : 0;
    }

    /// <summary>
    /// Descarta o codigo pendente, usado ao cancelar a recuperacao.
    /// </summary>
    public static void Descartar(string email) => Codigos.TryRemove(email, out _);
}
