import { Component, OnInit, OnDestroy, ElementRef, ViewChild, ChangeDetectorRef, Output, EventEmitter, Input, OnChanges, SimpleChanges } from '@angular/core';
import { DateTime } from '../../../services/utils/date-time';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { ChatService, MessageRow } from '../../../services/chat';
import { TicketCard } from '../../../models/ticket-card';
import { MessageCreatedEvent } from '../../../services/websocket';
import { ContactInfo } from '../contact-info/contact-info';

@Component({
  selector: 'app-chat-content',
  imports: [FormsModule, CommonModule, ContactInfo],
  standalone: true,
  templateUrl: './chat-content.html',
  styleUrl: './chat-content.css',
})
export class ChatContent implements OnInit, OnDestroy, OnChanges {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;
  @Output() contactUpdated = new EventEmitter<void>();
  @Input() initialConversationId: string | null = null;

  now: string = ""
  inputMessage: string = ""
  activeChat: TicketCard | null = null
  showContactInfo = false
  messages: MessageRow[] = []
  currentUserId: string = ''
  isLoadingMessages = false
  hasMoreMessages = true
  private offset = 0
  private readonly PAGE_SIZE = 20
  private newMessageIds = new Set<string>()
  private subs: Subscription[] = []

  constructor(
    private dateService: DateTime,
    private chatService: ChatService,
    private cdr: ChangeDetectorRef
  ) {
    this.now = this.dateService.getDate()
  }

  ngOnInit(): void {
    this.subs.push(
      this.chatService.selectedChat$.subscribe(chat => {
        this.activeChat = chat
        this.inputMessage = ''
        this.messages = []
        this.offset = 0
        this.hasMoreMessages = true
        this.newMessageIds.clear()
        this.cdr.detectChanges()

        if (chat) {
          this.loadMessages()
          this.chatService.markAsRead(chat.conversationId).then(() => {
            chat.read = true
            this.cdr.detectChanges()
          })
        }
      }),
      this.chatService.newMessageFromWs.subscribe(event => {
        this.handleNewMessage(event)
      })
    )
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe())
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['initialConversationId'] && changes['initialConversationId'].currentValue) {
      this.loadConversationFromUrl(changes['initialConversationId'].currentValue);
    }
  }

  private async loadConversationFromUrl(conversationId: string) {
    try {
      const detail = await this.chatService.getContactDetail(conversationId);

      const ticket: TicketCard = {
        conversationId: conversationId,
        conversationType: null,
        owner: detail.owner?.id ?? null,
        ticketId: detail.ticket?.id ?? '',
        subject: detail.ticket?.subject ?? '',
        status: detail.ticket?.status ?? 'open',
        contactId: detail.contactId,
        contactDisplayName: detail.name,
        contactPhone: detail.phone,
        contactProfilePic: detail.profilePic,
        tags: detail.tags ?? [],
        lastMessageContent: null,
        lastMessageCreatedAt: null,
        read: true,
      };

      this.chatService.setConversation(ticket);
    } catch (err) {
      console.error('[ChatContent] Erro ao carregar conversa pela URL:', err);
    }
  }

  private handleNewMessage(event: MessageCreatedEvent) {
    if (!this.activeChat) return
    if (event.conversationId !== this.activeChat.conversationId) return

    const existing = this.messages.find(m => m.id === event.messageId)
    if (existing) return

    const newMsg: MessageRow = {
      id: event.messageId,
      conversationId: event.conversationId,
      senderId: event.senderId,
      content: event.content,
      messageType: event.messageType,
      isEdited: false,
      isDeleted: false,
      createdAt: event.createdAt,
      updatedAt: null,
    }

    this.messages = [...this.messages, newMsg]
    this.newMessageIds.add(event.messageId)
    this.cdr.detectChanges()

    setTimeout(() => {
      this.scrollToBottom()
      setTimeout(() => {
        this.newMessageIds.delete(event.messageId)
        this.cdr.detectChanges()
      }, 2000)
    }, 0)
  }

  isNewMessage(messageId: string): boolean {
    return this.newMessageIds.has(messageId)
  }

  async loadMessages() {
    if (!this.activeChat || this.isLoadingMessages || !this.hasMoreMessages) return

    this.isLoadingMessages = true
    this.cdr.detectChanges()

    try {
      const response = await this.chatService.getMessages(
        this.activeChat.conversationId,
        this.offset
      )

      this.currentUserId = response.userId

      const newMessages = (response.messages || []).reverse()

      if (newMessages.length < this.PAGE_SIZE) {
        this.hasMoreMessages = false
      }

      this.messages = [...this.messages, ...newMessages]
      this.offset += newMessages.length

      this.cdr.detectChanges()
      setTimeout(() => this.scrollToBottom(), 0)
    } catch (err) {
      console.error('[ChatContent] Erro ao carregar mensagens:', err)
    } finally {
      this.isLoadingMessages = false
      this.cdr.detectChanges()
    }
  }

  onScroll() {
    const container = this.messagesContainer?.nativeElement
    if (!container) return

    if (container.scrollTop < 100 && this.hasMoreMessages && !this.isLoadingMessages) {
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

  async addNewMessage() {
    if (!this.activeChat || !this.inputMessage.trim()) return

    const content = this.inputMessage.trim()
    this.inputMessage = ''

    try {
      const message = await this.chatService.sendMessage(this.activeChat.conversationId, content)

      this.messages = [...this.messages, message]
      this.newMessageIds.add(message.id)

      this.activeChat.lastMessageContent = message.content
      this.activeChat.lastMessageCreatedAt = message.createdAt
      this.activeChat.read = true

      this.chatService.clearHighlight(this.activeChat.conversationId)

      this.cdr.detectChanges()
      setTimeout(() => {
        this.scrollToBottom()
        setTimeout(() => {
          this.newMessageIds.delete(message.id)
          this.cdr.detectChanges()
        }, 2000)
      }, 0)
    } catch (err) {
      console.error('[ChatContent] Erro ao enviar mensagem:', err)
      this.inputMessage = content
    }
  }

  formatPhone(phone: string | null): string {
    if (!phone) return '';
    const digits = phone.replace(/\D/g, '');
    if (digits.length === 11) {
      return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
    }
    if (digits.length === 10) {
      return `(${digits.slice(0, 2)}) ${digits.slice(2, 6)}-${digits.slice(6)}`;
    }
    return phone;
  }

  formatDisplayName(ticket: TicketCard): string {
    const name = ticket.contactDisplayName;
    if (!name) return 'Contato sem identificação';
    const digits = name.replace(/\D/g, '');
    if (digits.length >= 10 && /^\d+$/.test(digits)) {
      return this.formatPhone(digits);
    }
    return name;
  }

  getContactAvatar(): string {
    if (!this.activeChat) return ''
    return this.activeChat.contactProfilePic || `https://i.pravatar.cc/150?u=${this.activeChat.contactId}`
  }

  toggleContactInfo() {
    console.log('[ChatContent] toggleContactInfo, current:', this.showContactInfo)
    this.showContactInfo = !this.showContactInfo
    console.log('[ChatContent] showContactInfo agora:', this.showContactInfo)
    this.cdr.detectChanges()
  }

  closeContactInfo() {
    this.showContactInfo = false
    this.cdr.detectChanges()
  }

  onContactSaved() {
    this.contactUpdated.emit()
  }
}
