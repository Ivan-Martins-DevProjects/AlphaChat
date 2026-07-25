import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { TicketCard, TagItem } from '../models/ticket-card';
import { environment } from '../../environments/environment.development';
import { MessageCreatedEvent, WebSocketService } from './websocket';

export interface MessageRow {
  id: string;
  conversationId: string;
  senderId: string;
  content: string;
  messageType: string;
  isEdited: boolean;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface ChatMessagesResponse {
  userId: string;
  messages: MessageRow[];
}

export interface ContactDetail {
  contactId: string;
  name: string;
  phone: string | null;
  email: string | null;
  company: string | null;
  document: string | null;
  notes: string | null;
  profilePic: string | null;
  contactOwnerId: string | null;
  createdAt: string;
  updatedAt: string;
  tags: TagItem[];
  owner: {
    id: string;
    name: string;
    profilePic: string | null;
  } | null;
  ticket: {
    id: string;
    subject: string;
    status: string;
    createdAt: string;
  };
}

export interface PlatformUser {
  id: string;
  name: string;
  email: string;
  role: string;
  profilePic: string;
}

export interface ContactTicket {
  ticketId: string;
  subject: string;
  status: string;
  createdAt: string;
  closedAt: string | null;
  conversationId: string;
}

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private selectedChat = new BehaviorSubject<TicketCard | null>(null);
  private highlightedConversations = new Set<string>();
  private highlightChanged$ = new Subject<string>();
  private newMessageFromWs$ = new Subject<MessageCreatedEvent>();

  selectedChat$ = this.selectedChat.asObservable();
  highlightChanged = this.highlightChanged$.asObservable();
  newMessageFromWs = this.newMessageFromWs$.asObservable();

  constructor(private wsService: WebSocketService) {
    this.wsService.messageCreated.subscribe(event => {
      console.log('[ChatService] messageCreated recebido:', event);
      this.highlightedConversations.add(event.conversationId);
      this.highlightChanged$.next(event.conversationId);
      this.newMessageFromWs$.next(event);
    });
  }

  setConversation(chat: TicketCard) {
    this.selectedChat.next(chat);

    if (chat) {
      this.highlightedConversations.delete(chat.conversationId);
      this.highlightChanged$.next(chat.conversationId);
    }
  }

  clearHighlight(conversationId: string) {
    this.highlightedConversations.delete(conversationId);
    this.highlightChanged$.next(conversationId);
  }

  isHighlighted(conversationId: string): boolean {
    return this.highlightedConversations.has(conversationId);
  }

  updateTicketLastMessage(ticket: TicketCard, content: string, createdAt: string) {
    ticket.lastMessageContent = content;
    ticket.lastMessageCreatedAt = createdAt;
  }

  async getMessages(conversationId: string, offset: number): Promise<ChatMessagesResponse> {
    const response = await fetch(
      `${environment.apiUrl}/api/Chat?conversationId=${conversationId}&offset=${offset}&_t=${Date.now()}`,
      { credentials: 'include', cache: 'no-store' }
    );

    if (!response.ok) {
      throw new Error(`Erro ${response.status}`);
    }

    return response.json();
  }

  async getContactDetail(conversationId: string): Promise<ContactDetail> {
    const response = await fetch(
      `${environment.apiUrl}/api/contact/${conversationId}?_t=${Date.now()}`,
      { credentials: 'include', cache: 'no-store' }
    );

    if (!response.ok) {
      throw new Error(`Erro ${response.status}`);
    }

    return response.json();
  }

  async getUsers(): Promise<PlatformUser[]> {
    const response = await fetch(
      `${environment.apiUrl}/api/contact/users?_t=${Date.now()}`,
      { credentials: 'include', cache: 'no-store' }
    );

    if (!response.ok) {
      throw new Error(`Erro ${response.status}`);
    }

    return response.json();
  }

  async getTags(): Promise<TagItem[]> {
    const response = await fetch(
      `${environment.apiUrl}/api/contact/tags?_t=${Date.now()}`,
      { credentials: 'include', cache: 'no-store' }
    );

    if (!response.ok) {
      throw new Error(`Erro ${response.status}`);
    }

    return response.json();
  }

  async updateContact(conversationId: string, data: {
    name?: string;
    phone?: string;
    email?: string;
    company?: string;
    notes?: string;
    tagIds?: string[];
    ownerId?: string;
    contactOwnerId?: string;
    ticketSubject?: string;
    ticketStatus?: string;
  }): Promise<void> {
    const response = await fetch(
      `${environment.apiUrl}/api/contact/${conversationId}`,
      {
        method: 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }
    );

    if (!response.ok) {
      throw new Error(`Erro ${response.status}`);
    }
  }

  async getContactTickets(contactId: string): Promise<ContactTicket[]> {
    const response = await fetch(
      `${environment.apiUrl}/api/contact/${contactId}/tickets?_t=${Date.now()}`,
      { credentials: 'include', cache: 'no-store' }
    );

    if (!response.ok) {
      throw new Error(`Erro ${response.status}`);
    }

    return response.json();
  }

  async markAsRead(conversationId: string): Promise<void> {
    await fetch(
      `${environment.apiUrl}/api/chat/${conversationId}/read`,
      {
        method: 'PUT',
        credentials: 'include',
      }
    );
  }

  async sendMessage(conversationId: string, content: string, messageType: string = 'text'): Promise<MessageRow> {
    const response = await fetch(
      `${environment.apiUrl}/api/chat`,
      {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ conversationId, content, messageType }),
      }
    );

    if (!response.ok) {
      throw new Error(`Erro ${response.status}`);
    }

    const data = await response.json();
    return data.message;
  }
}
