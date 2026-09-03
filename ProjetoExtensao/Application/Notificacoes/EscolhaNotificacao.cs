namespace ProjetoExtensao.Application.Notificacoes;

/// <summary>
/// Como o aviso do lembrete deve se comportar: se avisa, se vibra e se toca.
///
/// A decisao fica separada da tela e do plugin de notificacao por dois motivos:
/// o nome do canal do Android precisa ser sempre o mesmo para a mesma escolha
/// (um canal ja criado nao muda), e o texto exibido ao usuario precisa
/// corresponder exatamente ao que o aparelho vai fazer.
/// </summary>
public static class EscolhaNotificacao
{
    /// <summary>
    /// Identificador do canal do Android correspondente a escolha.
    ///
    /// A partir do Android 8 sao o canal — e nao a notificacao — que definem som
    /// e vibracao, e o canal e imutavel depois de criado. Por isso existe um
    /// canal por combinacao, com o nome derivado dela.
    /// </summary>
    public static string CanalDe(bool vibrar, bool som) =>
        $"lembretes-{(som ? "com" : "sem")}-som-{(vibrar ? "com" : "sem")}-vibracao";

    /// <summary>Nome do canal como o usuario o ve nas configuracoes do Android.</summary>
    public static string NomeDoCanal(bool vibrar, bool som) => (vibrar, som) switch
    {
        (true, true) => "Lembretes (vibrar e tocar)",
        (true, false) => "Lembretes (somente vibrar)",
        (false, true) => "Lembretes (somente tocar)",
        _ => "Lembretes (silenciosos)"
    };

    /// <summary>
    /// Resumo em uma linha, exibido na tela de configuracoes.
    /// </summary>
    public static string Resumo(bool notificar, bool vibrar, bool som)
    {
        if (!notificar)
            return "Desativadas";

        if (som && vibrar)
            return "Vibrar e tocar";

        if (som)
            return "Somente tocar";

        if (vibrar)
            return "Somente vibrar";

        return "Sem som e sem vibração";
    }

    /// <summary>
    /// Se o aviso deve ser agendado. Nada e marcado quando o usuario desligou as
    /// notificacoes ou quando a hora ja passou — o segundo caso acontece ao
    /// editar um lembrete antigo.
    /// </summary>
    public static bool DeveAgendar(bool notificar, DateTime dataHora, DateTime agora) =>
        notificar && dataHora > agora;
}
