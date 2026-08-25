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

        // registra o evento de seleção dinamicamente para evitar erro de ligação XAML em tempo de compilação
        var cv = this.FindByName<CollectionView>("WeekCollection");
        cv?.SelectionChanged += WeekCollection_SelectionChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        string? UsuarioLogado = await SecureStorage.Default.GetAsync("UsuarioLogado");
        if (string.IsNullOrWhiteSpace(UsuarioLogado))
            UsuarioLogado = "Usuário";

        lblBoasVindas.Text = $"Olá, {UsuarioLogado}!";

        // Seleciona o dia atual na coleção
        var today = WeekDays.FirstOrDefault(d => d.IsToday);
        if (today != null)
        {
            var cv = this.FindByName<CollectionView>("WeekCollection");
            cv?.SelectedItem = today;
            // cv?.ScrollTo(today, position: ScrollToPosition.Start);
        }
    }

    private void BotaoConfiguracoes_Clicked(object sender, EventArgs e)
    {
        try
        {
            App.Current.MainPage = new Configuracoes();
        }
        catch
        {
            // se a página não existir, silencie para não quebrar a tela
        }
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
                // ignore se Navigation não estiver disponível
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

        // Coloca o dia atual primeiro e os próximos 6 dias
        for (int i = 0; i < 7; i++)
        {
            var dt = today.AddDays(i);
            var isToday = dt.Date == today.Date;
            WeekDays.Add(new WeekDay
            {
                Date = dt,
                ShortName = labelsMap[dt.DayOfWeek],
                DayNumber = dt.Day.ToString(),
                IsToday = isToday
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
    public event PropertyChangedEventHandler? PropertyChanged;
}
