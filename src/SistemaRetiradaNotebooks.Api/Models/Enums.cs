namespace SistemaRetiradaNotebooks.Api.Models;

public enum PerfilUsuario
{
    Comum,
    Admin
}

public enum StatusCadastro
{
    PendenteAprovacao,
    Ativo,
    Recusado
}

public enum StatusNotebook
{
    Disponivel,
    Emprestado,
    EmManutencao,
    ManutencaoSemGarantia,
    Quebrado,
    ChamadoAberto,
    UsoAdministrativo,
    Furtado
}

public enum StatusSolicitacao
{
    Pendente,
    Recusada,
    EmUso,
    Devolvida
}
