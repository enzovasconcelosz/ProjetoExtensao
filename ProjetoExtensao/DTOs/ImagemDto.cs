namespace ProjetoExtensao.DTOs;

public class ImagemDto
{
    public long Id { get; set; }
    public byte[] ArquivoImagem { get; set; } = null!;
    public DateTime? DataHoraRegistro { get; set; }
}
