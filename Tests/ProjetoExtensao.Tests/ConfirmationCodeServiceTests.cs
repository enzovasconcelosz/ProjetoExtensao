using ProjetoExtensao.Services;
using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Codigo de recuperacao de senha: e ele que autoriza a troca, entao precisa
/// ser de uso unico, limitado em tentativas e valido apenas para quem pediu.
/// </summary>
public class ConfirmationCodeServiceTests
{
    private static string EmailUnico() => $"{Guid.NewGuid():N}@teste.com";

    [Fact]
    public void Codigo_gerado_tem_seis_digitos()
    {
        var codigo = ConfirmationCodeService.GerarCodigoPara(EmailUnico());

        Assert.Equal(6, codigo.Length);
        Assert.True(codigo.All(char.IsDigit));
    }

    [Fact]
    public void Codigo_correto_e_aceito()
    {
        var email = EmailUnico();
        var codigo = ConfirmationCodeService.GerarCodigoPara(email);

        Assert.Equal(ConfirmationCodeService.ResultadoValidacao.Valido,
            ConfirmationCodeService.Validar(email, codigo));
    }

    [Fact]
    public void Codigo_e_de_uso_unico()
    {
        var email = EmailUnico();
        var codigo = ConfirmationCodeService.GerarCodigoPara(email);

        ConfirmationCodeService.Validar(email, codigo);

        Assert.Equal(ConfirmationCodeService.ResultadoValidacao.NaoSolicitado,
            ConfirmationCodeService.Validar(email, codigo));
    }

    [Fact]
    public void Codigo_nao_solicitado_e_recusado()
    {
        Assert.Equal(ConfirmationCodeService.ResultadoValidacao.NaoSolicitado,
            ConfirmationCodeService.Validar(EmailUnico(), "123456"));
    }

    [Fact]
    public void Codigo_vazio_e_recusado()
    {
        var email = EmailUnico();
        ConfirmationCodeService.GerarCodigoPara(email);

        Assert.Equal(ConfirmationCodeService.ResultadoValidacao.Incorreto,
            ConfirmationCodeService.Validar(email, "  "));
    }

    [Fact]
    public void Espacos_em_volta_do_codigo_sao_ignorados()
    {
        var email = EmailUnico();
        var codigo = ConfirmationCodeService.GerarCodigoPara(email);

        Assert.Equal(ConfirmationCodeService.ResultadoValidacao.Valido,
            ConfirmationCodeService.Validar(email, $" {codigo} "));
    }

    [Fact]
    public void Codigo_e_invalidado_apos_o_limite_de_tentativas()
    {
        var email = EmailUnico();
        var codigo = ConfirmationCodeService.GerarCodigoPara(email);
        var errado = codigo == "000000" ? "111111" : "000000";

        for (var i = 1; i < ConfirmationCodeService.MaximoTentativas; i++)
        {
            Assert.Equal(ConfirmationCodeService.ResultadoValidacao.Incorreto,
                ConfirmationCodeService.Validar(email, errado));
        }

        Assert.Equal(ConfirmationCodeService.ResultadoValidacao.TentativasExcedidas,
            ConfirmationCodeService.Validar(email, errado));

        // Nem mesmo o codigo certo funciona depois disso
        Assert.Equal(ConfirmationCodeService.ResultadoValidacao.NaoSolicitado,
            ConfirmationCodeService.Validar(email, codigo));
    }

    [Fact]
    public void Tentativas_restantes_diminuem_a_cada_erro()
    {
        var email = EmailUnico();
        var codigo = ConfirmationCodeService.GerarCodigoPara(email);
        var errado = codigo == "000000" ? "111111" : "000000";

        Assert.Equal(ConfirmationCodeService.MaximoTentativas, ConfirmationCodeService.TentativasRestantes(email));

        ConfirmationCodeService.Validar(email, errado);

        Assert.Equal(ConfirmationCodeService.MaximoTentativas - 1, ConfirmationCodeService.TentativasRestantes(email));
    }

    [Fact]
    public void Reenvio_imediato_precisa_esperar()
    {
        var email = EmailUnico();
        ConfirmationCodeService.GerarCodigoPara(email);

        Assert.True(ConfirmationCodeService.EsperaParaReenvio(email) > TimeSpan.Zero);
    }

    [Fact]
    public void Sem_codigo_pendente_o_reenvio_e_imediato()
    {
        Assert.Equal(TimeSpan.Zero, ConfirmationCodeService.EsperaParaReenvio(EmailUnico()));
    }

    [Fact]
    public void Descartar_cancela_a_recuperacao_em_andamento()
    {
        var email = EmailUnico();
        var codigo = ConfirmationCodeService.GerarCodigoPara(email);

        ConfirmationCodeService.Descartar(email);

        Assert.Equal(ConfirmationCodeService.ResultadoValidacao.NaoSolicitado,
            ConfirmationCodeService.Validar(email, codigo));
    }

    [Fact]
    public void Codigo_de_um_email_nao_serve_para_outro()
    {
        var email = EmailUnico();
        var codigo = ConfirmationCodeService.GerarCodigoPara(email);

        Assert.Equal(ConfirmationCodeService.ResultadoValidacao.NaoSolicitado,
            ConfirmationCodeService.Validar(EmailUnico(), codigo));
    }
}
