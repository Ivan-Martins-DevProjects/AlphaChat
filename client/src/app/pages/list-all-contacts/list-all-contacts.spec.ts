import { TestBed } from '@angular/core/testing';
import { ListAllContacts } from './list-all-contacts';
import { provideRouter } from '@angular/router';
import { ChatService } from '../../services/chat';
import { vi } from 'vitest';

describe('ListAllContacts', () => {
  let chatServiceMock: any;

  beforeEach(async () => {
    chatServiceMock = {
      getTags: vi.fn().mockResolvedValue([
        { id: '1', name: 'VIP', colorCode: '#fff' },
      ]),
    };

    await TestBed.configureTestingModule({
      imports: [ListAllContacts],
      providers: [
        provideRouter([]),
        { provide: ChatService, useValue: chatServiceMock },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(ListAllContacts);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should start with loading state', () => {
    const fixture = TestBed.createComponent(ListAllContacts);
    const comp = fixture.componentInstance;
    expect(comp.isLoading).toBe(true);
  });

  describe('formatPhone', () => {
    it('should format 11-digit phone', () => {
      const fixture = TestBed.createComponent(ListAllContacts);
      expect(fixture.componentInstance.formatPhone('11999887766')).toBe('(11) 99988-7766');
    });

    it('should format 10-digit phone', () => {
      const fixture = TestBed.createComponent(ListAllContacts);
      expect(fixture.componentInstance.formatPhone('1133344556')).toBe('(11) 3334-4556');
    });

    it('should return dash for null', () => {
      const fixture = TestBed.createComponent(ListAllContacts);
      expect(fixture.componentInstance.formatPhone(null)).toBe('-');
    });
  });

  describe('hasActiveFilters', () => {
    it('should return false when no filters', () => {
      const fixture = TestBed.createComponent(ListAllContacts);
      const comp = fixture.componentInstance;
      comp.searchQuery = '';
      comp.selectedCompany = '';
      comp.selectedTagId = '';
      expect(comp.hasActiveFilters()).toBe(false);
    });

    it('should return true when searchQuery is set', () => {
      const fixture = TestBed.createComponent(ListAllContacts);
      const comp = fixture.componentInstance;
      comp.searchQuery = 'test';
      expect(comp.hasActiveFilters()).toBe(true);
    });
  });

  describe('getAvatar', () => {
    it('should use profilePic when available', () => {
      const fixture = TestBed.createComponent(ListAllContacts);
      const contact = { contactId: 'c1', profilePic: 'http://img.com/a.png' } as any;
      expect(fixture.componentInstance.getAvatar(contact)).toBe('http://img.com/a.png');
    });

    it('should use placeholder when no profilePic', () => {
      const fixture = TestBed.createComponent(ListAllContacts);
      const contact = { contactId: 'c1', profilePic: null } as any;
      expect(fixture.componentInstance.getAvatar(contact)).toContain('c1');
    });
  });
});
