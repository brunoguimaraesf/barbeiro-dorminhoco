namespace BarbeiroDorminhoco.Modelo;

/// <summary>
/// Contrato do recurso compartilhado. Cada solução (ingênuas e ideal) implementa esta interface,
/// enquanto Cliente e Barbeiro (as Tasks) permanecem iguais em todas elas.
/// </summary>
public interface IBarbearia
{
    string Nome { get; }
    Estatisticas Stats { get; }

    /// <summary>Cliente tenta sentar numa cadeira de espera. Retorna false se desistiu.</summary>
    bool Entrar(Cliente cliente);

    /// <summary>Cliente que sentou aguarda até o corte terminar.</summary>
    void AguardarCorte(Cliente cliente, CancellationToken ct);

    /// <summary>Barbeiro pega o próximo cliente (dorme se não houver). Null = expediente acabou.</summary>
    Cliente? ProximoCliente(CancellationToken ct);

    /// <summary>Barbeiro avisa que terminou o corte do cliente.</summary>
    void FinalizarCorte(Cliente cliente);
}
