import { TestBed } from '@angular/core/testing';
import { EditContactModal } from './edit-contact-modal';
import { ChatService } from '../../../services/chat';
import { vi } from 'vitest';

describe('EditContactModal', () => {
  let component: EditContactModal;
  let chatServiceMock: any;

  beforeEach(async () => {
    chatServiceMock = {
      getTags: vi.fn().mockResolvedValue([
        { id: '1', name: 'VIP', colorCode: '#fff' },
      ]),
      getContactTickets: vi.fn().mockResolvedValue([]),
      getContactDetail: vi.fn(),
      updateContact: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [EditContactModal],
      providers: [{ provide: ChatService, useValue: chatServiceMock }],
    }).compileComponents();

    const fixture = TestBed.createComponent(EditContactModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('populateFields', () => {
    it('should populate fields from contact', () => {
      component.contact = {
        name: 'Maria',
        phone: '11999887766',
        email: 'maria@test.com',
        company: 'Acme',
        document: '123',
        notes: 'nota',
        tags: [{ id: '1', name: 'VIP', colorCode: '#fff' }],
      } as any;

      component.populateFields();

      expect(component.editName).toBe('Maria');
      expect(component.editPhone).toBe('11999887766');
      expect(component.editEmail).toBe('maria@test.com');
      expect(component.editCompany).toBe('Acme');
      expect(component.editNotes).toBe('nota');
    });

    it('should handle null contact', () => {
      component.contact = null;
      expect(() => component.populateFields()).not.toThrow();
    });
  });

  describe('addTag / removeTag', () => {
    it('should add and remove tags', () => {
      component.addTag({ id: '3', name: 'Novo', colorCode: '#111' });
      expect(component.editTags.length).toBe(1);

      component.removeTag('3');
      expect(component.editTags.length).toBe(0);
    });

    it('should not add duplicate tags', () => {
      component.addTag({ id: '3', name: 'Novo', colorCode: '#111' });
      component.addTag({ id: '3', name: 'Novo', colorCode: '#111' });
      expect(component.editTags.length).toBe(1);
    });
  });

  it('should emit close on onClose', () => {
    let emitted = false;
    component.close.subscribe(() => (emitted = true));
    component.onClose();
    expect(emitted).toBe(true);
  });
});
