namespace ProjetoExtensao;

public partial class TelaInicial : ContentPage
{
    public TelaInicial()
    {
        InitializeComponent();

        string? usuarioLogado = null;

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        string? UsuarioLogado = await SecureStorage.Default.GetAsync("UsuarioLogado");
        lblBoasVindas.Text = $"Olá, {UsuarioLogado}!";
    }

    private void BotaoConfiguracoes_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Configuracoes();
    }
}