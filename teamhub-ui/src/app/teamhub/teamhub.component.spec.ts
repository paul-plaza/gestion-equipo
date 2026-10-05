import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TeamHubComponent } from './teamhub.component';
import { TeamHubApi, Resource } from './api';
import { signal } from '@angular/core';
import { vi } from 'vitest';

describe('TeamHub resource forms',()=>{
 const resource:Resource={name:'logro',label:'Logros',key:'id',personal:true,managerOnly:false,readonly:false,fields:[{name:'id',label:'Id',type:'integer',required:false,options:[],reference:null,readonly:true},{name:'persona_id',label:'Persona',type:'integer',required:true,options:[],reference:'persona',readonly:false},{name:'descripcion',label:'Descripción',type:'text',required:true,options:[],reference:null,readonly:false},{name:'evidencia_url',label:'Evidencia',type:'text',required:true,options:[],reference:null,readonly:false}]};
 async function setup(){
  const api={actor:signal({id:7,role:'miembro',name:'Ana Test'}),config:signal({local:true}),save:vi.fn().mockResolvedValue({}),get:vi.fn().mockResolvedValue({items:[],total:0})};
  await TestBed.configureTestingModule({imports:[TeamHubComponent],providers:[provideRouter([]),{provide:TeamHubApi,useValue:api}]}).compileComponents();
  const component=TestBed.createComponent(TeamHubComponent).componentInstance;component.metadata.set([resource]);component.tab.set({label:'Logros',resource:'logro'});return{component,api};
 }
 it('does not submit missing required evidence',async()=>{const{component,api}=await setup();component.open();component.form['descripcion']='Aporte';await component.save();expect(api.save).not.toHaveBeenCalled();expect(component.error()).toContain('evidencia');});
 it('sends editable fields and omits server-owned identity',async()=>{const{component,api}=await setup();component.open();component.form['descripcion']='Aporte';component.form['evidencia_url']='https://example.test/evidencia';await component.save();expect(api.save).toHaveBeenCalledWith('logro',null,{persona_id:7,descripcion:'Aporte',evidencia_url:'https://example.test/evidencia'});});
});
