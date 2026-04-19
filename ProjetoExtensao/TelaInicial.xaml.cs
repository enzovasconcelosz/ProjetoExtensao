namespace ProjetoExtensao;

using System.Collections.ObjectModel;
using System.ComponentModel;

public partial class TelaInicial : ContentPage
{
    public ObservableCollection<WeekDay> WeekDays { get; set; } = [];

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

            // Navegar para a tela de calendário (placeholder)
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

        // calcula início da semana na segunda-feira
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var start = today.AddDays(-diff);

        string[] labels = ["SEG", "TER", "QUA", "QUI", "SEX", "SAB", "DOM"];

        for (int i = 0; i < 7; i++)
        {
            var dt = start.AddDays(i);
            var isToday = dt.Date == today.Date;
            WeekDays.Add(new WeekDay
            {
                Date = dt,
                ShortName = labels[i],
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
