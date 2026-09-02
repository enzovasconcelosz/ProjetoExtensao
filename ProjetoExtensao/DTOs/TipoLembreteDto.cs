namespace ProjetoExtensao.DTOs;

public class TipoLembreteDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = null!;
    public DateTime DataHoraRegistro { get; set; }
    public long? IdImagemUsuario { get; set; }
    public long? IdUsuario { get; set; }
}
