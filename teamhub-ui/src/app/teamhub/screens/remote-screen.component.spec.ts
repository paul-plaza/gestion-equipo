import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { RemoteScreenComponent } from './remote-screen.component';
import { ScreenData } from './screen-support';

describe('Planificador de teletrabajo',()=>{
 const person={id:2,nombres:'Ana',apellidos:'Vera',estado:'activo',piso:3,jefe_directo_id:1};
 const save=vi.fn();
 const actor=signal({id:1,role:'jefe_area',name:'Jefe'});
 function setup(){TestBed.configureTestingModule({providers:[{provide:ScreenData,useValue:{api:{actor,save},list:vi.fn().mockResolvedValue([])}}]});const screen=TestBed.runInInjectionContext(()=>new RemoteScreenComponent());screen.week.set('2026-10-05');screen.records.set({persona:[person],teletrabajo_plan:[],teletrabajo_ampliacion:[],ausencia:[]});return screen;}
 beforeEach(()=>{save.mockReset();actor.set({id:1,role:'jefe_area',name:'Jefe'});});
 it('crea una reserva y conserva su identificador al alternar modalidad',async()=>{const screen=setup();save.mockResolvedValue({id:19});await screen.toggle(person,'2026-10-05');expect(save).toHaveBeenLastCalledWith('teletrabajo_plan',null,{persona_id:2,fecha:'2026-10-05',modalidad:'oficina'});await screen.toggle(person,'2026-10-05');expect(save).toHaveBeenLastCalledWith('teletrabajo_plan',19,{persona_id:2,fecha:'2026-10-05',modalidad:'teletrabajo'});expect(screen.remote(person)).toBe(1);});
 it('mantiene la reserva ante rechazo de cupo del servidor',async()=>{const screen=setup();screen.records.update(r=>({...r,teletrabajo_plan:[{id:19,persona_id:2,fecha:'2026-10-05',modalidad:'oficina'}]}));save.mockRejectedValue(new Error('Máximo 2 días'));await screen.toggle(person,'2026-10-05');expect(screen.mode(person,'2026-10-05')).toBe('oficina');expect(screen.notice()).toBe('Máximo 2 días');expect(screen.saving()).toBe('');});
 it('permite aprobar solo al jefe directo y respeta el día autorizado',()=>{const screen=setup();expect(screen.canApprove(person)).toBe(true);actor.set({id:7,role:'jefe_desarrollo',name:'Otro jefe'});expect(screen.canApprove(person)).toBe(false);screen.records.update(r=>({...r,teletrabajo_ampliacion:[{persona_id:2,fecha_inicio:'2026-10-06',fecha_fin:'2026-10-09',dia_semana:1}]}));expect(screen.quota(person)).toBe(2);screen.records.update(r=>({...r,teletrabajo_ampliacion:[{persona_id:2,fecha_inicio:'2026-10-06',fecha_fin:'2026-10-09',dia_semana:3}]}));expect(screen.quota(person)).toBe(3);});
});
