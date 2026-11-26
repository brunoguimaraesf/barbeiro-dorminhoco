using BarbeiroDorminhoco.Modelo;

namespace BarbeiroDorminhoco.Solucoes;

/// <summary>
/// SOLUÇÃO INGÊNUA 3: tudo está correto (cadeiras sob lock, sem lost wakeup), mas o barbeiro nunca
/// dorme de verdade: quando a fila está vazia ele fica girando num laço, perguntando
/// "chegou alguém?" milhões de vezes por segundo.
///
/// Problema provocado: ESPERA OCUPADA (busy waiting). O resultado funciona, mas a CPU fica
/// ocupada o tempo todo em que o barbeiro "dorme", e o lock é disputado sem parar.
/// </summary>
public sealed class BarbeariaIngenua3 : IBarbearia
{
    private readonly object _lock = new();
    private readonly Queue<Cliente> _cadeirasDeEspera = new();

    public string Nome => "INGÊNUA 3 (lock + laço sem bloquear: espera ocupada)";
    public Estatisticas Stats { get; } = new();

    public bool Entrar(Cliente cliente)
    {
        lock (_lock)
        {
            if (_cadeirasDeEspera.Count >= Config.CadeirasEspera)
            {
                Logger.Log(cliente.Nome,
                    $"barbearia cheia ({_cadeirasDeEspera.Count}/{Config.CadeirasEspera}), foi embora");
                Stats.Desistiu();
                return false;
            }

            _cadeirasDeEspera.Enqueue(cliente);
            int esperando = _cadeirasDeEspera.Count;
            Stats.RegistrarEspera(esperando);
            Logger.Log(cliente.Nome,
                $"sentou na cadeira de espera ({esperando}/{Config.CadeirasEspera})");
        }
        return true;
    }

    public void AguardarCorte(Cliente cliente, CancellationToken ct)
    {
        try { cliente.CorteTerminado.Wait(ct); }
        catch (OperationCanceledException) { }
    }

    public Cliente? ProximoCliente(CancellationToken ct)
    {
        long giros = 0;
        bool avisou = false;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (Stats.Concluidos >= Config.TotalClientes)
                    return null;

                Cliente? cliente = null;
                lock (_lock)
                {
                    if (_cadeirasDeEspera.Count > 0)
                        cliente = _cadeirasDeEspera.Dequeue();
                }
                if (cliente is not null)
                {
                    Logger.Log(Barbeiro.Nome, $"chamou {cliente.Nome}");
                    return cliente;
                }

                if (!avisou)
                {
                    avisou = true;
                    Logger.Log(Barbeiro.Nome,
                        "fila vazia: ficou girando no laço sem dormir ⚠ ESPERA OCUPADA (CPU em uso)",
                        ConsoleColor.Red);
                }

                // Nada de Sleep, Wait ou semáforo: volta direto a perguntar. É o erro desta solução.
                giros++;
            }
            return null;
        }
        finally
        {
            Stats.RegistrarGiros(giros);
        }
    }

    public void FinalizarCorte(Cliente cliente)
    {
        Stats.Atendeu();
        cliente.CorteTerminado.Set();
    }
}
