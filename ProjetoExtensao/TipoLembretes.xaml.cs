using Microsoft.Extensions.DependencyInjection;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.DTOs;
using ProjetoExtensao.Mappings;

namespace ProjetoExtensao;

public partial class TipoLembretes : ContentPage
{
    private ITipoLembreteService? _service;
    private List<TipoLembreteDto> _dados = new();

    public TipoLembretes()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        _service = ObterServico();
        await CarregarLista();
    }

    private static ITipoLembreteService? ObterServico()
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        return services?.GetService<ITipoLembreteService>();
    }

    private async Task CarregarLista()
    {
        if (_service == null)
        {
            MostrarVazio("Serviço de tipos de lembrete indisponível.");
            return;
        }

        try
        {
            var lista = await _service.GetAllAsync();
            _dados = lista.Select(t => t.ToDto()).ToList();
            cvTipos.ItemsSource = _dados;

            lblVazio.Text = "Nenhum tipo de lembrete cadastrado. Toque em + para criar o primeiro.";
            lblVazio.IsVisible = _dados.Count == 0;
        }
        catch (Exception ex)
        {
            MostrarVazio("Não foi possível carregar a lista. " + ex.Message);
        }
    }

    private void MostrarVazio(string mensagem)
    {
        _dados = new List<TipoLembreteDto>();
        cvTipos.ItemsSource = _dados;
        lblVazio.Text = mensagem;
        lblVazio.IsVisible = true;
    }

    private void BtnAdicionar_Clicked(object sender, EventArgs e)
    {
        if (_service == null)
            return;

        Navegacao.IrPara(new CadastroTipoLembrete(_service));
    }

    private void BtnEditar_Clicked(object sender, EventArgs e)
    {
        if (_service != null && sender is Button botao && botao.BindingContext is TipoLembreteDto dto)
            Navegacao.IrPara(new CadastroTipoLembrete(_service, dto));
    }

    private async void BtnExcluir_Clicked(object sender, EventArgs e)
    {
        if (_service == null || sender is not Button botao || botao.BindingContext is not TipoLembreteDto dto)
            return;

        bool ok = await DisplayAlert("Confirmar", $"Deseja excluir '{dto.Nome}'?", "Sim", "Não");
        if (!ok)
            return;

        try
        {
            await _service.DeleteAsync(dto.Id);
            await CarregarLista();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível excluir. " + ex.Message, "Fechar");
        }
    }

    private void BotaoVoltar_Clicked(object sender, EventArgs e)
    {
        Navegacao.IrPara(new Configuracoes());
    }
}
