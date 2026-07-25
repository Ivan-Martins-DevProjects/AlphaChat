import { Component, Input, OnInit, ElementRef, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatService, MessageRow } from '../../../services/chat';

@Component({
  selector: 'app-ticket-messages',
  imports: [CommonModule],
  standalone: true,
  templateUrl: './ticket-messages.html',
  styleUrl: './ticket-messages.css',
})
export class TicketMessages implements OnInit {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;

  @Input() conversationId: string = ''

  messages: MessageRow[] = []
  currentUserId: string = ''
  isLoading = false
  hasMore = true
  private offset = 0
  private readonly PAGE_SIZE = 20

  constructor(
    private chatService: ChatService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (this.conversationId) {
      this.loadMessages()
    }
  }

  async loadMessages() {
    if (this.isLoading || !this.hasMore) return

    this.isLoading = true
    this.cdr.detectChanges()

    try {
      const response = await this.chatService.getMessages(this.conversationId, this.offset)
      this.currentUserId = response.userId

      const newMessages = (response.messages || []).reverse()

      if (newMessages.length < this.PAGE_SIZE) {
        this.hasMore = false
      }

      this.messages = [...this.messages, ...newMessages]
      this.offset += newMessages.length

      this.cdr.detectChanges()
      setTimeout(() => this.scrollToBottom(), 0)
    } catch (err) {
      console.error('[TicketMessages] Erro ao carregar mensagens:', err)
    } finally {
      this.isLoading = false
      this.cdr.detectChanges()
    }
  }

  onScroll() {
    const container = this.messagesContainer?.nativeElement
    if (!container) return

    if (container.scrollTop < 100 && this.hasMore && !this.isLoading) {
      this.loadMessages()
    }
  }

  scrollToBottom() {
    const container = this.messagesContainer?.nativeElement
    if (container) {
      container.scrollTop = container.scrollHeight
    }
  }

  isOwnMessage(message: MessageRow): boolean {
    return message.senderId === this.currentUserId
  }

  formatTime(dateString: string): string {
    if (!dateString) return ''
    const date = new Date(dateString)
    return date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
  }
}
