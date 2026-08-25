using System;
using ProjetoExtensao.Services;

namespace ProjetoExtensao;

public partial class CodigoConfirmacao : ContentPage
{
	private readonly string _email;

	public CodigoConfirmacao(string email)
	{
		InitializeComponent();
		_email = email;
	}

	private void BtnCancelar_Clicked(object sender, EventArgs e)
	{
		App.Current.MainPage = new Login();
	}

	private async void BtnConfirmarCodigo_Clicked(object sender, EventArgs e)
	{
		var codigo = CodigoEntry?.Text?.Trim();
		if (string.IsNullOrWhiteSpace(codigo))
		{
			await DisplayAlert("Atenção", "Informe o código recebido por e-mail.", "Fechar");
			return;
		}

		var valido = ConfirmationCodeService.ValidateCode(_email, codigo);
		if (!valido)
		{
			await DisplayAlert("Inválido", "Código incorreto.", "Fechar");
			return;
		}

		// Código correto: abrir formulário para alterar senha
		App.Current.MainPage = new AlterarSenha(_email);
	}
}
