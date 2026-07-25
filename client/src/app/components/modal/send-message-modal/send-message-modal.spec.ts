import { TestBed } from '@angular/core/testing';
import { SendMessageModal } from './send-message-modal';
import { ChatService } from '../../../services/chat';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';

describe('SendMessageModal', () => {
  let component: SendMessageModal;
  let chatServiceMock: any;

  beforeEach(async () => {
    chatServiceMock = {
      sendMessage: vi.fn().mockResolvedValue({ id: 'msg-1' }),
    };

    await TestBed.configureTestingModule({
      imports: [SendMessageModal],
      providers: [
        provideRouter([]),
        { provide: ChatService, useValue: chatServiceMock },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(SendMessageModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with empty message', () => {
    expect(component.message).toBe('');
  });

  it('should emit close on onClose', () => {
    let emitted = false;
    component.close.subscribe(() => (emitted = true));
    component.onClose();
    expect(emitted).toBe(true);
  });

  describe('cancelOpenTicketDialog', () => {
    it('should reset open ticket dialog state', () => {
      component.showOpenTicketDialog = true;
      component.openTicketConversationId = 'conv-123';

      component.cancelOpenTicketDialog();

      expect(component.showOpenTicketDialog).toBe(false);
      expect(component.openTicketConversationId).toBe('');
    });
  });

  it('should emit close on backdrop click', () => {
    let emitted = false;
    component.close.subscribe(() => (emitted = true));
    component.onBackdropClick({ target: document.body, currentTarget: document.body } as any);
    expect(emitted).toBe(true);
  });
});
