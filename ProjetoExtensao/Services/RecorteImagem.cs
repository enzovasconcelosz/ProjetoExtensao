namespace ProjetoExtensao;

/// <summary>
/// Recorta de fato os pixels da foto de perfil.
///
/// O enquadramento escolhido na tela de ajuste e reproduzido em uma imagem
/// quadrada nova. Assim os avatares exibem uma imagem comum, sem zoom nem
/// deslocamento aplicados no controle — o que antes fazia o circulo crescer
/// alem do espaco reservado e desalinhar as telas.
///
/// O recorte usa a API de imagem de cada plataforma. A camada grafica do MAUI
/// (BitmapExportContext) nao esta implementada no Windows e lanca
/// NotImplementedException ao gravar.
/// </summary>
public static class RecorteImagem
{
    /// <summary>Lado da imagem gerada, com folga para telas de alta densidade.</summary>
    public const int LadoSaida = 512;

    /// <summary>
    /// Geometria do recorte, em coordenadas da imagem de saida: onde o canto
    /// superior esquerdo da foto cai e com que tamanho ela e desenhada.
    /// </summary>
    private static (double X, double Y, double Largura, double Altura) Geometria(
        double imagemLargura, double imagemAltura,
        double ladoEditor, double escala, double deslocX, double deslocY)
    {
        // O editor mostra a imagem em AspectFill: ela cobre a area de ajuste,
        // e o menor lado e quem define o fator de preenchimento.
        var preenchimento = ladoEditor / Math.Min(imagemLargura, imagemAltura);

        var largura = imagemLargura * preenchimento * escala;
        var altura = imagemAltura * preenchimento * escala;

        var esquerda = (ladoEditor / 2) + deslocX - (largura / 2);
        var topo = (ladoEditor / 2) + deslocY - (altura / 2);

        // Mesma proporcao, ampliada para o tamanho final
        var fator = LadoSaida / ladoEditor;

        return (esquerda * fator, topo * fator, largura * fator, altura * fator);
    }

    public static async Task<(double Largura, double Altura)> DimensoesAsync(byte[] origem)
    {
#if WINDOWS
        using var stream = new MemoryStream(origem);
        var decodificador = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(
            stream.AsRandomAccessStream());

        return (decodificador.PixelWidth, decodificador.PixelHeight);
#elif ANDROID
        var opcoes = new Android.Graphics.BitmapFactory.Options { InJustDecodeBounds = true };
        await Task.Run(() => Android.Graphics.BitmapFactory.DecodeByteArray(origem, 0, origem.Length, opcoes));

        return (opcoes.OutWidth, opcoes.OutHeight);
#elif IOS || MACCATALYST
        using var dados = Foundation.NSData.FromArray(origem);
        using var imagem = UIKit.UIImage.LoadFromData(dados)!;

        return await Task.FromResult((imagem.Size.Width * imagem.CurrentScale, imagem.Size.Height * imagem.CurrentScale));
#else
        await Task.CompletedTask;
        return (LadoSaida, LadoSaida);
#endif
    }

    /// <summary>
    /// Gera o recorte quadrado correspondente ao que aparece dentro do circulo.
    /// </summary>
    /// <param name="origem">Conteudo da imagem original.</param>
    /// <param name="ladoEditor">Lado da area de ajuste, em unidades de tela.</param>
    /// <param name="escala">Zoom aplicado pelo usuario.</param>
    /// <param name="deslocX">Deslocamento horizontal, em unidades de tela.</param>
    /// <param name="deslocY">Deslocamento vertical, em unidades de tela.</param>
    public static async Task<byte[]> GerarAsync(
        byte[] origem, double ladoEditor, double escala, double deslocX, double deslocY)
    {
        var (largura, altura) = await DimensoesAsync(origem);
        var destino = Geometria(largura, altura, ladoEditor, escala, deslocX, deslocY);

#if WINDOWS
        using var entrada = new MemoryStream(origem);
        var decodificador = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(
            entrada.AsRandomAccessStream());

        // A imagem e reduzida ao tamanho com que aparece e depois recortada na
        // area visivel: e a mesma conta do editor, em pixels de saida.
        var escalada = new Windows.Graphics.Imaging.BitmapTransform
        {
            ScaledWidth = (uint)Math.Max(1, Math.Round(destino.Largura)),
            ScaledHeight = (uint)Math.Max(1, Math.Round(destino.Altura)),
            InterpolationMode = Windows.Graphics.Imaging.BitmapInterpolationMode.Fant
        };

        var recorteX = (int)Math.Round(-destino.X);
        var recorteY = (int)Math.Round(-destino.Y);

        // Nunca pedir area fora da imagem: o decodificador rejeita
        recorteX = Math.Clamp(recorteX, 0, Math.Max(0, (int)escalada.ScaledWidth - 1));
        recorteY = Math.Clamp(recorteY, 0, Math.Max(0, (int)escalada.ScaledHeight - 1));

        escalada.Bounds = new Windows.Graphics.Imaging.BitmapBounds
        {
            X = (uint)recorteX,
            Y = (uint)recorteY,
            Width = (uint)Math.Min(LadoSaida, escalada.ScaledWidth - recorteX),
            Height = (uint)Math.Min(LadoSaida, escalada.ScaledHeight - recorteY)
        };

        var pixels = await decodificador.GetPixelDataAsync(
            Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
            Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied,
            escalada,
            Windows.Graphics.Imaging.ExifOrientationMode.RespectExifOrientation,
            Windows.Graphics.Imaging.ColorManagementMode.DoNotColorManage);

        using var saida = new Windows.Storage.Streams.InMemoryRandomAccessStream();
        var codificador = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(
            Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, saida);

        codificador.SetPixelData(
            Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
            Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied,
            escalada.Bounds.Width,
            escalada.Bounds.Height,
            96, 96,
            pixels.DetachPixelData());

        await codificador.FlushAsync();

        var resultado = new byte[saida.Size];
        using (var leitura = saida.GetInputStreamAt(0).AsStreamForRead())
            await leitura.ReadExactlyAsync(resultado);

        return resultado;
#elif ANDROID
        return await Task.Run(() =>
        {
            using var original = Android.Graphics.BitmapFactory.DecodeByteArray(origem, 0, origem.Length)!;
            using var final = Android.Graphics.Bitmap.CreateBitmap(
                LadoSaida, LadoSaida, Android.Graphics.Bitmap.Config.Argb8888!)!;

            using var tela = new Android.Graphics.Canvas(final);
            using var area = new Android.Graphics.RectF(
                (float)destino.X,
                (float)destino.Y,
                (float)(destino.X + destino.Largura),
                (float)(destino.Y + destino.Altura));

            using var pincel = new Android.Graphics.Paint { FilterBitmap = true, AntiAlias = true };
            tela.DrawBitmap(original, null, area, pincel);

            using var saida = new MemoryStream();
            final.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 100, saida);
            return saida.ToArray();
        });
#elif IOS || MACCATALYST
        using var dados = Foundation.NSData.FromArray(origem);
        using var imagem = UIKit.UIImage.LoadFromData(dados)!;

        var renderizador = new UIKit.UIGraphicsImageRenderer(
            new CoreGraphics.CGSize(LadoSaida, LadoSaida),
            new UIKit.UIGraphicsImageRendererFormat { Scale = 1 });

        var final = renderizador.CreateImage(_ =>
            imagem.Draw(new CoreGraphics.CGRect(
                destino.X, destino.Y, destino.Largura, destino.Altura)));

        using var png = final.AsPNG()!;
        return await Task.FromResult(png.ToArray());
#else
        await Task.CompletedTask;
        return origem;
#endif
    }
}
