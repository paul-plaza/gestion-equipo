import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Proyecto } from '../models';

@Injectable({ providedIn: 'root' })
export class ProyectosService {
  private readonly api = inject(ApiService);

  getAll(estado?: string): Observable<Proyecto[]> {
    return this.api.get<Proyecto[]>('/api/proyectos', estado ? { estado } : undefined);
  }

  getById(id: number): Observable<unknown> {
    return this.api.get(`/api/proyectos/${id}`);
  }

  create(data: unknown): Observable<{ id: number }> {
    return this.api.post<{ id: number }>('/api/proyectos', data);
  }

  update(id: number, data: unknown): Observable<unknown> {
    return this.api.put(`/api/proyectos/${id}`, data);
  }

  cambiarEstado(id: number, estado: string): Observable<unknown> {
    return this.api.put(`/api/proyectos/${id}/estado`, { estado });
  }

  getAsignaciones(id: number): Observable<unknown[]> {
    return this.api.get<unknown[]>(`/api/proyectos/${id}/asignaciones`);
  }

  addAsignacion(id: number, data: unknown): Observable<unknown> {
    return this.api.post(`/api/proyectos/${id}/asignaciones`, data);
  }

  removeAsignacion(proyectoId: number, asignacionId: number): Observable<void> {
    return this.api.delete(`/api/proyectos/${proyectoId}/asignaciones/${asignacionId}`);
  }

  getAplicaciones(id: number): Observable<unknown[]> {
    return this.api.get<unknown[]>(`/api/proyectos/${id}/aplicaciones`);
  }

  addAplicacion(id: number, data: unknown): Observable<unknown> {
    return this.api.post(`/api/proyectos/${id}/aplicaciones`, data);
  }

  registrarPase(proyectoId: number, appId: number, data: unknown): Observable<unknown> {
    return this.api.post(`/api/proyectos/${proyectoId}/aplicaciones/${appId}/pase`, data);
  }

  getMetricas(id: number): Observable<unknown[]> {
    return this.api.get<unknown[]>(`/api/proyectos/${id}/metricas`);
  }

  getLineaTiempo(anio?: number): Observable<unknown> {
    return this.api.get('/api/proyectos/linea-tiempo', anio ? { anio } : undefined);
  }

  getAlertasPorTerminar(): Observable<unknown[]> {
    return this.api.get<unknown[]>('/api/proyectos/alertas/por-terminar');
  }
}
