using BarbeiroDorminhoco.Modelo;
using BarbeiroDorminhoco.Solucoes;

namespace BarbeiroDorminhoco;

/// <summary>Menu do console: uma opção por solução. Também aceita a opção como argumento (dotnet run -- 2).</summary>
public static class Menu
{
    private sealed record Opcao(string Numero, string Titulo, Func<IBarbearia>? Criar);

    private static readonly Opcao[] Opcoes =
    [
        new("1", "Ingênua 1 · sem controle de concorrência (race condition)", () => new BarbeariaIngenua1()),
        new("2", "Ingênua 2 · lock + flag de sono (lost wakeup)", () => new BarbeariaIngenua2()),
        new("3", "Ingênua 3 · laço sem bloquear (espera ocupada)", () => new BarbeariaIngenua3()),
        new("4", "Solução ideal · SemaphoreSlim + lock (Entrega 3, ainda não implementada)", null),
        new("5", "Comparar desempenho (Entrega 3, ainda não implementada)", null),
    ];

    public static void Executar(string[] args)
    {
        // Modo direto: dotnet run -- 1
        if (args.Length > 0)
        {
            Rodar(args[0]);
            return;
        }

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== BARBEIRO DORMINHOCO =====");
            foreach (Opcao o in Opcoes)
                Console.WriteLine($"  {o.Numero}) {o.Titulo}");
            Console.WriteLine("  0) Sair");
            Console.Write("Escolha: ");

            string? entrada = Console.ReadLine()?.Trim();
            if (entrada is null || entrada == "0")
                return;

            Console.WriteLine();
            Rodar(entrada);
        }
    }

    private static void Rodar(string numero)
    {
        Opcao? opcao = Array.Find(Opcoes, o => o.Numero == numero);
        if (opcao is null)
        {
            Console.WriteLine("Opção inválida.");
        }
        else if (opcao.Criar is null)
        {
            Console.WriteLine("Esta opção ainda não foi implementada (vem na Entrega 3).");
        }
        else
        {
            Simulacao.Executar(opcao.Criar());
        }
    }
}
