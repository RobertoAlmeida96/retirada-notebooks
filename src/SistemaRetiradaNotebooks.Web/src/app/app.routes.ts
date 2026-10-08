import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'cadastro',
    loadComponent: () => import('./pages/cadastro/cadastro').then(m => m.CadastroComponent)
  },
  {
    path: '',
    loadComponent: () => import('./components/shell/shell').then(m => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'notebooks', pathMatch: 'full' },
      {
        path: 'notebooks',
        loadComponent: () => import('./pages/notebooks/notebooks').then(m => m.NotebooksComponent)
      },
      {
        path: 'solicitacoes',
        loadComponent: () => import('./pages/solicitacoes/solicitacoes').then(m => m.SolicitacoesComponent)
      },
      {
        path: 'admin',
        loadComponent: () => import('./pages/admin/admin').then(m => m.AdminComponent),
        canActivate: [adminGuard]
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
