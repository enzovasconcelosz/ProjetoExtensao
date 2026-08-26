namespace ProjetoExtensao.Entities
{
    public class ImagemUsuario(
        byte[] arquivoImagem
        )
    {
        // Necessario para o EF: a tabela Imagem nao possui coluna para o binario,
        // entao o construtor primario nao pode ser usado na materializacao.
        public ImagemUsuario() : this(Array.Empty<byte>())
        {
        }

        public long Id { get; set; }

        public byte[] ArquivoImagem { get; set; } = arquivoImagem;
    }
}