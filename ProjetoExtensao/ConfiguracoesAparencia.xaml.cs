using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao;

/// <summary>
/// Aparencia (tema) do aplicativo. Aberta pela linha "Aparencia" das configuracoes.
/// </summary>
public partial class ConfiguracoesAparencia : ContentPage
{
    private bool _carregando;

    public ConfiguracoesAparencia()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        // Evita salvar durante a selecao inicial dos valores atuais
        _carregando = true;

        pickerTema.SelectedItem = Tema.Atual;

        pickerTamanhoFonte.ItemsSource = Acessibilidade.Opcoes.Select(o => o.Nome).ToList();
        pickerTamanhoFonte.SelectedItem = Acessibilidade.NomeAtual;

        _carregando = false;
    }

    private void PickerTamanhoFonte_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_carregando || pickerTamanhoFonte.SelectedIndex < 0)
            return;

        var escolha = Acessibilidade.Opcoes[pickerTamanhoFonte.SelectedIndex];
        Acessibilidade.Salvar(escolha.Tamanho);
    }

    private async void PickerTema_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_carregando || pickerTema.SelectedItem is not string tema)
            return;

        Tema.Salvar(tema);
        await VincularAparenciaAoUsuarioAsync(tema);
    }

    /// <summary>
    /// Guarda a escolha tambem na tabela Aparencia, quando ha usuario logado.
    /// </summary>
    private async Task VincularAparenciaAoUsuarioAsync(string descricao)
    {
        try
        {
            var login = await SecureStorage.Default.GetAsync("UsuarioLogado");
            if (string.IsNullOrWhiteSpace(login))
                return;

            var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
            if (services?.GetService(typeof(AppDbContext)) is not AppDbContext contexto)
                return;

            var usuario = await contexto.Usuarios.FirstOrDefaultAsync(u => u.Login == login);
            if (usuario == null)
                return;

            var aparencia = await contexto.Aparencias.FirstOrDefaultAsync(a => a.Descricao == descricao);
            if (aparencia == null)
            {
                aparencia = new Entities.Aparencia(descricao);
                contexto.Aparencias.Add(aparencia);
                await contexto.SaveChangesAsync();
            }

            usuario.IdAparencia = aparencia.Id;
            await contexto.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // O tema ja foi aplicado localmente; a falha aqui nao impede o uso
            await DisplayAlert("Atenção", "O tema foi aplicado, mas não foi possível salvá-lo no banco. " + ex.Message, "Fechar");
        }
    }

    private void BotaoVoltar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Configuracoes();
    }
}
