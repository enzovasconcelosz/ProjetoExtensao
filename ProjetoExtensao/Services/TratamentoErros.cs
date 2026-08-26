using System.Diagnostics;

namespace ProjetoExtensao;

/// <summary>
/// Captura falhas nao tratadas em qualquer ponto do aplicativo, exibe uma
/// mensagem generica ao usuario e mantem a execucao em vez de derrubar o app.
/// </summary>
public static class TratamentoErros
{
    private const string Titulo = "Ops!";
    private const string Mensagem =
        "Ocorreu um erro inesperado, mas o aplicativo continua funcionando. " +
        "Se o problema persistir, feche e abra o aplicativo novamente.";

    private static bool _registrado;
    private static bool _exibindo;

    public static void Registrar()
    {
        if (_registrado)
            return;

        _registrado = true;

        // Excecoes que escapam de qualquer thread
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Tratar(e.ExceptionObject as Exception, "AppDomain");

        // Tasks cujo erro nunca foi observado (async void, fire-and-forget)
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            e.SetObserved();
            Tratar(e.Exception, "Task");
        };

#if ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
        {
            e.Handled = true;
            Tratar(e.Exception, "Android");
        };
#endif
    }

    /// <summary>
    /// Registra a falha e mostra o aviso generico, sem interromper a execucao.
    /// </summary>
    public static void Tratar(Exception? excecao, string origem = "App")
    {
        Debug.WriteLine($"[erro nao tratado / {origem}] {excecao}");

        // Evita empilhar varios alertas quando a falha se repete
        if (_exibindo)
            return;

        _exibindo = true;

        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var pagina = Microsoft.Maui.Controls.Application.Current?.MainPage;
                    if (pagina != null)
                        await pagina.DisplayAlert(Titulo, Mensagem, "Fechar");
                }
                catch
                {
                    // se nem o alerta puder ser exibido, o app segue em execucao
                }
                finally
                {
                    _exibindo = false;
                }
            });
        }
        catch
        {
            _exibindo = false;
        }
    }
}
