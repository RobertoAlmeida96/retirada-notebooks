import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './cadastro.html',
  styleUrl: './cadastro.scss'
})
export class CadastroComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  form = this.fb.group({
    nome: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(6)]]
  });

  erro = '';
  sucesso = false;
  carregando = false;

  cadastrar() {
    if (this.form.invalid) return;
    this.carregando = true;
    this.erro = '';
    const { nome, email, senha } = this.form.value;

    this.auth.registrar({ nome: nome!, email: email!, senha: senha! }).subscribe({
      next: () => { this.sucesso = true; this.carregando = false; this.cdr.detectChanges(); },
      error: (err) => {
        this.erro = err.status === 409 ? 'Este email já está cadastrado.' : 'Erro ao realizar cadastro.';
        this.carregando = false;
        this.cdr.detectChanges();
      }
    });
  }
}
