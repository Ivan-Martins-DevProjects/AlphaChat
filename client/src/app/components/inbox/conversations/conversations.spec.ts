import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Conversations } from './conversations';
import { ChatService } from '../../../services/chat';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { TicketCard } from '../../../models/ticket-card';

function makeTicket(overrides: Partial<TicketCard> = {}): TicketCard {
  return {
    conversationId: 'conv-1',
    conversationType: null,
    owner: null,
    ticketId: 't-1',
    subject: 'Teste',
    status: 'open',
    contactId: 'c-1',
    contactDisplayName: 'João',
    contactPhone: null,
    contactProfilePic: null,
    tags: [],
    lastMessageContent: null,
    lastMessageCreatedAt: null,
    read: true,
    ...overrides,
  };
}

describe('Conversations', () => {
  let component: Conversations;
  let fixture: ComponentFixture<Conversations>;
  let chatServiceMock: any;

  beforeEach(async () => {
    chatServiceMock = {
      highlightChanged: { subscribe: vi.fn().mockReturnValue({ unsubscribe: vi.fn() }) },
      newMessageFromWs: { subscribe: vi.fn().mockReturnValue({ unsubscribe: vi.fn() }) },
      isHighlighted: vi.fn().mockReturnValue(false),
      updateTicketLastMessage: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [Conversations],
      providers: [
        provideRouter([]),
        { provide: ChatService, useValue: chatServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Conversations);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('formatTime', () => {
    it('should return empty string for null', () => {
      expect(component.formatTime(null)).toBe('');
    });

    it('should return "Agora" for recent messages', () => {
      const now = new Date().toISOString();
      expect(component.formatTime(now)).toBe('Agora');
    });

    it('should return minutes for messages < 1h', () => {
      const date = new Date(Date.now() - 30 * 60000).toISOString();
      expect(component.formatTime(date)).toBe('30min');
    });

    it('should return hours for messages < 24h', () => {
      const date = new Date(Date.now() - 3 * 3600000).toISOString();
      expect(component.formatTime(date)).toBe('3h');
    });

    it('should return days for messages < 7d', () => {
      const date = new Date(Date.now() - 5 * 86400000).toISOString();
      expect(component.formatTime(date)).toBe('5d');
    });
  });

  describe('formatStatus', () => {
    it('should format known statuses', () => {
      expect(component.formatStatus('open')).toBe('Aberto');
      expect(component.formatStatus('resolved')).toBe('Resolvido');
      expect(component.formatStatus('cancelled')).toBe('Cancelado');
      expect(component.formatStatus('pending')).toBe('Pendente');
      expect(component.formatStatus('aberto')).toBe('Aberto');
    });

    it('should return raw status for unknown', () => {
      expect(component.formatStatus('custom')).toBe('custom');
    });
  });

  describe('getStatusColor', () => {
    it('should return colors for known statuses', () => {
      expect(component.getStatusColor('open')).toBe('#16a34a');
      expect(component.getStatusColor('pending')).toBe('#ca8a04');
      expect(component.getStatusColor('closed')).toBe('#64748b');
    });

    it('should return default for unknown', () => {
      expect(component.getStatusColor('unknown')).toBe('#64748b');
    });
  });

  describe('formatPhone', () => {
    it('should format 11-digit phone', () => {
      expect(component.formatPhone('11999887766')).toBe('(11) 99988-7766');
    });

    it('should format 10-digit phone', () => {
      expect(component.formatPhone('1133344556')).toBe('(11) 3334-4556');
    });

    it('should return original for other lengths', () => {
      expect(component.formatPhone('123')).toBe('123');
    });

    it('should return empty for null', () => {
      expect(component.formatPhone(null)).toBe('');
    });
  });

  describe('formatDisplayName', () => {
    it('should return name when not a phone number', () => {
      const ticket = makeTicket({ contactDisplayName: 'Maria' });
      expect(component.formatDisplayName(ticket)).toBe('Maria');
    });

    it('should format as phone when all digits and >= 10', () => {
      const ticket = makeTicket({ contactDisplayName: '11999887766' });
      expect(component.formatDisplayName(ticket)).toBe('(11) 99988-7766');
    });

    it('should return fallback for empty name', () => {
      const ticket = makeTicket({ contactDisplayName: '' });
      expect(component.formatDisplayName(ticket)).toBe('Contato sem identificação');
    });
  });

  describe('getValidTags', () => {
    it('should filter tags with empty name', () => {
      const ticket = makeTicket({
        tags: [
          { id: '1', name: 'VIP', colorCode: '#fff' },
          { id: '2', name: '', colorCode: '#000' },
          { id: '3', name: '  ', colorCode: '#111' },
        ],
      });
      expect(component.getValidTags(ticket).length).toBe(1);
      expect(component.getValidTags(ticket)[0].name).toBe('VIP');
    });
  });

  describe('getAvatar', () => {
    it('should use profilePic when available', () => {
      const ticket = makeTicket({ contactProfilePic: 'http://img.com/a.png' });
      expect(component.getAvatar(ticket)).toBe('http://img.com/a.png');
    });

    it('should use placeholder when no profilePic', () => {
      const ticket = makeTicket({ contactId: 'c-42' });
      expect(component.getAvatar(ticket)).toContain('c-42');
    });
  });
});
