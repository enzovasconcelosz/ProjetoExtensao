using Microsoft.EntityFrameworkCore;
using ProjetoExtensao.Domain.Entities;
using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Lembrete> Lembretes { get; set; } = null!;
        public DbSet<TipoLembrete> TipoLembretes { get; set; } = null!;
        public DbSet<ImagemUsuario> ImagensUsuario { get; set; } = null!;
        public DbSet<ContatoEletronico> ContatosEletronico { get; set; } = null!;
        public DbSet<PreferenciaUsuario> PreferenciasUsuario { get; set; } = null!;
        public DbSet<Aparencia> Aparencias { get; set; } = null!;
        public DbSet<TipoNotificacao> TipoNotificacoes { get; set; } = null!;

        // O mapeamento abaixo segue o schema real do banco NaoMeEsquece
        // (tabelas no singular, chaves inteiras e colunas varchar).
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasConversion<int>().ValueGeneratedOnAdd();
                entity.Property(e => e.Login).HasColumnType("varchar(100)").IsRequired();
                entity.Property(e => e.Senha).HasColumnType("varchar(255)").IsRequired();
                entity.Property(e => e.NomeUsuario).HasColumnType("varchar(150)").IsRequired();
                entity.Property(e => e.DataHoraRegistro).HasColumnType("datetime");
                entity.Property(e => e.IdImagem).HasConversion<int>();
                entity.Property(e => e.IdContatoEletronico).HasConversion<int>();
                entity.Property(e => e.IdAparencia).HasConversion<int>();
                entity.Property(e => e.IdPreferenciaUsuario).HasConversion<int>();

                entity.HasOne(e => e.Imagem).WithMany().HasForeignKey(e => e.IdImagem);
                entity.HasOne(e => e.ContatoEletronico).WithMany().HasForeignKey(e => e.IdContatoEletronico);
                entity.HasOne(e => e.Aparencia).WithMany().HasForeignKey(e => e.IdAparencia);
                entity.HasOne(e => e.PreferenciaUsuario).WithMany().HasForeignKey(e => e.IdPreferenciaUsuario);
            });

            modelBuilder.Entity<ImagemUsuario>(entity =>
            {
                entity.ToTable("Imagem");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasConversion<int>().ValueGeneratedOnAdd();

                // A tabela Imagem guarda apenas Descricao/DataHoraRegistro; nao ha
                // coluna para o binario da foto, entao ele nao e persistido aqui.
                entity.Ignore(e => e.ArquivoImagem);
            });

            modelBuilder.Entity<ContatoEletronico>(entity =>
            {
                entity.ToTable("ContatoEletronico");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasConversion<int>().ValueGeneratedOnAdd();
                entity.Property(e => e.Descricao).HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<Aparencia>(entity =>
            {
                entity.ToTable("Aparencia");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasConversion<int>().ValueGeneratedOnAdd();
                entity.Property(e => e.Descricao).HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<PreferenciaUsuario>(entity =>
            {
                entity.ToTable("PreferenciaUsuario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasConversion<int>().ValueGeneratedOnAdd();
                entity.Property(e => e.Notificar).HasColumnName("FlNotificar").IsRequired();
                entity.Property(e => e.Vibrar).HasColumnName("FlVibrar").IsRequired();
                entity.Property(e => e.Som).HasColumnName("FlSom").IsRequired();

                // Nao existe coluna de tema nesta tabela; a preferencia fica no dispositivo.
                entity.Ignore(e => e.Tema);
            });

            modelBuilder.Entity<TipoNotificacao>(entity =>
            {
                entity.ToTable("TipoNotificacao");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasConversion<int>().ValueGeneratedOnAdd();
                entity.Property(e => e.Descricao).HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<TipoLembrete>(entity =>
            {
                entity.ToTable("TipoLembrete");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasConversion<int>().ValueGeneratedOnAdd();
                entity.Property(e => e.Nome).HasColumnType("varchar(100)").IsRequired();
                entity.Property(e => e.DataHoraRegistro).HasColumnType("datetime");
                entity.Property(e => e.IdImagemUsuario).HasColumnName("IdImagem").HasConversion<int>();
                entity.Property(e => e.IdUsuario).HasConversion<int>();

                entity.HasOne(e => e.ImagemUsuario).WithMany().HasForeignKey(e => e.IdImagemUsuario);
                entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario);
            });

            modelBuilder.Entity<Lembrete>(entity =>
            {
                entity.ToTable("Lembrete");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasConversion<int>().ValueGeneratedOnAdd();
                entity.Property(e => e.Nome).HasColumnType("varchar(100)").IsRequired();
                entity.Property(e => e.Descricao).HasColumnType("varchar(max)");
                entity.Property(e => e.DataHoraLembrete).HasColumnType("datetime").IsRequired();
                entity.Property(e => e.DataHoraRegistro).HasColumnType("datetime");
                entity.Property(e => e.IdTipoLembrete).HasConversion<int>();
                entity.Property(e => e.IdTipoNotificacao).HasConversion<int>();
                entity.Property(e => e.IdUsuario).HasConversion<int>();

                entity.HasOne(e => e.TipoLembrete).WithMany().HasForeignKey(e => e.IdTipoLembrete);
                entity.HasOne(e => e.TipoNotificacao).WithMany().HasForeignKey(e => e.IdTipoNotificacao);
                entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario);
            });
        }
    }
}
