import { Component, HostListener, computed, effect, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { A11yModule } from '@angular/cdk/a11y';
import { Row, Option, TeamHubApi } from './api';
import { ScreenNavigation } from './screens/screen-support';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, FormsModule, A11yModule],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss',
})
export class ProjectsComponent {
  readonly api = inject(TeamHubApi);
  readonly projects = input<Row[]>([]);
  readonly loading = input(false);
  readonly saved = output<void>();
  readonly selectedId = input<string|number|null>(null);
  readonly navigate = output<ScreenNavigation>();
  constructor(){effect(()=>{const id=this.selectedId();const projects=this.projects();if(id!==null){this.detail.set(projects.find(p=>String(p['id'])===String(id))??null);}else{this.detail.set(null);}});}
  closeDetail(){this.detail.set(null);this.navigate.emit({module:'proyectos'});}
  readonly view = signal<'cards' | 'gantt'>('cards');
  readonly filter = signal('todos');
  readonly query = signal('');
  readonly detail = signal<Row | null>(null);
  readonly editing = signal(false);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly directoryLoading = signal(false);
  readonly filters = [
    ['todos', 'Todos'],
    ['en_curso', 'En curso'],
    ['atrasado', 'Atrasados'],
    ['por_iniciar', 'Por iniciar'],
    ['finalizado', 'Finalizados'],
  ];
  readonly roles = [
    ['desarrollador', 'Desarrollador'],
    ['scrum_master', 'Scrum Master'],
    ['arquitecto', 'Arquitecto'],
  ];
  readonly months = [
    'Ene',
    'Feb',
    'Mar',
    'Abr',
    'May',
    'Jun',
    'Jul',
    'Ago',
    'Sep',
    'Oct',
    'Nov',
    'Dic',
  ];
  readonly assignmentFields = [
    'persona_id',
    'rol_proyecto',
    'porcentaje',
    'fecha_inicio',
    'fecha_fin',
    'horas_mensuales',
    'horas_presupuestadas',
    'reemplaza_asignacion_id',
  ];
  readonly projectFields = [
    'nombre',
    'url_tablero_devops',
    'product_owner_id',
    'fecha_inicio',
    'fecha_fin',
    'costo_total',
    'rom',
    'estado',
    'atrasado',
    'atrasado_desde',
    'atrasado_motivo',
  ];
  readonly visible = computed(() =>
    this.projects().filter(
      (p) =>
        this.matches(p, this.filter()) &&
        p['nombre'].toLocaleLowerCase().includes(this.query().toLocaleLowerCase()),
    ),
  );
  year = Number(this.today().slice(0, 4));
  form: Row = {};
  team: Row[] = [];
  people: Option[] = [];
  owners: Option[] = [];
  private focusBack?: HTMLElement;
  canManage() {
    return this.api.actor()?.role === 'jefe_desarrollo' || this.api.actor()?.role === 'jefe_area';
  }
  today() {
    return new Intl.DateTimeFormat('en-CA', {
      timeZone: 'America/Guayaquil',
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    }).format(new Date());
  }
  date(value: string) {
    return value
      ? new Intl.DateTimeFormat('es-EC', {
          day: 'numeric',
          month: 'short',
          year: 'numeric',
          timeZone: 'UTC',
        }).format(new Date(value.slice(0, 10) + 'T12:00:00Z'))
      : '—';
  }
  matches(p: Row, f: string) {
    return f === 'todos' || (f === 'atrasado' ? p['atrasado'] : p['estado'] === f);
  }
  count(f: string) {
    return this.projects().filter((p) => this.matches(p, f)).length;
  }
  status(p: Row) {
    return p['estado'] === 'finalizado'
      ? 'Finalizado'
      : p['atrasado']
        ? 'Atrasado'
        : p['estado'] === 'en_curso'
          ? 'En curso'
          : 'Por iniciar';
  }
  tone(p: Row) {
    return p['estado'] === 'finalizado'
      ? 'neutral'
      : p['atrasado']
        ? 'danger'
        : p['estado'] === 'en_curso'
          ? 'success'
          : 'info';
  }
  members(p: Row): Row[] {
    return p['equipo'] ?? [];
  }
  apps(p: Row): Row[] {
    return p['aplicaciones'] ?? [];
  }
  peopleCount(p: Row) {
    return new Set(this.members(p).map((m) => m['persona_id'])).size;
  }
  avatars(p: Row) {
    return this.members(p)
      .filter((m, i, a) => a.findIndex((x) => x['persona_id'] === m['persona_id']) === i)
      .slice(0, 4);
  }
  initials(name: string) {
    return name
      .split(' ')
      .slice(0, 2)
      .map((x) => x[0])
      .join('');
  }
  role(value: string) {
    return this.roles.find((r) => r[0] === value)?.[1] ?? value;
  }
  budget(p: Row) {
    return Number(p['rom']) > 0 ? (100 * Number(p['costo_total'])) / Number(p['rom']) : null;
  }
  elapsed(p: Row) {
    const start = Date.parse(p['fecha_inicio']),
      end = Date.parse(p['fecha_fin']);
    return Math.round(
      Math.max(
        0,
        Math.min(100, (100 * (Date.parse(this.today()) - start)) / Math.max(86400000, end - start)),
      ),
    );
  }
  overdue(p: Row) {
    return (
      !!p['fecha_fin'] &&
      p['fecha_fin'].slice(0, 10) < this.today() &&
      p['estado'] !== 'finalizado' &&
      !p['atrasado']
    );
  }
  left(date: string) {
    const a = Date.parse(`${this.year}-01-01`),
      b = Date.parse(`${this.year + 1}-01-01`);
    return Math.max(0, Math.min(100, (100 * (Date.parse(date) - a)) / (b - a)));
  }
  width(p: Row) {
    return Math.max(0, this.left(p['fecha_fin']) - this.left(p['fecha_inicio']));
  }
  inYear(p: Row) {
    return (
      p['fecha_inicio'].slice(0, 10) < `${this.year + 1}-01-01` &&
      p['fecha_fin'].slice(0, 10) >= `${this.year}-01-01`
    );
  }
  openDetail(p: Row) {
    this.detail.set(p);this.navigate.emit({module:'proyectos',id:p['id']});
  }
  async edit(p: Row | null = null) {
    this.focusBack = document.activeElement as HTMLElement;
    this.error.set('');
    this.form = p
      ? { ...p }
      : {
          nombre: '',
          url_tablero_devops: '',
          product_owner_id: null,
          fecha_inicio: this.today(),
          fecha_fin: '',
          costo_total: 0,
          rom: 0,
          estado: 'por_iniciar',
          atrasado: false,
        };
    for (const key of ['fecha_inicio', 'fecha_fin', 'atrasado_desde'])
      if (this.form[key]) this.form[key] = this.form[key].slice(0, 10);
    this.team = p
      ? this.members(p).map((m) => ({
          ...m,
          fecha_inicio: m['fecha_inicio'].slice(0, 10),
          fecha_fin: m['fecha_fin'].slice(0, 10),
        }))
      : [];
    this.editing.set(true);
    this.directoryLoading.set(true);
    try {
      [this.people, this.owners] = await Promise.all([
        this.api.get<Option[]>('/directory/persona'),
        this.api.get<Option[]>('/directory/product_owner'),
      ]);
    } catch (e) {
      this.error.set(e instanceof Error ? e.message : 'No se pudo cargar el equipo.');
    } finally {
      this.directoryLoading.set(false);
    }
  }
  addMember() {
    this.team.push({
      persona_id: null,
      rol_proyecto: 'desarrollador',
      porcentaje: 100,
      fecha_inicio: this.form['fecha_inicio'],
      fecha_fin: this.form['fecha_fin'],
      horas_mensuales: null,
      horas_presupuestadas: null,
    });
  }
  changeRole(m: Row) {
    if (m['rol_proyecto'] === 'desarrollador' && ![50, 100].includes(Number(m['porcentaje'])))
      m['porcentaje'] = 100;
  }
  personOptions(m: Row) {
    return m['id'] && !this.people.some((p) => p.id === m['persona_id'])
      ? [...this.people, { id: m['persona_id'], label: m['nombre'] }]
      : this.people;
  }
  close() {
    if (this.busy()) return;
    this.editing.set(false);
    queueMicrotask(() => this.focusBack?.focus());
  }
  @HostListener('document:keydown.escape') escape() {
    if (this.editing()) this.close();
    else if(this.detail())this.closeDetail();
  }
  validate() {
    if (!this.form['nombre']?.trim() || !this.form['fecha_inicio'] || !this.form['fecha_fin'])
      throw new Error('Completa el nombre y las fechas del proyecto.');
    if (this.form['fecha_fin'] < this.form['fecha_inicio'])
      throw new Error('La fecha de fin debe ser igual o posterior al inicio.');
    for (const m of this.team) {
      if (!m['persona_id'] || !m['fecha_inicio'] || !m['fecha_fin'])
        throw new Error('Completa la persona y las fechas de cada asignación.');
      if (m['fecha_fin'] < m['fecha_inicio'])
        throw new Error('Revisa las fechas de las asignaciones.');
      const percentage = Number(m['porcentaje']);
      if (
        !Number.isInteger(percentage) ||
        (m['rol_proyecto'] === 'desarrollador'
          ? ![50, 100].includes(percentage)
          : m['rol_proyecto'] === 'scrum_master'
            ? ![25, 50, 100].includes(percentage)
            : percentage < 25 || percentage > 100)
      )
        throw new Error('Revisa la dedicación permitida para cada rol.');
    }
    for (let i = 0; i < this.team.length; i++)
      for (let j = i + 1; j < this.team.length; j++) {
        const a = this.team[i],
          b = this.team[j];
        if (
          a['persona_id'] === b['persona_id'] &&
          a['fecha_inicio'] <= b['fecha_fin'] &&
          b['fecha_inicio'] <= a['fecha_fin']
        )
          throw new Error(
            'Una persona no puede tener asignaciones con fechas superpuestas en el mismo proyecto.',
          );
      }
  }
  async save() {
    this.error.set('');
    this.busy.set(true);
    try {
      this.validate();
      const payload: Row = {};
      for (const key of this.projectFields)
        payload[key] =
          this.form[key] === '' || this.form[key] === undefined ? null : this.form[key];
      payload['nombre'] = payload['nombre'].trim();
      if (!payload['atrasado']) {
        payload['atrasado_desde'] = null;
        payload['atrasado_motivo'] = null;
      }
      payload['equipo'] = this.team.map((m) => {
        const row: Row = {};
        if (m['id']) row['id'] = m['id'];
        for (const key of this.assignmentFields)
          row[key] = m[key] === '' || m[key] === undefined ? null : m[key];
        return row;
      });
      await this.api.save('proyecto', this.form['id'] ?? null, payload);
      this.editing.set(false);
      this.detail.set(null);
      this.saved.emit();
      queueMicrotask(() => this.focusBack?.focus());
    } catch (e) {
      this.error.set(e instanceof Error ? e.message : 'No se pudo guardar el proyecto.');
    } finally {
      this.busy.set(false);
    }
  }
  safeUrl(p: Row) {
    try {
      const u = new URL(p['url_tablero_devops']);
      return ['http:', 'https:'].includes(u.protocol) ? u.href : null;
    } catch {
      return null;
    }
  }
}
