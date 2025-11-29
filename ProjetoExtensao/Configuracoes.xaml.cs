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
}