import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Persona, PersonaDetalle } from '../models';

@Injectable({ providedIn: 'root' })
export class PersonasService {
  private readonly api = inject(ApiService);

  getAll(params?: { estado?: string; tipo?: string; areaId?: number }): Observable<Persona[]> {
    return this.api.get<Persona[]>('/api/personas', params as Record<string, string>);
  }

  getById(id: number): Observable<PersonaDetalle> {
    return this.api.get<PersonaDetalle>(`/api/personas/${id}`);
  }

  create(data: unknown): Observable<{ id: number }> {
    return this.api.post<{ id: number }>('/api/personas', data);
  }

  update(id: number, data: unknown): Observable<unknown> {
    return this.api.put(`/api/personas/${id}`, data);
  }

  deactivate(id: number): Observable<void> {
    return this.api.delete(`/api/personas/${id}`);
  }

  renovarContrato(id: number, data: { nuevaFechaFin: string; observaciones?: string }): Observable<unknown> {
    return this.api.post(`/api/personas/${id}/renovar-contrato`, data);
  }

  updateTecnologias(id: number, tecnologias: Array<{ tecnologiaId: number; nivel: string }>): Observable<unknown> {
    return this.api.put(`/api/personas/${id}/tecnologias`, { tecnologias });
  }

  updateAplicaciones(id: number, aplicacionIds: Array<{ aplicacionId: number; observaciones?: string }>): Observable<unknown> {
    return this.api.put(`/api/personas/${id}/aplicaciones`, { aplicacionIds });
  }

  getAlertasContratos(): Observable<unknown[]> {
    return this.api.get<unknown[]>('/api/personas/alertas/contratos-por-vencer');
  }
}
