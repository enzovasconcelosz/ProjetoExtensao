using ProjetoExtensao.Application.Validacoes;
using ProjetoExtensao.Entities;
using Xunit;

namespace ProjetoExtensao.Tests;

public class ValidacaoTipoLembreteTests
{
    [Theory]
    [InlineData("Saúde")]
    [InlineData("Casa")]
    public void Nome_com_tamanho_suficiente_e_aceito(string nome)
    {
        Assert.Empty(ValidacaoTipoLembrete.Validar(nome));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    [InlineData(" a ")]
    public void Nome_curto_ou_vazio_e_recusado(string? nome)
    {
        Assert.Single(ValidacaoTipoLembrete.Validar(nome));
    }

    [Fact]
    public void Garantir_lanca_excecao_para_tipo_invalido()
    {
        var excecao = Assert.Throws<ArgumentException>(() => ValidacaoTipoLembrete.Garantir(new TipoLembrete("x")));

        Assert.Contains("3 caracteres", excecao.Message);
    }
}
