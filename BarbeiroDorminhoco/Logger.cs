using System.Diagnostics;

namespace BarbeiroDorminhoco;

/// <summary>
/// Requisito 5: registra cada evento com horário (relativo ao início da simulação) e autor.
/// Ex.: [00:01.532] Cliente 03 · sentou na cadeira de espera (2/3)
/// O lock aqui só impede que linhas de threads diferentes se misturem no console;
/// ele faz parte da instrumentação e não protege nenhum estado do problema.
/// </summary>
public static class Logger
{
    private static readonly object _lockConsole = new();
    private static readonly Stopwatch _relogio = new();

    public static void Iniciar() => _relogio.Restart();

    public static void Log(string autor, string mensagem, ConsoleColor? cor = null)
    {
        TimeSpan t = _relogio.Elapsed;
        string horario = $"{(int)t.TotalMinutes:00}:{t.Seconds:00}.{t.Milliseconds:000}";
        string linha = $"[{horario}] {autor,-10} · {mensagem}  (T{Environment.CurrentManagedThreadId})";

        lock (_lockConsole)
        {
            if (cor is null)
            {
                Console.WriteLine(linha);
                return;
            }
            Console.ForegroundColor = cor.Value;
            Console.WriteLine(linha);
            Console.ResetColor();
        }
    }
}
