using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao;

public partial class TelaInicial : ContentPage
{
    public ObservableCollection<WeekDay> WeekDays { get; set; } = new ObservableCollection<WeekDay>();

    // Marcar o dia atual tambem dispara SelectionChanged; sem esta trava a tela
    // abriria o calendario sozinha assim que fosse exibida.
    private bool _selecionandoDiaAtual;

    // Dias da semana que ja possuem lembrete, destacados na faixa
    private HashSet<DateTime> _diasComLembrete = new();

    public TelaInicial()
    {
        InitializeComponent();
        BindingContext = this;
        BuildWeekDays();

        // registra o evento de selecao dinamicamente para evitar erro de ligacao XAML em tempo de compilacao
        var cv = this.FindByName<CollectionView>("WeekCollection");
        if (cv != null)
            cv.SelectionChanged += WeekCollection_SelectionChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Tema.Aplicar();

        btnPerfil.Source = ImagemPerfil.Obter();

        await CarregarSaudacao();
        await CarregarProximoLembrete();

        // A faixa e remontada depois de saber quais dias tem lembrete
        BuildWeekDays();

        // Seleciona o dia atual na colecao
        var today = WeekDays.FirstOrDefault(d => d.IsToday);
        if (today != null)
        {
            var cv = this.FindByName<CollectionView>("WeekCollection");
            if (cv != null)
            {
                _selecionandoDiaAtual = true;
                cv.SelectedItem = today;
                _selecionandoDiaAtual = false;
            }
        }
    }

    private static IServiceProvider? Servicos =>
        Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;

    /// <summary>
    /// Exibe o nome cadastrado do usuario; o login (e-mail) e apenas a chave de busca.
    /// </summary>
    private async Task CarregarSaudacao()
    {
        var nome = Preferences.Default.Get("PerfilNome", string.Empty);

        try
        {
            var login = await SecureStorage.Default.GetAsync("UsuarioLogado");

            if (!string.IsNullOrWhiteSpace(login) &&
                Servicos?.GetService(typeof(AppDbContext)) is AppDbContext contexto)
            {
                var usuario = await contexto.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Login == login);

                if (usuario != null && !string.IsNullOrWhiteSpace(usuario.NomeUsuario))
                {
                    nome = usuario.NomeUsuario;
                    Preferences.Default.Set("PerfilNome", nome);
                }
            }
        }
        catch
        {
            // sem banco disponivel, usa o nome guardado no dispositivo
        }

        if (string.IsNullOrWhiteSpace(nome))
            nome = "Usuário";

        lblBoasVindas.Text = $"Olá, {PrimeirosNomes(nome)}!";
    }

    /// <summary>
    /// Mantem a saudacao em uma linha: nome e sobrenome bastam.
    /// </summary>
    private static string PrimeirosNomes(string nomeCompleto)
    {
        var partes = nomeCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length <= 2 ? nomeCompleto : $"{partes[0]} {partes[1]}";
    }

    private async Task CarregarProximoLembrete()
    {
        Lembrete? proximo = null;

        try
        {
            if (Servicos?.GetService<ILembreteService>() is ILembreteService servico)
            {
                var agora = DateTime.Now;
                var lembretes = (await servico.GetAllAsync()).ToList();

                _diasComLembrete = lembretes.Select(l => l.DataHoraLembrete.Date).ToHashSet();

                proximo = lembretes
                    .Where(l => l.DataHoraLembrete >= agora)
                    .OrderBy(l => l.DataHoraLembrete)
                    .FirstOrDefault();
            }
        }
        catch
        {
            // sem acesso ao banco, a tela mostra o convite de cadastro
        }

        if (proximo == null)
        {
            MostrarSemLembrete();
            return;
        }

        PainelLembrete.IsVisible = true;
        PainelSemLembrete.IsVisible = false;

        lblDiasProximoLembrete.Text = DescreverPrazo(proximo.DataHoraLembrete);
        lblDataProximoLembrete.Text = proximo.DataHoraLembrete.ToString("dddd, dd/MM/yyyy 'às' HH:mm");

        lblTituloLembrete.Text = string.IsNullOrWhiteSpace(proximo.TipoLembrete?.Nome)
            ? proximo.Nome
            : $"{proximo.TipoLembrete!.Nome} - {proximo.Nome}";

        lblDescricaoLembrete.Text = string.IsNullOrWhiteSpace(proximo.Descricao)
            ? "Sem descrição."
            : proximo.Descricao;

        lblIniciaisLembrete.Text = Iniciais(proximo.Nome);

        var imagem = ImagemLembrete.Obter(proximo.Id);
        imgLembrete.Source = imagem;
        imgLembrete.IsVisible = imagem != null;
    }

    private void MostrarSemLembrete()
    {
        PainelLembrete.IsVisible = false;
        PainelSemLembrete.IsVisible = true;

        lblDiasProximoLembrete.Text = "Nenhum lembrete agendado";
        lblDataProximoLembrete.Text = string.Empty;
    }

    /// <summary>
    /// Texto do prazo restante, contado em dias inteiros de calendario.
    /// </summary>
    private static string DescreverPrazo(DateTime dataHora)
    {
        var dias = (dataHora.Date - DateTime.Today).Days;

        return dias switch
        {
            <= 0 => "Hoje",
            1 => "Amanhã",
            _ => $"{dias} dias"
        };
    }

    private static string Iniciais(string texto)
    {
        var partes = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 0)
            return "?";

        return partes.Length == 1
            ? partes[0][..1].ToUpperInvariant()
            : $"{partes[0][0]}{partes[1][0]}".ToUpperInvariant();
    }

    private void BotaoMenu_Tapped(object sender, TappedEventArgs e)
    {
        // O menu de tres linhas abre a tela que antes era aberta pela imagem redonda
        App.Current.MainPage = new Configuracoes();
    }

    private void BotaoPerfil_Clicked(object sender, EventArgs e)
    {
        // A imagem redonda abre as configuracoes do proprio usuario
        App.Current.MainPage = new ConfiguracoesUsuario();
    }

    private void BotaoCadastrarLembrete_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new CadastroLembrete(() => new TelaInicial());
    }

    private void BotaoVerLembretes_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Lembretes(() => new TelaInicial());
    }

    private void WeekCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selecionandoDiaAtual)
            return;

        if (e.CurrentSelection is not { Count: > 0 } || e.CurrentSelection[0] is not WeekDay selecionado)
            return;

        // Sem limpar a selecao, tocar duas vezes no mesmo dia nao dispara o
        // evento de novo e a tela pareceria travada.
        if (sender is CollectionView lista)
        {
            _selecionandoDiaAtual = true;
            lista.SelectedItem = null;
            _selecionandoDiaAtual = false;
        }

        // Trocar a MainPage dentro do proprio evento de selecao quebra a
        // renderizacao; a navegacao vai para o proximo ciclo da interface.
        Dispatcher.Dispatch(() =>
            App.Current.MainPage = new Calendario(selecionado.Date, () => new TelaInicial()));
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

        // Comeca no domingo da semana atual: DayOfWeek.Sunday e zero, entao o
        // proprio valor do dia diz quantos dias voltar.
        int diff = (int)today.DayOfWeek;
        var inicioSemana = today.AddDays(-diff);

        for (int i = 0; i < 7; i++)
        {
            var dt = inicioSemana.AddDays(i);
            WeekDays.Add(new WeekDay
            {
                Date = dt,
                ShortName = labelsMap[dt.DayOfWeek],
                DayNumber = dt.Day.ToString(),
                IsToday = dt.Date == today.Date,
                TemLembrete = _diasComLembrete.Contains(dt.Date)
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
    public bool TemLembrete { get; set; }

    private static bool Escuro =>
        Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

    // Mesmo criterio do calendario: hoje em azul escuro, dia com lembrete em
    // azul da paleta e, quando coincidem, o contorno mantem os dois sentidos.
    public Color CircleColor
    {
        get
        {
            if (IsToday)
                return Color.FromArgb("#48547C");

            if (TemLembrete)
                return Color.FromArgb("#749DD0");

            return Color.FromArgb(Escuro ? "#3F4149" : "#E8F2FB");
        }
    }

    public Color NumberColor
    {
        get
        {
            if (IsToday)
                return Color.FromArgb("#EDF3F8");

            // Sobre o azul da paleta, o texto escuro tem mais contraste
            if (TemLembrete)
                return Color.FromArgb("#33343B");

            return Color.FromArgb(Escuro ? "#EDF3F8" : "#33343B");
        }
    }

    public Color LabelColor => IsToday
        ? Color.FromArgb(Escuro ? "#EDF3F8" : "#48547C")
        : Color.FromArgb("#AAA59F");

    public Color ContornoColor => Color.FromArgb("#749DD0");

    public double ContornoEspessura => IsToday && TemLembrete ? 2 : 0;

    public event PropertyChangedEventHandler? PropertyChanged;
}
