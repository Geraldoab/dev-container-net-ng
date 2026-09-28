
using Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Services;

public sealed class PedidoService(AppDbContext context) : IPedidoService // Remover context e criar um repositório genérico
{
    public async Task<Result> CriarPedidoAsync(PedidoDto dto, CancellationToken token)
    {
        if (dto.Itens == null || dto.Itens.Count == 0) // Implementar result pattern
            return new Result(false, "Não foi possível criar o pedido. Por favor tente novamente.");

        var itensCadastradosResponse = await ExisteItensCadastradosAsync(dto, token);
        if(!itensCadastradosResponse.IsSuccess)
           return itensCadastradosResponse; 

        var itens = dto.Itens.Select(i => new ItemPedido
            {
                Id = i.Id,
                Nome = i.Nome,
                Preco = i.Preco
            }).ToList();

        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            Data = new DateTime(),
            Status = StatusPedido.NoCarrinho,
            DescontoPercentual = dto.DescontoPercentual,
            Itens = itens,
            Total = itens.Sum(i => i.Preco) - dto.DescontoPercentual
        };

        await context.Pedidos.AddAsync(pedido);
        await context.SaveChangesAsync();


        // 7(EXTRA) Evolução arquitetural: 
        // INotificationService.Send
        /*
        - Outbox Pattern
        -- Salva o pedido
        -- Salva a mensagem a ser enviada em uma tabela de Mensagens

        -- Worker para processar as mensagens (Inbox Pattern utilizando o Id da mensagem como primary key)
        -- Worker para limpar as mensagens antigas da tabela Inbox
        */

        return new Result(true, "Pedido cadastrado com sucesso."); // Implementar Global Exception Handler
    }

    private async Task<Result> ExisteItensCadastradosAsync(PedidoDto dto, CancellationToken token)
    {
        var itemIds = dto.Itens.Select(i => i.Id).ToList();
        var temTodosItens = await context.Produtos.AllAsync(p => itemIds.Contains(p.Id), token);

        return temTodosItens 
        ? new Result(true, string.Empty) 
        : new Result(false, "Alguns itens não foram encontrados no sistema, por favor verifique os itens do pedido.");
    }
}