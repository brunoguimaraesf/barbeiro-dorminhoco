namespace BarbeiroDorminhoco.Modelo;

/// <summary>Participante do problema: o barbeiro é uma Task que atende um cliente por vez.</summary>
public sealed class Barbeiro
{
    public const string Nome = "Barbeiro";

    public Task Iniciar(IBarbearia barbearia, CancellationToken ct) =>
        Task.Factory.StartNew(() => Executar(barbearia, ct), ct,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);

    private static void Executar(IBarbearia barbearia, CancellationToken ct)
    {
        try
        {
            Cliente? cliente;
            while ((cliente = barbearia.ProximoCliente(ct)) is not null)
            {
                barbearia.Stats.IniciarCorte();
                Logger.Log(Nome, $"começou a cortar o cabelo de {cliente.Nome}");

                Thread.Sleep(Random.Shared.Next(Config.CorteMinMs, Config.CorteMaxMs + 1));

                Logger.Log(Nome, $"terminou o corte de {cliente.Nome}");
                barbearia.Stats.FimCorte();
                barbearia.FinalizarCorte(cliente);
            }
            Logger.Log(Nome, "encerrou o expediente");
        }
        catch (Exception ex)
        {
            Logger.Log(Nome, $"erro inesperado: {ex.GetType().Name} - {ex.Message}", ConsoleColor.Red);
        }
    }
}
