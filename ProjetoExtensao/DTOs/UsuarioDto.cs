namespace ProjetoExtensao.DTOs;

public class UsuarioDto
{
    public long Id { get; set; }
    public string Login { get; set; } = null!;
    public string Senha { get; set; } = null!;
    public string? NomeUsuario { get; set; }
    public long? IdImagem { get; set; }
    public long? IdContatoEletronico { get; set; }
    public long? IdAparencia { get; set; }
    public long? IdPreferenciaUsuario { get; set; }
    public DateTime DataHoraRegistro { get; set; }
}
