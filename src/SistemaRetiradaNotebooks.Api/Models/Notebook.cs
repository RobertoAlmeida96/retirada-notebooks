namespace SistemaRetiradaNotebooks.Api.Models;

public class Notebook
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public required string Modelo { get; set; }
    public string? NumeroSerie { get; set; }
    public required string Patrimonio { get; set; }
    public string? Garantia { get; set; }
    public string? NotaFiscal { get; set; }
    public string? Observacoes { get; set; }
    public StatusNotebook Status { get; set; }
    public int CarrinhoId { get; set; }
}
