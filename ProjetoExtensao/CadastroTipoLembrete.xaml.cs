using Microsoft.Maui.Controls;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.DTOs;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Mappings;
using System;

namespace ProjetoExtensao;

public partial class CadastroTipoLembrete : ContentPage
{
    private readonly ITipoLembreteService _service;
    private TipoLembreteDto? _dto;

    public CadastroTipoLembrete(
        ITipoLembreteService service)
    {
        InitializeComponent();
        _service = service;
        Tema.Aplicar();
    }

    public CadastroTipoLembrete(ITipoLembreteService service, TipoLembreteDto dto) : this(service)
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
            await DisplayAlert("Atenção", "Informe o nome do tipo de lembrete.", "OK");
            return;
        }

        try
        {
            if (_dto == null || _dto.Id == 0)
            {
                var entidade = new TipoLembrete(nome);
                await _service.AddAsync(entidade);
            }
            else
            {
                var entidade = _dto.ToEntity();
                entidade.Nome = nome;
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
        App.Current.MainPage = new TipoLembretes();
    }
}