import { Component, OnInit, signal, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatTableModule } from '@angular/material/table';
import { ProyectosService } from '../../core/services/proyectos.service';
import { EstadoProyecto } from '../../core/models';

interface ProyectoDetalle {
  id: number;
  nombre: string;
  linkAzureDevOps?: string;
  fechaInicio: string;
  fechaFin: string;
  estado: EstadoProyecto;
  costoTotal: number;
  rom: number;
  descripcion?: string;
  asignaciones: Array<{
    id: number; personaNombre: string; rol: string; dedicacion: number;
    fechaInicio: string; fechaFin: string;
  }>;
  aplicaciones: Array<{ id: number; nombre: string; fechaInicioTrabajo: string }>;
}

@Component({
  selector: 'app-proyecto-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatTabsModule, MatCardModule, MatButtonModule,
    MatIconModule, MatProgressBarModule, MatChipsModule,
    MatDividerModule, MatTableModule
  ],
  template: `
    @if (loading()) {
      <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    } @else if (proyecto()) {
      <div class="page-header">
        <div class="flex-row">
          <button mat-icon-button routerLink="/proyectos"><mat-icon>arrow_back</mat-icon></button>
          <div>
            <h1>{{ proyecto()!.nombre }}</h1>
            <div class="text-muted flex-row">
              <span class="badge" [class]="estadoClass(proyecto()!.estado)">{{ estadoLabel(proyecto()!.estado) }}</span>
              <span>{{ proyecto()!.fechaInicio | date:'dd/MM/yyyy' }} — {{ proyecto()!.fechaFin | date:'dd/MM/yyyy' }}</span>
            </div>
          </div>
        </div>
        <button mat-raised-button color="primary"><mat-icon>edit</mat-icon> Editar</button>
      </div>

      <div class="kpi-row">
        <div class="kpi-card"><div class="kpi-value">{{ proyecto()!.costoTotal | number:'1.0-0' }}</div><div class="kpi-label">Costo Total (USD)</div></div>
        <div class="kpi-card"><div class="kpi-value">{{ proyecto()!.rom | number:'1.0-0' }}</div><div class="kpi-label">ROM (USD)</div></div>
        <div class="kpi-card"><div class="kpi-value">{{ proyecto()!.asignaciones.length }}</div><div class="kpi-label">Personas</div></div>
        <div class="kpi-card"><div class="kpi-value">{{ proyecto()!.aplicaciones.length }}</div><div class="kpi-label">Aplicaciones</div></div>
      </div>

      <mat-tab-group animationDuration="200ms">
        <mat-tab label="Equipo">
          <div class="tab-content">
            <div class="flex-row" style="justify-content: space-between; margin-bottom: 16px">
              <h3 style="margin: 0">Asignaciones</h3>
              <button mat-stroked-button color="primary"><mat-icon>person_add</mat-icon> Agregar</button>
            </div>
            <table mat-table [dataSource]="proyecto()!.asignaciones" class="full-width">
              <ng-container matColumnDef="persona">
                <th mat-header-cell *matHeaderCellDef>Persona</th>
                <td mat-cell *matCellDef="let a">{{ a.personaNombre }}</td>
              </ng-container>
              <ng-container matColumnDef="rol">
                <th mat-header-cell *matHeaderCellDef>Rol</th>
                <td mat-cell *matCellDef="let a"><span class="badge badge-info">{{ a.rol }}</span></td>
              </ng-container>
              <ng-container matColumnDef="dedicacion">
                <th mat-header-cell *matHeaderCellDef>Dedicación</th>
                <td mat-cell *matCellDef="let a">{{ a.dedicacion }}%</td>
              </ng-container>
              <ng-container matColumnDef="periodo">
                <th mat-header-cell *matHeaderCellDef>Período</th>
                <td mat-cell *matCellDef="let a">
                  {{ a.fechaInicio | date:'dd/MM/yy' }} — {{ a.fechaFin | date:'dd/MM/yy' }}
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="['persona','rol','dedicacion','periodo']"></tr>
              <tr mat-row *matRowDef="let r; columns: ['persona','rol','dedicacion','periodo'];"></tr>
            </table>
          </div>
        </mat-tab>

        <mat-tab label="Aplicaciones">
          <div class="tab-content">
            <div class="flex-row" style="justify-content: space-between; margin-bottom: 16px">
              <h3 style="margin: 0">Aplicaciones del Proyecto</h3>
              <button mat-stroked-button color="primary"><mat-icon>add</mat-icon> Agregar</button>
            </div>
            @if (proyecto()!.aplicaciones.length === 0) {
              <p class="text-muted">Sin aplicaciones registradas</p>
            }
            @for (a of proyecto()!.aplicaciones; track a.id) {
              <div class="list-item-card">
                <mat-icon>apps</mat-icon>
                <div>
                  <div style="font-weight: 500">{{ a.nombre }}</div>
                  <div class="text-muted">Inicio: {{ a.fechaInicioTrabajo | date:'dd/MM/yyyy' }}</div>
                </div>
                <button mat-icon-button color="primary" style="margin-left: auto">
                  <mat-icon>assignment</mat-icon>
                </button>
              </div>
            }
          </div>
        </mat-tab>
      </mat-tab-group>
    }
  `,
  styles: [`
    .kpi-row { display: flex; gap: 12px; flex-wrap: wrap; margin-bottom: 20px; }
    .kpi-row .kpi-card { min-width: 140px; }
    .tab-content { padding: 20px 0; }
    .list-item-card { display: flex; align-items: center; gap: 12px; padding: 12px; background: #f5f5f5; border-radius: 6px; margin-bottom: 8px; }
  `]
})
export class ProyectoDetailComponent implements OnInit {
  readonly id = input.required<string>();
  private readonly svc = inject(ProyectosService);
  readonly loading = signal(true);
  readonly proyecto = signal<ProyectoDetalle | null>(null);

  ngOnInit(): void {
    this.svc.getById(+this.id()).subscribe({
      next: p => { this.proyecto.set(p as unknown as ProyectoDetalle); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  estadoLabel(e: EstadoProyecto): string {
    const map: Record<EstadoProyecto, string> = {
      PorIniciar: 'Por Iniciar', EnCurso: 'En Curso', Atrasado: 'Atrasado', Finalizado: 'Finalizado'
    };
    return map[e] ?? e;
  }

  estadoClass(e: EstadoProyecto): string {
    const map: Record<EstadoProyecto, string> = {
      PorIniciar: 'badge-info', EnCurso: 'badge-success', Atrasado: 'badge-danger', Finalizado: 'badge-neutral'
    };
    return map[e] ?? '';
  }
}
