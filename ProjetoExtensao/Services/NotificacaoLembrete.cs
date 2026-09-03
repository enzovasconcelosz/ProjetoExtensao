using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;
using ProjetoExtensao.Application.Notificacoes;
using ProjetoExtensao.Entities;

namespace ProjetoExtensao;

/// <summary>
/// Aviso no aparelho quando chega a hora do lembrete.
///
/// O agendamento e local: o sistema operacional guarda o alarme e dispara o
/// aviso na hora marcada, mesmo com o aplicativo fechado e sem internet.
///
/// No Android 8 em diante, som e vibracao sao definidos pelo canal e nao pela
/// notificacao — e um canal ja criado nao muda mais. Por isso existem quatro
/// canais, um para cada combinacao de vibrar/tocar, e a escolha do usuario
/// decide qual deles recebe o aviso.
/// </summary>
public static class NotificacaoLembrete
{
    private const string GrupoId = "lembretes";
    private const string GrupoNome = "Lembretes";

    /// <summary>Pausa de 0,4s repetida, o padrao de vibracao de um aviso curto.</summary>
    private static readonly long[] PadraoVibracao = { 0, 400, 200, 400 };

    /// <summary>
    /// Os quatro canais, registrados na inicializacao do aplicativo.
    /// </summary>
    public static IEnumerable<AndroidNotificationChannelRequest> Canais()
    {
        foreach (var vibrar in new[] { true, false })
        {
            foreach (var som in new[] { true, false })
            {
                yield return new AndroidNotificationChannelRequest
                {
                    Id = EscolhaNotificacao.CanalDe(vibrar, som),
                    Name = EscolhaNotificacao.NomeDoCanal(vibrar, som),
                    Description = "Avisos dos lembretes cadastrados no aplicativo.",
                    Group = GrupoId,
                    Importance = AndroidImportance.High,
                    EnableSound = som,
                    VibrationPattern = vibrar ? PadraoVibracao : Array.Empty<long>(),
                    LockScreenVisibility = AndroidVisibilityType.Public,
                    ShowBadge = true
                };
            }
        }
    }

    public static AndroidNotificationChannelGroupRequest Grupo() => new()
    {
        Group = GrupoId,
        Name = GrupoNome
    };

    /// <summary>
    /// Pede ao sistema a autorizacao para exibir notificacoes. No Android 13 em
    /// diante ela e obrigatoria; nas versoes anteriores a chamada apenas retorna
    /// verdadeiro.
    /// </summary>
    public static async Task<bool> GarantirPermissaoAsync()
    {
        try
        {
            if (!LocalNotificationCenter.Current.IsSupported)
                return false;

            if (await LocalNotificationCenter.Current.AreNotificationsEnabled())
                return true;

            return await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Agenda o aviso do lembrete, substituindo o anterior quando ele ja existia.
    ///
    /// Nada e agendado quando o usuario desligou as notificacoes ou quando a
    /// hora do lembrete ja passou — este ultimo caso acontece ao editar um
    /// lembrete antigo.
    /// </summary>
    public static async Task AgendarAsync(Lembrete lembrete)
    {
        await AgendarAsync(lembrete.Id, lembrete.Nome, lembrete.Descricao, lembrete.DataHoraLembrete);
    }

    public static async Task AgendarAsync(long id, string nome, string? descricao, DateTime dataHora)
    {
        // Um lembrete tem um unico aviso: cancelar antes evita duplicar na edicao
        Cancelar(id);

        if (!EscolhaNotificacao.DeveAgendar(PreferenciaNotificacao.Notificar, dataHora, DateTime.Now))
            return;

        try
        {
            if (!LocalNotificationCenter.Current.IsSupported)
                return;

            var som = PreferenciaNotificacao.Som;
            var vibrar = PreferenciaNotificacao.Vibrar;

            var pedido = new NotificationRequest
            {
                NotificationId = IdDaNotificacao(id),
                Title = nome,
                Description = descricao ?? string.Empty,
                ReturningData = id.ToString(),

                // iOS e Windows: som e silencio vem da propria notificacao
                Silent = !som,

                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = dataHora,
                    Android = new AndroidScheduleOptions
                    {
                        // Um lembrete de consulta precisa tocar na hora marcada,
                        // mesmo com o aparelho em economia de bateria
                        ScheduleMode = AndroidScheduleMode.ExactAllowWhileIdle,
                        AlarmType = AndroidAlarmType.RtcWakeup
                    }
                },

                Android = new AndroidOptions
                {
                    ChannelId = EscolhaNotificacao.CanalDe(vibrar, som),
                    Priority = AndroidPriority.High,
                    AutoCancel = true,
                    LaunchAppWhenTapped = true,
                    VibrationPattern = vibrar ? PadraoVibracao : Array.Empty<long>()
                }
            };

            await LocalNotificationCenter.Current.Show(pedido);
        }
        catch
        {
            // O lembrete continua salvo; apenas o aviso nao pode ser agendado
        }
    }

    /// <summary>Remove o aviso de um lembrete excluido.</summary>
    public static void Cancelar(long id)
    {
        try
        {
            if (LocalNotificationCenter.Current.IsSupported)
                LocalNotificationCenter.Current.Cancel(IdDaNotificacao(id));
        }
        catch
        {
            // Sem aviso agendado, nao ha o que cancelar
        }
    }

    /// <summary>
    /// Remove todos os avisos agendados neste aparelho. Usado ao sair da conta:
    /// os avisos pertencem a quem estava logado.
    /// </summary>
    public static void CancelarTodos()
    {
        try
        {
            if (LocalNotificationCenter.Current.IsSupported)
                LocalNotificationCenter.Current.CancelAll();
        }
        catch
        {
            // Sem avisos agendados, nao ha o que cancelar
        }
    }

    /// <summary>
    /// Reagenda todos os lembretes futuros. Usado depois de alterar as
    /// preferencias (o canal muda) e no login, quando os avisos da conta
    /// precisam existir tambem neste aparelho.
    /// </summary>
    public static async Task ReagendarTodosAsync(IEnumerable<Lembrete> lembretes)
    {
        CancelarTodos();

        foreach (var lembrete in lembretes.Where(l => l.DataHoraLembrete > DateTime.Now))
            await AgendarAsync(lembrete);
    }

    /// <summary>
    /// O plugin identifica a notificacao por um inteiro; o Id do lembrete e
    /// long. A conversao e estavel: o mesmo lembrete sempre gera o mesmo id,
    /// que e o que permite cancelar e substituir o aviso depois.
    /// </summary>
    private static int IdDaNotificacao(long idLembrete) =>
        (int)(idLembrete % int.MaxValue);
}
