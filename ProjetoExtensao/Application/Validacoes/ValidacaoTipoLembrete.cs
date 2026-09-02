using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Application.Validacoes;

/// <summary>
/// Regras de preenchimento do tipo de lembrete.
///
/// Como em <see cref="ValidacaoLembrete"/>, a tela e o servico usam a mesma
/// verificacao: a tela para orientar o usuario, o servico para garantir que
/// nada invalido chegue ao banco por outro caminho.
/// </summary>
public static class ValidacaoTipoLembrete
{
    public const int TamanhoMinimoNome = 3;

    public static IReadOnlyList<string> Validar(string? nome)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < TamanhoMinimoNome)
            erros.Add($"Informe um nome com pelo menos {TamanhoMinimoNome} caracteres.");

        return erros;
    }

    public static IReadOnlyList<string> Validar(TipoLembrete tipoLembrete) =>
        Validar(tipoLembrete.Nome);

    /// <summary>
    /// Interrompe a gravacao quando ha erro, com todas as mensagens juntas.
    /// </summary>
    public static void Garantir(TipoLembrete tipoLembrete)
    {
        var erros = Validar(tipoLembrete);

        if (erros.Count > 0)
            throw new ArgumentException(string.Join(" ", erros), nameof(tipoLembrete));
    }
}
