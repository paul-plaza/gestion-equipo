import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Area, Cargo, Tecnologia, Aplicacion } from '../../core/models';

@Component({
  selector: 'app-catalogos',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatTabsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatProgressBarModule, MatTooltipModule
  ],
  template: `
    <div class="page-header">
      <h1>Catálogos</h1>
    </div>

    <mat-tab-group animationDuration="200ms">
      <!-- Áreas -->
      <mat-tab label="Áreas">
        <div class="tab-content">
          <div class="tab-header">
            <h3 style="margin: 0">Áreas</h3>
            <button mat-raised-button color="primary"><mat-icon>add</mat-icon> Nueva Área</button>
          </div>
          @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }
          <table mat-table [dataSource]="areas()" class="full-width">
            <ng-container matColumnDef="nombre">
              <th mat-header-cell *matHeaderCellDef>Nombre</th>
              <td mat-cell *matCellDef="let a">{{ a.nombre }}</td>
            </ng-container>
            <ng-container matColumnDef="descripcion">
              <th mat-header-cell *matHeaderCellDef>Descripción</th>
              <td mat-cell *matCellDef="let a">{{ a.descripcion || '—' }}</td>
            </ng-container>
            <ng-container matColumnDef="activo">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let a">
                <span class="badge" [class]="a.activo ? 'badge-success' : 'badge-neutral'">
                  {{ a.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </td>
            </ng-container>
            <ng-container matColumnDef="acciones">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let a">
                <button mat-icon-button matTooltip="Editar"><mat-icon>edit</mat-icon></button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="catalogoCols"></tr>
            <tr mat-row *matRowDef="let row; columns: catalogoCols;"></tr>
          </table>
        </div>
      </mat-tab>

      <!-- Cargos -->
      <mat-tab label="Cargos">
        <div class="tab-content">
          <div class="tab-header">
            <h3 style="margin: 0">Cargos</h3>
            <button mat-raised-button color="primary"><mat-icon>add</mat-icon> Nuevo Cargo</button>
          </div>
          <table mat-table [dataSource]="cargos()" class="full-width">
            <ng-container matColumnDef="nombre">
              <th mat-header-cell *matHeaderCellDef>Nombre</th>
              <td mat-cell *matCellDef="let c">{{ c.nombre }}</td>
            </ng-container>
            <ng-container matColumnDef="descripcion">
              <th mat-header-cell *matHeaderCellDef>Descripción</th>
              <td mat-cell *matCellDef="let c">{{ c.descripcion || '—' }}</td>
            </ng-container>
            <ng-container matColumnDef="activo">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let c">
                <span class="badge" [class]="c.activo ? 'badge-success' : 'badge-neutral'">
                  {{ c.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </td>
            </ng-container>
            <ng-container matColumnDef="acciones">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let c">
                <button mat-icon-button matTooltip="Editar"><mat-icon>edit</mat-icon></button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="catalogoCols"></tr>
            <tr mat-row *matRowDef="let row; columns: catalogoCols;"></tr>
          </table>
        </div>
      </mat-tab>

      <!-- Tecnologías -->
      <mat-tab label="Tecnologías">
        <div class="tab-content">
          <div class="tab-header">
            <h3 style="margin: 0">Tecnologías</h3>
            <button mat-raised-button color="primary"><mat-icon>add</mat-icon> Nueva Tecnología</button>
          </div>
          <table mat-table [dataSource]="tecnologias()" class="full-width">
            <ng-container matColumnDef="nombre">
              <th mat-header-cell *matHeaderCellDef>Nombre</th>
              <td mat-cell *matCellDef="let t">{{ t.nombre }}</td>
            </ng-container>
            <ng-container matColumnDef="categoria">
              <th mat-header-cell *matHeaderCellDef>Categoría</th>
              <td mat-cell *matCellDef="let t">{{ t.categoria }}</td>
            </ng-container>
            <ng-container matColumnDef="activo">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let t">
                <span class="badge" [class]="t.activo ? 'badge-success' : 'badge-neutral'">
                  {{ t.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </td>
            </ng-container>
            <ng-container matColumnDef="acciones">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let t">
                <button mat-icon-button matTooltip="Editar"><mat-icon>edit</mat-icon></button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="['nombre','categoria','activo','acciones']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['nombre','categoria','activo','acciones'];"></tr>
          </table>
        </div>
      </mat-tab>

      <!-- Aplicaciones -->
      <mat-tab label="Aplicaciones">
        <div class="tab-content">
          <div class="tab-header">
            <h3 style="margin: 0">Aplicaciones Especializadas</h3>
            <button mat-raised-button color="primary"><mat-icon>add</mat-icon> Nueva Aplicación</button>
          </div>
          <table mat-table [dataSource]="aplicaciones()" class="full-width">
            <ng-container matColumnDef="nombre">
              <th mat-header-cell *matHeaderCellDef>Nombre</th>
              <td mat-cell *matCellDef="let a">{{ a.nombre }}</td>
            </ng-container>
            <ng-container matColumnDef="descripcion">
              <th mat-header-cell *matHeaderCellDef>Descripción</th>
              <td mat-cell *matCellDef="let a">{{ a.descripcion || '—' }}</td>
            </ng-container>
            <ng-container matColumnDef="activo">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let a">
                <span class="badge" [class]="a.activo ? 'badge-success' : 'badge-neutral'">
                  {{ a.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </td>
            </ng-container>
            <ng-container matColumnDef="acciones">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let a">
                <button mat-icon-button matTooltip="Editar"><mat-icon>edit</mat-icon></button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="catalogoCols"></tr>
            <tr mat-row *matRowDef="let row; columns: catalogoCols;"></tr>
          </table>
        </div>
      </mat-tab>
    </mat-tab-group>
  `,
  styles: [`
    .tab-content { padding: 20px 0; }
    .tab-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
  `]
})
export class CatalogosComponent implements OnInit {
  readonly loading = signal(false);
  readonly areas = signal<Area[]>([]);
  readonly cargos = signal<Cargo[]>([]);
  readonly tecnologias = signal<Tecnologia[]>([]);
  readonly aplicaciones = signal<Aplicacion[]>([]);

  readonly catalogoCols = ['nombre', 'descripcion', 'activo', 'acciones'];

  ngOnInit(): void { /* TODO: connect to service */ }
}
