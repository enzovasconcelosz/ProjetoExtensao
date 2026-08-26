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
            // Usa a string do appsettings quando existir; senao cai no padrao do projeto
            Conexao.Definir(builder.Configuration.GetConnectionString("DefaultConnection"));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Conexao.Atual)
            );

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
    }
}