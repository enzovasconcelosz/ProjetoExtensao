namespace ProjetoExtensao;

/// <summary>
/// Controla a aparencia (tema) do aplicativo. A preferencia fica salva localmente
/// em <see cref="Preferences"/> e e reaplicada a cada abertura de tela.
/// </summary>
public static class Tema
{
    public const string Claro = "Claro";
    public const string Escuro = "Escuro";
    public const string Sistema = "Sistema";

    private const string Chave = "PerfilTema";

    public static string Atual
    {
        get => Preferences.Default.Get(Chave, Sistema);
        set => Preferences.Default.Set(Chave, value);
    }

    /// <summary>
    /// Chamado no OnAppearing de cada tela: alem do tema, reaplica o tamanho
    /// de fonte escolhido no painel de acessibilidade.
    /// </summary>
    public static void Aplicar()
    {
        Aplicar(Atual);
        Acessibilidade.AplicarNaPaginaAtual();
    }

    public static void Aplicar(string tema)
    {
        var app = Microsoft.Maui.Controls.Application.Current;
        if (app == null)
            return;

        app.UserAppTheme = tema switch
        {
            Claro => AppTheme.Light,
            Escuro => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    public static void Salvar(string tema)
    {
        Atual = tema;
        Aplicar(tema);
    }
}
