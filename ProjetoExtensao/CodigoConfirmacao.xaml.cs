using ProjetoExtensao.Services;

namespace ProjetoExtensao;

public partial class CodigoConfirmacao : ContentPage
{
    private readonly string _email;

    public CodigoConfirmacao(string email)
    {
        InitializeComponent();
        _email = email;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        lblInstrucao.Text =
            $"Informe o código de 6 dígitos enviado para {_email}. " +
            $"Ele vale por {(int)ConfirmationCodeService.Validade.TotalMinutes} minutos.";
    }

    private async void BtnConfirmarCodigo_Clicked(object sender, EventArgs e)
    {
        var codigo = CodigoEntry?.Text?.Trim();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            MostrarAviso("Informe o código recebido por e-mail.");
            return;
        }

        switch (ConfirmationCodeService.Validar(_email, codigo))
        {
            case ConfirmationCodeService.ResultadoValidacao.Valido:
                LimparAviso();
                App.Current.MainPage = new AlterarSenha(_email);
                return;

            case ConfirmationCodeService.ResultadoValidacao.Incorreto:
                var restantes = ConfirmationCodeService.TentativasRestantes(_email);
                MostrarAviso($"Código incorreto. Você ainda tem {restantes} tentativa(s).");
                return;

            case ConfirmationCodeService.ResultadoValidacao.Expirado:
                await DisplayAlert("Código expirado", "O código perdeu a validade. Solicite um novo.", "Fechar");
                App.Current.MainPage = new EsqueceuSenha();
                return;

            case ConfirmationCodeService.ResultadoValidacao.TentativasExcedidas:
                await DisplayAlert("Tentativas excedidas", "O código foi invalidado por excesso de tentativas. Solicite um novo.", "Fechar");
                App.Current.MainPage = new EsqueceuSenha();
                return;

            default:
                await DisplayAlert("Código não encontrado", "Nenhum código pendente para este e-mail. Solicite um novo.", "Fechar");
                App.Current.MainPage = new EsqueceuSenha();
                return;
        }
    }

    private async void BtnReenviar_Clicked(object sender, EventArgs e)
    {
        var espera = ConfirmationCodeService.EsperaParaReenvio(_email);
        if (espera > TimeSpan.Zero)
        {
            MostrarAviso($"Aguarde {espera.Seconds + espera.Minutes * 60} segundo(s) para pedir um novo código.");
            return;
        }

        MostrarCarregando(true);

        try
        {
            var codigo = ConfirmationCodeService.GerarCodigoPara(_email);

            var corpo =
                $"Seu novo código de acesso é: {codigo}\n\n" +
                $"O código vale por {(int)ConfirmationCodeService.Validade.TotalMinutes} minutos.";

            var envio = await EmailSender.EnviarAsync(_email, "Código de acesso - Não Me Esquece", corpo);

            LimparAviso();
            CodigoEntry.Text = string.Empty;

            switch (envio)
            {
                case EmailSender.Resultado.Falhou:
                    await DisplayAlert("Erro", "Não foi possível reenviar o e-mail. Tente novamente.", "Fechar");
                    break;

                case EmailSender.Resultado.NaoConfigurado:
                    await DisplayAlert("E-mail não configurado",
                        $"O envio de e-mail não está configurado neste ambiente.\n\nSeu código de acesso é: {codigo}", "Fechar");
                    break;

                default:
                    await DisplayAlert("Código reenviado", $"Enviamos um novo código para {_email}.", "Fechar");
                    break;
            }
        }
        finally
        {
            MostrarCarregando(false);
        }
    }

    private void BtnCancelar_Clicked(object sender, EventArgs e)
    {
        // Descarta o código pendente ao desistir da recuperação
        ConfirmationCodeService.Descartar(_email);
        App.Current.MainPage = new Login();
    }

    private void MostrarAviso(string mensagem)
    {
        lblAviso.Text = mensagem;
        lblAviso.IsVisible = true;
    }

    private void LimparAviso()
    {
        lblAviso.Text = string.Empty;
        lblAviso.IsVisible = false;
    }

    private void MostrarCarregando(bool exibindo)
    {
        LoadingOverlay.IsVisible = exibindo;
        Loader.IsRunning = exibindo;
        btnReenviar.IsEnabled = !exibindo;
        btnConfirmarCodigo.IsEnabled = !exibindo;
    }
}
