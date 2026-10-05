import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProyectosService } from '../../core/services/proyectos.service';
import { Proyecto, EstadoProyecto } from '../../core/models';

@Component({
  selector: 'app-proyectos-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule,
    MatTableModule, MatButtonModule, MatIconModule,
    MatInputModule, MatSelectModule, MatFormFieldModule,
    MatProgressBarModule, MatTooltipModule
  ],
  template: `
    <div class="page-header">
      <h1>Proyectos</h1>
      <button mat-raised-button color="primary">
        <mat-icon>add</mat-icon> Nuevo proyecto
      </button>
    </div>

    <div class="card filters-row">
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Buscar</mat-label>
        <input matInput [(ngModel)]="search" (ngModelChange)="applyFilter()" placeholder="Nombre del proyecto...">
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Estado</mat-label>
        <mat-select [(ngModel)]="estadoFilter" (ngModelChange)="loadProyectos()">
          <mat-option value="">Todos</mat-option>
          <mat-option value="PorIniciar">Por Iniciar</mat-option>
          <mat-option value="EnCurso">En Curso</mat-option>
          <mat-option value="Atrasado">Atrasado</mat-option>
          <mat-option value="Finalizado">Finalizado</mat-option>
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }

    <div class="card table-card">
      <table mat-table [dataSource]="filteredProyectos()" class="full-width">
        <ng-container matColumnDef="nombre">
          <th mat-header-cell *matHeaderCellDef>Proyecto</th>
          <td mat-cell *matCellDef="let p">
            <a [routerLink]="['/proyectos', p.id]" class="link-primary">{{ p.nombre }}</a>
          </td>
        </ng-container>

        <ng-container matColumnDef="fechas">
          <th mat-header-cell *matHeaderCellDef>Período</th>
          <td mat-cell *matCellDef="let p">
            <div class="text-sm">{{ p.fechaInicio | date:'dd/MM/yyyy' }}</div>
            <div class="text-muted text-sm">{{ p.fechaFin | date:'dd/MM/yyyy' }}</div>
          </td>
        </ng-container>

        <ng-container matColumnDef="estado">
          <th mat-header-cell *matHeaderCellDef>Estado</th>
          <td mat-cell *matCellDef="let p">
            <span class="badge" [class]="estadoClass(p.estado)">{{ estadoLabel(p.estado) }}</span>
          </td>
        </ng-container>

        <ng-container matColumnDef="equipo">
          <th mat-header-cell *matHeaderCellDef>Equipo</th>
          <td mat-cell *matCellDef="let p">{{ p.numPersonas }} personas</td>
        </ng-container>

        <ng-container matColumnDef="costo">
          <th mat-header-cell *matHeaderCellDef>Costo / ROM</th>
          <td mat-cell *matCellDef="let p">
            <div>{{ p.costoTotal | number:'1.0-0' }} USD</div>
            <div class="text-muted text-sm">ROM: {{ p.rom | number:'1.0-0' }}</div>
          </td>
        </ng-container>

        <ng-container matColumnDef="acciones">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let p">
            <button mat-icon-button [routerLink]="['/proyectos', p.id]" matTooltip="Ver detalle">
              <mat-icon>visibility</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns;"></tr>
        <tr class="mat-row" *matNoDataRow>
          <td class="mat-cell empty-state" [attr.colspan]="columns.length">
            <mat-icon>folder_off</mat-icon>
            <p>No se encontraron proyectos</p>
          </td>
        </tr>
      </table>
    </div>
  `,
  styles: [`
    .filters-row { display: flex; gap: 12px; flex-wrap: wrap; padding: 16px 20px; margin-bottom: 0; border-radius: var(--radius) var(--radius) 0 0; }
    .filter-field { min-width: 180px; }
    .table-card { padding: 0; border-radius: 0 0 var(--radius) var(--radius); }
    .text-sm { font-size: .875rem; }
    .empty-state { text-align: center; padding: 40px; mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; } p { color: var(--color-text-secondary); } }
  `]
})
export class ProyectosListComponent implements OnInit {
  private readonly svc = inject(ProyectosService);

  readonly loading = signal(true);
  readonly proyectos = signal<Proyecto[]>([]);
  readonly filteredProyectos = signal<Proyecto[]>([]);

  search = '';
  estadoFilter = '';
  readonly columns = ['nombre', 'fechas', 'estado', 'equipo', 'costo', 'acciones'];

  ngOnInit(): void { this.loadProyectos(); }

  loadProyectos(): void {
    this.loading.set(true);
    this.svc.getAll(this.estadoFilter || undefined).subscribe({
      next: p => { this.proyectos.set(p); this.applyFilter(); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  applyFilter(): void {
    const q = this.search.toLowerCase();
    this.filteredProyectos.set(q
      ? this.proyectos().filter(p => p.nombre.toLowerCase().includes(q))
      : this.proyectos()
    );
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
