using System.Globalization;
using BarbeiroDorminhoco.Modelo;

namespace BarbeiroDorminhoco;

/// <summary>Relatório final com contadores e verificações automáticas ([OK] / [ERRO]).</summary>
public static class Relatorio
{
    // Acima disso o barbeiro girou em laço sem bloquear: é espera ocupada.
    private const long LimiteGirosEspera = 100_000;

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static void Imprimir(IBarbearia barbearia, TimeSpan duracao, TimeSpan cpuGasta, bool estourouPrazo)
    {
        Estatisticas s = barbearia.Stats;
        int semDesfecho = Config.TotalClientes - s.Concluidos;
        string giros = s.GirosEspera.ToString("N0", PtBr);
        double usoCpu = duracao.TotalSeconds > 0 ? cpuGasta.TotalSeconds / duracao.TotalSeconds : 0;

        Console.WriteLine();
        Console.WriteLine("===== RELATÓRIO · Barbeiro Dorminhoco =====");
        Console.WriteLine($"Solução: {barbearia.Nome}");
        Console.WriteLine($"Clientes: {Config.TotalClientes} | Cadeiras de espera: {Config.CadeirasEspera} | Barbeiros: 1");
        Linha("Clientes atendidos", s.Atendidos);
        Linha("Clientes que desistiram", s.Desistentes);
        Linha("Clientes sem desfecho (presos)", semDesfecho);
        Linha("Maior nº esperando", s.MaxEsperando);
        Linha("Maior nº de cortes juntos", s.MaxCortesSimultaneos);
        Linha("Giros de espera ocupada", giros);
        Linha("CPU gasta", $"{cpuGasta.TotalSeconds:0.0} s em {duracao.TotalSeconds:0.0} s ({usoCpu.ToString("P0", PtBr)} de um núcleo)");

        Verificar(s.Atendidos + s.Desistentes == Config.TotalClientes,
            "atendidos + desistentes = total de clientes");
        Verificar(s.MaxEsperando <= Config.CadeirasEspera,
            $"nunca mais de {Config.CadeirasEspera} clientes esperando");
        Verificar(s.MaxCortesSimultaneos <= 1,
            "um corte por vez");
        Verificar(s.GirosEspera <= LimiteGirosEspera,
            s.GirosEspera <= LimiteGirosEspera
                ? "barbeiro nunca ficou em espera ocupada"
                : $"o barbeiro ficou em espera ocupada ({giros} giros sem bloquear)");

        if (estourouPrazo)
            Console.WriteLine($"[ERRO] a simulação não terminou sozinha (travou até o prazo de {Config.PrazoSegundos} s)");

        Console.WriteLine($"Tempo total: {duracao.TotalSeconds:0.0} s");
    }

    private static void Linha(string rotulo, object valor) =>
        Console.WriteLine($"{(rotulo + " ").PadRight(34, '.')} {valor}");

    private static void Verificar(bool condicao, string descricao)
    {
        Console.ForegroundColor = condicao ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{(condicao ? "[OK]  " : "[ERRO]")} {descricao}");
        Console.ResetColor();
    }
}
