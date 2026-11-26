namespace BarbeiroDorminhoco.Modelo;

/// <summary>Participante do problema: cada cliente é uma Task com tempo de chegada aleatório.</summary>
public sealed class Cliente
{
    public Cliente(int id) => Id = id;

    public int Id { get; }
    public string Nome => $"Cliente {Id:00}";

    // Campo comum, sem volatile/lock: faz parte da versão ingênua (ver BarbeariaIngenua1).
    public bool Atendido;

    // Sinal "meu corte terminou", usado pelas Ingênuas 2 e 3 para o cliente esperar sem girar em laço.
    public readonly ManualResetEventSlim CorteTerminado = new(false);

    public Task Iniciar(IBarbearia barbearia, CancellationToken ct) =>
        // LongRunning = thread dedicada; evita que o pool de threads atrase as chegadas.
        Task.Factory.StartNew(() => Executar(barbearia, ct), ct,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);

    private void Executar(IBarbearia barbearia, CancellationToken ct)
    {
        try
        {
            bool retardatario = Id > Config.ClientesNaRajada;
            Thread.Sleep(retardatario
                ? Random.Shared.Next(Config.ChegadaTardiaMinMs, Config.ChegadaTardiaMaxMs + 1)
                : Random.Shared.Next(Config.ChegadaMinMs, Config.ChegadaMaxMs + 1));
            Logger.Log(Nome, "chegou na barbearia");

            if (barbearia.Entrar(this))
                barbearia.AguardarCorte(this, ct);
        }
        catch (Exception ex)
        {
            Logger.Log(Nome, $"erro inesperado: {ex.GetType().Name} - {ex.Message}", ConsoleColor.Red);
        }
    }
}
