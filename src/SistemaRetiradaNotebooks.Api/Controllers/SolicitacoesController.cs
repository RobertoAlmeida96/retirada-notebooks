using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRetiradaNotebooks.Api.Data;
using SistemaRetiradaNotebooks.Api.Dtos;
using SistemaRetiradaNotebooks.Api.Models;

namespace SistemaRetiradaNotebooks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolicitacoesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SolicitacoesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Solicitacao>>> Listar()
    {
        return await _context.Solicitacoes.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Solicitacao>> ObterPorId(int id)
    {
        var solicitacao = await _context.Solicitacoes.FindAsync(id);

        if (solicitacao is null)
            return NotFound();

        return solicitacao;
    }

    [HttpPost]
    public async Task<ActionResult<Solicitacao>> Criar(CriarSolicitacaoRequest request)
    {
        if (request.Quantidade <= 0)
            return BadRequest("A quantidade deve ser maior que zero.");

        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == request.UsuarioId);
        if (!usuarioExiste)
            return BadRequest("Usuário não encontrado.");

        var solicitacao = new Solicitacao
        {
            UsuarioId = request.UsuarioId,
            Quantidade = request.Quantidade,
            AulasPrevistas = request.AulasPrevistas,
            DataHoraSolicitacao = DateTime.UtcNow,
            Status = StatusSolicitacao.Pendente
        };

        _context.Solicitacoes.Add(solicitacao);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = solicitacao.Id }, solicitacao);
    }

    [HttpPost("{id}/aprovar")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Solicitacao>> Aprovar(int id, AprovarSolicitacaoRequest request)
    {
        if (request.NotebookIds is null || request.NotebookIds.Count == 0)
            return BadRequest("Informe ao menos um notebook.");

        var solicitacao = await _context.Solicitacoes.FindAsync(id);
        if (solicitacao is null)
            return NotFound();

        var idsDistintos = request.NotebookIds.Distinct().ToList();
        var notebooks = await _context.Notebooks
            .Where(n => idsDistintos.Contains(n.Id) && n.Status == StatusNotebook.Disponivel)
            .ToListAsync();

        if (notebooks.Count != idsDistintos.Count)
            return BadRequest("Um ou mais notebooks não encontrados ou não disponíveis.");

        try
        {
            solicitacao.Aprovar();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        foreach (var notebook in notebooks)
        {
            notebook.Status = StatusNotebook.Emprestado;
            _context.SolicitacoesNotebooks.Add(new SolicitacaoNotebook
            {
                SolicitacaoId = solicitacao.Id,
                NotebookId = notebook.Id
            });
        }

        await _context.SaveChangesAsync();

        return Ok(solicitacao);
    }

    [HttpPost("{id}/devolver")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Solicitacao>> Devolver(int id)
    {
        var solicitacao = await _context.Solicitacoes.FindAsync(id);
        if (solicitacao is null)
            return NotFound();

        try
        {
            solicitacao.Devolver();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        await AtualizarStatusNotebooks(id, StatusNotebook.Disponivel);
        await _context.SaveChangesAsync();

        return Ok(solicitacao);
    }

    [HttpPost("{id}/recusar")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Solicitacao>> Recusar(int id, RecusarSolicitacaoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            return BadRequest("Informe o motivo da recusa.");

        var solicitacao = await _context.Solicitacoes.FindAsync(id);
        if (solicitacao is null)
            return NotFound();

        try
        {
            solicitacao.Recusar(request.Motivo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        await _context.SaveChangesAsync();

        return Ok(solicitacao);
    }

    private async Task AtualizarStatusNotebooks(int solicitacaoId, StatusNotebook status)
    {
        var notebookIds = await _context.SolicitacoesNotebooks
            .Where(sn => sn.SolicitacaoId == solicitacaoId)
            .Select(sn => sn.NotebookId)
            .ToListAsync();

        var notebooks = await _context.Notebooks
            .Where(n => notebookIds.Contains(n.Id))
            .ToListAsync();

        foreach (var notebook in notebooks)
            notebook.Status = status;
    }
}
