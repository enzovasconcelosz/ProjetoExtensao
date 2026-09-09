using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao;

/// <summary>
/// Configuracoes do proprio usuario: nome, e-mail, senha, imagem e aparencia (tema).
/// Aberta ao tocar na imagem redonda da tela inicial.
/// </summary>
public partial class ConfiguracoesUsuario : ContentPage
{
    private Usuario? _usuario;

    // Quem abriu a tela: o retorno volta para a mesma origem
    private readonly Func<Page> _paginaAnterior;

    public ConfiguracoesUsuario(Func<Page>? paginaAnterior = null)
    {
        InitializeComponent();
        _paginaAnterior = paginaAnterior ?? (() => new TelaInicial());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        await CarregarUsuarioAsync();
    }

    private static AppDbContext? ObterContexto()
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        return services?.GetService(typeof(AppDbContext)) as AppDbContext;
    }

    private async Task CarregarUsuarioAsync()
    {
        // Valores locais servem de fallback quando nao ha usuario no banco
        txtNome.Text = Preferences.Default.Get("PerfilNome", string.Empty);
        txtEmail.Text = Preferences.Default.Get("PerfilEmail", string.Empty);
        CarregarImagemLocal();

        var login = await SecureStorage.Default.GetAsync("UsuarioLogado");
        if (string.IsNullOrWhiteSpace(login))
            return;

        var contexto = ObterContexto();
        if (contexto == null)
            return;

        try
        {
            _usuario = await contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Login == login);

            if (_usuario == null)
                return;

            txtNome.Text = _usuario.NomeUsuario;
            txtEmail.Text = _usuario.Login;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível carregar seus dados. " + ex.Message, "Fechar");
        }
    }

    private void CarregarImagemLocal()
    {
        imgPerfil.Source = ImagemPerfil.Obter();
    }

    private async void BotaoAlterarImagem_Clicked(object sender, EventArgs e)
    {
        try
        {
            var arquivo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Selecione sua foto de perfil"
            });

            if (arquivo == null)
                return;

            using var origem = await arquivo.OpenReadAsync();
            using var memoria = new MemoryStream();
            await origem.CopyToAsync(memoria);

            // O recorte e escolhido em tela propria, ja que o avatar e circular
            Navegacao.IrPara(new AjusteImagemPerfil(
                memoria.ToArray(),
                () => new ConfiguracoesUsuario(_paginaAnterior)));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível selecionar a imagem. " + ex.Message, "Fechar");
        }
    }

    private async void BotaoSalvar_Clicked(object sender, EventArgs e)
    {
        var nome = txtNome.Text?.Trim();
        var email = txtEmail.Text?.Trim();
        var senha = txtSenha.Text?.Trim();
        var confirmacao = txtConfirmaSenha.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nome))
        {
            await DisplayAlert("Atenção", "Informe o seu nome.", "Fechar");
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            await DisplayAlert("Atenção", "Informe um e-mail válido.", "Fechar");
            return;
        }

        if (!string.IsNullOrWhiteSpace(senha) && senha != confirmacao)
        {
            await DisplayAlert("Atenção", "As senhas não conferem.", "Fechar");
            return;
        }

        // Sempre guarda uma copia local para a tela inicial exibir mesmo sem banco
        Preferences.Default.Set("PerfilNome", nome);
        Preferences.Default.Set("PerfilEmail", email);

        var contexto = ObterContexto();
        if (_usuario == null || contexto == null)
        {
            await DisplayAlert("Salvo", "Suas preferências foram salvas neste dispositivo.", "Fechar");
            Voltar();
            return;
        }

        try
        {
            _usuario.NomeUsuario = nome;
            _usuario.Login = email;

            if (!string.IsNullOrWhiteSpace(senha))
                _usuario.Senha = SenhaHash.Gerar(senha);

            // A foto fica gravada no dispositivo: a tabela Imagem do banco nao
            // possui coluna para o binario da imagem.

            await contexto.SaveChangesAsync();
            await SecureStorage.Default.SetAsync("UsuarioLogado", email);

            await DisplayAlert("Sucesso", "Dados atualizados com sucesso.", "Fechar");
            Voltar();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível salvar os dados. " + ex.Message, "Fechar");
        }
    }

    private void BotaoVoltar_Clicked(object sender, EventArgs e)
    {
        Voltar();
    }

    private void Voltar()
    {
        Navegacao.IrPara(_paginaAnterior());
    }
}
