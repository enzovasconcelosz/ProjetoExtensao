using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProjetoExtensao;

public partial class TelaInicial : ContentPage
{
    public ObservableCollection<WeekDay> WeekDays { get; set; } = new ObservableCollection<WeekDay>();

    public TelaInicial()
    {
        InitializeComponent();
        BindingContext = this;
        BuildWeekDays();

        // registra o lembrete de selecao dinamicamente para evitar erro de ligacao XAML em tempo de compilacao
        var cv = this.FindByName<CollectionView>("WeekCollection");
        if (cv != null)
            cv.SelectionChanged += WeekCollection_SelectionChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Tema.Aplicar();

        string? UsuarioLogado = await SecureStorage.Default.GetAsync("UsuarioLogado");
        if (string.IsNullOrWhiteSpace(UsuarioLogado))
            UsuarioLogado = Preferences.Default.Get("PerfilNome", "Usuário");

        lblBoasVindas.Text = $"Olá, {UsuarioLogado}!";

        // Seleciona o dia atual na colecao
        var today = WeekDays.FirstOrDefault(d => d.IsToday);
        if (today != null)
        {
            var cv = this.FindByName<CollectionView>("WeekCollection");
            if (cv != null)
                cv.SelectedItem = today;
        }
    }

    private void BotaoMenu_Tapped(object sender, TappedEventArgs e)
    {
        // O menu de tres linhas abre a tela que antes era aberta pela imagem redonda
        App.Current.MainPage = new Configuracoes();
    }

    private void BotaoPerfil_Clicked(object sender, EventArgs e)
    {
        // A imagem redonda passa a abrir as configuracoes do proprio usuario
        App.Current.MainPage = new ConfiguracoesUsuario();
    }

    private async void WeekCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            WeekDay? selected = e.CurrentSelection[0] as WeekDay;
            if (selected == null)
                return;

            try
            {
                await Navigation.PushAsync(new ContentPage { Title = "Calendário" });
            }
            catch
            {
                // ignore se Navigation nao estiver disponivel
            }
        }
    }

    private void BuildWeekDays()
    {
        WeekDays.Clear();
        var today = DateTime.Today;

        var labelsMap = new Dictionary<DayOfWeek, string>
        {
            [DayOfWeek.Monday] = "SEG",
            [DayOfWeek.Tuesday] = "TER",
            [DayOfWeek.Wednesday] = "QUA",
            [DayOfWeek.Thursday] = "QUI",
            [DayOfWeek.Friday] = "SEX",
            [DayOfWeek.Saturday] = "SAB",
            [DayOfWeek.Sunday] = "DOM"
        };

        // Comeca na segunda-feira da semana atual, como no modelo
        int diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        var inicioSemana = today.AddDays(-diff);

        for (int i = 0; i < 7; i++)
        {
            var dt = inicioSemana.AddDays(i);
            WeekDays.Add(new WeekDay
            {
                Date = dt,
                ShortName = labelsMap[dt.DayOfWeek],
                DayNumber = dt.Day.ToString(),
                IsToday = dt.Date == today.Date
            });
        }
    }
}

public class WeekDay : INotifyPropertyChanged
{
    public DateTime Date { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string DayNumber { get; set; } = string.Empty;
    public bool IsToday { get; set; }

    private static bool Escuro =>
        Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

    // Cores derivadas da paleta padrao do projeto, acompanhando o tema
    public Color CircleColor => IsToday
        ? Color.FromArgb(Escuro ? "#749DD0" : "#48547C")
        : Color.FromArgb(Escuro ? "#3F4149" : "#E8F2FB");

    public Color NumberColor => IsToday
        ? Color.FromArgb(Escuro ? "#33343B" : "#EDF3F8")
        : Color.FromArgb(Escuro ? "#EDF3F8" : "#33343B");

    public Color LabelColor => IsToday
        ? Color.FromArgb(Escuro ? "#EDF3F8" : "#48547C")
        : Color.FromArgb("#AAA59F");

    public event PropertyChangedEventHandler? PropertyChanged;
}
