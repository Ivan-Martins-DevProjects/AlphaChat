import { Component, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../services/chat';
import { TagItem } from '../../../models/ticket-card';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-create-contact-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './create-contact-modal.html',
  styleUrl: './create-contact-modal.css',
})
export class CreateContactModal {
  @Output() close = new EventEmitter<void>();
  @Output() created = new EventEmitter<void>();

  name = '';
  phone = '';
  email = '';
  company = '';
  document = '';
  notes = '';
  selectedTags: TagItem[] = [];

  showTagDropdown = false;
  availableTags: TagItem[] = [];
  isSaving = false;
  error = '';

  constructor(
    private chatService: ChatService,
    private cdr: ChangeDetectorRef
  ) {
    this.loadTags();
  }

  async loadTags() {
    try {
      this.availableTags = await this.chatService.getTags();
      this.cdr.detectChanges();
    } catch (err) {
      console.error('[CreateContactModal] Erro ao carregar tags:', err);
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

  getAvailableTags(): TagItem[] {
    const currentIds = this.selectedTags.map(t => t.id);
    return this.availableTags.filter(t => !currentIds.includes(t.id));
  }

  addTag(tag: TagItem) {
    if (!this.selectedTags.find(t => t.id === tag.id)) {
      this.selectedTags = [...this.selectedTags, tag];
    }
    this.showTagDropdown = false;
  }

  removeTag(tagId: string) {
    this.selectedTags = this.selectedTags.filter(t => t.id !== tagId);
  }

  async save() {
    if (!this.name.trim() || this.isSaving) return;

    this.isSaving = true;
    this.error = '';
    this.cdr.detectChanges();

    try {
      const response = await fetch(`${environment.apiUrl}/api/contact`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: this.name.trim(),
          phone: this.phone || null,
          email: this.email || null,
          company: this.company || null,
          document: this.document || null,
          notes: this.notes || null,
          tagIds: this.selectedTags.map(t => t.id),
        }),
      });

      if (!response.ok) {
        throw new Error(`Erro ${response.status}`);
      }

      this.created.emit();
      this.onClose();
    } catch (err) {
      this.error = 'Erro ao criar contato';
      console.error('[CreateContactModal] Erro:', err);
    } finally {
      this.isSaving = false;
      this.cdr.detectChanges();
    }
  }
}
