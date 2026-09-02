using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Application.Validacoes;

/// <summary>
/// Regras de preenchimento do lembrete, em um unico lugar.
///
/// A tela usa a lista de erros para orientar o usuario e o servico usa a mesma
/// verificacao antes de gravar, para que nenhum caminho alternativo (ou uma
/// tela futura) grave um lembrete invalido.
/// </summary>
public static class ValidacaoLembrete
{
    public const int TamanhoMinimoNome = 4;

    public static IReadOnlyList<string> Validar(
        string? nome,
        long? idTipoLembrete,
        DateTime dataHora,
        string? descricao,
        DateTime? agora = null)
    {
        var erros = new List<string>();
        var referencia = agora ?? DateTime.Now;

        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < TamanhoMinimoNome)
            erros.Add($"Informe um nome com pelo menos {TamanhoMinimoNome} caracteres.");

        if (idTipoLembrete is null or 0)
            erros.Add("Selecione o tipo de lembrete.");

        if (dataHora <= referencia)
            erros.Add("A data e a hora devem ser posteriores ao momento atual.");

        if (string.IsNullOrWhiteSpace(descricao))
            erros.Add("Informe a descrição do lembrete.");

        return erros;
    }

    public static IReadOnlyList<string> Validar(Lembrete lembrete, DateTime? agora = null) =>
        Validar(lembrete.Nome, lembrete.IdTipoLembrete, lembrete.DataHoraLembrete, lembrete.Descricao, agora);

    /// <summary>
    /// Interrompe a gravacao quando ha erro, com todas as mensagens juntas.
    /// </summary>
    public static void Garantir(Lembrete lembrete, DateTime? agora = null)
    {
        var erros = Validar(lembrete, agora);

        if (erros.Count > 0)
            throw new ArgumentException(string.Join(" ", erros), nameof(lembrete));
    }
}
