import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { TeamHubApi } from '../api';
import { ScreenData, dateLabel, personName } from './screen-support';

describe('Datos de pantallas',()=>{
 it('carga todas las páginas para no truncar matrices y resúmenes',async()=>{
  const get=vi.fn().mockResolvedValueOnce({items:[{id:1}],total:2}).mockResolvedValueOnce({items:[{id:2}],total:2});
  TestBed.configureTestingModule({providers:[{provide:TeamHubApi,useValue:{get}}]});
  const data=TestBed.inject(ScreenData);
  expect(await data.list('persona')).toEqual([{id:1},{id:2}]);
  expect(get).toHaveBeenLastCalledWith('/resources/persona?pageSize=100&page=2');
 });
 it('termina si una página queda vacía por cambios concurrentes',async()=>{
  const get=vi.fn().mockResolvedValueOnce({items:[{id:1}],total:3}).mockResolvedValueOnce({items:[],total:0});
  TestBed.configureTestingModule({providers:[{provide:TeamHubApi,useValue:{get}}]});
  expect(await TestBed.inject(ScreenData).list('persona')).toEqual([{id:1}]);
  expect(get).toHaveBeenCalledTimes(2);
 });
 it('muestra fechas incompletas sin romper la pantalla',()=>{expect(dateLabel(null)).toBe('—');expect(dateLabel('invalid')).toBe('—');});
 it('resuelve nombres de personas tanto de recursos como de informes',()=>{expect(personName({nombres:'Ana',apellidos:'Vera'})).toBe('Ana Vera');expect(personName({persona:'Ana Vera'})).toBe('Ana Vera');});
});
