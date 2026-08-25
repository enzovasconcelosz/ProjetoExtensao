using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Application.Services;
using ProjetoExtensao.Infrastructure.Data;
using ProjetoExtensao.Infrastructure.Repositories;

namespace ProjetoExtensao
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            builder.UseMauiApp<App>()
                   .ConfigureFonts(fonts =>
                   {
                       fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                       fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                   });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            //string stringDeConexao = "Server=192.168.1.4,1433;Database=NaoMeEsquece;User Id=sa;Password=123456;TrustServerCertificate=True";

            var connectionString = @"Server=192.168.1.4,1433;Database=NaoMeEsquece;User Id=sa;Password=123456;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=10;";
            
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    connectionString
                )
            );

            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IClientService, ClientService>();

            // TipoEvento repository/service
            builder.Services.AddScoped<ProjetoExtensao.Application.Interfaces.ITipoEventoRepository, ProjetoExtensao.Infrastructure.Repositories.TipoEventoRepository>();
            builder.Services.AddScoped<ProjetoExtensao.Application.Interfaces.ITipoEventoService, ProjetoExtensao.Application.Services.TipoEventoService>();

            return builder.Build();
        }
    }
}