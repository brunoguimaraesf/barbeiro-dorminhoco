using System.Diagnostics;
using BarbeiroDorminhoco.Modelo;

namespace BarbeiroDorminhoco;

/// <summary>Monta o cenário (1 barbeiro + N clientes, todos Tasks), executa e imprime o relatório.</summary>
public static class Simulacao
{
    public static void Executar(IBarbearia barbearia)
    {
        Console.WriteLine($"=== {barbearia.Nome} ===");
        Console.WriteLine($"{Config.TotalClientes} clientes, {Config.CadeirasEspera} cadeiras de espera, 1 barbeiro\n");

        using var prazo = new CancellationTokenSource(TimeSpan.FromSeconds(Config.PrazoSegundos));
        var cronometro = Stopwatch.StartNew();
        Logger.Iniciar();

        var tarefas = new List<Task> { new Barbeiro().Iniciar(barbearia, prazo.Token) };
        for (int id = 1; id <= Config.TotalClientes; id++)
            tarefas.Add(new Cliente(id).Iniciar(barbearia, prazo.Token));

        Task.WaitAll(tarefas.ToArray());
        cronometro.Stop();

        Relatorio.Imprimir(barbearia, cronometro.Elapsed, prazo.IsCancellationRequested);
    }
}
