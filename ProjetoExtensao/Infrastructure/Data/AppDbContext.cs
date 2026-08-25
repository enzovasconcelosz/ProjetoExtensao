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
        public DbSet<Evento> Eventos { get; set; } = null!;
        public DbSet<TipoEvento> TipoEventos { get; set; } = null!;
        public DbSet<ImagemUsuario> ImagensUsuario { get; set; } = null!;
        public DbSet<ContatoEletronico> ContatosEletronico { get; set; } = null!;
        public DbSet<PreferenciaUsuario> PreferenciasUsuario { get; set; } = null!;
        public DbSet<Aparencia> Aparencias { get; set; } = null!;
        public DbSet<TipoNotificacao> TipoNotificacoes { get; set; } = null!;

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
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Login).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Senha).HasMaxLength(200).IsRequired();
                entity.Property(e => e.NomeUsuario).HasMaxLength(200);
            });

            modelBuilder.Entity<ImagemUsuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ArquivoImagem).IsRequired();
            });

            modelBuilder.Entity<ContatoEletronico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descricao).HasMaxLength(200);
            });

            modelBuilder.Entity<Aparencia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descricao).HasMaxLength(200);
            });

            modelBuilder.Entity<PreferenciaUsuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notificar).IsRequired();
                entity.Property(e => e.Tema).IsRequired();
            });

            modelBuilder.Entity<TipoNotificacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descricao).HasMaxLength(200);
            });

            modelBuilder.Entity<TipoEvento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
                entity.Property(e => e.DataHoraRegistro).IsRequired();
            });

            modelBuilder.Entity<Evento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Descricao).HasMaxLength(1000);
                entity.Property(e => e.DataHoraEvento).IsRequired();
                entity.Property(e => e.DataHoraRegistro).IsRequired();
            });
        }
    }
}
