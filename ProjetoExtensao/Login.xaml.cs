using ProjetoExtensao.Entities;

namespace ProjetoExtensao;

public partial class Login : ContentPage
{
    public Login()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();
    }

    private void BotaoEsqueceuSenha_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new EsqueceuSenha();
    }

    private void BotaoLogin_Clicked(object sender, EventArgs e)
    {
        //var listaUsuarios = new List<Usuario>()
        //{
        //    new("a", "a", "Enzo")
        //};

        //var usuarioTentativaLogin = listaUsuarios.FirstOrDefault(x => x.Login == Usuario.Text && x.Senha == Senha.Text);

        //if (usuarioTentativaLogin != null)
        //{
            //SecureStorage.Default.SetAsync("UsuarioLogado", usuarioTentativaLogin.Login);

            App.Current.MainPage = new TelaInicial();

            return;
        //}

        //DisplayAlertAsync("Erro no login!", "O e-mail ou senha informados estão incorretos.", "Fechar");
    }

    private void BotaoCadastrar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Cadastro();
    }

    private void TogglePasswordVisibility_Clicked(object sender, EventArgs e)
    {
        if (Senha != null)
            Senha.IsPassword = !Senha.IsPassword;
    }
}