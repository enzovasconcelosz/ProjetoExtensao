using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Infrastructure.Data;

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

    private async void BotaoLogin_Clicked(object sender, EventArgs e)
    {
        var login = Usuario?.Text?.Trim();
        var senha = Senha?.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
        {
            await DisplayAlert("Atenção", "Informe o e-mail e a senha.", "Fechar");
            return;
        }

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        if (services?.GetService(typeof(AppDbContext)) is not AppDbContext contexto)
        {
            await DisplayAlert("Erro", "Não foi possível acessar o banco de dados.", "Fechar");
            return;
        }

        MostrarCarregando(true);

        try
        {
            var usuario = await contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Login == login);

            // A mesma mensagem para usuario inexistente e senha errada, para nao
            // revelar quais e-mails estao cadastrados.
            if (usuario == null || !SenhaHash.Conferir(senha, usuario.Senha))
            {
                await DisplayAlert("Erro no login!", "O e-mail ou senha informados estão incorretos.", "Fechar");
                return;
            }

            // Cadastros antigos podem ter a senha em texto puro; ao acertar a
            // senha, aproveita para grava-la com hash.
            if (SenhaHash.EmTextoPuro(usuario.Senha))
            {
                usuario.Senha = SenhaHash.Gerar(senha);
                await contexto.SaveChangesAsync();
            }

            await SecureStorage.Default.SetAsync("UsuarioLogado", usuario.Login);
            Preferences.Default.Set("PerfilNome", usuario.NomeUsuario);
            Preferences.Default.Set("PerfilEmail", usuario.Login);

            if (Senha != null)
                Senha.Text = string.Empty;

            App.Current.MainPage = new TelaInicial();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível concluir o login. " + ex.Message, "Fechar");
        }
        finally
        {
            MostrarCarregando(false);
        }
    }

    private void MostrarCarregando(bool exibindo)
    {
        LoadingOverlay.IsVisible = exibindo;
        Loader.IsRunning = exibindo;
        btnLogin.IsEnabled = !exibindo;
    }

    private void BotaoCadastrar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Cadastro();
    }

    private void TogglePasswordVisibility_Clicked(object sender, EventArgs e)
    {
        if (Senha == null)
            return;

        Senha.IsPassword = !Senha.IsPassword;
        btnVerSenha.Text = Senha.IsPassword ? "Mostrar" : "Ocultar";
    }
}
