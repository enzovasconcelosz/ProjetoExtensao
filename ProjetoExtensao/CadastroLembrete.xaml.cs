using Microsoft.Extensions.DependencyInjection;
using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Application.Validacoes;
using ProjetoExtensao.DTOs;
using ProjetoExtensao.Entities;
using ProjetoExtensao.Mappings;

namespace ProjetoExtensao;

public partial class CadastroLembrete : ContentPage
{
    private readonly LembreteDto? _dto;
    private ILembreteService? _service;

    private byte[]? _novaImagem;
    private bool _removerImagem;

    // Quem abriu a tela: o retorno volta para a mesma origem
    private readonly Func<Page> _paginaAnterior;

    // Data vinda do calendario, quando a tela e aberta por ele
    private readonly DateTime? _dataInicial;

    public CadastroLembrete(Func<Page>? paginaAnterior = null)
    {
        InitializeComponent();
        _paginaAnterior = paginaAnterior ?? (() => new Lembretes());
    }

    public CadastroLembrete(DateTime dataInicial, Func<Page>? paginaAnterior = null)
        : this(paginaAnterior)
    {
        _dataInicial = dataInicial;
    }

    public CadastroLembrete(LembreteDto dto, Func<Page>? paginaAnterior = null) : this(paginaAnterior)
    {
        _dto = dto;
        lblTitulo.Text = "Editar lembrete";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Tema.Aplicar();

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        _service = services?.GetService<ILembreteService>();

        await CarregarTipos(services?.GetService<ITipoLembreteService>());
        PreencherCampos();
        MostrarImagem(_dto != null ? ImagemLembrete.Obter(_dto.Id) : null);
    }

    private async Task CarregarTipos(ITipoLembreteService? tipoService)
    {
        if (tipoService == null)
            return;

        try
        {
            var tipos = (await tipoService.GetAllAsync()).Select(t => t.ToDto()).ToList();
            pickerTipo.ItemsSource = tipos;

            if (_dto?.IdTipoLembrete != null)
                pickerTipo.SelectedItem = tipos.FirstOrDefault(t => t.Id == _dto.IdTipoLembrete);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível carregar os tipos de lembrete. " + ex.Message, "Fechar");
        }
    }

    private void PreencherCampos()
    {
        if (_dto == null)
        {
            // Novo lembrete: sugere a proxima hora cheia, na data escolhida
            // no calendario quando a tela veio de la.
            var sugestao = DateTime.Now.AddHours(1);
            pickerData.Date = _dataInicial?.Date ?? sugestao.Date;
            pickerHora.Time = new TimeSpan(sugestao.Hour, 0, 0);
            return;
        }

        entryNome.Text = _dto.Nome;
        editorDescricao.Text = _dto.Descricao;
        pickerData.Date = _dto.DataHoraLembrete.Date;
        pickerHora.Time = _dto.DataHoraLembrete.TimeOfDay;
    }

    private async void BtnEscolherImagem_Clicked(object sender, EventArgs e)
    {
        try
        {
            var arquivo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Selecione uma imagem para o lembrete"
            });

            if (arquivo == null)
                return;

            using var origem = await arquivo.OpenReadAsync();
            using var memoria = new MemoryStream();
            await origem.CopyToAsync(memoria);

            _novaImagem = memoria.ToArray();
            _removerImagem = false;

            MostrarImagem(ImageSource.FromStream(() => new MemoryStream(_novaImagem)));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível selecionar a imagem. " + ex.Message, "Fechar");
        }
    }

    private void BtnRemoverImagem_Clicked(object sender, EventArgs e)
    {
        _novaImagem = null;
        _removerImagem = true;
        MostrarImagem(null);
    }

    private void MostrarImagem(ImageSource? origem)
    {
        imgLembrete.Source = origem;
        imgLembrete.IsVisible = origem != null;
        lblSemImagem.IsVisible = origem == null;
        btnRemoverImagem.IsVisible = origem != null;
    }

    private async void BtnSalvar_Clicked(object sender, EventArgs e)
    {
        if (_service == null)
        {
            await DisplayAlert("Erro", "Serviço de lembretes indisponível.", "Fechar");
            return;
        }

        var nome = entryNome.Text?.Trim() ?? string.Empty;
        var descricao = editorDescricao.Text?.Trim() ?? string.Empty;
        var data = pickerData.Date ?? DateTime.Today;
        var hora = pickerHora.Time ?? TimeSpan.Zero;
        var dataHora = data.Date.Add(hora);
        var tipoSelecionado = pickerTipo.SelectedItem as TipoLembreteDto;

        // Mesmas regras usadas pelo servico, aqui apenas para orientar o usuario
        var erros = ValidacaoLembrete.Validar(nome, tipoSelecionado?.Id, dataHora, descricao);

        if (erros.Count > 0)
        {
            lblAviso.Text = string.Join("\n", erros);
            lblAviso.IsVisible = true;
            return;
        }

        lblAviso.IsVisible = false;

        try
        {
            Lembrete entidade;

            if (_dto == null || _dto.Id == 0)
            {
                entidade = new Lembrete(nome, descricao, dataHora)
                {
                    IdTipoLembrete = tipoSelecionado!.Id
                };

                await _service.AddAsync(entidade);
            }
            else
            {
                entidade = _dto.ToEntity();
                entidade.Nome = nome;
                entidade.Descricao = descricao;
                entidade.DataHoraLembrete = dataHora;
                entidade.IdTipoLembrete = tipoSelecionado!.Id;

                await _service.UpdateAsync(entidade);
            }

            // A imagem depende do Id, que so existe depois de gravar
            if (_novaImagem != null)
                await ImagemLembrete.SalvarAsync(entidade.Id, _novaImagem);
            else if (_removerImagem)
                ImagemLembrete.Remover(entidade.Id);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível salvar. " + ex.Message, "Fechar");
            return;
        }

        Voltar();
    }

    private void BtnCancelar_Clicked(object sender, EventArgs e)
    {
        Voltar();
    }

    // O projeto troca a MainPage em vez de usar pilha de navegacao
    private void Voltar()
    {
        App.Current.MainPage = _paginaAnterior();
    }
}
