namespace ProjetoExtensao;

/// <summary>
/// Remove a borda nativa dos campos de entrada.
///
/// O projeto envolve cada campo em um <c>Border</c> com o arredondamento da
/// paleta; sem isso, o controle nativo desenha a propria borda por cima,
/// aparecendo como uma segunda moldura (e, no Windows, um sublinhado colorido
/// quando o campo recebe o foco).
/// </summary>
public static class AjustesVisuais
{
    public static void Aplicar()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SemBordaNativa", (handler, _) => RemoverBorda(handler.PlatformView));
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("SemBordaNativa", (handler, _) => RemoverBorda(handler.PlatformView));
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("SemBordaNativa", (handler, _) => RemoverBorda(handler.PlatformView));
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("SemBordaNativa", (handler, _) => RemoverBorda(handler.PlatformView));
        Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("SemBordaNativa", (handler, _) => RemoverBorda(handler.PlatformView));

        // O Title do Picker vira cabecalho fixo acima da lista no Windows.
        // Aqui ele funciona como placeholder: some assim que houver selecao.
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("TituloComoPlaceholder", (handler, view) =>
            AjustarPlaceholder(handler.PlatformView, (view as Picker)?.Title));

        // Data centralizada, com o icone de calendario na ponta direita
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("DataCentralizada", (handler, _) =>
            AjustarData(handler.PlatformView));

        // Hora centralizada na vertical dentro do campo
        Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("HoraCentralizada", (handler, _) =>
            AjustarHora(handler.PlatformView));
    }

    private static void AjustarHora(object? controleNativo)
    {
        try
        {
#if WINDOWS
            if (controleNativo is Microsoft.UI.Xaml.Controls.TimePicker relogio)
            {
                // Como no ComboBox, o espaco do cabecalho continua reservado
                // mesmo sem cabecalho, empurrando os numeros para baixo do topo.
                relogio.Header = null;
                relogio.HeaderTemplate = null;
                relogio.Padding = new Microsoft.UI.Xaml.Thickness(0);
                relogio.MinHeight = 0;
                relogio.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
                relogio.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;

                // O conteudo fica dentro de um botao do template, que tambem
                // precisa esticar para que os numeros centralizem de fato.
                relogio.Loaded += (_, _) => EsticarConteudo(relogio, "FlyoutButton");
                EsticarConteudo(relogio, "FlyoutButton");
            }
#endif
        }
        catch
        {
            // Ajuste apenas cosmetico: nunca deve impedir a tela de abrir
        }
    }

    private static void AjustarData(object? controleNativo)
    {
        try
        {
#if WINDOWS
            if (controleNativo is Microsoft.UI.Xaml.Controls.CalendarDatePicker calendario)
            {
                // Esticado, o template coloca o texto em uma coluna elastica e o
                // icone em uma coluna fixa a direita.
                calendario.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
                calendario.HorizontalContentAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;

                // O texto da data e um TextBlock do template, com alinhamento
                // proprio: so da para centraliza-lo depois que o template carrega.
                calendario.Loaded += (_, _) => CentralizarTextoDaData(calendario);
                CentralizarTextoDaData(calendario);
            }
#endif
        }
        catch
        {
            // Ajuste apenas cosmetico: nunca deve impedir a tela de abrir
        }
    }

#if WINDOWS
    /// <summary>
    /// Faz um elemento do template ocupar toda a altura disponivel, para que
    /// o proprio controle possa centralizar o conteudo.
    /// </summary>
    private static void EsticarConteudo(Microsoft.UI.Xaml.DependencyObject raiz, string nome)
    {
        if (ProcurarPorNome(raiz, nome) is not Microsoft.UI.Xaml.FrameworkElement elemento)
            return;

        elemento.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

        if (elemento is Microsoft.UI.Xaml.Controls.Control controle)
        {
            controle.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            controle.Padding = new Microsoft.UI.Xaml.Thickness(0);
            controle.MinHeight = 0;
        }
    }

    /// <summary>
    /// Procura o TextBlock "DateText" dentro do template do CalendarDatePicker
    /// e centraliza o texto, mantendo o icone de calendario na direita.
    /// </summary>
    private static void CentralizarTextoDaData(Microsoft.UI.Xaml.DependencyObject raiz)
    {
        var texto = ProcurarPorNome(raiz, "DateText") as Microsoft.UI.Xaml.Controls.TextBlock;

        if (texto == null)
            return;

        texto.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
        texto.TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center;
    }

    private static Microsoft.UI.Xaml.FrameworkElement? ProcurarPorNome(Microsoft.UI.Xaml.DependencyObject raiz, string nome)
    {
        var total = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(raiz);

        for (int i = 0; i < total; i++)
        {
            var filho = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(raiz, i);

            if (filho is Microsoft.UI.Xaml.FrameworkElement elemento && elemento.Name == nome)
                return elemento;

            var encontrado = ProcurarPorNome(filho, nome);
            if (encontrado != null)
                return encontrado;
        }

        return null;
    }
#endif

    private static void AjustarPlaceholder(object? controleNativo, string? titulo)
    {
        try
        {
#if WINDOWS
            if (controleNativo is Microsoft.UI.Xaml.Controls.ComboBox combo)
            {
                // Sem cabecalho o espaco reservado para ele tambem precisa sair,
                // senao o campo fica mais alto que os demais do formulario.
                combo.Header = null;
                combo.HeaderTemplate = null;
                combo.MinHeight = 0;
                combo.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
                combo.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;

                combo.PlaceholderText = titulo ?? string.Empty;
            }
#endif
        }
        catch
        {
            // Ajuste apenas cosmetico: nunca deve impedir a tela de abrir
        }
    }

    private static void RemoverBorda(object? controleNativo)
    {
        try
        {
#if WINDOWS
            if (controleNativo is Microsoft.UI.Xaml.Controls.Control controle)
            {
                var transparente = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                controle.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                controle.Background = transparente;

                // O sublinhado de foco vem de recursos do tema, nao da borda.
                // Um ResourceDictionary do WinUI nao aceita null, entao o valor
                // neutro e um pincel transparente.
                controle.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
                controle.Resources["TextControlBackgroundFocused"] = transparente;
                controle.Resources["TextControlBackgroundPointerOver"] = transparente;
                controle.Resources["TextControlBorderBrushFocused"] = transparente;
                controle.Resources["TextControlBorderBrushPointerOver"] = transparente;
            }
#elif ANDROID
            if (controleNativo is Android.Views.View view)
                view.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
            if (controleNativo is UIKit.UITextField campo)
                campo.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
        }
        catch
        {
            // Ajuste apenas cosmetico: nunca deve impedir a tela de abrir
        }
    }
}
