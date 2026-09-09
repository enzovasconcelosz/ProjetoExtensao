using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjetoExtensao.Application.Interfaces;
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
        Navegacao.IrPara(new EsqueceuSenha());
    }

    /// <summary>
    /// Recria os avisos dos lembretes futuros neste aparelho. Necessario porque
    /// o agendamento vive no sistema operacional, nao no banco: um aparelho novo
    /// (ou reinstalado) entra sem nenhum aviso marcado.
    /// </summary>
    private static async Task ReagendarAvisosAsync()
    {
        try
        {
            var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
            var service = services?.GetService<ILembreteService>();

            if (service == null)
                return;

            await NotificacaoLembrete.ReagendarTodosAsync(await service.GetAllAsync());
        }
        catch
        {
            // O login nao pode falhar por causa dos avisos
        }
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

            // Antes de qualquer consulta: e o Id da sessao que separa os dados
            // desta conta dos das outras.
            SessaoUsuario.Entrar(usuario.Id);

            await SecureStorage.Default.SetAsync("UsuarioLogado", usuario.Login);
            Preferences.Default.Set("PerfilNome", usuario.NomeUsuario);
            Preferences.Default.Set("PerfilEmail", usuario.Login);

            if (Senha != null)
                Senha.Text = string.Empty;

            // A escolha de notificacao acompanha a conta: em um aparelho novo
            // ela vem do banco, e os avisos passam a existir tambem aqui.
            await PreferenciaNotificacao.CarregarDoBancoAsync(usuario.Login);
            await ReagendarAvisosAsync();

            Navegacao.IrPara(new TelaInicial());
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
        Navegacao.IrPara(new Cadastro());
    }

    private void TogglePasswordVisibility_Clicked(object sender, EventArgs e)
    {
        if (Senha == null)
            return;

        Senha.IsPassword = !Senha.IsPassword;
        btnVerSenha.Text = Senha.IsPassword ? "Mostrar" : "Ocultar";
    }
}
