namespace ProjetoExtensao;

public partial class EsqueceuSenha : ContentPage
{
	public EsqueceuSenha()
	{
		InitializeComponent();
	}

    private void BotaoCancelar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Login();
    }

    private void BotaoPesquisar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Login();
    }
}