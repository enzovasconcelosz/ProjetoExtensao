using ProjetoExtensao.Application.Interfaces;
using ProjetoExtensao.Entities;

namespace ProjetoExtensao.Tests;

/// <summary>
/// Repositorio em memoria: permite testar o servico sem banco de dados e
/// verificar se a gravacao chegou (ou nao) ate a camada de persistencia.
/// </summary>
public class LembreteRepositorioFake : ILembreteRepository
{
    public List<Lembrete> Gravados { get; } = new();
    public List<Lembrete> Atualizados { get; } = new();
    public List<long> Excluidos { get; } = new();

    public Task AddAsync(Lembrete lembrete)
    {
        lembrete.Id = Gravados.Count + 1;
        Gravados.Add(lembrete);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Lembrete lembrete)
    {
        Atualizados.Add(lembrete);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(long id)
    {
        Excluidos.Add(id);
        Gravados.RemoveAll(l => l.Id == id);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Lembrete>> GetAllAsync() => Task.FromResult<IEnumerable<Lembrete>>(Gravados);

    public Task<Lembrete?> GetByIdAsync(long id) => Task.FromResult(Gravados.FirstOrDefault(l => l.Id == id));
}

public class TipoLembreteRepositorioFake : ITipoLembreteRepository
{
    public List<TipoLembrete> Gravados { get; } = new();
    public List<TipoLembrete> Atualizados { get; } = new();

    public Task AddAsync(TipoLembrete tipoLembrete)
    {
        tipoLembrete.Id = Gravados.Count + 1;
        Gravados.Add(tipoLembrete);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TipoLembrete tipoLembrete)
    {
        Atualizados.Add(tipoLembrete);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(long id)
    {
        Gravados.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TipoLembrete>> GetAllAsync() => Task.FromResult<IEnumerable<TipoLembrete>>(Gravados);

    public Task<TipoLembrete?> GetByIdAsync(long id) => Task.FromResult(Gravados.FirstOrDefault(t => t.Id == id));
}
