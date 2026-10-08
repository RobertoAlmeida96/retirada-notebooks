import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { UsuariosService } from '../../services/usuarios.service';
import { UsuarioResponse } from '../../models';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [],
  templateUrl: './admin.html',
  styleUrl: './admin.scss'
})
export class AdminComponent implements OnInit {
  private service = inject(UsuariosService);
  private cdr = inject(ChangeDetectorRef);

  usuarios: UsuarioResponse[] = [];
  carregando = true;

  get pendentes() { return this.usuarios.filter(u => u.status === 'PendenteAprovacao'); }
  get ativos() { return this.usuarios.filter(u => u.status === 'Ativo'); }
  get recusados() { return this.usuarios.filter(u => u.status === 'Recusado'); }

  ngOnInit() { this.carregar(); }

  carregar() {
    this.carregando = true;
    this.service.listar().subscribe({
      next: (data) => { this.usuarios = data; this.carregando = false; this.cdr.detectChanges(); },
      error: () => { this.carregando = false; this.cdr.detectChanges(); }
    });
  }

  aprovar(id: number) {
    this.service.aprovar(id).subscribe({
      next: () => this.carregar(),
      error: () => { alert('Erro ao aprovar.'); this.cdr.detectChanges(); }
    });
  }

  recusar(id: number) {
    this.service.recusar(id).subscribe({
      next: () => this.carregar(),
      error: () => { alert('Erro ao recusar.'); this.cdr.detectChanges(); }
    });
  }

  labelStatus(status: string) {
    const map: Record<string, string> = {
      'PendenteAprovacao': 'Pendente',
      'Ativo': 'Ativo',
      'Recusado': 'Recusado'
    };
    return map[status] ?? status;
  }

  badgeStatus(status: string) {
    const map: Record<string, string> = {
      'PendenteAprovacao': 'badge-yellow',
      'Ativo': 'badge-green',
      'Recusado': 'badge-red'
    };
    return map[status] ?? 'badge-gray';
  }
}
