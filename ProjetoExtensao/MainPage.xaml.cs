using ProjetoExtensao.Entities;

namespace ProjetoExtensao
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void BotaoEsqueceuSenha_Clicked(object sender, EventArgs e)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch
            {
                DisplayAlertAsync("Erro!", "Ocorreu um erro ao recuperar a senha.", "Fechar");
            }
        }

        private void BotaoLogin_Clicked(object sender, EventArgs e)
        {
            var listaUsuarios = new List<Usuario>()
            {
                new("Armando@gmail.com", "123", "Armando"),
                new("Enzo@gmail.com", "321", "Enzo")
            };

            var usuarioTentativaLogin = listaUsuarios.FirstOrDefault(x => x.Login == Usuario.Text && x.Senha == Senha.Text);

            if (usuarioTentativaLogin != null)
            {
                DisplayAlertAsync("Sucesso!", "Login efetuado com sucesso!", "Fechar");

                SecureStorage.Default.SetAsync("LoginUsuarioLogado", usuarioTentativaLogin.Login);

                App.Current.MainPage = new TelaInicial();

                return;
            }

            DisplayAlertAsync("Erro no login!", "O e-mail ou senha informados estão incorretos.", "Fechar");
        }

        private void BotaoCadastrar_Clicked(object sender, EventArgs e)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch
            {
                DisplayAlertAsync("Erro no cadastro!", "Ocorreu um erro ao realizar o cadastro.", "Fechar");
            }
        }
    }
}