using Microsoft.Extensions.DependencyInjection;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.DTOs;
using ProjetoExtensao.Mappings;

namespace ProjetoExtensao;

public partial class Lembretes : ContentPage
{
    private ILembreteService? _service;

    // Quem abriu a tela: o retorno volta para a mesma origem
    private readonly Func<Page> _paginaAnterior;

    public Lembretes(Func<Page>? paginaAnterior = null)
    {
        InitializeComponent();
        _paginaAnterior = paginaAnterior ?? (() => new Configuracoes());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        _service = ObterServico();
        await CarregarLista();
    }

    private static ILembreteService? ObterServico()
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        return services?.GetService<ILembreteService>();
    }

    private async Task CarregarLista()
    {
        if (_service == null)
        {
            MostrarVazio("Serviço de lembretes indisponível.");
            return;
        }

        try
        {
            var lista = await _service.GetAllAsync();

            var itens = lista
                .Select(l => new LembreteItem(l.ToDto(), l.TipoLembrete?.Nome))
                .ToList();

            cvLembretes.ItemsSource = itens;

            lblVazio.Text = "Nenhum lembrete cadastrado. Toque em + para criar o primeiro.";
            lblVazio.IsVisible = itens.Count == 0;
        }
        catch (Exception ex)
        {
            MostrarVazio("Não foi possível carregar a lista. " + ex.Message);
        }
    }

    private void MostrarVazio(string mensagem)
    {
        cvLembretes.ItemsSource = new List<LembreteItem>();
        lblVazio.Text = mensagem;
        lblVazio.IsVisible = true;
    }

    private void BtnAdicionar_Clicked(object sender, EventArgs e)
    {
        Navegacao.IrPara(new CadastroLembrete(() => new Lembretes(_paginaAnterior)));
    }

    private void BtnEditar_Clicked(object sender, EventArgs e)
    {
        if (sender is Button botao && botao.BindingContext is LembreteItem item)
            Navegacao.IrPara(new CadastroLembrete(item.Dto, () => new Lembretes(_paginaAnterior)));
    }

    private async void BtnExcluir_Clicked(object sender, EventArgs e)
    {
        if (_service == null || sender is not Button botao || botao.BindingContext is not LembreteItem item)
            return;

        bool ok = await DisplayAlert("Confirmar", $"Deseja excluir '{item.Nome}'?", "Sim", "Não");
        if (!ok)
            return;

        try
        {
            await _service.DeleteAsync(item.Dto.Id);
            ImagemLembrete.Remover(item.Dto.Id);
            NotificacaoLembrete.Cancelar(item.Dto.Id);
            await CarregarLista();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível excluir. " + ex.Message, "Fechar");
        }
    }

    private void BotaoVoltar_Clicked(object sender, EventArgs e)
    {
        Navegacao.IrPara(_paginaAnterior());
    }
}

/// <summary>
/// Linha da lista: o DTO mais um resumo pronto para exibicao.
/// </summary>
public class LembreteItem
{
    public LembreteItem(LembreteDto dto, string? nomeTipo)
    {
        Dto = dto;

        var partes = new List<string> { dto.DataHoraLembrete.ToString("dd/MM/yyyy HH:mm") };

        if (!string.IsNullOrWhiteSpace(nomeTipo))
            partes.Add(nomeTipo);

        Resumo = string.Join("  •  ", partes);
    }

    public LembreteDto Dto { get; }

    public string Nome => Dto.Nome;

    public string Resumo { get; }
}
