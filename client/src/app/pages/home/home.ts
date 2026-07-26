import { Component, ChangeDetectorRef, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { Conversations } from '../../components/inbox/conversations/conversations';
import { ChatContent } from '../../components/inbox/chat-content/chat-content';
import { TicketCard } from '../../models/ticket-card';
import { environment } from '../../../environments/environment';
import { WebSocketService } from '../../services/websocket';
import { ChatService } from '../../services/chat';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    Conversations,
    ChatContent
  ],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit, OnDestroy {
  tickets: TicketCard[] = [];
  currentUserId: string = '';
  isLoading = true;
  isLoadingMore = false;
  hasMore = true;
  initialConversationId: string | null = null;
  private offset = 0;
  private readonly PAGE_SIZE = 20;
  private routeSub?: Subscription;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private wsService: WebSocketService,
    private chatService: ChatService
  ) {}

  ngOnInit() {
    this.loadConversations();
    this.routeSub = this.route.queryParamMap.subscribe(params => {
      const conversationId = params.get('conversationId');
      if (conversationId) {
        this.initialConversationId = conversationId;
        this.cdr.detectChanges();
      }
    });
  }

  ngOnDestroy() {
    this.routeSub?.unsubscribe();
  }

  async loadConversations() {
    try {
      const response = await fetch(`${environment.apiUrl}/api/home?offset=${this.offset}&limit=${this.PAGE_SIZE}`, {
        method: 'GET',
        credentials: 'include',
      });

      if (response.status === 401) {
        this.router.navigate(['/login']);
        return;
      }

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Erro ${response.status}: ${errorText}`);
      }

      const data = await response.json();
      this.currentUserId = data.userId;

      const newTickets = data.conversations || [];
      this.tickets = [...this.tickets, ...newTickets];
      this.offset += newTickets.length;

      if (newTickets.length < this.PAGE_SIZE) {
        this.hasMore = false;
      }

      if (this.offset === this.PAGE_SIZE) {
        this.wsService.connect(this.currentUserId);
      }
    } catch (err) {
      this.error = 'Erro ao carregar conversas';
      console.error('[Home] Erro:', err);
    } finally {
      this.isLoading = false;
      this.isLoadingMore = false;
      this.cdr.detectChanges();
    }
  }

  async loadMore() {
    if (this.isLoadingMore || !this.hasMore) return;
    console.log('[Home] loadMore chamado, offset atual:', this.offset);
    this.isLoadingMore = true;
    this.cdr.detectChanges();
    await this.loadConversations();
  }

  async onContactUpdated() {
    this.tickets = [];
    this.offset = 0;
    this.hasMore = true;
    this.isLoading = true;
    this.cdr.detectChanges();
    await this.loadConversations();
  }

  error = '';
}
