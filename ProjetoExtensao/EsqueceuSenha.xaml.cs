using Microsoft.EntityFrameworkCore;

namespace ProjetoExtensao;

public partial class EsqueceuSenha : ContentPage
{
    public EsqueceuSenha()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();
    }

    private void MostrarCarregando(bool exibindo)
    {
        LoadingOverlay.IsVisible = exibindo;
        Loader.IsRunning = exibindo;
        btnPesquisar.IsEnabled = !exibindo;
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
            MostrarCarregando(true);
            await Task.Delay(50);

            var usuario = await context.Set<ProjetoExtensao.Entities.Usuario>()
                .FirstOrDefaultAsync(u => u.Login == emailInformado);

            if (usuario == null)
            {
                await DisplayAlert("Não encontrado", $"Não foi encontrado um e-mail cadastrado com {emailInformado}.", "Fechar");
                App.Current.MainPage = new Login();
                return;
            }

            // Gera o código e envia por e-mail
            var codigo = ProjetoExtensao.Services.ConfirmationCodeService.GerarCodigoPara(emailInformado);

            var corpo =
                $"Olá, {usuario.NomeUsuario}!\n\n" +
                $"Seu código de acesso para redefinir a senha é: {codigo}\n\n" +
                $"O código vale por {(int)ProjetoExtensao.Services.ConfirmationCodeService.Validade.TotalMinutes} minutos. " +
                "Se você não solicitou a troca de senha, ignore esta mensagem.";

            var envio = await ProjetoExtensao.Services.EmailSender.EnviarAsync(
                emailInformado, "Código de acesso - Não Me Esquece", corpo);

            switch (envio)
            {
                case ProjetoExtensao.Services.EmailSender.Resultado.Falhou:
                    await DisplayAlert("Erro", "Não foi possível enviar o e-mail com o código. Verifique sua conexão e tente novamente.", "Fechar");
                    return;

                case ProjetoExtensao.Services.EmailSender.Resultado.NaoConfigurado:
                    // Sem SMTP configurado o código não sai do dispositivo; mostrá-lo
                    // aqui mantém o fluxo utilizável em desenvolvimento.
                    await DisplayAlert("E-mail não configurado",
                        $"O envio de e-mail não está configurado neste ambiente.\n\nSeu código de acesso é: {codigo}", "Continuar");
                    break;

                default:
                    await DisplayAlert("Código enviado", $"Enviamos um código de acesso para {emailInformado}.", "Continuar");
                    break;
            }

            App.Current.MainPage = new CodigoConfirmacao(emailInformado);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Ocorreu um erro ao consultar o e-mail. " + ex.Message, "Fechar");
        }
        finally
        {
            MostrarCarregando(false);
        }
    }
}