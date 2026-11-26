using BarbeiroDorminhoco.Modelo;

namespace BarbeiroDorminhoco.Solucoes;

/// <summary>
/// SOLUÇÃO INGÊNUA 2: as cadeiras estão protegidas por lock (a race condition da Ingênua 1 sumiu),
/// mas o barbeiro "dorme" com uma flag booleana e um evento, e a decisão de dormir é feita em
/// DOIS passos separados: (1) olhar se a fila está vazia, (2) marcar que está dormindo.
///
/// Problema provocado: LOST WAKEUP (acordar perdido). Se um cliente chega entre os passos 1 e 2,
/// ele enxerga "barbeiro acordado", não manda sinal nenhum, e logo depois o barbeiro vai dormir
/// com cliente esperando. Ninguém mais o acorda: o programa trava até o prazo.
/// </summary>
public sealed class BarbeariaIngenua2 : IBarbearia
{
    private readonly object _lock = new();                    // protege apenas a fila e a flag
    private readonly Queue<Cliente> _cadeirasDeEspera = new();
    private readonly ManualResetEventSlim _acordar = new(false);
    private bool _barbeiroDormindo;

    public string Nome => "INGÊNUA 2 (lock + flag de sono: lost wakeup)";
    public Estatisticas Stats { get; } = new();

    public bool Entrar(Cliente cliente)
    {
        lock (_lock)
        {
            // Cadeiras corretas: verificar e sentar acontecem juntos, dentro do lock.
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

            // O aviso só é enviado SE a flag já estiver marcada. Esse é o ponto frágil.
            if (_barbeiroDormindo)
            {
                _barbeiroDormindo = false;
                Logger.Log(cliente.Nome, "acordou o barbeiro", ConsoleColor.Yellow);
                _acordar.Set();
            }
            else
            {
                Logger.Log(cliente.Nome, "barbeiro não está dormindo, não precisa acordar ninguém");
            }
        }
        return true;
    }

    public void AguardarCorte(Cliente cliente, CancellationToken ct)
    {
        try { cliente.CorteTerminado.Wait(ct); }
        catch (OperationCanceledException)
        {
            Logger.Log(cliente.Nome, "ainda esperando quando o prazo acabou (ninguém o chamou)", ConsoleColor.Red);
        }
    }

    public Cliente? ProximoCliente(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (Stats.Concluidos >= Config.TotalClientes)
                return null;

            // PASSO 1: olhar a fila (e pegar o cliente, se houver).
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

            Logger.Log(Barbeiro.Nome, "a fila estava vazia, vou dormir...", ConsoleColor.DarkCyan);

            // Janela do erro: entre olhar a fila (passo 1) e marcar que dorme (passo 2),
            // um cliente pode chegar, sentar e ver o barbeiro "acordado".
            Thread.Sleep(Config.JanelaLostWakeupMs);

            // PASSO 2: marcar que está dormindo e dormir.
            int esperando;
            lock (_lock)
            {
                _barbeiroDormindo = true;
                esperando = _cadeirasDeEspera.Count;
            }

            if (esperando > 0)
            {
                Logger.Log(Barbeiro.Nome,
                    $"foi dormir com {esperando} cliente(s) esperando ⚠ LOST WAKEUP: o aviso já passou e ninguém vai acordá-lo!",
                    ConsoleColor.Red);
            }
            else
            {
                Logger.Log(Barbeiro.Nome, "dormindo", ConsoleColor.DarkCyan);
            }

            try { _acordar.Wait(ct); }
            catch (OperationCanceledException) { return null; }
            _acordar.Reset();
        }
        return null;
    }

    public void FinalizarCorte(Cliente cliente)
    {
        Stats.Atendeu();
        cliente.CorteTerminado.Set();
    }
}
