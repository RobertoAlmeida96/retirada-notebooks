import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { UsuarioResponse } from '../models';

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/usuarios`;

  listar() { return this.http.get<UsuarioResponse[]>(this.url); }

  aprovar(id: number) { return this.http.put<UsuarioResponse>(`${this.url}/${id}/aprovar`, {}); }

  recusar(id: number) { return this.http.put<UsuarioResponse>(`${this.url}/${id}/recusar`, {}); }
}
