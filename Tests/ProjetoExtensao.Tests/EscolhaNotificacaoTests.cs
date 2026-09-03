using ProjetoExtensao.Application.Notificacoes;
using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Decisao de como o aviso do lembrete se comporta. O canal precisa ser estavel
/// (no Android ele nao muda depois de criado) e o texto exibido precisa
/// corresponder ao que o aparelho realmente vai fazer.
/// </summary>
public class EscolhaNotificacaoTests
{
    private static readonly DateTime Agora = new(2026, 1, 10, 12, 0, 0);

    [Fact]
    public void Cada_combinacao_tem_o_seu_proprio_canal()
    {
        var canais = new[]
        {
            EscolhaNotificacao.CanalDe(vibrar: true, som: true),
            EscolhaNotificacao.CanalDe(vibrar: true, som: false),
            EscolhaNotificacao.CanalDe(vibrar: false, som: true),
            EscolhaNotificacao.CanalDe(vibrar: false, som: false)
        };

        Assert.Equal(4, canais.Distinct().Count());
    }

    [Fact]
    public void A_mesma_escolha_gera_sempre_o_mesmo_canal()
    {
        Assert.Equal(
            EscolhaNotificacao.CanalDe(vibrar: true, som: false),
            EscolhaNotificacao.CanalDe(vibrar: true, som: false));
    }

    [Fact]
    public void Canal_com_som_e_diferente_do_canal_sem_som()
    {
        Assert.NotEqual(
            EscolhaNotificacao.CanalDe(vibrar: true, som: true),
            EscolhaNotificacao.CanalDe(vibrar: true, som: false));
    }

    [Fact]
    public void Cada_combinacao_tem_o_seu_proprio_nome_de_canal()
    {
        var nomes = new[]
        {
            EscolhaNotificacao.NomeDoCanal(vibrar: true, som: true),
            EscolhaNotificacao.NomeDoCanal(vibrar: true, som: false),
            EscolhaNotificacao.NomeDoCanal(vibrar: false, som: true),
            EscolhaNotificacao.NomeDoCanal(vibrar: false, som: false)
        };

        Assert.Equal(4, nomes.Distinct().Count());
    }

    [Theory]
    [InlineData(true, true, "Vibrar e tocar")]
    [InlineData(true, false, "Somente vibrar")]
    [InlineData(false, true, "Somente tocar")]
    [InlineData(false, false, "Sem som e sem vibração")]
    public void Resumo_descreve_a_escolha_ativa(bool vibrar, bool som, string esperado)
    {
        Assert.Equal(esperado, EscolhaNotificacao.Resumo(notificar: true, vibrar, som));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Com_o_aviso_desligado_o_resumo_ignora_vibrar_e_tocar(bool vibrar, bool som)
    {
        Assert.Equal("Desativadas", EscolhaNotificacao.Resumo(notificar: false, vibrar, som));
    }

    [Fact]
    public void Lembrete_futuro_com_aviso_ligado_e_agendado()
    {
        Assert.True(EscolhaNotificacao.DeveAgendar(notificar: true, Agora.AddMinutes(1), Agora));
    }

    [Fact]
    public void Lembrete_futuro_com_aviso_desligado_nao_e_agendado()
    {
        Assert.False(EscolhaNotificacao.DeveAgendar(notificar: false, Agora.AddDays(1), Agora));
    }

    [Fact]
    public void Lembrete_no_passado_nao_e_agendado()
    {
        // Acontece ao editar um lembrete cuja hora ja passou
        Assert.False(EscolhaNotificacao.DeveAgendar(notificar: true, Agora.AddMinutes(-1), Agora));
    }

    [Fact]
    public void Lembrete_marcado_para_o_exato_momento_atual_nao_e_agendado()
    {
        Assert.False(EscolhaNotificacao.DeveAgendar(notificar: true, Agora, Agora));
    }
}
