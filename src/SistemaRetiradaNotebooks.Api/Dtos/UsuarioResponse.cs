using SistemaRetiradaNotebooks.Api.Models;

namespace SistemaRetiradaNotebooks.Api.Dtos;

public record UsuarioResponse(int Id, string Nome, string Email, PerfilUsuario Perfil, StatusCadastro Status);
