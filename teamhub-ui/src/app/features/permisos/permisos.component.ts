import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Permiso } from '../../core/models';

interface Vacacion {
  id: number; personaId: number; persona: string;
  fechaInicio: string; fechaFin: string; dias: number; estado: string;
}

interface CalendarItem {
  tipo: 'permiso' | 'vacacion' | 'cumpleanos';
  persona: string; dia: number; descripcion: string;
}

@Component({
  selector: 'app-permisos',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatTabsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatSelectModule, MatFormFieldModule, MatProgressBarModule, MatTooltipModule
  ],
  template: `
    <div class="page-header">
      <h1>Permisos y Vacaciones</h1>
      <div class="flex-row gap-8">
        <button mat-stroked-button color="primary"><mat-icon>beach_access</mat-icon> Solicitar Vacación</button>
        <button mat-raised-button color="primary"><mat-icon>add</mat-icon> Nuevo Permiso</button>
      </div>
    </div>

    <mat-tab-group animationDuration="200ms">
      <!-- Permisos -->
      <mat-tab label="Permisos">
        <div class="tab-content">
          @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }
          <table mat-table [dataSource]="permisos()" class="full-width">
            <ng-container matColumnDef="persona">
              <th mat-header-cell *matHeaderCellDef>Persona</th>
              <td mat-cell *matCellDef="let p">{{ p.persona }}</td>
            </ng-container>
            <ng-container matColumnDef="periodo">
              <th mat-header-cell *matHeaderCellDef>Período</th>
              <td mat-cell *matCellDef="let p">
                {{ p.fechaInicio | date:'dd/MM/yyyy' }}
                @if (p.fechaFin !== p.fechaInicio) { — {{ p.fechaFin | date:'dd/MM/yyyy' }} }
              </td>
            </ng-container>
            <ng-container matColumnDef="motivo">
              <th mat-header-cell *matHeaderCellDef>Motivo</th>
              <td mat-cell *matCellDef="let p">
                <div>{{ p.motivo }}</div>
                @if (p.esSalidaTemprana) { <span class="badge badge-info" style="font-size:.7rem">Salida Temprana</span> }
              </td>
            </ng-container>
            <ng-container matColumnDef="estado">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let p">
                <span class="badge" [class]="estadoClass(p.estado)">{{ p.estado }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="acciones">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let p">
                @if (p.estado === 'Pendiente') {
                  <button mat-icon-button color="primary" matTooltip="Aprobar"><mat-icon>check</mat-icon></button>
                  <button mat-icon-button color="warn" matTooltip="Rechazar"><mat-icon>close</mat-icon></button>
                }
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="permisoCols"></tr>
            <tr mat-row *matRowDef="let row; columns: permisoCols;"></tr>
            <tr class="mat-row" *matNoDataRow>
              <td class="mat-cell empty-state" [attr.colspan]="permisoCols.length">
                <mat-icon>event_available</mat-icon><p>Sin permisos</p>
              </td>
            </tr>
          </table>
        </div>
      </mat-tab>

      <!-- Vacaciones -->
      <mat-tab label="Vacaciones">
        <div class="tab-content">
          <table mat-table [dataSource]="vacaciones()" class="full-width">
            <ng-container matColumnDef="persona">
              <th mat-header-cell *matHeaderCellDef>Persona</th>
              <td mat-cell *matCellDef="let v">{{ v.persona }}</td>
            </ng-container>
            <ng-container matColumnDef="periodo">
              <th mat-header-cell *matHeaderCellDef>Período</th>
              <td mat-cell *matCellDef="let v">
                {{ v.fechaInicio | date:'dd/MM/yyyy' }} — {{ v.fechaFin | date:'dd/MM/yyyy' }}
              </td>
            </ng-container>
            <ng-container matColumnDef="dias">
              <th mat-header-cell *matHeaderCellDef>Días</th>
              <td mat-cell *matCellDef="let v">{{ v.dias }}</td>
            </ng-container>
            <ng-container matColumnDef="estado">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let v">
                <span class="badge" [class]="estadoClass(v.estado)">{{ v.estado }}</span>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="vacacionCols"></tr>
            <tr mat-row *matRowDef="let row; columns: vacacionCols;"></tr>
            <tr class="mat-row" *matNoDataRow>
              <td class="mat-cell empty-state" [attr.colspan]="vacacionCols.length">
                <mat-icon>beach_access</mat-icon><p>Sin vacaciones registradas</p>
              </td>
            </tr>
          </table>
        </div>
      </mat-tab>

      <!-- Calendario -->
      <mat-tab label="Calendario">
        <div class="tab-content">
          <div class="calendar-header">
            <button mat-icon-button (click)="prevMonth()"><mat-icon>chevron_left</mat-icon></button>
            <h3>{{ mesActual | date:'MMMM yyyy':'':'es' }}</h3>
            <button mat-icon-button (click)="nextMonth()"><mat-icon>chevron_right</mat-icon></button>
          </div>
          <p class="text-muted">Vista de calendario en construcción</p>
        </div>
      </mat-tab>
    </mat-tab-group>
  `,
  styles: [`
    .tab-content { padding: 20px 0; }
    .gap-8 { gap: 8px; }
    .calendar-header { display: flex; align-items: center; gap: 16px; margin-bottom: 16px; h3 { margin: 0; } }
    .empty-state { text-align: center; padding: 40px; mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; } p { color: var(--color-text-secondary); } }
  `]
})
export class PermisosComponent implements OnInit {
  readonly loading = signal(false);
  readonly permisos = signal<Permiso[]>([]);
  readonly vacaciones = signal<Vacacion[]>([]);

  mesActual = new Date();
  readonly permisoCols = ['persona', 'periodo', 'motivo', 'estado', 'acciones'];
  readonly vacacionCols = ['persona', 'periodo', 'dias', 'estado'];

  ngOnInit(): void { /* TODO: connect to service */ }

  prevMonth(): void { this.mesActual = new Date(this.mesActual.getFullYear(), this.mesActual.getMonth() - 1, 1); }
  nextMonth(): void { this.mesActual = new Date(this.mesActual.getFullYear(), this.mesActual.getMonth() + 1, 1); }

  estadoClass(e: string): string {
    const map: Record<string, string> = {
      Pendiente: 'badge-warning', Aprobado: 'badge-success', Rechazado: 'badge-danger'
    };
    return map[e] ?? 'badge-neutral';
  }
}
