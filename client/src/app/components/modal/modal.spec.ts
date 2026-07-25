import { TestBed } from '@angular/core/testing';
import { Modal } from './modal';
import { CommonModule } from '@angular/common';

describe('Modal', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Modal],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(Modal);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should have default inputs', () => {
    const fixture = TestBed.createComponent(Modal);
    const modal = fixture.componentInstance;
    expect(modal.isOpen).toBe(false);
    expect(modal.title).toBe('Erro');
    expect(modal.message).toBe('');
    expect(modal.type).toBe('error');
  });

  describe('onClose', () => {
    it('should emit close event', () => {
      const fixture = TestBed.createComponent(Modal);
      const modal = fixture.componentInstance;
      let emitted = false;

      modal.close.subscribe(() => (emitted = true));
      modal.onClose();

      expect(emitted).toBe(true);
    });
  });

  describe('onBackdropClick', () => {
    it('should emit close when clicking the backdrop', () => {
      const fixture = TestBed.createComponent(Modal);
      const modal = fixture.componentInstance;
      let emitted = false;

      modal.close.subscribe(() => (emitted = true));
      modal.onBackdropClick({ target: document.body, currentTarget: document.body } as any);

      expect(emitted).toBe(true);
    });

    it('should not emit close when clicking inside modal', () => {
      const fixture = TestBed.createComponent(Modal);
      const modal = fixture.componentInstance;
      let emitted = false;

      modal.close.subscribe(() => (emitted = true));
      const child = document.createElement('div');
      modal.onBackdropClick({ target: child, currentTarget: document.body } as any);

      expect(emitted).toBe(false);
    });
  });
});
