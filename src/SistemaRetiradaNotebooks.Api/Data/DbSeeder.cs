using Microsoft.EntityFrameworkCore;
using SistemaRetiradaNotebooks.Api.Models;

namespace SistemaRetiradaNotebooks.Api.Data;

/// <summary>
/// Popula o banco com dados de exemplo na primeira execução.
/// Os notebooks abaixo são FICTÍCIOS, apenas para demonstração.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Carrinhos.AnyAsync()) return;

        var carrinhos = new[]
        {
            new Carrinho { Id = 1, Nome = "Carrinho 1" },
            new Carrinho { Id = 2, Nome = "Carrinho 2" },
            new Carrinho { Id = 3, Nome = "Carrinho 3" },
        };
        db.Carrinhos.AddRange(carrinhos);

        // Dados de exemplo (fictícios). Em produção, o inventário real é cadastrado pela tela de admin.
        var notebooks = new[]
        {
            // Carrinho 1 — modelo de exemplo A
            N(1, "Notebook Sala de Aula — Modelo Exemplo A", "SN-EXEMPLO-0001", "PAT-0001", "dez/27", "NF-1000", 1),
            N(2, "Notebook Sala de Aula — Modelo Exemplo A", "SN-EXEMPLO-0002", "PAT-0002", "dez/27", "NF-1000", 1),
            N(3, "Notebook Sala de Aula — Modelo Exemplo A", "SN-EXEMPLO-0003", "PAT-0003", "dez/27", "NF-1000", 1),
            N(4, "Notebook Sala de Aula — Modelo Exemplo A", "SN-EXEMPLO-0004", "PAT-0004", "dez/27", "NF-1000", 1, StatusNotebook.EmManutencao, "Em manutenção (exemplo)"),

            // Carrinho 2 — modelo de exemplo B
            N(5, "Notebook Educacional — Modelo Exemplo B", "SN-EXEMPLO-0005", "PAT-0005", "fev/28", "NF-2000", 2),
            N(6, "Notebook Educacional — Modelo Exemplo B", "SN-EXEMPLO-0006", "PAT-0006", "fev/28", "NF-2000", 2),
            N(7, "Notebook Educacional — Modelo Exemplo B", "SN-EXEMPLO-0007", "PAT-0007", "fev/28", "NF-2000", 2, StatusNotebook.ChamadoAberto, "Chamado aberto (exemplo)"),

            // Carrinho 3 — modelo de exemplo C
            N(8,  "Notebook Básico — Modelo Exemplo C", "SN-EXEMPLO-0008", "PAT-0008", "abr/30", "NF-3000", 3),
            N(9,  "Notebook Básico — Modelo Exemplo C", "SN-EXEMPLO-0009", "PAT-0009", "abr/30", "NF-3000", 3),
            N(10, "Notebook Básico — Modelo Exemplo C", "SN-EXEMPLO-0010", "PAT-0010", "abr/30", "NF-3000", 3, StatusNotebook.UsoAdministrativo, "Uso administrativo (exemplo)"),
        };
        db.Notebooks.AddRange(notebooks);
        await db.SaveChangesAsync();
    }

    private static Notebook N(
        int numero, string modelo, string? serie, string? patrimonio,
        string garantia, string nf, int carrinhoId,
        StatusNotebook status = StatusNotebook.Disponivel,
        string? obs = null)
        => new()
        {
            Numero      = numero,
            Modelo      = modelo,
            NumeroSerie = serie,
            Patrimonio  = patrimonio ?? "",
            Garantia    = garantia,
            NotaFiscal  = nf,
            CarrinhoId  = carrinhoId,
            Status      = status,
            Observacoes = obs
        };
}
