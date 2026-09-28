namespace Interfaces;

public interface IPedidoService
{
    Task<Result> CriarPedidoAsync(PedidoDto dto, CancellationToken token);
}