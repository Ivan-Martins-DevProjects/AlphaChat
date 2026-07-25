import { TestBed } from '@angular/core/testing';
import { ChatService } from './chat';
import { WebSocketService } from './websocket';
import { vi } from 'vitest';
import { TicketCard } from '../models/ticket-card';

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

describe('ChatService', () => {
  let service: ChatService;
  let wsService: any;

  beforeEach(() => {
    wsService = { messageCreated: { subscribe: vi.fn() } };

    TestBed.configureTestingModule({
      providers: [
        ChatService,
        { provide: WebSocketService, useValue: wsService },
      ],
    });

    service = TestBed.inject(ChatService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('setConversation', () => {
    it('should emit the selected chat', () => {
      const ticket = makeTicket();
      let received: TicketCard | null = null;

      service.selectedChat$.subscribe(c => (received = c));
      service.setConversation(ticket);

      expect(received).toBe(ticket);
    });

    it('should clear highlight for the selected conversation', () => {
      const ticket = makeTicket({ conversationId: 'conv-42' });
      // manually add highlight via WS emulation
      (service as any).highlightedConversations.add('conv-42');

      service.setConversation(ticket);

      expect(service.isHighlighted('conv-42')).toBe(false);
    });

    it('should emit highlightChanged when selecting a chat', () => {
      const ticket = makeTicket({ conversationId: 'conv-99' });
      let emitted = '';

      service.highlightChanged.subscribe(id => (emitted = id));
      service.setConversation(ticket);

      expect(emitted).toBe('conv-99');
    });
  });

  describe('clearHighlight', () => {
    it('should remove a conversation from highlights', () => {
      (service as any).highlightedConversations.add('conv-1');
      service.clearHighlight('conv-1');
      expect(service.isHighlighted('conv-1')).toBe(false);
    });

    it('should emit highlightChanged', () => {
      let emitted = '';
      service.highlightChanged.subscribe(id => (emitted = id));
      service.clearHighlight('conv-5');
      expect(emitted).toBe('conv-5');
    });
  });

  describe('isHighlighted', () => {
    it('should return false for unknown conversation', () => {
      expect(service.isHighlighted('unknown')).toBe(false);
    });

    it('should return true after manual add', () => {
      (service as any).highlightedConversations.add('conv-x');
      expect(service.isHighlighted('conv-x')).toBe(true);
    });
  });

  describe('updateTicketLastMessage', () => {
    it('should set lastMessageContent and lastMessageCreatedAt', () => {
      const ticket = makeTicket();
      service.updateTicketLastMessage(ticket, 'Olá', '2025-01-01T12:00:00Z');

      expect(ticket.lastMessageContent).toBe('Olá');
      expect(ticket.lastMessageCreatedAt).toBe('2025-01-01T12:00:00Z');
    });
  });
});
