import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ChatContent } from './chat-content';
import { ChatService } from '../../../services/chat';
import { DateTime } from '../../../services/utils/date-time';
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

describe('ChatContent', () => {
  let component: ChatContent;
  let fixture: ComponentFixture<ChatContent>;
  let chatServiceMock: any;

  beforeEach(async () => {
    chatServiceMock = {
      selectedChat$: { subscribe: vi.fn().mockReturnValue({ unsubscribe: vi.fn() }) },
      newMessageFromWs: { subscribe: vi.fn().mockReturnValue({ unsubscribe: vi.fn() }) },
      markAsRead: vi.fn().mockResolvedValue(undefined),
      sendMessage: vi.fn().mockResolvedValue({
        id: 'msg-1',
        conversationId: 'conv-1',
        senderId: 'user-1',
        content: 'Olá',
        messageType: 'text',
        isEdited: false,
        isDeleted: false,
        createdAt: new Date().toISOString(),
        updatedAt: null,
      }),
      getMessages: vi.fn().mockResolvedValue({ userId: 'user-1', messages: [] }),
      getContactDetail: vi.fn(),
      clearHighlight: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [ChatContent],
      providers: [
        { provide: ChatService, useValue: chatServiceMock },
        { provide: DateTime, useValue: { getDate: () => '14:30' } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ChatContent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with current time from DateTime service', () => {
    expect(component.now).toBe('14:30');
  });

  describe('formatTime', () => {
    it('should return empty string for empty input', () => {
      expect(component.formatTime('')).toBe('');
    });

    it('should format a valid date string', () => {
      const result = component.formatTime('2025-01-15T14:30:00Z');
      expect(result).toMatch(/^\d{2}:\d{2}$/);
    });
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

    it('should return empty for null', () => {
      expect(component.formatPhone(null)).toBe('');
    });
  });

  describe('formatDisplayName', () => {
    it('should return fallback for empty name', () => {
      const ticket = makeTicket({ contactDisplayName: '' });
      expect(component.formatDisplayName(ticket)).toBe('Contato sem identificação');
    });

    it('should return name when not a phone', () => {
      const ticket = makeTicket({ contactDisplayName: 'Maria' });
      expect(component.formatDisplayName(ticket)).toBe('Maria');
    });

    it('should format as phone when all digits', () => {
      const ticket = makeTicket({ contactDisplayName: '11999887766' });
      expect(component.formatDisplayName(ticket)).toBe('(11) 99988-7766');
    });
  });

  describe('getContactAvatar', () => {
    it('should return empty when no active chat', () => {
      component.activeChat = null;
      expect(component.getContactAvatar()).toBe('');
    });

    it('should use profilePic when available', () => {
      component.activeChat = makeTicket({ contactProfilePic: 'http://img.com/a.png' });
      expect(component.getContactAvatar()).toBe('http://img.com/a.png');
    });

    it('should use placeholder when no profilePic', () => {
      component.activeChat = makeTicket({ contactId: 'c-42' });
      expect(component.getContactAvatar()).toContain('c-42');
    });
  });

  describe('isOwnMessage', () => {
    it('should return true for own messages', () => {
      component.currentUserId = 'user-1';
      expect(component.isOwnMessage({ senderId: 'user-1' } as any)).toBe(true);
    });

    it('should return false for other messages', () => {
      component.currentUserId = 'user-1';
      expect(component.isOwnMessage({ senderId: 'user-2' } as any)).toBe(false);
    });
  });

  describe('toggleContactInfo', () => {
    it('should toggle showContactInfo', () => {
      expect(component.showContactInfo).toBe(false);
      component.toggleContactInfo();
      expect(component.showContactInfo).toBe(true);
      component.toggleContactInfo();
      expect(component.showContactInfo).toBe(false);
    });
  });
});
