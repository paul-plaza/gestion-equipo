import { Injectable, signal } from '@angular/core';
import type { PublicClientApplication } from '@azure/msal-browser';

export interface Actor { id:number; role:string; name:string }
export interface Field {name:string;label:string;type:string;required:boolean;options:string[];reference:string|null;readonly:boolean}
export interface Resource {name:string;label:string;key:string;personal:boolean;managerOnly:boolean;readonly:boolean;fields:Field[]}
export interface Option {id:number|string;label:string}
export type Row=Record<string,any>;
@Injectable({providedIn:'root'})
export class TeamHubApi {
  readonly actor=signal<Actor|null>(null);
  readonly config=signal<any>(null);
  private msal?:PublicClientApplication;
  private async token():Promise<string|null> {
    if(!this.msal) return null;
    const account=this.msal.getActiveAccount()??this.msal.getAllAccounts()[0];
    if(!account) return null;
    return (await this.msal.acquireTokenSilent({account,scopes:[this.config().scope]})).accessToken;
  }
  async init() {
    const response=await fetch('/api/auth/config');
    if(!response.ok) throw new Error('No se pudo conectar con el servidor.');
    const cfg=await response.json();this.config.set(cfg);
    if(cfg.clientId && cfg.tenantId && cfg.scope) {
      const {PublicClientApplication}=await import('@azure/msal-browser');
      this.msal=new PublicClientApplication({auth:{clientId:cfg.clientId,authority:`https://login.microsoftonline.com/${cfg.tenantId}`,redirectUri:window.location.origin+'/auth.html'},cache:{cacheLocation:'sessionStorage'}});
      await this.msal.initialize();
    }
    try {this.actor.set(await this.get<Actor>('/me'));} catch {this.actor.set(null);}
  }
  async login(local=false) {
    if(local) await this.request('/api/auth/local','POST',{});
    else {
      if(!this.msal) throw new Error('Microsoft Entra ID aún no está configurado.');
      const result=await this.msal.loginPopup({scopes:[this.config().scope]});this.msal.setActiveAccount(result.account);
    }
    this.actor.set(await this.get<Actor>('/me'));
  }
  async logout() {
    await this.request('/api/auth/logout','POST',{});this.actor.set(null);
    if(this.msal?.getActiveAccount()) await this.msal.logoutPopup({postLogoutRedirectUri:window.location.origin});
  }
  async request<T>(url:string,method='GET',body?:unknown):Promise<T> {
    const headers:Record<string,string>={'X-TeamHub-Request':'1'};
    const token=await this.token();if(token) headers['Authorization']='Bearer '+token;
    if(body!==undefined && !(body instanceof FormData)) headers['Content-Type']='application/json';
    const response=await fetch(url,{method,headers,credentials:'include',body:body===undefined?undefined:body instanceof FormData?body:JSON.stringify(body)});
    if(!response.ok) {
      if(response.status===401) this.actor.set(null);
      const error=await response.json().catch(()=>null);
      throw new Error(error?.message??(response.status===401?'Inicia sesión para continuar.':response.status===403?'No tienes permiso para esta acción.':'No se pudo completar la operación. Intenta de nuevo.'));
    }
    if(response.status===204 || response.headers.get('content-length')==='0') return undefined as T;
    const result=await response.json().catch(()=>null);return (result?.data??result) as T;
  }
  get<T>(path:string) {return this.request<T>('/api/v2'+path);}
  save(resource:string,id:string|number|null,data:Row) {return this.request('/api/v2/resources/'+resource+(id!==null?'/'+encodeURIComponent(id):''),id!==null?'PUT':'POST',data);}
  async download(id:number) {
    const token=await this.token();const response=await fetch(`/api/v2/evidence/${id}/download`,{credentials:'include',headers:token?{Authorization:'Bearer '+token}:{}});
    if(!response.ok) throw new Error('No se pudo descargar la evidencia.');
    const blob=await response.blob();const url=URL.createObjectURL(blob);const link=document.createElement('a');link.href=url;link.download='evidencia';link.click();setTimeout(()=>URL.revokeObjectURL(url),1000);
  }
}
