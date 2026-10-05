import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
describe('App',()=>{
 it('renders the application router without the Angular starter screen',async()=>{
  await TestBed.configureTestingModule({imports:[App],providers:[provideRouter([])]}).compileComponents();
  const fixture=TestBed.createComponent(App);fixture.detectChanges();
  expect(fixture.nativeElement.querySelector('router-outlet')).not.toBeNull();
  expect(fixture.nativeElement.textContent).not.toContain('Congratulations');
 });
});
