import { Component, Input, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ChatService } from '../../../services/chat';
import { environment } from '../../../../environments/environment.development';

@Component({
  selector: 'app-send-message-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './send-message-modal.html',
  styleUrl: './send-message-modal.css',
})
export class SendMessageModal {
  @Input() contactId: string = '';
  @Output() close = new EventEmitter<void>();
  @Output() sent = new EventEmitter<void>();

  message = '';
  isSending = false;
  error = '';

  showOpenTicketDialog = false;
  openTicketConversationId = '';

  constructor(
    private chatService: ChatService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  onClose() {
    this.close.emit();
  }

  onBackdropClick(event: Event) {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }

  async send() {
    if (!this.message.trim() || !this.contactId || this.isSending) return;

    this.isSending = true;
    this.cdr.detectChanges();

    try {
      const checkResponse = await fetch(
        `${environment.apiUrl}/api/contact/${this.contactId}/open-ticket`,
        { credentials: 'include' }
      );

      if (checkResponse.ok) {
        const checkData = await checkResponse.json();

        if (checkData.exists) {
          this.openTicketConversationId = checkData.conversationId;
          this.showOpenTicketDialog = true;
          this.isSending = false;
          this.cdr.detectChanges();
          return;
        }
      }

      await this.createConversationAndSend();
    } catch (err) {
      this.error = 'Erro ao verificar tickets';
      console.error('[SendMessageModal] Erro:', err);
      this.isSending = false;
      this.cdr.detectChanges();
    }
  }

  async createConversationAndSend() {
    try {
      const response = await fetch(`${environment.apiUrl}/api/chat/conversation`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ contactId: this.contactId }),
      });

      if (!response.ok) {
        throw new Error(`Erro ${response.status}`);
      }

      const data = await response.json();
      const conversationId = data.conversationId;

      await this.chatService.sendMessage(conversationId, this.message.trim());

      this.sent.emit();
      this.router.navigate(['/home'], { queryParams: { conversationId } });
      this.onClose();
    } catch (err) {
      this.error = 'Erro ao criar conversa';
      console.error('[SendMessageModal] Erro ao criar conversa:', err);
    } finally {
      this.isSending = false;
      this.cdr.detectChanges();
    }
  }

  async goToExistingConversation() {
    this.showOpenTicketDialog = false;
    this.isSending = true;
    this.cdr.detectChanges();

    try {
      await this.chatService.sendMessage(this.openTicketConversationId, this.message.trim());

      this.sent.emit();
      this.router.navigate(['/home'], { queryParams: { conversationId: this.openTicketConversationId } });
      this.onClose();
    } catch (err) {
      this.error = 'Erro ao enviar mensagem';
      console.error('[SendMessageModal] Erro ao enviar:', err);
    } finally {
      this.isSending = false;
      this.cdr.detectChanges();
    }
  }

  cancelOpenTicketDialog() {
    this.showOpenTicketDialog = false;
    this.openTicketConversationId = '';
    this.cdr.detectChanges();
  }
}
