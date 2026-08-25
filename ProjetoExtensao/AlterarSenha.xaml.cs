using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;

namespace ProjetoExtensao;

public partial class AlterarSenha : ContentPage
{
    private readonly string _email;
    private Entry NovaSenhaEntry;
    private Entry ConfirmacaoSenhaEntry;

    public AlterarSenha(string email)
    {
        _email = email;

        // Construir UI no code-behind para evitar dependência do gerador XAML
        BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#EAF4FF");

        NovaSenhaEntry = new Entry { Placeholder = "Nova senha", IsPassword = true, Margin = new Thickness(8) };
        ConfirmacaoSenhaEntry = new Entry { Placeholder = "Confirme a nova senha", IsPassword = true, Margin = new Thickness(8) };

        var confirmar = new Button
        {
            Text = "Confirmar",
            BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#087AFE"),
            TextColor = Microsoft.Maui.Graphics.Colors.White,
            CornerRadius = 24,
            HeightRequest = 48
        };
        confirmar.Clicked += BtnConfirmar_Clicked;

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 18,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = "Alterar senha", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#0B2A4A"), HorizontalOptions = LayoutOptions.Center },
                new Frame { BorderColor = Microsoft.Maui.Graphics.Color.FromArgb("#D6DCE6"), BackgroundColor = Microsoft.Maui.Graphics.Colors.White, CornerRadius = 12, Content = NovaSenhaEntry },
                new Frame { BorderColor = Microsoft.Maui.Graphics.Color.FromArgb("#D6DCE6"), BackgroundColor = Microsoft.Maui.Graphics.Colors.White, CornerRadius = 12, Content = ConfirmacaoSenhaEntry },
                confirmar
            }
        };
    }

    private async void BtnConfirmar_Clicked(object sender, EventArgs e)
    {
        var nova = NovaSenhaEntry?.Text?.Trim();
        var conf = ConfirmacaoSenhaEntry?.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nova) || string.IsNullOrWhiteSpace(conf))
        {
            await DisplayAlert("Atenção", "Informe a nova senha e a confirmação.", "Fechar");
            return;
        }

        if (nova != conf)
        {
            await DisplayAlert("Atenção", "As senhas não conferem.", "Fechar");
            return;
        }

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        var context = services?.GetService(typeof(ProjetoExtensao.Infrastructure.Data.AppDbContext)) as ProjetoExtensao.Infrastructure.Data.AppDbContext;
        if (context == null)
        {
            await DisplayAlert("Erro", "Não foi possível acessar o banco de dados.", "Fechar");
            return;
        }

        try
        {
            var usuario = await context.Set<ProjetoExtensao.Entities.Usuario>()
                .FirstOrDefaultAsync(u => u.Login == _email);

            if (usuario == null)
            {
                await DisplayAlert("Erro", "Usuário não encontrado.", "Fechar");
                Microsoft.Maui.Controls.Application.Current.MainPage = new Login();
                return;
            }

            usuario.Senha = nova;
            await context.SaveChangesAsync();

            await DisplayAlert("Sucesso", "Senha alterada com sucesso.", "Fechar");
            Microsoft.Maui.Controls.Application.Current.MainPage = new Login();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Ocorreu um erro ao salvar a nova senha. " + ex.Message, "Fechar");
        }
    }
}
