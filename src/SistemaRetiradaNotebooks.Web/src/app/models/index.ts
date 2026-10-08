export interface LoginRequest { email: string; senha: string; }
export interface RegistrarRequest { nome: string; email: string; senha: string; }

export interface UsuarioResponse {
  id: number;
  nome: string;
  email: string;
  perfil: 'Comum' | 'Admin';
  status: 'PendenteAprovacao' | 'Ativo' | 'Recusado';
}

export interface LoginResponse { token: string; usuario: UsuarioResponse; }

export type StatusNotebook =
  'Disponivel' | 'Emprestado' | 'EmManutencao' | 'ManutencaoSemGarantia' |
  'Quebrado' | 'ChamadoAberto' | 'UsoAdministrativo' | 'Furtado';

export interface Notebook {
  id: number;
  numero: number;
  modelo: string;
  numeroSerie?: string;
  patrimonio: string;
  garantia?: string;
  notaFiscal?: string;
  observacoes?: string;
  status: StatusNotebook;
  carrinhoId: number;
}

export interface CarrinhoComNotebooks {
  id: number;
  nome: string;
  notebooks: Notebook[];
}

export interface Solicitacao {
  id: number;
  usuarioId: number;
  quantidade: number;
  dataHoraSolicitacao: string;
  aulasPrevistas: number;
  status: 'Pendente' | 'Recusada' | 'EmUso' | 'Devolvida';
  motivoRecusa?: string;
  dataHoraRetirada?: string;
  dataHoraDevolucao?: string;
  previsaoDevolucao?: string;
  estaAtrasada: boolean;
}

export interface CriarSolicitacaoRequest {
  usuarioId: number;
  quantidade: number;
  aulasPrevistas: number;
}

export interface AprovarSolicitacaoRequest { notebookIds: number[]; }
export interface RecusarSolicitacaoRequest { motivo: string; }
