using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;
    public PedidosController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public IActionResult GetPedidos()
    {
        var retorno = _context.Pedidos.AsNoTracking().Select(p => new {
            p.Id,
            p.Data,
            p.Status,
            p.Total,
            p.Itens,
            ItensCount = p.Itens.Count
        });

        return Ok(retorno);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePedido(Guid id, CancellationToken token)
    {
        var pedidoAtual = await _context.Pedidos
                                .Where(p => p.Id == id)
                                .Include(x => x.Itens)
                                .FirstOrDefaultAsync(token);

        if(pedidoAtual is null)
            return NotFound();

        _context.Pedidos.Remove(pedidoAtual);
        await _context.SaveChangesAsync(token);
        
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> CriarPedido([FromBody] PedidoDto dto, [FromServices] IPedidoService pedidoService, CancellationToken token)
    {
        // Adicionar FluentValidation
        // Criar baseController para simplicar os retornos

        var response = await pedidoService.CriarPedidoAsync(dto, token);
        if(!response.IsSuccess)
            return BadRequest(response.Message);

        return Created();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> AtualizarPedido([FromBody] PedidoDto dto)
    {
        return Ok();
    }
}