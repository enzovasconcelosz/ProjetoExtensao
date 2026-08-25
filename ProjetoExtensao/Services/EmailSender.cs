using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ProjetoExtensao.Services;

public static class EmailSender
{
    public static async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
            var configuration = services?.GetService(typeof(IConfiguration)) as IConfiguration;

            string? host = configuration?["Smtp:Host"];
            string? portStr = configuration?["Smtp:Port"];
            string? user = configuration?["Smtp:User"];
            string? pass = configuration?["Smtp:Password"];
            string? from = configuration?["Smtp:From"] ?? user;
            bool enableSsl = bool.TryParse(configuration?["Smtp:EnableSsl"], out var ssl) && ssl;

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portStr) || string.IsNullOrEmpty(from))
            {
                // Configuração SMTP não encontrada — em ambiente de desenvolvimento apenas logar
                Console.WriteLine($"Enviar e-mail para: {to}\nAssunto: {subject}\nCorpo: {body}");
                return;
            }

            if (!int.TryParse(portStr, out var port)) port = 25;

            using var message = new MailMessage(from, to, subject, body);
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrEmpty(user))
            {
                client.Credentials = new NetworkCredential(user, pass);
            }

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Não interromper fluxo da aplicação: apenas logar
            Console.WriteLine("Falha ao enviar e-mail: " + ex.Message);
        }
    }
}
