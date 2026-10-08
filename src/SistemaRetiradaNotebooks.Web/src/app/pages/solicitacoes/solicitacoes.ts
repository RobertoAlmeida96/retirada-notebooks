import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { SolicitacoesService } from '../../services/solicitacoes.service';
import { NotebooksService } from '../../services/notebooks.service';
import { CarrinhoComNotebooks, Notebook, Solicitacao } from '../../models';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-solicitacoes',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './solicitacoes.html',
  styleUrl: './solicitacoes.scss'
})
export class SolicitacoesComponent implements OnInit {
  private service = inject(SolicitacoesService);
  private notebooksService = inject(NotebooksService);
  private auth = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  solicitacoes: Solicitacao[] = [];
  isAdmin = this.auth.isAdmin();
  usuario = this.auth.getUsuario();
  carregando = true;

  // Recusa
  motivoRecusa: Record<number, string> = {};
  mostrandoMotivoId: number | null = null;

  // Aprovação com seleção de notebooks
  aprovandoId: number | null = null;
  carrinhos: CarrinhoComNotebooks[] = [];
  notebooksSelecionados = new Set<number>();
  erroAprovacao = '';

  ngOnInit() { this.carregar(); }

  carregar() {
    this.carregando = true;
    this.service.listar().subscribe({
      next: (data) => {
        this.solicitacoes = this.isAdmin
          ? data
          : data.filter(s => s.usuarioId === this.usuario!.id);
        this.carregando = false;
        this.cdr.detectChanges();
      },
      error: () => { this.carregando = false; this.cdr.detectChanges(); }
    });
  }

  abrirAprovacao(s: Solicitacao) {
    this.aprovandoId = s.id;
    this.notebooksSelecionados.clear();
    this.erroAprovacao = '';
    this.notebooksService.porCarrinho().subscribe({
      next: (data) => { this.carrinhos = data; this.cdr.detectChanges(); },
      error: () => { this.erroAprovacao = 'Erro ao carregar notebooks.'; this.cdr.detectChanges(); }
    });
  }

  toggleNotebook(id: number) {
    if (this.notebooksSelecionados.has(id)) {
      this.notebooksSelecionados.delete(id);
    } else {
      this.notebooksSelecionados.add(id);
    }
  }

  get solicitacaoAtual(): Solicitacao | undefined {
    return this.solicitacoes.find(s => s.id === this.aprovandoId);
  }

  confirmarAprovacao() {
    if (!this.aprovandoId) return;
    const solicitacao = this.solicitacaoAtual;
    if (solicitacao && this.notebooksSelecionados.size !== solicitacao.quantidade) {
      this.erroAprovacao = `Selecione exatamente ${solicitacao.quantidade} notebook(s).`;
      this.cdr.detectChanges();
      return;
    }
    this.service.aprovar(this.aprovandoId, { notebookIds: Array.from(this.notebooksSelecionados) }).subscribe({
      next: () => { this.aprovandoId = null; this.carregar(); },
      error: (e) => { this.erroAprovacao = e.error || 'Erro ao aprovar.'; this.cdr.detectChanges(); }
    });
  }

  devolver(id: number) {
    this.service.devolver(id).subscribe({
      next: () => this.carregar(),
      error: (e) => { alert(e.error || 'Erro ao devolver.'); this.cdr.detectChanges(); }
    });
  }

  recusar(id: number) {
    const motivo = this.motivoRecusa[id]?.trim();
    if (!motivo) { alert('Informe o motivo da recusa.'); return; }
    this.service.recusar(id, motivo).subscribe({
      next: () => { this.mostrandoMotivoId = null; this.carregar(); },
      error: (e) => { alert(e.error || 'Erro ao recusar.'); this.cdr.detectChanges(); }
    });
  }

  notebooksDisponiveis(carrinho: CarrinhoComNotebooks): Notebook[] {
    return carrinho.notebooks.filter(n => n.status === 'Disponivel');
  }

  badgeSolicitacao(status: string) {
    const map: Record<string, string> = {
      'Pendente':  'badge-yellow',
      'EmUso':     'badge-blue',
      'Devolvida': 'badge-green',
      'Recusada':  'badge-red'
    };
    return map[status] ?? 'badge-gray';
  }

  labelSolicitacao(status: string) {
    const map: Record<string, string> = {
      'Pendente':  'Pendente',
      'EmUso':     'Em uso',
      'Devolvida': 'Devolvida',
      'Recusada':  'Recusada'
    };
    return map[status] ?? status;
  }

  formatarData(data?: string) {
    if (!data) return '—';
    return new Date(data).toLocaleString('pt-BR');
  }
}
