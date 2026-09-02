using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Seguranca da senha: o valor gravado nunca pode ser a senha digitada e a
/// conferencia precisa aceitar apenas a senha correta.
/// </summary>
public class SenhaHashTests
{
    [Fact]
    public void Senha_gerada_nao_contem_a_senha_original()
    {
        var gravada = SenhaHash.Gerar("MinhaSenha123");

        Assert.DoesNotContain("MinhaSenha123", gravada);
    }

    [Fact]
    public void Mesma_senha_gera_valores_diferentes_por_causa_do_salt()
    {
        Assert.NotEqual(SenhaHash.Gerar("MinhaSenha123"), SenhaHash.Gerar("MinhaSenha123"));
    }

    [Fact]
    public void Senha_correta_e_aceita()
    {
        var gravada = SenhaHash.Gerar("MinhaSenha123");

        Assert.True(SenhaHash.Conferir("MinhaSenha123", gravada));
    }

    [Theory]
    [InlineData("minhasenha123")]
    [InlineData("MinhaSenha124")]
    [InlineData("")]
    public void Senha_errada_e_recusada(string tentativa)
    {
        var gravada = SenhaHash.Gerar("MinhaSenha123");

        Assert.False(SenhaHash.Conferir(tentativa, gravada));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Senha_gravada_ausente_e_recusada(string? gravada)
    {
        Assert.False(SenhaHash.Conferir("MinhaSenha123", gravada));
    }

    [Fact]
    public void Cadastro_antigo_em_texto_puro_continua_entrando()
    {
        Assert.True(SenhaHash.Conferir("senhaAntiga", "senhaAntiga"));
        Assert.False(SenhaHash.Conferir("outra", "senhaAntiga"));
    }

    [Fact]
    public void Texto_puro_e_identificado_para_atualizacao_no_proximo_login()
    {
        Assert.True(SenhaHash.EmTextoPuro("senhaAntiga"));
        Assert.False(SenhaHash.EmTextoPuro(SenhaHash.Gerar("senhaAntiga")));
    }

    [Fact]
    public void Valor_gravado_corrompido_nao_derruba_o_login()
    {
        Assert.False(SenhaHash.Conferir("MinhaSenha123", "aaa.bbb.ccc"));
    }
}
