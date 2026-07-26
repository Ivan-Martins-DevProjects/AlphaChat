import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TagItem } from '../../../models/ticket-card';
import { ChatService, ContactDetail } from '../../../services/chat';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-edit-contact-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-contact-modal.html',
  styleUrl: './edit-contact-modal.css',
})
export class EditContactModal implements OnInit {
  @Input() contactId: string = '';
  @Input() contactData: ContactDetail | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  contact: ContactDetail | null = null;
  conversationId: string = '';
  isLoading = true;
  error = '';

  editName = '';
  editPhone = '';
  editEmail = '';
  editCompany = '';
  editDocument = '';
  editNotes = '';
  editTags: TagItem[] = [];

  showTagDropdown = false;
  availableTags: TagItem[] = [];
  isAssuming = false;

  constructor(
    private chatService: ChatService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadTags();
    if (this.contactData) {
      this.contact = this.contactData;
      this.populateFields();
      this.isLoading = false;
    } else if (this.contactId) {
      this.loadContact();
    } else {
      this.isLoading = false;
    }
  }

  populateFields() {
    if (!this.contact) return;
    this.editName = this.contact.name || '';
    this.editPhone = this.contact.phone || '';
    this.editEmail = this.contact.email || '';
    this.editCompany = this.contact.company || '';
    this.editDocument = this.contact.document || '';
    this.editNotes = this.contact.notes || '';
    this.editTags = [...this.contact.tags];
  }

  async loadTags() {
    try {
      this.availableTags = await this.chatService.getTags();
      this.cdr.detectChanges();
    } catch (err) {
      console.error('[EditContactModal] Erro ao carregar tags:', err);
    }
  }

  async loadContact() {
    this.isLoading = true;
    this.error = '';

    try {
      const tickets = await this.chatService.getContactTickets(this.contactId);

      if (tickets && tickets.length > 0) {
        this.conversationId = tickets[0].conversationId;
        this.contact = await this.chatService.getContactDetail(this.conversationId);
      } else {
        this.contact = null;
        this.error = '';
      }

      this.populateFields();
    } catch (err) {
      this.error = 'Erro ao carregar informações do contato';
      console.error('[EditContactModal] Erro:', err);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  onClose() {
    this.close.emit();
  }

  onBackdropClick(event: Event) {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }

  getAvatar(): string {
    if (!this.contact) return `https://i.pravatar.cc/150?u=${this.contactId}`;
    return this.contact.profilePic || `https://i.pravatar.cc/150?u=${this.contact.contactId}`;
  }

  getAvailableTags(): TagItem[] {
    const currentIds = this.editTags.map(t => t.id);
    return this.availableTags.filter(t => !currentIds.includes(t.id));
  }

  addTag(tag: TagItem) {
    if (!this.editTags.find(t => t.id === tag.id)) {
      this.editTags = [...this.editTags, tag];
    }
    this.showTagDropdown = false;
  }

  removeTag(tagId: string) {
    this.editTags = this.editTags.filter(t => t.id !== tagId);
  }

  async addToWallet() {
    if (!this.contactId || this.isAssuming) return;

    this.isAssuming = true;
    this.cdr.detectChanges();

    try {
      const response = await fetch(`${environment.apiUrl}/api/contact/${this.contactId}/assume`, {
        method: 'POST',
        credentials: 'include',
      });

      if (!response.ok) {
        throw new Error(`Erro ${response.status}`);
      }

      const data = await response.json();

      if (this.contact) {
        this.contact.owner = {
          id: data.ownerId,
          name: data.ownerName,
          profilePic: data.ownerProfilePic,
        };
      }

      this.cdr.detectChanges();
    } catch (err) {
      console.error('[EditContactModal] Erro ao assumir contato:', err);
      this.error = 'Erro ao adicionar à carteira';
      this.cdr.detectChanges();
    } finally {
      this.isAssuming = false;
      this.cdr.detectChanges();
    }
  }

  async removeOwner() {
    if (!this.contactId || !this.contact) return;

    try {
      const response = await fetch(`${environment.apiUrl}/api/contact/${this.contactId}/owner`, {
        method: 'DELETE',
        credentials: 'include',
      });

      if (!response.ok) {
        throw new Error(`Erro ${response.status}`);
      }

      this.contact.owner = null;
      this.cdr.detectChanges();
    } catch (err) {
      console.error('[EditContactModal] Erro ao remover responsável:', err);
      this.error = 'Erro ao remover responsável';
      this.cdr.detectChanges();
    }
  }

  async save() {
    if (!this.conversationId) {
      this.error = 'Não é possível salvar um contato sem conversa associada';
      this.cdr.detectChanges();
      return;
    }

    try {
      await this.chatService.updateContact(this.conversationId, {
        name: this.editName,
        phone: this.editPhone || undefined,
        email: this.editEmail || undefined,
        company: this.editCompany || undefined,
        notes: this.editNotes || undefined,
        tagIds: this.editTags.map(t => t.id),
      });

      this.saved.emit();
      this.onClose();
    } catch (err) {
      console.error('[EditContactModal] Erro ao salvar:', err);
      this.error = 'Erro ao salvar informações';
      this.cdr.detectChanges();
    }
  }
}
