using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRetiradaNotebooks.Api.Data;
using SistemaRetiradaNotebooks.Api.Models;

namespace SistemaRetiradaNotebooks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarrinhosController : ControllerBase
{
    private readonly AppDbContext _context;

    public CarrinhosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Carrinho>>> Listar()
    {
        return await _context.Carrinhos.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Carrinho>> ObterPorId(int id)
    {
        var carrinho = await _context.Carrinhos.FindAsync(id);

        if (carrinho is null)
            return NotFound();

        return carrinho;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Carrinho>> Criar(Carrinho carrinho)
    {
        _context.Carrinhos.Add(carrinho);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = carrinho.Id }, carrinho);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, Carrinho carrinho)
    {
        if (id != carrinho.Id)
            return BadRequest();

        var existe = await _context.Carrinhos.AnyAsync(c => c.Id == id);
        if (!existe)
            return NotFound();

        _context.Entry(carrinho).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Remover(int id)
    {
        var carrinho = await _context.Carrinhos.FindAsync(id);

        if (carrinho is null)
            return NotFound();

        _context.Carrinhos.Remove(carrinho);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
