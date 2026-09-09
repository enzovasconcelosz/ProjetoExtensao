using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Shapes;
using ProjetoExtensao.Application.Interfaces;

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

    // Dias que ja possuem lembrete, destacados na grade
    private HashSet<DateTime> _diasComLembrete = new();

    public Calendario(DateTime? dataInicial = null, Func<Page>? paginaAnterior = null)
    {
        InitializeComponent();

        _paginaAnterior = paginaAnterior ?? (() => new Configuracoes());

        var referencia = dataInicial ?? DateTime.Today;
        _mesExibido = new DateTime(referencia.Year, referencia.Month, 1);

        MontarCabecalho();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        await CarregarDiasComLembrete();
        MontarMes();
    }

    private async Task CarregarDiasComLembrete()
    {
        try
        {
            var servicos = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;

            if (servicos?.GetService<ILembreteService>() is not ILembreteService servico)
                return;

            var lembretes = await servico.GetAllAsync();

            _diasComLembrete = lembretes
                .Select(l => l.DataHoraLembrete.Date)
                .ToHashSet();
        }
        catch
        {
            // sem acesso ao banco, o calendario segue sem os destaques
        }
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
        var comLembrete = _diasComLembrete.Contains(data.Date);

        var numero = new Label
        {
            Text = data.Day.ToString(),
            FontSize = 14,
            FontAttributes = hoje || comLembrete ? FontAttributes.Bold : FontAttributes.None,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TextColor = CorDoTexto(hoje, passado, comLembrete)
        };

        var celula = new Border
        {
            WidthRequest = 36,
            HeightRequest = 36,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Content = numero,

            // Hoje continua com o preenchimento escuro; os dias com lembrete
            // usam o azul da paleta. Quando coincidem, o contorno azul mantem
            // os dois significados visiveis.
            BackgroundColor = hoje
                ? Color.FromArgb("#48547C")
                : comLembrete ? Color.FromArgb("#749DD0") : Colors.Transparent,
            Stroke = new SolidColorBrush(Color.FromArgb("#749DD0")),
            StrokeThickness = hoje && comLembrete ? 2 : 0
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

    private static Color CorDoTexto(bool hoje, bool passado, bool comLembrete)
    {
        var escuro = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

        if (hoje)
            return escuro ? Color.FromArgb("#EDF3F8") : Colors.White;

        // Sobre o azul da paleta, o texto escuro tem mais contraste
        if (comLembrete)
            return Color.FromArgb("#33343B");

        // Datas passadas ficam esmaecidas: o lembrete precisa ser futuro
        if (passado)
            return Color.FromArgb("#AAA59F");

        return escuro ? Color.FromArgb("#EDF3F8") : Color.FromArgb("#33343B");
    }

    private void AbrirCadastro(DateTime data)
    {
        Navegacao.IrPara(new CadastroLembrete(
            data,
            () => new Calendario(data, _paginaAnterior)));
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
        Navegacao.IrPara(_paginaAnterior());
    }
}
