namespace ProjetoExtensao
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            TratamentoErros.Registrar();

            InitializeComponent();

            Tema.Aplicar();

            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Width = 400;
            window.Height = 700;

            return window;
        }
    }
}