import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { HoraExtra, SaldoHorasExtra, EstadoHoraExtra, TipoHoraExtra } from '../../core/models';

@Component({
  selector: 'app-horas-extra',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatTableModule, MatButtonModule, MatIconModule,
    MatInputModule, MatSelectModule, MatFormFieldModule,
    MatProgressBarModule, MatTooltipModule, MatChipsModule
  ],
  template: `
    <div class="page-header">
      <h1>Horas Extra</h1>
      <button mat-raised-button color="primary">
        <mat-icon>add</mat-icon> Registrar HE
      </button>
    </div>

    <div class="saldo-cards">
      <div class="kpi-card">
        <div class="kpi-value">{{ saldo().horasPendientes }}</div>
        <div class="kpi-label">Pendientes</div>
      </div>
      <div class="kpi-card">
        <div class="kpi-value">{{ saldo().horasAprobadas }}</div>
        <div class="kpi-label">Aprobadas</div>
      </div>
      <div class="kpi-card">
        <div class="kpi-value">{{ saldo().horasRecuperadas }}</div>
        <div class="kpi-label">Recuperadas</div>
      </div>
      <div class="kpi-card">
        <div class="kpi-value">{{ saldo().horasNoRecuperables }}</div>
        <div class="kpi-label">No Recuperables</div>
      </div>
    </div>

    <div class="card filters-row">
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Estado</mat-label>
        <mat-select [(ngModel)]="estadoFilter" (ngModelChange)="load()">
          <mat-option value="">Todos</mat-option>
          <mat-option value="Pendiente">Pendiente</mat-option>
          <mat-option value="Aprobado">Aprobado</mat-option>
          <mat-option value="Recuperado">Recuperado</mat-option>
          <mat-option value="NoRecuperable">No Recuperable</mat-option>
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Tipo</mat-label>
        <mat-select [(ngModel)]="tipoFilter" (ngModelChange)="load()">
          <mat-option value="">Todos</mat-option>
          <mat-option value="Proyecto">Proyecto</mat-option>
          <mat-option value="SoporteApoyo">Soporte/Apoyo</mat-option>
          <mat-option value="Pruebas">Pruebas</mat-option>
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }

    <div class="card table-card">
      <table mat-table [dataSource]="horasExtra()" class="full-width">
        <ng-container matColumnDef="persona">
          <th mat-header-cell *matHeaderCellDef>Persona</th>
          <td mat-cell *matCellDef="let h">{{ h.persona }}</td>
        </ng-container>
        <ng-container matColumnDef="fecha">
          <th mat-header-cell *matHeaderCellDef>Fecha</th>
          <td mat-cell *matCellDef="let h">{{ h.fecha | date:'dd/MM/yyyy' }}</td>
        </ng-container>
        <ng-container matColumnDef="actividad">
          <th mat-header-cell *matHeaderCellDef>Actividad</th>
          <td mat-cell *matCellDef="let h">
            <div>{{ h.actividad }}</div>
            @if (h.proyecto) { <div class="text-muted text-sm">{{ h.proyecto }}</div> }
          </td>
        </ng-container>
        <ng-container matColumnDef="horas">
          <th mat-header-cell *matHeaderCellDef>Horas</th>
          <td mat-cell *matCellDef="let h">{{ h.horas }}h</td>
        </ng-container>
        <ng-container matColumnDef="tipo">
          <th mat-header-cell *matHeaderCellDef>Tipo</th>
          <td mat-cell *matCellDef="let h">
            <span class="badge badge-info">{{ tipoLabel(h.tipo) }}</span>
          </td>
        </ng-container>
        <ng-container matColumnDef="estado">
          <th mat-header-cell *matHeaderCellDef>Estado</th>
          <td mat-cell *matCellDef="let h">
            <span class="badge" [class]="estadoClass(h.estado)">{{ estadoLabel(h.estado) }}</span>
          </td>
        </ng-container>
        <ng-container matColumnDef="acciones">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let h">
            @if (h.estado === 'Pendiente') {
              <button mat-icon-button color="primary" matTooltip="Aprobar"><mat-icon>check_circle</mat-icon></button>
            }
            @if (h.tieneEvidencias) {
              <button mat-icon-button matTooltip="Ver evidencias"><mat-icon>attach_file</mat-icon></button>
            }
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns;"></tr>
        <tr class="mat-row" *matNoDataRow>
          <td class="mat-cell empty-state" [attr.colspan]="columns.length">
            <mat-icon>schedule</mat-icon><p>Sin registros</p>
          </td>
        </tr>
      </table>
    </div>
  `,
  styles: [`
    .saldo-cards { display: flex; gap: 12px; flex-wrap: wrap; margin-bottom: 20px; }
    .filters-row { display: flex; gap: 12px; flex-wrap: wrap; padding: 16px 20px; margin-bottom: 0; border-radius: var(--radius) var(--radius) 0 0; }
    .filter-field { min-width: 180px; }
    .table-card { padding: 0; border-radius: 0 0 var(--radius) var(--radius); }
    .text-sm { font-size: .875rem; }
    .empty-state { text-align: center; padding: 40px; mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; } p { color: var(--color-text-secondary); } }
  `]
})
export class HorasExtraComponent implements OnInit {
  readonly loading = signal(false);
  readonly horasExtra = signal<HoraExtra[]>([]);
  readonly saldo = signal<SaldoHorasExtra>({
    totalHorasExtra: 0, horasPendientes: 0, horasAprobadas: 0,
    horasRecuperadas: 0, horasNoRecuperables: 0
  });

  estadoFilter = '';
  tipoFilter = '';
  readonly columns = ['persona', 'fecha', 'actividad', 'horas', 'tipo', 'estado', 'acciones'];

  ngOnInit(): void { this.load(); }

  load(): void { /* TODO: connect to service */ }

  tipoLabel(t: TipoHoraExtra): string {
    const map: Record<TipoHoraExtra, string> = {
      Proyecto: 'Proyecto', SoporteApoyo: 'Soporte/Apoyo', Pruebas: 'Pruebas'
    };
    return map[t] ?? t;
  }

  estadoLabel(e: EstadoHoraExtra): string {
    const map: Record<EstadoHoraExtra, string> = {
      Pendiente: 'Pendiente', Aprobado: 'Aprobado', Recuperado: 'Recuperado', NoRecuperable: 'No Recuperable'
    };
    return map[e] ?? e;
  }

  estadoClass(e: EstadoHoraExtra): string {
    const map: Record<EstadoHoraExtra, string> = {
      Pendiente: 'badge-warning', Aprobado: 'badge-success',
      Recuperado: 'badge-info', NoRecuperable: 'badge-neutral'
    };
    return map[e] ?? '';
  }
}
