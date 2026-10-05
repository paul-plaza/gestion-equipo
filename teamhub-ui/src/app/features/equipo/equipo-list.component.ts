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
import { MatChipsModule } from '@angular/material/chips';
import { PersonasService } from '../../core/services/personas.service';
import { Persona } from '../../core/models';

@Component({
  selector: 'app-equipo-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule,
    MatTableModule, MatButtonModule, MatIconModule,
    MatInputModule, MatSelectModule, MatFormFieldModule,
    MatProgressBarModule, MatTooltipModule, MatChipsModule
  ],
  template: `
    <div class="page-header">
      <h1>Equipo</h1>
      <button mat-raised-button color="primary" routerLink="/equipo/nuevo">
        <mat-icon>add</mat-icon> Nuevo miembro
      </button>
    </div>

    <!-- Filters -->
    <div class="card filters-row">
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Buscar</mat-label>
        <input matInput [(ngModel)]="search" (ngModelChange)="applyFilter()" placeholder="Nombre, correo...">
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Estado</mat-label>
        <mat-select [(ngModel)]="estadoFilter" (ngModelChange)="loadPersonas()">
          <mat-option value="">Todos</mat-option>
          <mat-option value="Activo">Activo</mat-option>
          <mat-option value="Inactivo">Inactivo</mat-option>
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Tipo</mat-label>
        <mat-select [(ngModel)]="tipoFilter" (ngModelChange)="loadPersonas()">
          <mat-option value="">Todos</mat-option>
          <mat-option value="Interno">Internos</mat-option>
          <mat-option value="Externo">Externos</mat-option>
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }

    <div class="card table-card">
      <table mat-table [dataSource]="filteredPersonas()" class="full-width">
        <ng-container matColumnDef="nombre">
          <th mat-header-cell *matHeaderCellDef>Nombre</th>
          <td mat-cell *matCellDef="let p">
            <a [routerLink]="['/equipo', p.id]" class="person-link">
              <div class="avatar">{{ initials(p) }}</div>
              <div>
                <div class="person-name">{{ p.apellidos }}, {{ p.nombres }}</div>
                <div class="text-muted">{{ p.correoInstitucional }}</div>
              </div>
            </a>
          </td>
        </ng-container>

        <ng-container matColumnDef="area">
          <th mat-header-cell *matHeaderCellDef>Área / Cargo</th>
          <td mat-cell *matCellDef="let p">
            <div>{{ p.area }}</div>
            <div class="text-muted">{{ p.cargo }}</div>
          </td>
        </ng-container>

        <ng-container matColumnDef="tipo">
          <th mat-header-cell *matHeaderCellDef>Tipo</th>
          <td mat-cell *matCellDef="let p">
            <span class="badge" [class]="p.tipo === 'Externo' ? 'badge-warning' : 'badge-info'">
              {{ p.tipo }}
            </span>
          </td>
        </ng-container>

        <ng-container matColumnDef="contrato">
          <th mat-header-cell *matHeaderCellDef>Fin Contrato</th>
          <td mat-cell *matCellDef="let p">
            @if (p.fechaFinContrato) {
              <span [class]="contratoClass(p.fechaFinContrato)">
                {{ p.fechaFinContrato | date:'dd/MM/yyyy' }}
              </span>
            } @else {
              <span class="text-muted">—</span>
            }
          </td>
        </ng-container>

        <ng-container matColumnDef="estado">
          <th mat-header-cell *matHeaderCellDef>Estado</th>
          <td mat-cell *matCellDef="let p">
            <span class="badge" [class]="p.estado === 'Activo' ? 'badge-success' : 'badge-neutral'">
              {{ p.estado }}
            </span>
          </td>
        </ng-container>

        <ng-container matColumnDef="acciones">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let p">
            <button mat-icon-button [routerLink]="['/equipo', p.id]" matTooltip="Ver detalle">
              <mat-icon>visibility</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns;"></tr>

        <tr class="mat-row" *matNoDataRow>
          <td class="mat-cell empty-state" [attr.colspan]="columns.length">
            <mat-icon>people_outline</mat-icon>
            <p>No se encontraron personas</p>
          </td>
        </tr>
      </table>
    </div>
  `,
  styles: [`
    .filters-row { display: flex; gap: 12px; flex-wrap: wrap; padding: 16px 20px; margin-bottom: 0; border-radius: var(--radius) var(--radius) 0 0; }
    .filter-field { min-width: 180px; }
    .table-card { padding: 0; border-radius: 0 0 var(--radius) var(--radius); }
    .person-link { display: flex; align-items: center; gap: 10px; text-decoration: none; color: inherit; }
    .avatar { width: 36px; height: 36px; border-radius: 50%; background: var(--color-primary); color: white; display: flex; align-items: center; justify-content: center; font-weight: 600; font-size: 13px; flex-shrink: 0; }
    .person-name { font-weight: 500; }
    .empty-state { text-align: center; padding: 40px; mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; } p { color: var(--color-text-secondary); } }
  `]
})
export class EquipoListComponent implements OnInit {
  private readonly svc = inject(PersonasService);

  readonly loading = signal(true);
  readonly personas = signal<Persona[]>([]);
  readonly filteredPersonas = signal<Persona[]>([]);

  search = '';
  estadoFilter = 'Activo';
  tipoFilter = '';

  readonly columns = ['nombre', 'area', 'tipo', 'contrato', 'estado', 'acciones'];

  ngOnInit(): void {
    this.loadPersonas();
  }

  loadPersonas(): void {
    this.loading.set(true);
    this.svc.getAll({
      estado: this.estadoFilter || undefined,
      tipo: this.tipoFilter || undefined
    }).subscribe({
      next: personas => {
        this.personas.set(personas);
        this.applyFilter();
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  applyFilter(): void {
    const q = this.search.toLowerCase();
    this.filteredPersonas.set(
      q ? this.personas().filter(p =>
        `${p.nombres} ${p.apellidos}`.toLowerCase().includes(q) ||
        p.correoInstitucional.toLowerCase().includes(q)
      ) : this.personas()
    );
  }

  initials(p: Persona): string {
    return `${p.nombres[0]}${p.apellidos[0]}`.toUpperCase();
  }

  contratoClass(fecha: string): string {
    const dias = Math.ceil((new Date(fecha).getTime() - Date.now()) / 86400000);
    if (dias <= 7) return 'badge badge-danger';
    if (dias <= 15) return 'badge badge-warning';
    if (dias <= 30) return 'badge badge-info';
    return 'text-muted';
  }
}
