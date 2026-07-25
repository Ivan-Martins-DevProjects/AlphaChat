import { TestBed } from '@angular/core/testing';
import { ContactInfo } from './contact-info';
import { ChatService } from '../../../services/chat';
import { vi } from 'vitest';

describe('ContactInfo', () => {
  let component: ContactInfo;
  let chatServiceMock: any;

  beforeEach(async () => {
    chatServiceMock = {
      getUsers: vi.fn().mockResolvedValue([
        { id: 'u1', name: 'Ana', email: 'ana@test.com', role: 'admin', profilePic: '' },
      ]),
      getTags: vi.fn().mockResolvedValue([
        { id: '1', name: 'VIP', colorCode: '#fff' },
      ]),
      getContactDetail: vi.fn().mockResolvedValue(null),
      getContactTickets: vi.fn().mockResolvedValue([]),
      updateContact: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [ContactInfo],
      providers: [{ provide: ChatService, useValue: chatServiceMock }],
    }).compileComponents();

    const fixture = TestBed.createComponent(ContactInfo);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('formatPhone', () => {
    it('should format 11-digit phone', () => {
      expect(component.formatPhone('11999887766')).toBe('(11) 99988-7766');
    });

    it('should format 10-digit phone', () => {
      expect(component.formatPhone('1133344556')).toBe('(11) 3334-4556');
    });

    it('should return original for short phone', () => {
      expect(component.formatPhone('123')).toBe('123');
    });

    it('should return empty for empty string', () => {
      expect(component.formatPhone('')).toBe('');
    });
  });

  describe('formatTicketStatus', () => {
    it('should format known statuses', () => {
      expect(component.formatTicketStatus('aberto')).toBe('Aberto');
      expect(component.formatTicketStatus('resolvido')).toBe('Resolvido');
      expect(component.formatTicketStatus('cancelado')).toBe('Cancelado');
    });

    it('should return raw for unknown', () => {
      expect(component.formatTicketStatus('custom')).toBe('custom');
    });
  });

  describe('formatTicketDate', () => {
    it('should format a date to pt-BR', () => {
      const result = component.formatTicketDate('2025-06-15T10:00:00Z');
      expect(result).toMatch(/\d{2}\/\d{2}\/\d{4}/);
    });
  });

  describe('getAvatar', () => {
    it('should return empty when no contact', () => {
      component.contact = null;
      expect(component.getAvatar()).toBe('');
    });

    it('should use profilePic when available', () => {
      component.contact = { contactId: 'c1', profilePic: 'http://img.com/a.png' } as any;
      expect(component.getAvatar()).toBe('http://img.com/a.png');
    });

    it('should use placeholder when no profilePic', () => {
      component.contact = { contactId: 'c1', profilePic: null } as any;
      expect(component.getAvatar()).toContain('c1');
    });
  });

  describe('addTag / removeTag', () => {
    it('should add and remove tags', () => {
      component.addTag({ id: '3', name: 'Novo', colorCode: '#111' });
      expect(component.editTags.length).toBe(1);

      component.removeTag('3');
      expect(component.editTags.length).toBe(0);
    });
  });

  describe('getFilteredUsers', () => {
    it('should return all users when no query', () => {
      component.allUsers = [
        { id: 'u1', name: 'Ana', email: 'ana@test.com', role: 'admin', profilePic: '' },
      ];
      component.userSearchQuery = '';
      expect(component.getFilteredUsers().length).toBe(1);
    });

    it('should filter by name', () => {
      component.allUsers = [
        { id: 'u1', name: 'Ana', email: 'ana@test.com', role: 'admin', profilePic: '' },
        { id: 'u2', name: 'Bruno', email: 'bruno@test.com', role: 'user', profilePic: '' },
      ];
      component.userSearchQuery = 'Ana';
      expect(component.getFilteredUsers().length).toBe(1);
      expect(component.getFilteredUsers()[0].name).toBe('Ana');
    });
  });

  it('should emit close on onClose', () => {
    let emitted = false;
    component.close.subscribe(() => (emitted = true));
    component.onClose();
    expect(emitted).toBe(true);
  });
});
