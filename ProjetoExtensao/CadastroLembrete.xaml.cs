using Microsoft.Extensions.DependencyInjection;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.DTOs;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Mappings;

namespace ProjetoExtensao;

public partial class CadastroLembrete : ContentPage
{
    private readonly LembreteDto? _dto;
    private ILembreteService? _service;

    public CadastroLembrete()
    {
        InitializeComponent();
    }

    public CadastroLembrete(LembreteDto dto) : this()
    {
        _dto = dto;
        lblTitulo.Text = "Editar lembrete";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        _service = services?.GetService<ILembreteService>();

        await CarregarTipos(services?.GetService<ITipoLembreteService>());
        PreencherCampos();
    }

    private async Task CarregarTipos(ITipoLembreteService? tipoService)
    {
        if (tipoService == null)
            return;

        try
        {
            var tipos = (await tipoService.GetAllAsync()).Select(t => t.ToDto()).ToList();
            pickerTipo.ItemsSource = tipos;

            if (_dto?.IdTipoLembrete != null)
                pickerTipo.SelectedItem = tipos.FirstOrDefault(t => t.Id == _dto.IdTipoLembrete);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível carregar os tipos de lembrete. " + ex.Message, "Fechar");
        }
    }

    private void PreencherCampos()
    {
        if (_dto == null)
        {
            // Novo lembrete: sugere a proxima hora cheia
            var sugestao = DateTime.Now.AddHours(1);
            pickerData.Date = sugestao.Date;
            pickerHora.Time = new TimeSpan(sugestao.Hour, 0, 0);
            return;
        }

        entryNome.Text = _dto.Nome;
        editorDescricao.Text = _dto.Descricao;
        pickerData.Date = _dto.DataHoraLembrete.Date;
        pickerHora.Time = _dto.DataHoraLembrete.TimeOfDay;
    }

    private async void BtnSalvar_Clicked(object sender, EventArgs e)
    {
        var nome = entryNome.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nome))
        {
            await DisplayAlert("Atenção", "Informe o nome do lembrete.", "Fechar");
            return;
        }

        if (_service == null)
        {
            await DisplayAlert("Erro", "Serviço de lembretes indisponível.", "Fechar");
            return;
        }

        var descricao = editorDescricao.Text?.Trim() ?? string.Empty;
        var data = pickerData.Date ?? DateTime.Today;
        var hora = pickerHora.Time ?? TimeSpan.Zero;
        var dataHora = data.Date.Add(hora);
        var tipoSelecionado = pickerTipo.SelectedItem as TipoLembreteDto;

        try
        {
            if (_dto == null || _dto.Id == 0)
            {
                var entidade = new Lembrete(nome, descricao, dataHora)
                {
                    IdTipoLembrete = tipoSelecionado?.Id
                };

                await _service.AddAsync(entidade);
            }
            else
            {
                var entidade = _dto.ToEntity();
                entidade.Nome = nome;
                entidade.Descricao = descricao;
                entidade.DataHoraLembrete = dataHora;
                entidade.IdTipoLembrete = tipoSelecionado?.Id;

                await _service.UpdateAsync(entidade);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível salvar. " + ex.Message, "Fechar");
            return;
        }

        Voltar();
    }

    private void BtnCancelar_Clicked(object sender, EventArgs e)
    {
        Voltar();
    }

    // O projeto troca a MainPage em vez de usar pilha de navegacao
    private static void Voltar()
    {
        App.Current.MainPage = new Lembretes();
    }
}
