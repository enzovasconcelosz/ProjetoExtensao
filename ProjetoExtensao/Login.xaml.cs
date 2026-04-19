using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao;

public partial class Login : ContentPage
{

    public Login()
    {
        InitializeComponent();
    }

    private void BotaoEsqueceuSenha_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new EsqueceuSenha();
    }

    private void BotaoLogin_Clicked(object sender, EventArgs e)
    {
        var listaUsuarios = new List<Usuario>()
        {
            new("a", "a", "Enzo")
        };

        var usuarioTentativaLogin = listaUsuarios.FirstOrDefault(x => x.Login == Usuario.Text && x.Senha == Senha.Text);

        if (usuarioTentativaLogin != null)
        {
            SecureStorage.Default.SetAsync("UsuarioLogado", usuarioTentativaLogin.Login);

            App.Current.MainPage = new TelaInicial();

            return;
        }

        DisplayAlertAsync("Erro no login!", "O e-mail ou senha informados estão incorretos.", "Fechar");
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

    private async void BtnTestarConexao_Clicked(object sender, EventArgs e)
    {
        string stringDeConexao = "Server=192.168.1.4,1433;Database=NaoMeEsquece;User Id=sa;Password=123456;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(stringDeConexao);

        using (var context = new AppDbContext(optionsBuilder.Options))
        {
            try
            {
                bool conectou = await context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                // Lidar com o erro
                Console.WriteLine($"Erro ao conectar: {ex.Message}");
            }
        }
    }
}