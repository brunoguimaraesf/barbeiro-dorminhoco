namespace BarbeiroDorminhoco;

/// <summary>
/// Contadores de medição usados no relatório. São instrumentação: usam Interlocked/lock
/// para que o número impresso seja confiável mesmo quando a solução testada é insegura.
/// </summary>
public sealed class Estatisticas
{
    private readonly object _lockMedicao = new();
    private int _atendidos;
    private int _desistentes;
    private int _cortesAtivos;
    private int _maxCortesSimultaneos;
    private int _maxEsperando;

    public int Atendidos => Volatile.Read(ref _atendidos);
    public int Desistentes => Volatile.Read(ref _desistentes);
    public int Concluidos => Atendidos + Desistentes;
    public int MaxCortesSimultaneos => Volatile.Read(ref _maxCortesSimultaneos);
    public int MaxEsperando => Volatile.Read(ref _maxEsperando);

    public void Atendeu() => Interlocked.Increment(ref _atendidos);
    public void Desistiu() => Interlocked.Increment(ref _desistentes);

    public void RegistrarEspera(int esperando)
    {
        lock (_lockMedicao)
        {
            if (esperando > _maxEsperando) _maxEsperando = esperando;
        }
    }

    public void IniciarCorte()
    {
        int ativos = Interlocked.Increment(ref _cortesAtivos);
        lock (_lockMedicao)
        {
            if (ativos > _maxCortesSimultaneos) _maxCortesSimultaneos = ativos;
        }
    }

    public void FimCorte() => Interlocked.Decrement(ref _cortesAtivos);
}
