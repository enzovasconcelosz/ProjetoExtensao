using ProjetoExtensao.Application.Services;
using ProjetoExtensao.Entities;
using Xunit;

namespace ProjetoExtensao.Tests;

/// <summary>
/// O servico e o ultimo ponto antes do banco: mesmo que uma tela deixe passar
/// um dado invalido, nada invalido pode ser gravado.
/// </summary>
public class LembreteServiceTests
{
    private static Lembrete LembreteValido() =>
        new("Consulta médica", "Cardiologista", DateTime.Now.AddDays(1)) { IdTipoLembrete = 1 };

    [Fact]
    public async Task Lembrete_valido_chega_ao_repositorio()
    {
        var repositorio = new LembreteRepositorioFake();
        var servico = new LembreteService(repositorio);

        await servico.AddAsync(LembreteValido());

        Assert.Single(repositorio.Gravados);
    }

    [Fact]
    public async Task Lembrete_invalido_nao_chega_ao_repositorio()
    {
        var repositorio = new LembreteRepositorioFake();
        var servico = new LembreteService(repositorio);
        var invalido = new Lembrete("ab", "", DateTime.Now.AddDays(-1));

        await Assert.ThrowsAsync<ArgumentException>(() => servico.AddAsync(invalido));

        Assert.Empty(repositorio.Gravados);
    }

    [Fact]
    public async Task Atualizacao_invalida_nao_chega_ao_repositorio()
    {
        var repositorio = new LembreteRepositorioFake();
        var servico = new LembreteService(repositorio);
        var lembrete = LembreteValido();
        lembrete.IdTipoLembrete = null;

        await Assert.ThrowsAsync<ArgumentException>(() => servico.UpdateAsync(lembrete));

        Assert.Empty(repositorio.Atualizados);
    }

    [Fact]
    public async Task Exclusao_repassa_o_identificador_ao_repositorio()
    {
        var repositorio = new LembreteRepositorioFake();
        var servico = new LembreteService(repositorio);
        await servico.AddAsync(LembreteValido());

        await servico.DeleteAsync(1);

        Assert.Equal(new long[] { 1 }, repositorio.Excluidos);
        Assert.Empty(repositorio.Gravados);
    }

    [Fact]
    public async Task Busca_por_id_devolve_o_lembrete_gravado()
    {
        var repositorio = new LembreteRepositorioFake();
        var servico = new LembreteService(repositorio);
        await servico.AddAsync(LembreteValido());

        var encontrado = await servico.GetByIdAsync(1);

        Assert.NotNull(encontrado);
        Assert.Equal("Consulta médica", encontrado!.Nome);
    }
}

public class TipoLembreteServiceTests
{
    [Fact]
    public async Task Tipo_valido_chega_ao_repositorio()
    {
        var repositorio = new TipoLembreteRepositorioFake();
        var servico = new TipoLembreteService(repositorio);

        await servico.AddAsync(new TipoLembrete("Saúde"));

        Assert.Single(repositorio.Gravados);
    }

    [Fact]
    public async Task Tipo_invalido_nao_chega_ao_repositorio()
    {
        var repositorio = new TipoLembreteRepositorioFake();
        var servico = new TipoLembreteService(repositorio);

        await Assert.ThrowsAsync<ArgumentException>(() => servico.AddAsync(new TipoLembrete("ab")));

        Assert.Empty(repositorio.Gravados);
    }
}
