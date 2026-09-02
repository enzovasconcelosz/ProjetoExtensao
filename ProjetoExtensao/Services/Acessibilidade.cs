using System.Runtime.CompilerServices;

namespace ProjetoExtensao;

/// <summary>
/// Tamanho da fonte do aplicativo, ajustavel pelo painel de acessibilidade.
///
/// A escolha vale para o dispositivo e e reaplicada a cada tela aberta. Cada
/// elemento e escalado a partir do tamanho que ele declara no XAML, entao a
/// hierarquia visual (titulos maiores, apoio menor) e preservada.
/// </summary>
public static class Acessibilidade
{
    /// <summary>Tamanho de referencia, usado como base da escala.</summary>
    public const double TamanhoMedio = 14;

    /// <summary>
    /// Cinco degraus, nas proporcoes usadas por Android e iOS:
    /// 0,85 / 0,93 / 1,0 / 1,15 / 1,3 aplicadas sobre o tamanho medio.
    /// </summary>
    public static readonly (string Nome, double Tamanho)[] Opcoes =
    {
        ("Pequena", 12),
        ("Reduzida", 13),
        ("Média (padrão)", TamanhoMedio),
        ("Grande", 16),
        ("Muito grande", 18)
    };

    private const string Chave = "AcessibilidadeTamanhoFonte";

    // Guarda o tamanho original de cada elemento para que reaplicar a escala
    // varias vezes no mesmo objeto nao va acumulando aumentos.
    private static readonly ConditionalWeakTable<Element, object> TamanhosOriginais = new();

    public static double Tamanho
    {
        get => Preferences.Default.Get(Chave, TamanhoMedio);
        set => Preferences.Default.Set(Chave, value);
    }

    public static string NomeAtual =>
        Opcoes.FirstOrDefault(o => Math.Abs(o.Tamanho - Tamanho) < 0.01).Nome ?? "Média (padrão)";

    public static double Escala => Tamanho / TamanhoMedio;

    public static void Salvar(double tamanho)
    {
        Tamanho = tamanho;
        AplicarNaPaginaAtual();
    }

    public static void AplicarNaPaginaAtual()
    {
        var pagina = Microsoft.Maui.Controls.Application.Current?.MainPage;
        if (pagina != null)
            Aplicar(pagina);
    }

    public static void Aplicar(Element raiz)
    {
        var escala = Escala;

        foreach (var elemento in Percorrer(raiz))
        {
            var original = TamanhoOriginal(elemento);
            if (original <= 0)
                continue;

            DefinirTamanho(elemento, original * escala);
        }
    }

    private static IEnumerable<Element> Percorrer(Element raiz)
    {
        yield return raiz;

        if (raiz is not IVisualTreeElement visual)
            yield break;

        foreach (var filho in visual.GetVisualChildren())
        {
            if (filho is not Element elemento)
                continue;

            foreach (var descendente in Percorrer(elemento))
                yield return descendente;
        }
    }

    private static double TamanhoOriginal(Element elemento)
    {
        if (TamanhosOriginais.TryGetValue(elemento, out var guardado) && guardado is double valor)
            return valor;

        var atual = LerTamanho(elemento);

        if (double.IsNaN(atual) || atual <= 0)
            return 0;

        TamanhosOriginais.Add(elemento, atual);
        return atual;
    }

    private static double LerTamanho(Element elemento) => elemento switch
    {
        Label label => label.FontSize,
        Button botao => botao.FontSize,
        Entry entry => entry.FontSize,
        Editor editor => editor.FontSize,
        Picker picker => picker.FontSize,
        DatePicker data => data.FontSize,
        TimePicker hora => hora.FontSize,
        SearchBar busca => busca.FontSize,
        _ => 0
    };

    private static void DefinirTamanho(Element elemento, double tamanho)
    {
        switch (elemento)
        {
            case Label label: label.FontSize = tamanho; break;
            case Button botao: botao.FontSize = tamanho; break;
            case Entry entry: entry.FontSize = tamanho; break;
            case Editor editor: editor.FontSize = tamanho; break;
            case Picker picker: picker.FontSize = tamanho; break;
            case DatePicker data: data.FontSize = tamanho; break;
            case TimePicker hora: hora.FontSize = tamanho; break;
            case SearchBar busca: busca.FontSize = tamanho; break;
        }
    }
}
