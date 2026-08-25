using Microsoft.Maui.Controls;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.DTOs;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Mappings;
using System;

namespace ProjetoExtensao;

public partial class CadastroTipoEvento : ContentPage
{
    private readonly ITipoEventoService _service;
    private TipoEventoDto? _dto;

    public CadastroTipoEvento(
        ITipoEventoService service)
    {
        InitializeComponent();
        _service = service;
    }

    public CadastroTipoEvento(ITipoEventoService service, TipoEventoDto dto) : this(service)
    {
        _dto = dto;
        PreencherCampos();
    }

    private void PreencherCampos()
    {
        if (_dto == null) return;
        entryNome.Text = _dto.Nome;
    }

    private async void BtnSalvar_Clicked(object sender, EventArgs e)
    {
        var nome = entryNome.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nome))
        {
            await DisplayAlert("Atenção", "Informe o nome do tipo de evento.", "OK");
            return;
        }

        if (_dto == null || _dto.Id == 0)
        {
            var entidade = new TipoEvento(nome);
            await _service.AddAsync(entidade);
        }
        else
        {
            var entidade = _dto.ToEntity();
            entidade.Nome = nome;
            await _service.UpdateAsync(entidade);
        }

        await Navigation.PopAsync();
    }

    private async void BtnCancelar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}