namespace ProjetoExtensao;

/// <summary>
/// Troca a pagina exibida com um esmaecimento curto.
///
/// A navegacao do aplicativo e feita substituindo a MainPage, o que troca a tela
/// em um unico quadro: o corte seco faz o usuario perder de vista onde estava.
/// A transicao aqui e deliberadamente breve — o suficiente para o olho
/// acompanhar a troca, curta o bastante para nao atrasar quem ja sabe para onde
/// vai.
/// </summary>
public static class Navegacao
{
    /// <summary>Saida da tela atual.</summary>
    private const uint DuracaoSaida = 110;

    /// <summary>Entrada da nova tela, um pouco mais longa para nao parecer um salto.</summary>
    private const uint DuracaoEntrada = 160;

    /// <summary>
    /// Exibe <paramref name="destino"/> no lugar da tela atual.
    ///
    /// Nao devolve Task de proposito: as telas chamam este metodo de dentro de
    /// manipuladores de clique, e esperar pela animacao so deixaria o botao
    /// travado enquanto ela roda.
    /// </summary>
    public static void IrPara(Page destino) => _ = TrocarAsync(destino);

    private static async Task TrocarAsync(Page destino)
    {
        // Qualificado porque o projeto tem um namespace ProjetoExtensao.Application
        var aplicacao = Microsoft.Maui.Controls.Application.Current;

        if (aplicacao is null)
            return;

        var atual = aplicacao.MainPage;

        if (atual is not null)
            await atual.FadeTo(0, DuracaoSaida, Easing.CubicIn);

        // A pagina entra invisivel para que o primeiro quadro dela ja apareca
        // esmaecido, sem o piscar de quem aparece opaca e so depois some.
        destino.Opacity = 0;
        aplicacao.MainPage = destino;

        try
        {
            await destino.FadeTo(1, DuracaoEntrada, Easing.CubicOut);
        }
        finally
        {
            // Rede de seguranca: se a animacao nao chegar a rodar (a pagina ainda
            // sem handler, ou o aparelho com animacoes desligadas), a tela ficaria
            // invisivel e o usuario veria um retangulo em branco.
            destino.Opacity = 1;
        }
    }
}
