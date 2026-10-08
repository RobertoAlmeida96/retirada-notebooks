using Microsoft.EntityFrameworkCore;
using SistemaRetiradaNotebooks.Api.Models;

namespace SistemaRetiradaNotebooks.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Carrinho> Carrinhos => Set<Carrinho>();
    public DbSet<Notebook> Notebooks => Set<Notebook>();
    public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
    public DbSet<SolicitacaoNotebook> SolicitacoesNotebooks => Set<SolicitacaoNotebook>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tabela de ligação N-para-N sem Id próprio: a chave é o par de FKs.
        modelBuilder.Entity<SolicitacaoNotebook>()
            .HasKey(sn => new { sn.SolicitacaoId, sn.NotebookId });
    }
}
