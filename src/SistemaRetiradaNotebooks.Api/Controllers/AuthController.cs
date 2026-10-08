using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRetiradaNotebooks.Api.Data;
using SistemaRetiradaNotebooks.Api.Dtos;
using SistemaRetiradaNotebooks.Api.Models;
using SistemaRetiradaNotebooks.Api.Services;

namespace SistemaRetiradaNotebooks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;
    private readonly PasswordHasher<Usuario> _hasher = new();

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("registrar")]
    public async Task<ActionResult<UsuarioResponse>> Registrar(RegistrarRequest request)
    {
        var emailEmUso = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
        if (emailEmUso)
            return BadRequest("E-mail já cadastrado.");

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            SenhaHash = "",
            Perfil = PerfilUsuario.Comum,
            Status = StatusCadastro.PendenteAprovacao
        };
        usuario.SenhaHash = _hasher.HashPassword(usuario, request.Senha);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Registrar), ParaResponse(usuario));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario is null)
            return Unauthorized("E-mail ou senha inválidos.");

        var resultado = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);
        if (resultado == PasswordVerificationResult.Failed)
            return Unauthorized("E-mail ou senha inválidos.");

        if (usuario.Status != StatusCadastro.Ativo)
            return StatusCode(StatusCodes.Status403Forbidden, "Cadastro ainda não aprovado.");

        var token = _tokenService.GerarToken(usuario);
        return new LoginResponse(token, ParaResponse(usuario));
    }

    private static UsuarioResponse ParaResponse(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.Perfil, u.Status);
}
