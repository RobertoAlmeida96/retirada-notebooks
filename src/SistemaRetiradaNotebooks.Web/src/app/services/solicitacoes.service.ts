import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AprovarSolicitacaoRequest, CriarSolicitacaoRequest, Solicitacao } from '../models';

@Injectable({ providedIn: 'root' })
export class SolicitacoesService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/solicitacoes`;

  listar() { return this.http.get<Solicitacao[]>(this.url); }

  criar(request: CriarSolicitacaoRequest) {
    return this.http.post<Solicitacao>(this.url, request);
  }

  aprovar(id: number, request: AprovarSolicitacaoRequest) {
    return this.http.post<Solicitacao>(`${this.url}/${id}/aprovar`, request);
  }

  devolver(id: number) {
    return this.http.post<Solicitacao>(`${this.url}/${id}/devolver`, {});
  }

  recusar(id: number, motivo: string) {
    return this.http.post<Solicitacao>(`${this.url}/${id}/recusar`, { motivo });
  }
}
