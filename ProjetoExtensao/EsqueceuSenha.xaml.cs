using Microsoft.EntityFrameworkCore;

namespace ProjetoExtensao;

public partial class EsqueceuSenha : ContentPage
{
    public EsqueceuSenha()
    {
        InitializeComponent();
    }

    private void BotaoCancelar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Login();
    }

    private async void BotaoPesquisar_Clicked(object sender, EventArgs e)
    {
        var emailInformado = Email?.Text?.Trim();

        if (string.IsNullOrWhiteSpace(emailInformado))
        {
            await DisplayAlert("Atenção", "Informe um e-mail válido.", "Fechar");
            return;
        }

        // Obter o contexto via DI  
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        var context = services?.GetService(typeof(ProjetoExtensao.Infrastructure.Data.AppDbContext)) as ProjetoExtensao.Infrastructure.Data.AppDbContext;
        if (context == null)
        {
            await DisplayAlert("Erro", "Não foi possível acessar o banco de dados.", "Fechar");
            return;
        }

        try
        {
            // Show loading overlay and disable button to prevent double clicks
            try { LoadingOverlay.IsVisible = true; Loader.IsRunning = true; btnPesquisar.IsEnabled = false; await Task.Delay(50); } catch { }

            var usuario = await context.Set<ProjetoExtensao.Entities.Usuario>()
                .FirstOrDefaultAsync(u => u.Login == emailInformado);

            if (usuario == null)
            {
                await DisplayAlert("Não encontrado", $"Não foi encontrado um e-mail cadastrado com {emailInformado}.", "Fechar");
                App.Current.MainPage = new Login();
                return;
            }

            // Gerar código e enviar e-mail
            var codigo = ProjetoExtensao.Services.ConfirmationCodeService.GenerateCodeFor(emailInformado);

            await ProjetoExtensao.Services.EmailSender.SendEmailAsync(emailInformado, "Código de recuperação", $"Seu código de recuperação é: {codigo}");

            // Abrir formulário de informar código
            App.Current.MainPage = new CodigoConfirmacao(emailInformado);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Ocorreu um erro ao consultar o e-mail. " + ex.Message, "Fechar");
        }
        finally
        {
            try { LoadingOverlay.IsVisible = false; Loader.IsRunning = false; btnPesquisar.IsEnabled = true; } catch { }
        }
    }
}