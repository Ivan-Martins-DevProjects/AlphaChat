import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ChatService } from '../../../services/chat';
import { TicketCard, TagItem } from '../../../models/ticket-card';
import { MessageCreatedEvent } from '../../../services/websocket';

@Component({
  selector: 'app-conversations',
  imports: [CommonModule],
  templateUrl: './conversations.html',
  styleUrl: './conversations.css',
})
export class Conversations implements OnInit, OnDestroy {
  @ViewChild('listContainer') listContainer!: ElementRef<HTMLDivElement>;

  @Input() tickets: TicketCard[] = [];
  @Input() isLoading = true;
  @Input() isLoadingMore = false;
  @Input() hasMore = true;
  @Input() currentUserId: string = '';
  @Output() loadMore = new EventEmitter<void>();

  private subs: Subscription[] = [];

  constructor(
    private chatService: ChatService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.subs.push(
      this.chatService.highlightChanged.subscribe(conversationId => {
        this.cdr.detectChanges();
      }),
      this.chatService.newMessageFromWs.subscribe(event => {
        const ticket = this.tickets.find(t => t.conversationId === event.conversationId);
        if (ticket) {
          this.chatService.updateTicketLastMessage(ticket, event.content, event.createdAt);
          this.cdr.detectChanges();
        }
      })
    );
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
  }

  onSelect(ticket: TicketCard) {
    this.router.navigate(['/home'], { queryParams: { conversationId: ticket.conversationId } });
  }

  onScroll() {
    const el = this.listContainer?.nativeElement;
    if (!el) return;

    const atBottom = el.scrollTop + el.clientHeight >= el.scrollHeight - 100;

    if (atBottom && this.hasMore && !this.isLoadingMore) {
      console.log('[Conversations] Scroll no fim, carregando mais...');
      this.loadMore.emit();
    }
  }

  get filteredTickets(): TicketCard[] {
    switch (this.activeFilter) {
      case 'unread':
        return this.tickets.filter(t => !t.read);
      case 'mine':
        return this.tickets.filter(t => t.owner === this.currentUserId);
      case 'aberto':
        return this.tickets.filter(t => t.status === 'aberto');
      case 'resolvido':
        return this.tickets.filter(t => t.status === 'resolvido');
      case 'cancelado':
        return this.tickets.filter(t => t.status === 'cancelado');
      default:
        return this.tickets;
    }
  }

  filters = [
    { id: 'all', label: 'Todas', icon: 'fa-comments' },
    { id: 'unread', label: 'Não lidas', icon: 'fa-envelope' },
    { id: 'mine', label: 'Minhas', icon: 'fa-user' },
    { id: 'aberto', label: 'Abertas', icon: 'fa-circle' },
    { id: 'resolvido', label: 'Resolvidas', icon: 'fa-check-circle' },
    { id: 'cancelado', label: 'Canceladas', icon: 'fa-times-circle' },
  ];

  activeFilter = 'all';
  showFilterDropdown = false;
  setActiveFilter(filterId: string) {
    this.activeFilter = filterId;
  }

  getFilterCount(filterId: string): number {
    switch (filterId) {
      case 'unread':
        return this.tickets.filter(t => !t.read).length;
      case 'mine':
        return this.tickets.filter(t => t.owner === this.currentUserId).length;
      case 'aberto':
        return this.tickets.filter(t => t.status === 'aberto').length;
      case 'resolvido':
        return this.tickets.filter(t => t.status === 'resolvido').length;
      case 'cancelado':
        return this.tickets.filter(t => t.status === 'cancelado').length;
      default:
        return this.tickets.length;
    }
  }

  isHighlighted(conversationId: string): boolean {
    return this.chatService.isHighlighted(conversationId);
  }

  getAvatar(ticket: TicketCard): string {
    return ticket.contactProfilePic || `https://i.pravatar.cc/150?u=${ticket.contactId}`;
  }

  formatTime(dateString: string | null): string {
    if (!dateString) return '';

    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Agora';
    if (diffMins < 60) return `${diffMins}min`;
    if (diffHours < 24) return `${diffHours}h`;
    if (diffDays < 7) return `${diffDays}d`;

    return date.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
  }

  getStatusColor(status: string): string {
    const colors: Record<string, string> = {
      'open': '#16a34a',
      'pending': '#ca8a04',
      'closed': '#64748b',
      'resolved': '#15803d',
    };
    return colors[status.toLowerCase()] || '#64748b';
  }

  formatStatus(status: string): string {
    const labels: Record<string, string> = {
      'open': 'Aberto',
      'aberto': 'Aberto',
      'resolved': 'Resolvido',
      'resolvido': 'Resolvido',
      'cancelled': 'Cancelado',
      'cancelado': 'Cancelado',
      'pending': 'Pendente',
    };
    return labels[status.toLowerCase()] || status;
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

  getValidTags(ticket: TicketCard): TagItem[] {
    return ticket.tags.filter(tag => tag.name && tag.name.trim() !== '');
  }
}
