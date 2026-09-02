using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace ProjetoExtensao;

/// <summary>
/// Calendario mensal. Tocar em um dia abre o cadastro de lembrete ja com a
/// data escolhida.
/// </summary>
public partial class Calendario : ContentPage
{
    private static readonly string[] DiasDaSemana = { "DOM", "SEG", "TER", "QUA", "QUI", "SEX", "SAB" };

    private readonly Func<Page> _paginaAnterior;
    private DateTime _mesExibido;

    public Calendario(DateTime? dataInicial = null, Func<Page>? paginaAnterior = null)
    {
        InitializeComponent();

        _paginaAnterior = paginaAnterior ?? (() => new Configuracoes());

        var referencia = dataInicial ?? DateTime.Today;
        _mesExibido = new DateTime(referencia.Year, referencia.Month, 1);

        MontarCabecalho();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        MontarMes();
    }

    private void MontarCabecalho()
    {
        GradeSemana.Children.Clear();

        for (int coluna = 0; coluna < DiasDaSemana.Length; coluna++)
        {
            var rotulo = new Label
            {
                Text = DiasDaSemana[coluna],
                FontSize = 11,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#AAA59F")
            };

            Grid.SetColumn(rotulo, coluna);
            GradeSemana.Children.Add(rotulo);
        }
    }

    private void MontarMes()
    {
        lblMes.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
            _mesExibido.ToString("MMMM yyyy", new CultureInfo("pt-BR")));

        GradeDias.Children.Clear();

        // Domingo e o primeiro dia da semana, como na tela inicial
        var primeiraColuna = (int)_mesExibido.DayOfWeek;
        var diasNoMes = DateTime.DaysInMonth(_mesExibido.Year, _mesExibido.Month);

        for (int dia = 1; dia <= diasNoMes; dia++)
        {
            var data = new DateTime(_mesExibido.Year, _mesExibido.Month, dia);
            var posicao = primeiraColuna + dia - 1;

            var celula = CriarCelula(data);

            Grid.SetColumn(celula, posicao % 7);
            Grid.SetRow(celula, posicao / 7);
            GradeDias.Children.Add(celula);
        }
    }

    private View CriarCelula(DateTime data)
    {
        var hoje = data.Date == DateTime.Today;
        var passado = data.Date < DateTime.Today;

        var numero = new Label
        {
            Text = data.Day.ToString(),
            FontSize = 14,
            FontAttributes = hoje ? FontAttributes.Bold : FontAttributes.None,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TextColor = CorDoTexto(hoje, passado)
        };

        var celula = new Border
        {
            WidthRequest = 36,
            HeightRequest = 36,
            StrokeThickness = 0,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = hoje ? Color.FromArgb("#48547C") : Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Content = numero
        };

        // Datas passadas nao aceitam toque: o lembrete precisa ser futuro e a
        // validacao recusaria o cadastro na hora de salvar.
        if (!passado)
        {
            var toque = new TapGestureRecognizer();
            toque.Tapped += (_, _) => AbrirCadastro(data);
            celula.GestureRecognizers.Add(toque);
        }

        return celula;
    }

    private static Color CorDoTexto(bool hoje, bool passado)
    {
        var escuro = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

        if (hoje)
            return escuro ? Color.FromArgb("#EDF3F8") : Colors.White;

        // Datas passadas ficam esmaecidas: o lembrete precisa ser futuro
        if (passado)
            return Color.FromArgb("#AAA59F");

        return escuro ? Color.FromArgb("#EDF3F8") : Color.FromArgb("#33343B");
    }

    private void AbrirCadastro(DateTime data)
    {
        App.Current.MainPage = new CadastroLembrete(
            data,
            () => new Calendario(data, _paginaAnterior));
    }

    private void BotaoMesAnterior_Clicked(object sender, EventArgs e)
    {
        _mesExibido = _mesExibido.AddMonths(-1);
        MontarMes();
    }

    private void BotaoProximoMes_Clicked(object sender, EventArgs e)
    {
        _mesExibido = _mesExibido.AddMonths(1);
        MontarMes();
    }

    private void BotaoVoltar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = _paginaAnterior();
    }
}
