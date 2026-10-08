import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { NotebooksService } from '../../services/notebooks.service';
import { SolicitacoesService } from '../../services/solicitacoes.service';
import { CarrinhoComNotebooks, Notebook, StatusNotebook } from '../../models';

@Component({
  selector: 'app-notebooks',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './notebooks.html',
  styleUrl: './notebooks.scss'
})
export class NotebooksComponent implements OnInit {
  private notebooksService = inject(NotebooksService);
  private solicitacoesService = inject(SolicitacoesService);
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  carrinhos: CarrinhoComNotebooks[] = [];
  isAdmin = this.auth.isAdmin();
  usuario = this.auth.getUsuario();
  carregando = true;

  // Formulário de solicitação (usuário)
  mostrarFormSolicitacao = false;
  quantidade = 1;
  aulasPrevistas = 2;
  erroSolicitacao = '';
  sucessoSolicitacao = '';

  // Formulário de notebook (admin)
  mostrarFormNotebook = false;
  editando: Notebook | null = null;
  notebookForm = this.fb.group({
    numero:      [0],
    modelo:      ['', Validators.required],
    numeroSerie: [''],
    patrimonio:  ['', Validators.required],
    garantia:    [''],
    notaFiscal:  [''],
    observacoes: [''],
    status:      ['Disponivel' as StatusNotebook],
    carrinhoId:  [0]
  });

  readonly statusOpcoes: { valor: StatusNotebook; label: string }[] = [
    { valor: 'Disponivel',          label: 'Disponível' },
    { valor: 'Emprestado',          label: 'Emprestado' },
    { valor: 'EmManutencao',        label: 'Em Manutenção' },
    { valor: 'ManutencaoSemGarantia', label: 'Manutenção Sem Garantia' },
    { valor: 'Quebrado',            label: 'Quebrado' },
    { valor: 'ChamadoAberto',       label: 'Chamado Aberto' },
    { valor: 'UsoAdministrativo',   label: 'Uso Administrativo' },
    { valor: 'Furtado',             label: 'Furtado' },
  ];

  ngOnInit() { this.carregar(); }

  carregar() {
    this.carregando = true;
    this.notebooksService.porCarrinho().subscribe({
      next: (data) => { this.carrinhos = data; this.carregando = false; this.cdr.detectChanges(); },
      error: () => { this.carregando = false; this.cdr.detectChanges(); }
    });
  }

  get totalDisponiveis() {
    return this.carrinhos.reduce((acc, c) =>
      acc + c.notebooks.filter(n => n.status === 'Disponivel').length, 0);
  }

  contarDisponiveis(notebooks: Notebook[]) {
    return notebooks.filter(n => n.status === 'Disponivel').length;
  }

  // Admin: editar notebook
  abrirFormEditar(n: Notebook) {
    this.editando = n;
    this.notebookForm.setValue({
      numero:      n.numero,
      modelo:      n.modelo,
      numeroSerie: n.numeroSerie ?? '',
      patrimonio:  n.patrimonio,
      garantia:    n.garantia ?? '',
      notaFiscal:  n.notaFiscal ?? '',
      observacoes: n.observacoes ?? '',
      status:      n.status,
      carrinhoId:  n.carrinhoId
    });
    this.mostrarFormNotebook = true;
  }

  salvarNotebook() {
    if (this.notebookForm.invalid || !this.editando) return;
    const v = this.notebookForm.value;
    const atualizado: Notebook = { ...this.editando, ...v as any };
    this.notebooksService.atualizar(atualizado).subscribe({
      next: () => { this.mostrarFormNotebook = false; this.carregar(); },
      error: () => alert('Erro ao atualizar notebook.')
    });
  }

  removerNotebook(id: number) {
    if (!confirm('Remover este notebook?')) return;
    this.notebooksService.remover(id).subscribe({
      next: () => this.carregar(),
      error: () => { alert('Erro ao remover notebook.'); this.cdr.detectChanges(); }
    });
  }

  // Usuário: solicitar
  enviarSolicitacao() {
    if (this.quantidade <= 0) {
      this.erroSolicitacao = 'Informe a quantidade.';
      return;
    }
    if (this.quantidade > this.totalDisponiveis) {
      this.erroSolicitacao = `Apenas ${this.totalDisponiveis} notebook(s) disponível(is) no momento.`;
      return;
    }
    this.erroSolicitacao = '';

    this.solicitacoesService.criar({
      usuarioId: this.usuario!.id,
      quantidade: this.quantidade,
      aulasPrevistas: this.aulasPrevistas
    }).subscribe({
      next: () => {
        this.sucessoSolicitacao = 'Solicitação enviada! Aguarde a aprovação do administrador.';
        this.mostrarFormSolicitacao = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.erroSolicitacao = err.error || 'Erro ao enviar solicitação.';
        this.cdr.detectChanges();
      }
    });
  }

  badgeNotebook(status: StatusNotebook) {
    const map: Record<StatusNotebook, string> = {
      'Disponivel':           'badge-green',
      'Emprestado':           'badge-blue',
      'EmManutencao':         'badge-yellow',
      'ManutencaoSemGarantia':'badge-yellow',
      'Quebrado':             'badge-red',
      'ChamadoAberto':        'badge-orange',
      'UsoAdministrativo':    'badge-gray',
      'Furtado':              'badge-red',
    };
    return map[status] ?? 'badge-gray';
  }

  labelNotebook(status: StatusNotebook) {
    const map: Record<StatusNotebook, string> = {
      'Disponivel':           'Disponível',
      'Emprestado':           'Emprestado',
      'EmManutencao':         'Em Manutenção',
      'ManutencaoSemGarantia':'Manut. S/ Garantia',
      'Quebrado':             'Quebrado',
      'ChamadoAberto':        'Chamado Aberto',
      'UsoAdministrativo':    'Uso Admin.',
      'Furtado':              'Furtado',
    };
    return map[status] ?? status;
  }
}
