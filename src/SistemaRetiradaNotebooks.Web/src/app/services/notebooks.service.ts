import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { CarrinhoComNotebooks, Notebook } from '../models';

@Injectable({ providedIn: 'root' })
export class NotebooksService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/notebooks`;

  listar() { return this.http.get<Notebook[]>(this.url); }

  porCarrinho() { return this.http.get<CarrinhoComNotebooks[]>(`${this.url}/por-carrinho`); }

  criar(notebook: Omit<Notebook, 'id'>) {
    return this.http.post<Notebook>(this.url, notebook);
  }

  atualizar(notebook: Notebook) {
    return this.http.put<void>(`${this.url}/${notebook.id}`, notebook);
  }

  remover(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
