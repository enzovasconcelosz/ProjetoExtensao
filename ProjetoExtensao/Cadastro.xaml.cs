using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.Maui.Graphics;

namespace ProjetoExtensao;

public partial class Cadastro : ContentPage
{
    private bool _nomeTouched = false;
	private bool _emailTouched = false;
	private bool _senhaTouched = false;
	private bool _confirmTouched = false;

	public Cadastro()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		UpdateValidationState();
	}

	private void BotaoJaTenhoConta_Clicked(object sender, EventArgs e)
	{
		App.Current.MainPage = new Login();
	}

    private async void BotaoRegistrar_Clicked(object sender, EventArgs e)
	{
        // Mostrar loadmask imediatamente ao clicar
		var overlayStart = this.FindByName<Frame>("LoadingOverlay");
		if (overlayStart != null) overlayStart.IsVisible = true;

		// Marcar que o usuário tentou submeter para exibir erros visuais caso haja problemas
		_nomeTouched = true;
		_emailTouched = true;
		_senhaTouched = true;
		_confirmTouched = true;
		UpdateValidationState(forceShow: true);

       // Validações básicas
		var nome = Nome?.Text?.Trim();
		var email = Email?.Text?.Trim();
		var senha = Senha?.Text ?? string.Empty;
		var confirmacao = ConfirmacaoSenha?.Text ?? string.Empty;

		// Revalida antes de tentar salvar (botão normalmente está habilitado somente se válido)
		if (string.IsNullOrWhiteSpace(nome) || nome.Length < 3)
		{
			await DisplayAlert("Atenção", "Informe um nome com mais de 3 caracteres.", "Fechar");
			return;
		}

		if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
		{
			await DisplayAlert("Atenção", "Informe um e-mail válido.", "Fechar");
			return;
		}

		if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
		{
			await DisplayAlert("Atenção", "A senha deve conter ao menos 6 caracteres.", "Fechar");
			return;
		}

		if (senha != confirmacao)
		{
			await DisplayAlert("Atenção", "A confirmação de senha não confere.", "Fechar");
			return;
		}

        try
		{
            // (overlay já exibido no início do clique)

			// Cria contexto com a mesma connection string usada no MauiProgram
			var connectionString = @"Server=192.168.1.4,1433;Database=NaoMeEsquece;User Id=sa;Password=123456;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=10;";

			var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Infrastructure.Data.AppDbContext>()
				.UseSqlServer(connectionString)
				.Options;

			using var context = new Infrastructure.Data.AppDbContext(options);

			// Verifica se já existe usuário com o mesmo login (e-mail)
			var existe = await context.Usuarios.AnyAsync(u => u.Login == email);
			if (existe)
			{
				await DisplayAlert("Erro", "Já existe um usuário cadastrado com este e-mail.", "Fechar");
				return;
			}

            // Cria entidade e salva (senha com hash)
			var senhaHash = HashPassword(senha);
			var usuario = new Entities.Usuario(email, senhaHash, nome);
			await context.Usuarios.AddAsync(usuario);
			await context.SaveChangesAsync();

			await DisplayAlert("Sucesso", "Usuário cadastrado com sucesso.", "Ok");

			// Volta para tela de login
			App.Current.MainPage = new Login();
        }
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Não foi possível salvar o usuário: {ex.Message}", "Fechar");
		}
		finally
		{
            var overlay2 = this.FindByName<Frame>("LoadingOverlay");
			if (overlay2 != null) overlay2.IsVisible = false;
		}
	}

	private bool IsValidEmail(string email)
	{
		try
		{
			var addr = new System.Net.Mail.MailAddress(email);
			return addr.Address == email;
		}
		catch
		{
			return false;
		}
	}

	private string HashPassword(string password)
	{
		// PBKDF2 com SHA256
		var salt = new byte[16];
		using (var rng = RandomNumberGenerator.Create())
		{
			rng.GetBytes(salt);
		}

		const int iterations = 100000;
		using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
		var hash = pbkdf2.GetBytes(32);

		return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}.{iterations}";
	}

    private void Nome_TextChanged(object sender, TextChangedEventArgs e)
	{
		_nomeTouched = true;
		UpdateValidationState();
	}

	private void Email_TextChanged(object sender, TextChangedEventArgs e)
	{
		_emailTouched = true;
		UpdateValidationState();
	}

	private void Senha_TextChanged(object sender, TextChangedEventArgs e)
	{
		_senhaTouched = true;
		UpdateValidationState();
	}

	private void ConfirmacaoSenha_TextChanged(object sender, TextChangedEventArgs e)
	{
		_confirmTouched = true;
		UpdateValidationState();
	}

	private void UpdateValidationState(bool forceShow = false)
	{
      var nomeEntry = this.FindByName<Entry>("Nome");
		var emailEntry = this.FindByName<Entry>("Email");
		var senhaEntry = this.FindByName<Entry>("Senha");
		var confirmEntry = this.FindByName<Entry>("ConfirmacaoSenha");

		var nomeLbl = this.FindByName<Label>("NomeError");
		var emailLbl = this.FindByName<Label>("EmailError");
		var senhaLbl = this.FindByName<Label>("SenhaError");
		var confirmLbl = this.FindByName<Label>("ConfirmacaoError");

		var registrarBtn = this.FindByName<Button>("btnRegistrar");

		var nome = nomeEntry?.Text?.Trim() ?? string.Empty;
		var email = emailEntry?.Text?.Trim() ?? string.Empty;
		var senha = senhaEntry?.Text ?? string.Empty;
		var confirmacao = confirmEntry?.Text ?? string.Empty;

        bool nomeValido = nome.Length >= 3;
		bool emailValido = !string.IsNullOrWhiteSpace(email) && IsValidEmail(email);
		bool senhaValida = !string.IsNullOrWhiteSpace(senha) && senha.Length >= 6;
		bool confirmacaoValida = senha == confirmacao && senhaValida;

		var showNomeError = !nomeValido && (_nomeTouched || forceShow);
		var showEmailError = !emailValido && (_emailTouched || forceShow);
		var showSenhaError = !senhaValida && (_senhaTouched || forceShow);
		var showConfirmError = !confirmacaoValida && (_confirmTouched || forceShow);

		if (nomeLbl != null)
		{
			nomeLbl.IsVisible = showNomeError;
			nomeLbl.Text = nomeValido ? string.Empty : "Nome deve ter mais de 3 caracteres.";
		}
		if (nomeEntry != null)
			nomeEntry.TextColor = (_nomeTouched || forceShow) ? (nomeValido ? Colors.Black : Colors.Red) : Colors.Black;

		if (emailLbl != null)
		{
			emailLbl.IsVisible = showEmailError;
			emailLbl.Text = emailValido ? string.Empty : "E-mail inválido.";
		}
		if (emailEntry != null)
			emailEntry.TextColor = (_emailTouched || forceShow) ? (emailValido ? Colors.Black : Colors.Red) : Colors.Black;

		if (senhaLbl != null)
		{
			senhaLbl.IsVisible = showSenhaError;
			senhaLbl.Text = senhaValida ? string.Empty : "Senha deve ter ao menos 6 caracteres.";
		}
		if (senhaEntry != null)
			senhaEntry.TextColor = (_senhaTouched || forceShow) ? (senhaValida ? Colors.Black : Colors.Red) : Colors.Black;

		if (confirmLbl != null)
		{
			confirmLbl.IsVisible = showConfirmError;
			confirmLbl.Text = confirmacaoValida ? string.Empty : "Confirmação de senha não confere.";
		}
		if (confirmEntry != null)
			confirmEntry.TextColor = (_confirmTouched || forceShow) ? (confirmacaoValida ? Colors.Black : Colors.Red) : Colors.Black;

		if (registrarBtn != null)
			registrarBtn.IsEnabled = nomeValido && emailValido && senhaValida && confirmacaoValida;
	}
}