using Microsoft.Extensions.DependencyInjection;
using ProjetoExtensao.Application.Interfaces;

namespace ProjetoExtensao;

public partial class Configuracoes : ContentPage
{
    public Configuracoes()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        imgPerfil.Source = ImagemPerfil.Obter();

        // Exibe o nome cadastrado, nao o login (e-mail)
        var nome = Preferences.Default.Get("PerfilNome", string.Empty);

        if (string.IsNullOrWhiteSpace(nome))
            nome = await SecureStorage.Default.GetAsync("UsuarioLogado") ?? "Usuário";

        lblUsuarioLogado.Text = nome;
    }

    private async void BotaoSair_Clicked(object sender, EventArgs e)
    {
        bool confirmacao = await DisplayAlertAsync("Tem certeza?", "Deseja realmente desconectar do aplicativo?", "Sim", "Não");

        if (confirmacao)
        {
            SecureStorage.Default.Remove("UsuarioLogado");
            App.Current.MainPage = new Login();
        }
    }

    private void BotaoPerfil_Clicked(object sender, EventArgs e)
    {
        // Aberta a partir daqui, a tela de perfil volta para as configuracoes
        App.Current.MainPage = new ConfiguracoesUsuario(() => new Configuracoes());
    }

    private void BtnLembretes_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Lembretes();
    }

    private void BtnCalendario_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Calendario(null, () => new Configuracoes());
    }

    private void BotaoAparencia_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new ConfiguracoesAparencia();
    }

    private void BotaoVoltar_Clicked(object sender, EventArgs e)
    {
        try
        {
            App.Current.MainPage = new TelaInicial();
        }
        catch
        {
            // ignore navigation failures
        }
    }

    private async void BtnTipoLembretes_Clicked(object sender, EventArgs e)
    {
        try
        {
            App.Current.MainPage = new TipoLembretes();
        }
        catch
        {
            // ignore navigation failures
        }
        //try
        //{
        //    var svc = Application.Current?.Handler?.MauiContext?.Services?.GetService<ProjetoExtensao.Application.Interfaces.ITipoLembreteService>();
        //    if (svc == null)
        //    {
        //        await DisplayAlert("Erro", "Serviço de tipo de lembretes não disponível.", "OK");
        //        return;
        //    }

        //    var pagina = new TipoLembretes(svc);
        //    await Navigation.PushAsync(pagina);
        //}
        //catch (Exception ex)
        //{
        //    await DisplayAlert("Erro", ex.Message, "OK");
        //}
    }
}