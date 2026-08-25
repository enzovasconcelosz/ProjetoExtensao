using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.DTOs;
using ProjetoExtensao.Mappings;
using ProjetoExtensao.Entities;

namespace ProjetoExtensao;

public partial class TipoEventos : ContentPage
{
    private readonly ITipoEventoService _service;
    private List<TipoEventoDto> _dados = new();

    public TipoEventos(
        //ITipoEventoService service
        )
    {
        InitializeComponent();
       // _service = service;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarLista();
    }

    private async Task CarregarLista()
    {
        //var lista = await _service.GetAllAsync();
        //_dados = lista.Select(t => t.ToDto()).ToList();
        //cvTipos.ItemsSource = _dados;
    }

    private async void BtnAdicionar_Clicked(object sender, EventArgs e)
    {
        var pagina = new CadastroTipoEvento(_service);
        await Navigation.PushAsync(pagina);
    }

    private async void BtnEditar_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton ib && ib.BindingContext is TipoEventoDto dto)
        {
            var pagina = new CadastroTipoEvento(_service, dto);
            await Navigation.PushAsync(pagina);
        }
    }

    private async void BtnExcluir_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton ib && ib.BindingContext is TipoEventoDto dto)
        {
            bool ok = await DisplayAlert("Confirmar", $"Deseja excluir '{dto.Nome}'?", "Sim", "Não");
            if (!ok) return;
            await _service.DeleteAsync(dto.Id);
            await CarregarLista();
        }
    }
}
