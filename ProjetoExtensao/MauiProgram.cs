using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Application.Services;
using ProjetoExtensao.Infrastructure.Data;
using ProjetoExtensao.Infrastructure.Repositories;
using Plugin.LocalNotification;

namespace ProjetoExtensao
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            AjustesVisuais.Aplicar();

            CarregarConfiguracao(builder, "appsettings.json");
            CarregarConfiguracao(builder, "appsettings.Development.json");

            builder.UseMauiApp<App>()
                   .ConfigureFonts(fonts =>
                   {
                       fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                       fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                   })
                   // Avisos dos lembretes. Os canais precisam existir antes do
                   // primeiro agendamento: no Android eles nao mudam depois de criados.
                   .UseLocalNotification(notificacoes =>
                   {
                       notificacoes.AddAndroid(android =>
                       {
                           android.AddChannelGroup(NotificacaoLembrete.Grupo());

                           foreach (var canal in NotificacaoLembrete.Canais())
                               android.AddChannel(canal);
                       });
                   });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Usa a string do appsettings quando existir; senao cai no padrao do projeto
            Conexao.Definir(builder.Configuration.GetConnectionString("DefaultConnection"));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Conexao.Atual)
            );

            // Quem esta logado: os repositorios usam para separar os dados por conta
            builder.Services.AddSingleton<IUsuarioAtual, UsuarioAtual>();

            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IClientService, ClientService>();

            // TipoLembrete repository/service
            builder.Services.AddScoped<ProjetoExtensao.Application.Interfaces.ITipoLembreteRepository, ProjetoExtensao.Infrastructure.Repositories.TipoLembreteRepository>();
            builder.Services.AddScoped<ProjetoExtensao.Application.Interfaces.ITipoLembreteService, ProjetoExtensao.Application.Services.TipoLembreteService>();

            // Lembrete repository/service
            builder.Services.AddScoped<ProjetoExtensao.Application.Interfaces.ILembreteRepository, ProjetoExtensao.Infrastructure.Repositories.LembreteRepository>();
            builder.Services.AddScoped<ProjetoExtensao.Application.Interfaces.ILembreteService, ProjetoExtensao.Application.Services.LembreteService>();

            return builder.Build();
        }

        /// <summary>
        /// Le o arquivo empacotado com o app. O AddJsonFile sozinho so funciona
        /// no Windows, onde o arquivo e copiado para o diretorio de saida.
        /// </summary>
        private static void CarregarConfiguracao(MauiAppBuilder builder, string arquivo)
        {
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync(arquivo).GetAwaiter().GetResult();
                builder.Configuration.AddJsonStream(stream);
            }
            catch
            {
                // Arquivo ausente (o de Development e opcional) ou plataforma
                // sem acesso ao pacote: tenta o disco antes de desistir
                builder.Configuration.AddJsonFile(arquivo, optional: true, reloadOnChange: false);
            }
        }
    }
}