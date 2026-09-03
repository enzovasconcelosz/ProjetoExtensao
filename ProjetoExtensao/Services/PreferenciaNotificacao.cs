using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Application.Notificacoes;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Infrastructure.Data;

namespace ProjetoExtensao;

/// <summary>
/// Como o aviso do lembrete chega ao usuario: se avisa, se vibra e se toca.
///
/// A escolha fica no aparelho (<see cref="Preferences"/>), porque e ele quem
/// dispara o aviso mesmo sem rede, e e copiada para a tabela PreferenciaUsuario
/// para acompanhar a conta em outro aparelho — do mesmo jeito que o tema faz
/// com a tabela Aparencia.
/// </summary>
public static class PreferenciaNotificacao
{
    private const string ChaveNotificar = "NotificacaoAtiva";
    private const string ChaveVibrar = "NotificacaoVibrar";
    private const string ChaveSom = "NotificacaoSom";

    /// <summary>Se o aplicativo avisa quando chega a hora do lembrete.</summary>
    public static bool Notificar
    {
        get => Preferences.Default.Get(ChaveNotificar, true);
        set => Preferences.Default.Set(ChaveNotificar, value);
    }

    /// <summary>Se o aviso faz o aparelho vibrar.</summary>
    public static bool Vibrar
    {
        get => Preferences.Default.Get(ChaveVibrar, true);
        set => Preferences.Default.Set(ChaveVibrar, value);
    }

    /// <summary>Se o aviso tambem toca o som de notificacao.</summary>
    public static bool Som
    {
        get => Preferences.Default.Get(ChaveSom, true);
        set => Preferences.Default.Set(ChaveSom, value);
    }

    /// <summary>
    /// Resumo em uma linha, usado na tela de configuracoes.
    /// </summary>
    public static string Resumo() => EscolhaNotificacao.Resumo(Notificar, Vibrar, Som);

    /// <summary>
    /// Grava a escolha no aparelho e, quando ha usuario logado, tambem na
    /// tabela PreferenciaUsuario.
    /// </summary>
    public static async Task SalvarAsync(bool notificar, bool vibrar, bool som)
    {
        Notificar = notificar;
        Vibrar = vibrar;
        Som = som;

        await SincronizarComBancoAsync();
    }

    /// <summary>
    /// Le a preferencia gravada para a conta, sobrescrevendo a do aparelho.
    /// Chamado no login, para que a escolha acompanhe o usuario.
    /// </summary>
    public static async Task CarregarDoBancoAsync(string? login)
    {
        try
        {
            var (contexto, usuario) = await ObterAsync(login);

            if (contexto == null || usuario?.IdPreferenciaUsuario == null)
                return;

            var preferencia = await contexto.PreferenciasUsuario
                .FirstOrDefaultAsync(p => p.Id == usuario.IdPreferenciaUsuario);

            if (preferencia == null)
                return;

            Notificar = preferencia.Notificar;
            Vibrar = preferencia.Vibrar;
            Som = preferencia.Som;
        }
        catch
        {
            // Sem banco, valem as preferencias que ja estao no aparelho
        }
    }

    /// <summary>
    /// Copia a escolha atual para a tabela PreferenciaUsuario, criando o
    /// registro quando o usuario ainda nao tem um.
    /// </summary>
    public static async Task SincronizarComBancoAsync(string? login = null)
    {
        try
        {
            var (contexto, usuario) = await ObterAsync(login);

            if (contexto == null || usuario == null)
                return;

            PreferenciaUsuario? preferencia = null;

            if (usuario.IdPreferenciaUsuario != null)
            {
                preferencia = await contexto.PreferenciasUsuario
                    .FirstOrDefaultAsync(p => p.Id == usuario.IdPreferenciaUsuario);
            }

            if (preferencia == null)
            {
                preferencia = new PreferenciaUsuario();
                contexto.PreferenciasUsuario.Add(preferencia);
            }

            preferencia.Notificar = Notificar;
            preferencia.Vibrar = Vibrar;
            preferencia.Som = Som;

            await contexto.SaveChangesAsync();

            if (usuario.IdPreferenciaUsuario != preferencia.Id)
            {
                usuario.IdPreferenciaUsuario = preferencia.Id;
                await contexto.SaveChangesAsync();
            }
        }
        catch
        {
            // A escolha ja vale no aparelho; a copia no banco pode esperar
        }
    }

    private static async Task<(AppDbContext? Contexto, Usuario? Usuario)> ObterAsync(string? login)
    {
        login ??= await SecureStorage.Default.GetAsync("UsuarioLogado");

        if (string.IsNullOrWhiteSpace(login))
            return (null, null);

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;

        if (services?.GetService(typeof(AppDbContext)) is not AppDbContext contexto)
            return (null, null);

        var usuario = await contexto.Usuarios.FirstOrDefaultAsync(u => u.Login == login);

        return (contexto, usuario);
    }
}
