import { TestBed } from '@angular/core/testing';
import { TicketMessages } from './ticket-messages';
import { ChatService } from '../../../services/chat';
import { vi } from 'vitest';

describe('TicketMessages', () => {
  let component: TicketMessages;
  let chatServiceMock: any;

  beforeEach(async () => {
    chatServiceMock = {
      getMessages: vi.fn().mockResolvedValue({
        userId: 'user-1',
        messages: [],
      }),
    };

    await TestBed.configureTestingModule({
      imports: [TicketMessages],
      providers: [{ provide: ChatService, useValue: chatServiceMock }],
    }).compileComponents();

    const fixture = TestBed.createComponent(TicketMessages);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with no messages', () => {
    expect(component.messages.length).toBe(0);
  });

  describe('formatTime', () => {
    it('should return empty for empty string', () => {
      expect(component.formatTime('')).toBe('');
    });

    it('should format a valid date', () => {
      const result = component.formatTime('2025-01-15T14:30:00Z');
      expect(result).toMatch(/^\d{2}:\d{2}$/);
    });
  });

  describe('isOwnMessage', () => {
    it('should return true for own messages', () => {
      component.currentUserId = 'user-1';
      expect(component.isOwnMessage({ senderId: 'user-1' } as any)).toBe(true);
    });

    it('should return false for others', () => {
      component.currentUserId = 'user-1';
      expect(component.isOwnMessage({ senderId: 'user-2' } as any)).toBe(false);
    });
  });
});
