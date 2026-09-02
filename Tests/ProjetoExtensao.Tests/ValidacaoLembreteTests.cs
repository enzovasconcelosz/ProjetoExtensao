using ProjetoExtensao.Application.Validacoes;
using ProjetoExtensao.Entities;
using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Regras de preenchimento do lembrete: e a validacao que a tela de cadastro e
/// o servico compartilham, entao um erro aqui aparece nos dois lugares.
/// </summary>
public class ValidacaoLembreteTests
{
    private static readonly DateTime Agora = new(2026, 1, 10, 12, 0, 0);

    [Fact]
    public void Lembrete_completo_e_valido_nao_gera_erro()
    {
        var erros = ValidacaoLembrete.Validar("Consulta médica", 1, Agora.AddDays(1), "Cardiologista", Agora);

        Assert.Empty(erros);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("  a  ")]
    public void Nome_curto_ou_vazio_e_recusado(string? nome)
    {
        var erros = ValidacaoLembrete.Validar(nome, 1, Agora.AddDays(1), "Descrição", Agora);

        Assert.Contains(erros, e => e.Contains("nome", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    public void Tipo_de_lembrete_nao_selecionado_e_recusado(long? idTipo)
    {
        var erros = ValidacaoLembrete.Validar("Consulta médica", idTipo, Agora.AddDays(1), "Descrição", Agora);

        Assert.Contains(erros, e => e.Contains("tipo de lembrete", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Data_no_passado_e_recusada()
    {
        var erros = ValidacaoLembrete.Validar("Consulta médica", 1, Agora.AddMinutes(-1), "Descrição", Agora);

        Assert.Contains(erros, e => e.Contains("posteriores", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Data_igual_ao_momento_atual_e_recusada()
    {
        var erros = ValidacaoLembrete.Validar("Consulta médica", 1, Agora, "Descrição", Agora);

        Assert.Contains(erros, e => e.Contains("posteriores", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Descricao_vazia_e_recusada()
    {
        var erros = ValidacaoLembrete.Validar("Consulta médica", 1, Agora.AddDays(1), "   ", Agora);

        Assert.Contains(erros, e => e.Contains("descrição", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Formulario_em_branco_reune_todos_os_erros_de_uma_vez()
    {
        var erros = ValidacaoLembrete.Validar(null, null, Agora.AddDays(-1), null, Agora);

        Assert.Equal(4, erros.Count);
    }

    [Fact]
    public void Garantir_lanca_excecao_com_todas_as_mensagens_juntas()
    {
        var lembrete = new Lembrete("ab", "", Agora.AddDays(-1));

        var excecao = Assert.Throws<ArgumentException>(() => ValidacaoLembrete.Garantir(lembrete, Agora));

        Assert.Contains("nome", excecao.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("descrição", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Garantir_aceita_lembrete_valido()
    {
        var lembrete = new Lembrete("Tomar remédio", "Após o almoço", Agora.AddHours(2))
        {
            IdTipoLembrete = 3
        };

        ValidacaoLembrete.Garantir(lembrete, Agora);
    }
}
