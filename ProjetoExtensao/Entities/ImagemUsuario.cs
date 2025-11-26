namespace ProjetoExtensao.Entities
{
    public class ImagemUsuario(
        byte[] arquivoImagem
        )
    {
        public long Id { get; set; }

        public byte[] ArquivoImagem { get; set; } = arquivoImagem;
    }
}