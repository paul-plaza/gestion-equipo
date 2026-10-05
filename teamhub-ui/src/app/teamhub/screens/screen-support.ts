import { Injectable, inject } from '@angular/core';
import { TeamHubApi, Row } from '../api';
export interface ScreenAction { resource:string; row?:Row; detail?:boolean; defaults?:Row; }
export interface ScreenNavigation { module:string; id?:number|string; tab?:string; }
@Injectable({providedIn:'root'})
export class ScreenData {
 readonly api=inject(TeamHubApi);
 async list(resource:string):Promise<Row[]> {
  const rows:Row[]=[];
  for(let page=1;;page++){
   const result=await this.api.get<{items:Row[];total:number}>(`/resources/${resource}?pageSize=100&page=${page}`);
   rows.push(...result.items);
   if(!result.items.length||rows.length>=result.total)return rows;
  }
 }
 report(name:string,year=Number(today().slice(0,4))){return this.api.get<Row[]>(`/reports/${name}?year=${year}`);}
}
export function today(){return new Intl.DateTimeFormat('en-CA',{timeZone:'America/Guayaquil',year:'numeric',month:'2-digit',day:'2-digit'}).format(new Date());}
export function dateLabel(value:unknown){if(!value)return '—';const date=new Date(String(value).slice(0,10)+'T12:00:00Z');return Number.isNaN(date.getTime())?'—':new Intl.DateTimeFormat('es-EC',{day:'numeric',month:'short',year:'numeric',timeZone:'UTC'}).format(date);}
export function personName(p:Row){return p['nombre']??p['persona']??[p['nombres'],p['apellidos']].filter(Boolean).join(' ');}
export function initials(name:string){return name.split(' ').filter(Boolean).slice(0,2).map(n=>n[0]).join('');}
