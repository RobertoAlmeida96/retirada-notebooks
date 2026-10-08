namespace SistemaRetiradaNotebooks.Api.Models;

public class Solicitacao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime DataHoraSolicitacao { get; set; }
    public int Quantidade { get; set; }
    public int AulasPrevistas { get; set; }
    public StatusSolicitacao Status { get; set; }
    public string? MotivoRecusa { get; set; }
    public DateTime? DataHoraRetirada { get; set; }
    public DateTime? DataHoraDevolucao { get; set; }

    public DateTime? PrevisaoDevolucao =>
        DataHoraRetirada == null ? null : DataHoraRetirada.Value.AddMinutes(AulasPrevistas * 50);

    public bool EstaAtrasada =>
        PrevisaoDevolucao != null && Status == StatusSolicitacao.EmUso && DateTime.UtcNow > PrevisaoDevolucao.Value;

    public void Aprovar()
    {
        if (Status == StatusSolicitacao.Pendente)
        {
            Status = StatusSolicitacao.EmUso;
            DataHoraRetirada = DateTime.UtcNow;
        }
        else
        {
            throw new InvalidOperationException("A transação não pode ser concluída, porque o computador já está em uso.");
        }
    }

    public void Devolver()
    {
        if (Status == StatusSolicitacao.EmUso)
        {
            Status = StatusSolicitacao.Devolvida;
            DataHoraDevolucao = DateTime.UtcNow;
        }
        else
        {
            throw new InvalidOperationException("O computador não está em uso, não é possível devolver a máquina.");
        }
    }

    public void Recusar(string motivo)
    {
        if (Status == StatusSolicitacao.Pendente)
        {
            Status = StatusSolicitacao.Recusada;
            MotivoRecusa = motivo;
        }
        else
        {
            throw new InvalidOperationException("A solicitação não pode ser recusada, pois já foi aprovada ou recusada.");
        }
    }
}
