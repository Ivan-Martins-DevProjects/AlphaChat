import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TagItem } from '../../../models/ticket-card';
import { ChatService, ContactDetail, PlatformUser, ContactTicket } from '../../../services/chat';
import { TicketMessages } from '../ticket-messages/ticket-messages';

@Component({
  selector: 'app-contact-info',
  imports: [CommonModule, FormsModule, TicketMessages],
  standalone: true,
  templateUrl: './contact-info.html',
  styleUrl: './contact-info.css',
})
export class ContactInfo implements OnInit {
  @ViewChild('userSearchInput') userSearchInput!: ElementRef<HTMLInputElement>;

  @Input() conversationId: string = ''
  @Input() currentUserId: string = ''
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  contact: ContactDetail | null = null
  contactOwnerId: string | null = null
  isLoading = true
  error = ''

  editName = ''
  editPhone = ''
  editEmail = ''
  editCompany = ''
  editNotes = ''
  editTags: TagItem[] = []
  assignedUser: { id: string; name: string; avatar: string } | null = null
  editTicketSubject = ''
  editTicketStatus = ''

  showTagDropdown = false
  showUserSelector = false
  userSearchQuery = ''

  allUsers: PlatformUser[] = []
  availableTags: TagItem[] = []
  contactTickets: ContactTicket[] = []

  showTicketModal = false
  selectedTicket: ContactTicket | null = null

  constructor(
    private chatService: ChatService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadUsersAndTags()
    if (this.conversationId) {
      this.loadContactDetail()
    }
  }

  async loadUsersAndTags() {
    try {
      const [users, tags] = await Promise.all([
        this.chatService.getUsers(),
        this.chatService.getTags()
      ])
      this.allUsers = users
      this.availableTags = tags
      this.cdr.detectChanges()
    } catch (err) {
      console.error('[ContactInfo] Erro ao carregar usuários/tags:', err)
    }
  }

  async loadContactDetail() {
    this.isLoading = true
    this.error = ''

    try {
      this.contact = await this.chatService.getContactDetail(this.conversationId)

      this.editName = this.contact.name || ''
      this.editPhone = this.contact.phone || ''
      this.editEmail = this.contact.email || ''
      this.editCompany = this.contact.company || ''
      this.editNotes = this.contact.notes || ''
      this.editTags = [...this.contact.tags]
      this.contactOwnerId = this.contact.contactOwnerId

      this.editTicketSubject = this.contact.ticket.subject
      this.editTicketStatus = this.contact.ticket.status

      if (this.contact.owner) {
        this.assignedUser = {
          id: this.contact.owner.id,
          name: this.contact.owner.name,
          avatar: this.contact.owner.profilePic || `https://i.pravatar.cc/150?u=${this.contact.owner.id}`,
        }
      }

      this.contactTickets = await this.chatService.getContactTickets(this.contact.contactId)
    } catch (err) {
      this.error = 'Erro ao carregar informações do contato'
      console.error('[ContactInfo] Erro:', err)
    } finally {
      this.isLoading = false
      this.cdr.detectChanges()
    }
  }

  onClose() {
    this.close.emit()
  }

  formatPhone(phone: string): string {
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

  getAvatar(): string {
    if (!this.contact) return ''
    return this.contact.profilePic || `https://i.pravatar.cc/150?u=${this.contact.contactId}`
  }

  formatTicketStatus(status: string): string {
    const labels: Record<string, string> = {
      'aberto': 'Aberto',
      'resolvido': 'Resolvido',
      'cancelado': 'Cancelado',
    };
    return labels[status.toLowerCase()] || status;
  }

  formatTicketDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  openTicketModal(ticket: ContactTicket) {
    this.selectedTicket = ticket
    this.showTicketModal = true
    this.cdr.detectChanges()
  }

  closeTicketModal() {
    this.showTicketModal = false
    this.selectedTicket = null
    this.cdr.detectChanges()
  }

  getAvailableTags(): TagItem[] {
    const currentIds = this.editTags.map(t => t.id)
    return this.availableTags.filter(t => !currentIds.includes(t.id))
  }

  addTag(tag: TagItem) {
    if (!this.editTags.find(t => t.id === tag.id)) {
      this.editTags = [...this.editTags, tag]
    }
    this.showTagDropdown = false
  }

  removeTag(tagId: string) {
    this.editTags = this.editTags.filter(t => t.id !== tagId)
  }

  openUserSelector() {
    this.showUserSelector = true
    this.userSearchQuery = ''
    this.cdr.detectChanges()
    setTimeout(() => {
      this.userSearchInput?.nativeElement?.focus()
    }, 100)
  }

  closeUserSelector() {
    this.showUserSelector = false
    this.userSearchQuery = ''
    this.cdr.detectChanges()
  }

  getFilteredUsers(): PlatformUser[] {
    const query = this.userSearchQuery.toLowerCase().trim()
    if (!query) return this.allUsers
    return this.allUsers.filter(u =>
      u.name.toLowerCase().includes(query) ||
      u.email.toLowerCase().includes(query)
    )
  }

  selectUser(user: PlatformUser) {
    this.assignedUser = {
      id: user.id,
      name: user.name,
      avatar: user.profilePic,
    }
    this.closeUserSelector()
  }

  removeOwner() {
    this.assignedUser = null
  }

  async addToWallet() {
    if (!this.contact || !this.currentUserId) return

    try {
      await this.chatService.updateContact(this.conversationId, {
        contactOwnerId: this.currentUserId,
      })

      this.contactOwnerId = this.currentUserId
      this.saved.emit()
      this.cdr.detectChanges()
    } catch (err) {
      console.error('[ContactInfo] Erro ao adicionar à carteira:', err)
      this.error = 'Erro ao adicionar à carteira'
      this.cdr.detectChanges()
    }
  }

  async removeFromWallet() {
    if (!this.contact) return

    try {
      await this.chatService.updateContact(this.conversationId, {
        contactOwnerId: null as any,
      })

      this.contactOwnerId = null
      this.saved.emit()
      this.cdr.detectChanges()
    } catch (err) {
      console.error('[ContactInfo] Erro ao remover da carteira:', err)
      this.error = 'Erro ao remover da carteira'
      this.cdr.detectChanges()
    }
  }

  isOwner(): boolean {
    return this.contactOwnerId === this.currentUserId
  }

  async save() {
    if (!this.contact) return

    try {
      await this.chatService.updateContact(this.conversationId, {
        name: this.editName,
        phone: this.editPhone || undefined,
        email: this.editEmail || undefined,
        company: this.editCompany || undefined,
        notes: this.editNotes || undefined,
        tagIds: this.editTags.map(t => t.id),
        ownerId: this.assignedUser?.id,
        ticketSubject: this.editTicketSubject,
        ticketStatus: this.editTicketStatus,
      })

      this.saved.emit()
      this.onClose()
    } catch (err) {
      console.error('[ContactInfo] Erro ao salvar:', err)
      this.error = 'Erro ao salvar informações'
      this.cdr.detectChanges()
    }
  }
}
