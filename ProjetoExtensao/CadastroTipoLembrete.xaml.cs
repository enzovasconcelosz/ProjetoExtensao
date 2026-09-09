using Microsoft.Maui.Controls;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Application.Validacoes;
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

        // Mesmas regras usadas pelo servico, aqui apenas para orientar o usuario
        var erros = ValidacaoTipoLembrete.Validar(nome);

        if (erros.Count > 0)
        {
            lblAviso.Text = string.Join("\n", erros);
            lblAviso.IsVisible = true;
            return;
        }

        lblAviso.IsVisible = false;

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
        Navegacao.IrPara(new TipoLembretes());
    }
}