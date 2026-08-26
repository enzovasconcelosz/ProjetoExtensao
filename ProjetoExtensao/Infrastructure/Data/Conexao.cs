using Microsoft.EntityFrameworkCore;

namespace ProjetoExtensao.Infrastructure.Data
{
    /// <summary>
    /// Ponto unico da string de conexao do projeto, para nao haver copias
    /// divergentes espalhadas pelas telas.
    /// </summary>
    public static class Conexao
    {
        public const string Padrao =
            @"Server=localhost\SQLEXPRESS;Database=NaoMeEsquece;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=10;";

        private static string? _configurada;

        /// <summary>Define a string vinda do appsettings, quando disponivel.</summary>
        public static void Definir(string? connectionString)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
                _configurada = connectionString;
        }

        public static string Atual => _configurada ?? Padrao;

        public static AppDbContext CriarContexto()
        {
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Atual)
                .Options;

            return new AppDbContext(options);
        }
    }
}
