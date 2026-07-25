import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { provideRouter } from '@angular/router';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should start with sidebar closed', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance.sidebarOpen).toBe(false);
  });

  it('should toggle sidebar', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.toggleSidebar();
    expect(app.sidebarOpen).toBe(true);

    app.toggleSidebar();
    expect(app.sidebarOpen).toBe(false);
  });
});
