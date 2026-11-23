using BarbeiroDorminhoco.Modelo;

namespace BarbeiroDorminhoco.Solucoes;

/// <summary>
/// SOLUÇÃO INGÊNUA 1: nenhum controle de concorrência (sem lock, sem semáforo).
///
/// Problema provocado: RACE CONDITION (check-then-act). Vários clientes leem "há cadeira livre?"
/// ao mesmo tempo, todos recebem "sim" e todos sentam. Resultado: mais de 3 clientes esperando
/// numa sala de 3 cadeiras. A fila (Queue) e a flag do barbeiro também são acessadas sem proteção.
/// </summary>
public sealed class BarbeariaIngenua1 : IBarbearia
{
    // Recursos compartilhados SEM proteção: a fila das cadeiras (Queue não é thread-safe)
    // e a flag que diz se o barbeiro está dormindo.
    private readonly Queue<Cliente> _cadeirasDeEspera = new();
    private bool _barbeiroDormindo;

    public string Nome => "INGÊNUA 1 (sem controle de concorrência)";
    public Estatisticas Stats { get; } = new();

    public bool Entrar(Cliente cliente)
    {
        // LER: existe cadeira livre?
        if (_cadeirasDeEspera.Count < Config.CadeirasEspera)
        {
            // Janela do erro: entre ler e gravar, outros clientes também passam pelo "if".
            Thread.Sleep(Config.JanelaErroMs);

            // GRAVAR: senta. Quando chega aqui, a informação lida acima já pode estar vencida.
            _cadeirasDeEspera.Enqueue(cliente);

            int esperando = _cadeirasDeEspera.Count;
            Stats.RegistrarEspera(esperando);

            if (esperando > Config.CadeirasEspera)
            {
                Logger.Log(cliente.Nome,
                    $"sentou na cadeira de espera ({esperando}/{Config.CadeirasEspera}) ⚠ RACE CONDITION: mais clientes que cadeiras!",
                    ConsoleColor.Red);
            }
            else
            {
                Logger.Log(cliente.Nome,
                    $"sentou na cadeira de espera ({esperando}/{Config.CadeirasEspera})");
            }

            if (_barbeiroDormindo)
            {
                _barbeiroDormindo = false;
                Logger.Log(cliente.Nome, "acordou o barbeiro", ConsoleColor.Yellow);
            }
            return true;
        }

        Logger.Log(cliente.Nome,
            $"barbearia cheia ({_cadeirasDeEspera.Count}/{Config.CadeirasEspera}), foi embora");
        Stats.Desistiu();
        return false;
    }

    public void AguardarCorte(Cliente cliente, CancellationToken ct)
    {
        // Espera ocupada: fica perguntando "já fui atendido?" em vez de ser avisado.
        while (!cliente.Atendido && !ct.IsCancellationRequested)
            Thread.Sleep(Config.PollMs);
    }

    public Cliente? ProximoCliente(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (Stats.Concluidos >= Config.TotalClientes)
                return null;

            if (_cadeirasDeEspera.Count == 0)
            {
                if (!_barbeiroDormindo)
                {
                    _barbeiroDormindo = true;
                    Logger.Log(Barbeiro.Nome, "sem clientes, foi dormir", ConsoleColor.DarkCyan);
                }
                Thread.Sleep(Config.PollMs);
                continue;
            }

            Cliente cliente = _cadeirasDeEspera.Dequeue();
            Logger.Log(Barbeiro.Nome,
                $"chamou {cliente.Nome} (restam {_cadeirasDeEspera.Count} esperando)");
            return cliente;
        }
        return null;
    }

    public void FinalizarCorte(Cliente cliente)
    {
        cliente.Atendido = true;
        Stats.Atendeu();
    }
}
