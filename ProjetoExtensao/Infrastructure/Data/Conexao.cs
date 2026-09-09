using Microsoft.EntityFrameworkCore;

namespace ProjetoExtensao.Infrastructure.Data
{
    /// <summary>
    /// Ponto unico da string de conexao do projeto, para nao haver copias
    /// divergentes espalhadas pelas telas.
    ///
    /// Os dados ficam em um arquivo SQLite dentro da area privada do aplicativo
    /// (FileSystem.AppDataDirectory). Assim o aparelho nao depende de rede nem de
    /// um servidor de banco para salvar os lembretes, e o arquivo e apagado junto
    /// com o aplicativo quando o usuario o desinstala.
    /// </summary>
    public static class Conexao
    {
        /// <summary>Nome do arquivo do banco dentro da pasta do aplicativo.</summary>
        public const string NomeDoArquivo = "naomeesquece.db3";

        private static string? _configurada;

        /// <summary>Caminho completo do arquivo do banco no aparelho.</summary>
        public static string CaminhoDoBanco =>
            Path.Combine(FileSystem.AppDataDirectory, NomeDoArquivo);

        /// <summary>String usada quando o appsettings nao traz nenhuma.</summary>
        public static string Padrao => $"Data Source={CaminhoDoBanco}";

        /// <summary>Define a string vinda do appsettings, quando disponivel.</summary>
        public static void Definir(string? connectionString)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
                _configurada = connectionString;
        }

        public static string Atual => _configurada ?? Padrao;

        public static AppDbContext CriarContexto()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(Atual)
                .Options;

            return new AppDbContext(options);
        }

        /// <summary>
        /// Cria o arquivo e as tabelas no primeiro uso. Chamado uma vez na
        /// inicializacao: sem isso a primeira tela a consultar o banco falharia,
        /// porque o arquivo ainda nao existe no aparelho recem-instalado.
        /// </summary>
        public static void GarantirBancoCriado()
        {
            using var contexto = CriarContexto();
            contexto.Database.EnsureCreated();
        }
    }
}
