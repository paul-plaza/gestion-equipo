export interface Area { id: number; nombre: string; descripcion?: string; activo: boolean; }
export interface Cargo { id: number; nombre: string; descripcion?: string; activo: boolean; }
export interface Tecnologia { id: number; nombre: string; categoria: string; activo: boolean; }
export interface Aplicacion { id: number; nombre: string; descripcion?: string; activo: boolean; }

export type TipoPersona = 'Interno' | 'Externo';
export type EstadoPersona = 'Activo' | 'Inactivo';
export type RolSistema = 'JefeDesarrollo' | 'JefeArea' | 'Persona';
export type NivelDominio = 'Basico' | 'Intermedio' | 'Avanzado';

export interface Persona {
  id: number;
  nombres: string;
  apellidos: string;
  correoInstitucional: string;
  tipo: TipoPersona;
  estado: EstadoPersona;
  area: string;
  cargo: string;
  rolEquipo?: string;
  fechaFinContrato?: string;
  fechaCumpleanos?: string;
}

export interface PersonaDetalle extends Persona {
  cedula: string;
  celular?: string;
  fechaIngreso?: string;
  tecnologias: Array<{ id: number; nombre: string; categoria: string; nivel: NivelDominio }>;
  aplicaciones: Array<{ id: number; nombre: string; observaciones?: string }>;
  historialContrato: Array<{ fechaFinAnterior: string; fechaFinNueva: string; fechaRenovacion: string }>;
}

export type EstadoProyecto = 'PorIniciar' | 'EnCurso' | 'Atrasado' | 'Finalizado';
export type RolProyecto = 'Desarrollador' | 'ScrumMaster' | 'Arquitecto';

export interface Proyecto {
  id: number;
  nombre: string;
  linkAzureDevOps?: string;
  fechaInicio: string;
  fechaFin: string;
  estado: EstadoProyecto;
  costoTotal: number;
  rom: number;
  numPersonas: number;
}

export interface Asignacion {
  id: number;
  personaId: number;
  personaNombre: string;
  rol: RolProyecto;
  dedicacion: 25 | 50 | 100;
  fechaInicio: string;
  fechaFin: string;
}

export type EstadoHoraExtra = 'Pendiente' | 'Aprobado' | 'Recuperado' | 'NoRecuperable';
export type TipoHoraExtra = 'Proyecto' | 'SoporteApoyo' | 'Pruebas';

export interface HoraExtra {
  id: number;
  personaId: number;
  persona: string;
  fecha: string;
  actividad: string;
  horas: number;
  tipo: TipoHoraExtra;
  estado: EstadoHoraExtra;
  proyecto?: string;
  tieneEvidencias: boolean;
}

export interface SaldoHorasExtra {
  totalHorasExtra: number;
  horasPendientes: number;
  horasAprobadas: number;
  horasRecuperadas: number;
  horasNoRecuperables: number;
}

export interface Permiso {
  id: number;
  personaId: number;
  persona: string;
  fechaInicio: string;
  fechaFin: string;
  motivo: string;
  estado: string;
  esSalidaTemprana: boolean;
}

export interface Logro {
  id: number;
  personaId: number;
  persona: string;
  titulo: string;
  descripcion: string;
  impacto?: string;
  evidenciaOLink?: string;
  fecha: string;
}

export interface Objetivo {
  id: number;
  personaId: number;
  numeroObjetivo: number;
  alcance: string;
  detalle?: string;
  porcentajeAvance: number;
  fechaSeguimiento?: string;
  observaciones?: string;
}

export interface Evolutivo {
  id: number;
  personaId: number;
  persona: string;
  fecha: string;
  aplicacion: string;
  actividad: string;
  horas: number;
  aprobado: boolean;
}

export interface ProgresoEvolutivo {
  personaId: number;
  horasCompletadas: number;
  metaHoras: number;
  porcentaje: number;
  horasRestantes: number;
}

export interface Alerta {
  tipo: string;
  mensaje: string;
  criticidad: 'alta' | 'media' | 'baja';
  entidadId?: number;
}

export interface KpiDashboard {
  personasActivas: number;
  internos: number;
  externos: number;
  proyectosActivos: number;
  proyectosPorIniciar: number;
  horasExtraPendientes: number;
}

export interface ApiResponse<T> { data: T; message?: string; }
export interface PaginatedResponse<T> { items: T[]; total: number; page: number; pageSize: number; }
