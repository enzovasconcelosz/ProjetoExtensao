using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao;

public partial class AlterarSenha : ContentPage
{
    private const int TamanhoMinimoSenha = 6;

    private readonly string _email;

    public AlterarSenha(string email)
    {
        InitializeComponent();
        _email = email;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();
    }

    private async void BtnConfirmar_Clicked(object sender, EventArgs e)
    {
        var nova = NovaSenhaEntry?.Text?.Trim();
        var confirmacao = ConfirmacaoSenhaEntry?.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nova) || string.IsNullOrWhiteSpace(confirmacao))
        {
            MostrarAviso("Informe a nova senha e a confirmação.");
            return;
        }

        if (nova.Length < TamanhoMinimoSenha)
        {
            MostrarAviso($"A senha deve conter ao menos {TamanhoMinimoSenha} caracteres.");
            return;
        }

        if (nova != confirmacao)
        {
            MostrarAviso("As senhas não conferem.");
            return;
        }

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        if (services?.GetService(typeof(AppDbContext)) is not AppDbContext contexto)
        {
            await DisplayAlert("Erro", "Não foi possível acessar o banco de dados.", "Fechar");
            return;
        }

        try
        {
            var usuario = await contexto.Usuarios.FirstOrDefaultAsync(u => u.Login == _email);

            if (usuario == null)
            {
                await DisplayAlert("Erro", "Usuário não encontrado.", "Fechar");
                App.Current.MainPage = new Login();
                return;
            }

            usuario.Senha = SenhaHash.Gerar(nova);
            await contexto.SaveChangesAsync();

            await DisplayAlert("Sucesso", "Senha alterada com sucesso.", "Fechar");
            App.Current.MainPage = new Login();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Ocorreu um erro ao salvar a nova senha. " + ex.Message, "Fechar");
        }
    }

    private void BtnCancelar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Login();
    }

    private void MostrarAviso(string mensagem)
    {
        lblAviso.Text = mensagem;
        lblAviso.IsVisible = true;
    }
}
