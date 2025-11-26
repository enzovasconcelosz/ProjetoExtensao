namespace ProjetoExtensao.Entities
{
    public class TipoEvento(
        string nome
        )
    {
        public long Id { get; set; }

        public string Nome { get; set; } = nome;

        public DateTime DataHoraRegistro { get; set; } = DateTime.Now;

        public long? IdImagemUsuario { get; set; }
        public ImagemUsuario? ImagemUsuario { get; set; }

        public long? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
    }
}