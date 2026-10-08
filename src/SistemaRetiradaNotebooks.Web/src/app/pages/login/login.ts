import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required]
  });

  erro = '';
  carregando = false;

  entrar() {
    if (this.form.invalid || this.carregando) return;
    this.carregando = true;
    this.erro = '';
    const { email, senha } = this.form.value;

    this.auth.login({ email: email!, senha: senha! }).subscribe({
      next: () => this.router.navigate(['/notebooks']),
      error: (err) => {
        this.erro = err.status === 401 ? 'Email ou senha incorretos.' :
                    err.status === 403 ? 'Cadastro pendente de aprovação pelo administrador.' :
                    'Erro ao conectar. Verifique se a API está rodando.';
        this.carregando = false;
      }
    });
  }
}
