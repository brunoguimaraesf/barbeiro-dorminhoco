namespace BarbeiroDorminhoco;

/// <summary>
/// Requisito 1: todo o cenário (threads, capacidades e tempos) é definido aqui, em um único lugar.
/// </summary>
public static class Config
{
    // Cenário do enunciado: 1 barbeiro, 3 cadeiras de espera e 20 clientes.
    public const int TotalClientes = 20;
    public const int CadeirasEspera = 3;

    // Chegadas em duas levas, cada uma com atraso aleatório (Random.Shared) nestas faixas:
    // uma rajada no começo (lota as cadeiras) e poucos retardatários depois (o barbeiro fica ocioso).
    public const int ClientesNaRajada = 15;
    public const int ChegadaMinMs = 0;
    public const int ChegadaMaxMs = 1200;
    public const int ChegadaTardiaMinMs = 3000;
    public const int ChegadaTardiaMaxMs = 7000;

    // Duração aleatória de cada corte de cabelo.
    public const int CorteMinMs = 200;
    public const int CorteMaxMs = 400;

    // Janela de erro: pausa entre "ler" (há cadeira livre?) e "gravar" (sentar).
    // Alarga a race condition para ela aparecer em toda execução.
    public const int JanelaErroMs = 250;

    // Janela do lost wakeup (Ingênua 2): pausa entre "vi a fila vazia" e "fui dormir".
    public const int JanelaLostWakeupMs = 800;

    // Intervalo de verificação usado nas esperas ocupadas das soluções ingênuas.
    public const int PollMs = 5;

    // Prazo máximo da simulação; evita que uma solução ingênua trave o programa para sempre.
    public const int PrazoSegundos = 15;
}
