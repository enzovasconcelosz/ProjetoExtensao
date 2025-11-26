using ProjetoExtensao.Entities.Enums;

namespace ProjetoExtensao.Entities
{
    public class Usuario(
        string login,
        string senha,
        string nomeUsuario
        )
    {
        public long Id { get; set; }

        public string Login { get; set; } = login;

        public string Senha { get; set; } = senha;

        public string NomeUsuario { get; set; } = nomeUsuario;

        public DateTime DataHoraRegistro { get; set; } = DateTime.Now;

        public long? IdImagem { get; set; }
        public ImagemUsuario? Imagem { get; set; }

        public long? IdContatoEletronico { get; set; }
        public ContatoEletronico? ContatoEletronico { get; set; }

        public long? IdPreferenciaUsuario { get; set; }
        public PreferenciaUsuario? PreferenciaUsuario { get; set; }
    }
}