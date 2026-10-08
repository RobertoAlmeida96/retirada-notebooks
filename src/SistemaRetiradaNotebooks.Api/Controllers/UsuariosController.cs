using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRetiradaNotebooks.Api.Data;
using SistemaRetiradaNotebooks.Api.Dtos;
using SistemaRetiradaNotebooks.Api.Models;

namespace SistemaRetiradaNotebooks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioResponse>>> Listar()
    {
        return await _context.Usuarios
            .Select(u => new UsuarioResponse(u.Id, u.Nome, u.Email, u.Perfil, u.Status))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioResponse>> ObterPorId(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario is null)
            return NotFound();

        return ParaResponse(usuario);
    }

    [HttpPut("{id}/aprovar")]
    public async Task<ActionResult<UsuarioResponse>> Aprovar(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
            return NotFound();

        usuario.Status = StatusCadastro.Ativo;
        await _context.SaveChangesAsync();

        return ParaResponse(usuario);
    }

    [HttpPut("{id}/recusar")]
    public async Task<ActionResult<UsuarioResponse>> Recusar(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
            return NotFound();

        usuario.Status = StatusCadastro.Recusado;
        await _context.SaveChangesAsync();

        return ParaResponse(usuario);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario is null)
            return NotFound();

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static UsuarioResponse ParaResponse(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.Perfil, u.Status);
}
