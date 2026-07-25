import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Sidebar } from './sidebar';
import { provideRouter } from '@angular/router';

describe('Sidebar', () => {
  let component: Sidebar;
  let fixture: ComponentFixture<Sidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Sidebar],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Sidebar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with Inbox as active item', () => {
    expect(component.activeItem).toBe('Inbox');
  });

  describe('setActive', () => {
    it('should set the active item', () => {
      component.setActive('Contacts');
      expect(component.activeItem).toBe('Contacts');
    });

    it('should toggle off when clicking the same item', () => {
      component.setActive('Inbox');
      expect(component.activeItem).toBe('none');
      expect(component.activeSubItem).toBe('');
    });

    it('should clear sub item when toggling off', () => {
      component.activeSubItem = 'Todos';
      component.setActive('Inbox');
      expect(component.activeSubItem).toBe('');
    });
  });

  it('should have BodyItems and FooterItems defined', () => {
    expect(component.BodyItems.length).toBeGreaterThan(0);
    expect(component.FooterItems.length).toBeGreaterThan(0);
  });

  it('should have Inbox in BodyItems', () => {
    const ids = component.BodyItems.map(item => item.id);
    expect(ids).toContain('Inbox');
  });
});
