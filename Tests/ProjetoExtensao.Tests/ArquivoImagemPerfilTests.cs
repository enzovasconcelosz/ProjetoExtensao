using ProjetoExtensao.Application.Perfil;
using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Onde a foto de perfil de cada conta e guardada no aparelho.
///
/// Era uma chave unica do aparelho que fazia todas as contas exibirem a mesma
/// foto; o Id do usuario na chave e no nome do arquivo e o que separa uma da
/// outra.
/// </summary>
public class ArquivoImagemPerfilTests
{
    [Fact]
    public void Cada_usuario_tem_a_sua_propria_chave()
    {
        Assert.NotEqual(ArquivoImagemPerfil.Chave(1), ArquivoImagemPerfil.Chave(2));
    }

    [Fact]
    public void O_mesmo_usuario_gera_sempre_a_mesma_chave()
    {
        Assert.Equal(ArquivoImagemPerfil.Chave(7), ArquivoImagemPerfil.Chave(7));
    }

    [Fact]
    public void A_chave_do_usuario_nao_colide_com_a_chave_antiga()
    {
        // A chave antiga guardava a foto compartilhada por todo o aparelho e
        // precisa continuar distinguivel para poder ser migrada e apagada
        Assert.NotEqual(ArquivoImagemPerfil.ChaveAntiga, ArquivoImagemPerfil.Chave(1));
    }

    [Fact]
    public void Cada_usuario_tem_o_seu_proprio_arquivo()
    {
        Assert.NotEqual(
            ArquivoImagemPerfil.NomeArquivo(1, 100),
            ArquivoImagemPerfil.NomeArquivo(2, 100));
    }

    [Fact]
    public void Gravacoes_diferentes_do_mesmo_usuario_geram_arquivos_diferentes()
    {
        // Nome repetido faria a tela continuar exibindo a foto anterior,
        // porque o ImageSource guarda cache pelo caminho
        Assert.NotEqual(
            ArquivoImagemPerfil.NomeArquivo(1, 100),
            ArquivoImagemPerfil.NomeArquivo(1, 200));
    }

    [Fact]
    public void O_nome_do_arquivo_identifica_o_dono()
    {
        Assert.StartsWith("perfil_42_", ArquivoImagemPerfil.NomeArquivo(42, 100));
    }

    [Fact]
    public void O_arquivo_tem_a_extensao_esperada()
    {
        Assert.EndsWith(".img", ArquivoImagemPerfil.NomeArquivo(1, 100));
    }
}
