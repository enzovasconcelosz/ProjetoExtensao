namespace ProjetoExtensao;

/// <summary>
/// Escolha do recorte da foto de perfil.
///
/// A imagem e arrastada e ampliada dentro de um circulo; o que fica fora
/// nao aparece no avatar. Sao gravados o zoom e o deslocamento em fracao do
/// lado do circulo, para que o mesmo recorte valha em qualquer tamanho de
/// avatar (48, 96 ou 110 pixels).
/// </summary>
public partial class AjusteImagemPerfil : ContentPage
{
    /// <summary>Lado da area de recorte, igual ao definido no XAML.</summary>
    private const double LadoRecorte = 280;

    private readonly byte[] _imagem;
    private readonly Func<Page> _paginaAnterior;

    private double _deslocX;
    private double _deslocY;
    private double _deslocInicialX;
    private double _deslocInicialY;
    private double _escala = 1;
    private double _escalaInicial = 1;

    // Tamanho com que a imagem aparece no editor (AspectFill, zoom 1)
    private double _larguraExibida = LadoRecorte;
    private double _alturaExibida = LadoRecorte;

    public AjusteImagemPerfil(byte[] imagem, Func<Page>? paginaAnterior = null)
    {
        InitializeComponent();

        _imagem = imagem;
        _paginaAnterior = paginaAnterior ?? (() => new ConfiguracoesUsuario());

        imgOriginal.Source = ImageSource.FromStream(() => new MemoryStream(_imagem));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        await MedirImagem();

        sliderZoom.Value = _escala;
        Aplicar();
    }

    /// <summary>
    /// Descobre com que tamanho a foto aparece no editor, o que define ate
    /// onde o arrasto pode ir.
    /// </summary>
    private async Task MedirImagem()
    {
        try
        {
            var (largura, altura) = await RecorteImagem.DimensoesAsync(_imagem);

            // AspectFill: o menor lado da foto e quem preenche a area
            var preenchimento = LadoRecorte / Math.Min(largura, altura);

            _larguraExibida = largura * preenchimento;
            _alturaExibida = altura * preenchimento;
        }
        catch
        {
            // sem as dimensoes, o arrasto fica limitado apenas pelo zoom
        }
    }

    private void SliderZoom_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        _escala = e.NewValue;
        Aplicar();
    }

    private void Imagem_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _deslocInicialX = _deslocX;
                _deslocInicialY = _deslocY;
                break;

            case GestureStatus.Running:
                _deslocX = _deslocInicialX + e.TotalX;
                _deslocY = _deslocInicialY + e.TotalY;
                Aplicar();
                break;
        }
    }

    private void Imagem_PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _escalaInicial = _escala;
                break;

            case GestureStatus.Running:
                _escala = Math.Clamp(_escalaInicial * e.Scale, 1, 3);
                sliderZoom.Value = _escala;
                Aplicar();
                break;
        }
    }

    /// <summary>
    /// Aplica zoom e deslocamento, impedindo que sobre area vazia no recorte.
    ///
    /// O limite vem do tamanho real exibido, nao do zoom: uma foto em pe ja
    /// sobra em altura mesmo sem zoom, e precisa poder ser deslocada para que
    /// o rosto nao fique cortado.
    /// </summary>
    private void Aplicar()
    {
        var limiteX = Math.Max(0, ((_larguraExibida * _escala) - LadoRecorte) / 2);
        var limiteY = Math.Max(0, ((_alturaExibida * _escala) - LadoRecorte) / 2);

        _deslocX = Math.Clamp(_deslocX, -limiteX, limiteX);
        _deslocY = Math.Clamp(_deslocY, -limiteY, limiteY);

        imgOriginal.Scale = _escala;
        imgOriginal.TranslationX = _deslocX;
        imgOriginal.TranslationY = _deslocY;
    }

    private async void BtnConfirmar_Clicked(object sender, EventArgs e)
    {
        try
        {
            // O recorte e gravado na propria imagem: os avatares exibem uma
            // foto quadrada comum, sem transformacoes que estourem o circulo.
            var recortada = await RecorteImagem.GerarAsync(_imagem, LadoRecorte, _escala, _deslocX, _deslocY);
            await ImagemPerfil.SalvarAsync(recortada);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível salvar a foto. " + ex.Message, "Fechar");
            return;
        }

        Voltar();
    }

    private void BtnCancelar_Clicked(object sender, EventArgs e) => Voltar();

    private void Voltar()
    {
        App.Current.MainPage = _paginaAnterior();
    }
}
