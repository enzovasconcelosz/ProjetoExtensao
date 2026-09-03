using ProjetoExtensao.Application.Interfaces;

namespace ProjetoExtensao;

/// <summary>
/// Quem esta usando o aplicativo neste momento.
///
/// O login (e-mail) ja ficava no <see cref="SecureStorage"/> para exibir o nome
/// na tela; o que faltava era o Id, que e o que separa os dados de uma conta dos
/// da outra. Sem ele os repositorios nao tinham como filtrar, e toda conta
/// enxergava os lembretes de todas as outras.
/// </summary>
public static class SessaoUsuario
{
    private const string ChaveId = "SessaoIdUsuario";

    /// <summary>Id do usuario logado, ou nulo quando ninguem entrou.</summary>
    public static long? Id
    {
        get
        {
            var valor = Preferences.Default.Get(ChaveId, 0L);
            return valor > 0 ? valor : null;
        }
    }

    public static void Entrar(long idUsuario) => Preferences.Default.Set(ChaveId, idUsuario);

    public static void Sair() => Preferences.Default.Remove(ChaveId);
}

/// <summary>
/// Ponte entre a sessao e os repositorios: eles precisam saber de quem sao os
/// dados, mas nao devem depender das preferencias do aparelho.
/// </summary>
public class UsuarioAtual : IUsuarioAtual
{
    public long? Id => SessaoUsuario.Id;
}
