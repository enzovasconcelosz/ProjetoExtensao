using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ProjetoExtensao.Services;

/// <summary>
/// Envio de e-mail por SMTP, configurado pela secao "Smtp" do appsettings.
/// </summary>
public static class EmailSender
{
    public enum Resultado
    {
        /// <summary>E-mail entregue ao servidor SMTP.</summary>
        Enviado,

        /// <summary>Sem configuracao SMTP: o conteudo foi apenas registrado no log.</summary>
        NaoConfigurado,

        /// <summary>O servidor SMTP recusou ou a conexao falhou.</summary>
        Falhou
    }

    public static async Task<Resultado> EnviarAsync(string destinatario, string assunto, string corpo)
    {
        var configuracao = ObterConfiguracao();

        string? host = configuracao?["Smtp:Host"];
        string? porta = configuracao?["Smtp:Port"];
        string? usuario = configuracao?["Smtp:User"];
        string? senha = configuracao?["Smtp:Password"];
        string? remetente = configuracao?["Smtp:From"] ?? usuario;
        bool ssl = !bool.TryParse(configuracao?["Smtp:EnableSsl"], out var valorSsl) || valorSsl;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(remetente))
        {
            // Em desenvolvimento, sem SMTP configurado, o conteudo vai para o log
            Debug.WriteLine($"[e-mail nao enviado: SMTP nao configurado]\nPara: {destinatario}\nAssunto: {assunto}\n{corpo}");
            return Resultado.NaoConfigurado;
        }

        if (!int.TryParse(porta, out var numeroPorta))
            numeroPorta = 587;

        try
        {
            using var mensagem = new MailMessage(remetente, destinatario, assunto, corpo);
            using var cliente = new SmtpClient(host, numeroPorta) { EnableSsl = ssl };

            if (!string.IsNullOrWhiteSpace(usuario))
                cliente.Credentials = new NetworkCredential(usuario, senha);

            await cliente.SendMailAsync(mensagem);
            return Resultado.Enviado;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Falha ao enviar e-mail: " + ex);
            return Resultado.Falhou;
        }
    }

    private static IConfiguration? ObterConfiguracao()
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        return services?.GetService(typeof(IConfiguration)) as IConfiguration;
    }
}
