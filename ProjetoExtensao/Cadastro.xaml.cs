namespace ProjetoExtensao;

public partial class Cadastro : ContentPage
{
	public Cadastro()
	{
		InitializeComponent();
	}

    private void BotaoJaTenhoConta_Clicked(object sender, EventArgs e)
    {
		App.Current.MainPage = new Login();
    }
}