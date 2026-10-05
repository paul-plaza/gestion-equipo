import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { ProjectsComponent } from './projects.component';
import { TeamHubApi } from './api';

describe('Proyectos con equipo', () => {
  async function setup() {
    const api = {
      actor: signal({ id: 1, role: 'jefe_desarrollo', name: 'Test' }),
      get: vi.fn().mockResolvedValue([]),
      save: vi.fn().mockResolvedValue({ id: 10 }),
    };
    await TestBed.configureTestingModule({
      imports: [ProjectsComponent],
      providers: [{ provide: TeamHubApi, useValue: api }],
    }).compileComponents();
    const fixture = TestBed.createComponent(ProjectsComponent),
      c = fixture.componentInstance;
    await c.edit();
    c.form = { ...c.form, nombre: 'Portal', fecha_inicio: '2027-01-01', fecha_fin: '2027-06-30' };
    return { fixture, c, api };
  }
  it('guarda varias personas en una sola operación sin campos globales de horas', async () => {
    const { c, api } = await setup();
    c.addMember();
    c.team[0]['persona_id'] = 2;
    c.addMember();
    c.team[1]['persona_id'] = 3;
    await c.save();
    expect(api.save).toHaveBeenCalledTimes(1);
    const payload = api.save.mock.calls[0][2];
    expect(payload.equipo).toHaveLength(2);
    expect(payload.equipo[0].horas_mensuales).toBeNull();
    expect(payload).not.toHaveProperty('horas_desarrollador');
    expect(payload).not.toHaveProperty('horas_scrum');
    expect(c.editing()).toBe(false);
  });
  it('conserva el formulario si el servidor rechaza una asignación', async () => {
    const { c, api } = await setup();
    api.save.mockRejectedValue(new Error('El externo requiere PO'));
    c.addMember();
    c.team[0]['persona_id'] = 2;
    await c.save();
    expect(c.editing()).toBe(true);
    expect(c.team).toHaveLength(1);
    expect(c.error()).toContain('requiere PO');
  });
  it('rechaza personas duplicadas con fechas superpuestas', async () => {
    const { c, api } = await setup();
    c.addMember();
    c.team[0]['persona_id'] = 2;
    c.addMember();
    c.team[1]['persona_id'] = 2;
    await c.save();
    expect(api.save).not.toHaveBeenCalled();
    expect(c.error()).toContain('superpuestas');
  });
  it('preserva identidad e historial de reemplazos al editar', async () => {
    const { c, api } = await setup();
    await c.edit({
      id: 7,
      nombre: 'Portal',
      fecha_inicio: '2027-01-01T00:00:00',
      fecha_fin: '2027-06-30T00:00:00',
      equipo: [
        {
          id: 9,
          persona_id: 2,
          nombre: 'Ana Test',
          rol_proyecto: 'arquitecto',
          porcentaje: 25,
          horas_mensuales: 40,
          horas_presupuestadas: 100,
          reemplaza_asignacion_id: 8,
          fecha_inicio: '2027-01-01',
          fecha_fin: '2027-06-30',
        },
      ],
    });
    await c.save();
    const payload = api.save.mock.calls[0][2];
    expect(payload.equipo[0]).toMatchObject({
      id: 9,
      reemplaza_asignacion_id: 8,
      horas_mensuales: 40,
    });
    expect(payload.equipo[0]).not.toHaveProperty('nombre');
  });
  it('muestra el editor de equipo sin entradas globales de horas', async () => {
    const { fixture } = await setup();
    fixture.detectChanges();
    await fixture.whenStable();
    const html: HTMLElement = fixture.nativeElement;
    expect(html.textContent).toContain('Equipo del proyecto');
    expect(html.textContent).not.toContain('Horas desarrollador');
    expect(html.textContent).not.toContain('Horas scrum');
  });
});
