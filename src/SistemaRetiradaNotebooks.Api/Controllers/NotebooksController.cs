using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRetiradaNotebooks.Api.Data;
using SistemaRetiradaNotebooks.Api.Models;

namespace SistemaRetiradaNotebooks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotebooksController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotebooksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notebook>>> Listar()
    {
        return await _context.Notebooks.OrderBy(n => n.Numero).ToListAsync();
    }

    [HttpGet("por-carrinho")]
    public async Task<IActionResult> PorCarrinho()
    {
        var resultado = await _context.Carrinhos
            .OrderBy(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.Nome,
                Notebooks = _context.Notebooks
                    .Where(n => n.CarrinhoId == c.Id)
                    .OrderBy(n => n.Numero)
                    .ToList()
            })
            .ToListAsync();

        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Notebook>> ObterPorId(int id)
    {
        var notebook = await _context.Notebooks.FindAsync(id);

        if (notebook is null)
            return NotFound();

        return notebook;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Notebook>> Criar(Notebook notebook)
    {
        _context.Notebooks.Add(notebook);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = notebook.Id }, notebook);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, Notebook notebook)
    {
        if (id != notebook.Id)
            return BadRequest();

        var existe = await _context.Notebooks.AnyAsync(n => n.Id == id);
        if (!existe)
            return NotFound();

        _context.Entry(notebook).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Remover(int id)
    {
        var notebook = await _context.Notebooks.FindAsync(id);

        if (notebook is null)
            return NotFound();

        _context.Notebooks.Remove(notebook);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
