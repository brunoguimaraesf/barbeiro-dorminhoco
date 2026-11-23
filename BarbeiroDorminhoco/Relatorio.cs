using BarbeiroDorminhoco.Modelo;

namespace BarbeiroDorminhoco;

/// <summary>Relatório final com contadores e verificações automáticas ([OK] / [ERRO]).</summary>
public static class Relatorio
{
    public static void Imprimir(IBarbearia barbearia, TimeSpan duracao, bool estourouPrazo)
    {
        Estatisticas s = barbearia.Stats;

        Console.WriteLine();
        Console.WriteLine("===== RELATÓRIO · Barbeiro Dorminhoco =====");
        Console.WriteLine($"Solução: {barbearia.Nome} | Clientes: {Config.TotalClientes} | Cadeiras: {Config.CadeirasEspera}");
        Console.WriteLine($"Clientes atendidos .......... {s.Atendidos}");
        Console.WriteLine($"Clientes que desistiram ..... {s.Desistentes}");
        Console.WriteLine($"Maior nº esperando .......... {s.MaxEsperando}");
        Console.WriteLine($"Maior nº de cortes juntos ... {s.MaxCortesSimultaneos}");

        Verificar(s.Atendidos + s.Desistentes == Config.TotalClientes,
            "atendidos + desistentes = total de clientes");
        Verificar(s.MaxEsperando <= Config.CadeirasEspera,
            $"nunca mais de {Config.CadeirasEspera} clientes esperando");
        Verificar(s.MaxCortesSimultaneos <= 1,
            "um corte por vez");

        if (estourouPrazo)
            Console.WriteLine($"[ERRO] a simulação não terminou sozinha (prazo de {Config.PrazoSegundos} s estourado)");

        Console.WriteLine($"Tempo total: {duracao.TotalSeconds:0.0} s");
    }

    private static void Verificar(bool condicao, string descricao)
    {
        Console.ForegroundColor = condicao ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{(condicao ? "[OK]  " : "[ERRO]")} {descricao}");
        Console.ResetColor();
    }
}
