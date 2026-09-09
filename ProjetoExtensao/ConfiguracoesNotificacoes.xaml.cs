using Microsoft.Extensions.DependencyInjection;
using ProjetoExtensao.Application.Interfaces;

namespace ProjetoExtensao;

/// <summary>
/// Notificacoes dos lembretes. Aberta pela linha "Notificacoes" das configuracoes.
///
/// Mudar a escolha reagenda os avisos ja existentes: no Android o som e a
/// vibracao vem do canal, entao um aviso agendado no canal antigo continuaria
/// se comportando do jeito antigo.
/// </summary>
public partial class ConfiguracoesNotificacoes : ContentPage
{
    private bool _carregando;

    public ConfiguracoesNotificacoes()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        // Evita salvar e reagendar durante a leitura dos valores atuais
        _carregando = true;

        switchNotificar.IsToggled = PreferenciaNotificacao.Notificar;
        switchVibrar.IsToggled = PreferenciaNotificacao.Vibrar;
        switchSom.IsToggled = PreferenciaNotificacao.Som;

        _carregando = false;

        AtualizarTela();

        lblPermissao.IsVisible = PreferenciaNotificacao.Notificar
            && !await NotificacaoLembrete.GarantirPermissaoAsync();
    }

    private async void SwitchNotificar_Toggled(object sender, ToggledEventArgs e)
    {
        if (_carregando)
            return;

        // Ligar o aviso so tem efeito depois que o sistema autoriza
        if (e.Value)
            lblPermissao.IsVisible = !await NotificacaoLembrete.GarantirPermissaoAsync();
        else
            lblPermissao.IsVisible = false;

        await SalvarEReagendarAsync();
    }

    private async void SwitchPreferencia_Toggled(object sender, ToggledEventArgs e)
    {
        if (_carregando)
            return;

        await SalvarEReagendarAsync();
    }

    private async Task SalvarEReagendarAsync()
    {
        await PreferenciaNotificacao.SalvarAsync(
            switchNotificar.IsToggled,
            switchVibrar.IsToggled,
            switchSom.IsToggled);

        AtualizarTela();
        await ReagendarLembretesAsync();
    }

    private void AtualizarTela()
    {
        cardComoAvisar.IsEnabled = switchNotificar.IsToggled;
        cardComoAvisar.Opacity = switchNotificar.IsToggled ? 1 : 0.5;
        lblResumo.Text = "Seus lembretes vão avisar assim: " + PreferenciaNotificacao.Resumo().ToLowerInvariant() + ".";
    }

    /// <summary>
    /// Reaplica a escolha aos lembretes futuros ja cadastrados.
    /// </summary>
    private async Task ReagendarLembretesAsync()
    {
        try
        {
            var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
            var service = services?.GetService<ILembreteService>();

            if (service == null)
                return;

            await NotificacaoLembrete.ReagendarTodosAsync(await service.GetAllAsync());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Atenção", "A escolha foi salva, mas não foi possível reagendar os lembretes já cadastrados. " + ex.Message, "Fechar");
        }
    }

    private async void BotaoTestar_Clicked(object sender, EventArgs e)
    {
        if (!await NotificacaoLembrete.GarantirPermissaoAsync())
        {
            lblPermissao.IsVisible = true;
            await DisplayAlert("Notificações bloqueadas",
                "O sistema não autorizou as notificações do aplicativo. Libere-as nas configurações do aparelho para receber os avisos.",
                "Fechar");
            return;
        }

        lblPermissao.IsVisible = false;

        // Id negativo: nao colide com o de nenhum lembrete
        await NotificacaoLembrete.AgendarAsync(
            id: -1,
            nome: "Teste de aviso",
            descricao: "É assim que o Não Me Esquece vai avisar você.",
            dataHora: DateTime.Now.AddSeconds(5));

        await DisplayAlert("Aviso enviado", "O aviso de teste chega em alguns segundos.", "Fechar");
    }

    private void BotaoVoltar_Clicked(object sender, EventArgs e)
    {
        Navegacao.IrPara(new Configuracoes());
    }
}
