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
        string? UsuarioLogado = await SecureStorage.Default.GetAsync("UsuarioLogado");
        lblUsuarioLogado.Text = UsuarioLogado;
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

    private async void BtnTipoEventos_Clicked(object sender, EventArgs e)
    {
        try
        {
            App.Current.MainPage = new TipoEventos();
        }
        catch
        {
            // ignore navigation failures
        }
        //try
        //{
        //    var svc = Application.Current?.Handler?.MauiContext?.Services?.GetService<ProjetoExtensao.Application.Interfaces.ITipoEventoService>();
        //    if (svc == null)
        //    {
        //        await DisplayAlert("Erro", "Serviço de tipo de eventos não disponível.", "OK");
        //        return;
        //    }

        //    var pagina = new TipoEventos(svc);
        //    await Navigation.PushAsync(pagina);
        //}
        //catch (Exception ex)
        //{
        //    await DisplayAlert("Erro", ex.Message, "OK");
        //}
    }
}