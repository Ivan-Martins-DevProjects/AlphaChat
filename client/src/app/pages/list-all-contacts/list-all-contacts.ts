import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ChatService, ContactDetail } from '../../services/chat';
import { TagItem } from '../../models/ticket-card';
import { environment } from '../../../environments/environment';
import { EditContactModal } from '../../components/modal/edit-contact-modal/edit-contact-modal';
import { SendMessageModal } from '../../components/modal/send-message-modal/send-message-modal';
import { CreateContactModal } from '../../components/modal/create-contact-modal/create-contact-modal';

@Component({
  selector: 'app-list-all-contacts',
  standalone: true,
  imports: [CommonModule, FormsModule, EditContactModal, SendMessageModal, CreateContactModal],
  templateUrl: './list-all-contacts.html',
  styleUrl: './list-all-contacts.css',
})
export class ListAllContacts implements OnInit {
  contacts: ContactDetail[] = [];
  isLoading = true;
  error = '';

  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  searchQuery = '';
  selectedCompany = '';
  selectedTagId = '';

  companies: string[] = [];
  availableTags: TagItem[] = [];

  showCompanyDropdown = false;
  companySearchQuery = '';

  showTagDropdown = false;
  tagSearchQuery = '';

  showEditModal = false;
  editingContactId = '';
  editingContactData: ContactDetail | null = null;

  showSendMessageModal = false;
  sendMessageContactId = '';

  showCreateModal = false;

  private searchTimeout: any;

  constructor(
    private chatService: ChatService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadInitialData();
  }

  async loadInitialData() {
    try {
      const [companies, tags] = await Promise.all([
        this.loadCompanies(),
        this.chatService.getTags()
      ]);
      this.availableTags = tags;
      await this.loadContacts();
    } catch (err) {
      console.error('[ListAllContacts] Erro ao carregar dados iniciais:', err);
    }
  }

  async loadCompanies() {
    try {
      const response = await fetch(
        `${environment.apiUrl}/api/contact/companies`,
        { credentials: 'include' }
      );

      if (!response.ok) {
        throw new Error(`Erro ${response.status}`);
      }

      this.companies = await response.json();
    } catch (err) {
      console.error('[ListAllContacts] Erro ao carregar empresas:', err);
    }
  }

  async loadContacts() {
    this.isLoading = true;
    this.error = '';

    try {
      const params = new URLSearchParams({
        page: this.currentPage.toString(),
        pageSize: this.pageSize.toString(),
      });

      if (this.searchQuery) params.append('search', this.searchQuery);
      if (this.selectedCompany) params.append('company', this.selectedCompany);
      if (this.selectedTagId) params.append('tagId', this.selectedTagId);

      const response = await fetch(
        `${environment.apiUrl}/api/contact/all?${params.toString()}`,
        { credentials: 'include' }
      );

      if (!response.ok) {
        throw new Error(`Erro ${response.status}`);
      }

      const data = await response.json();
      this.contacts = data.contacts || [];
      this.totalCount = data.totalCount || 0;
    } catch (err) {
      this.error = 'Erro ao carregar contatos';
      console.error('[ListAllContacts] Erro:', err);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  onSearchChange() {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage = 1;
      this.loadContacts();
    }, 300);
  }

  onCompanySelect(company: string) {
    this.selectedCompany = company;
    this.showCompanyDropdown = false;
    this.companySearchQuery = '';
    this.currentPage = 1;
    this.loadContacts();
  }

  clearCompany() {
    this.selectedCompany = '';
    this.currentPage = 1;
    this.loadContacts();
  }

  onTagSelect(tagId: string) {
    this.selectedTagId = this.selectedTagId === tagId ? '' : tagId;
    this.showTagDropdown = false;
    this.tagSearchQuery = '';
    this.currentPage = 1;
    this.loadContacts();
  }

  clearTag() {
    this.selectedTagId = '';
    this.currentPage = 1;
    this.loadContacts();
  }

  getFilteredTags(): TagItem[] {
    if (!this.tagSearchQuery) return this.availableTags;
    const query = this.tagSearchQuery.toLowerCase();
    return this.availableTags.filter(t => t.name.toLowerCase().includes(query));
  }

  getSelectedTagName(): string {
    if (!this.selectedTagId) return '';
    const tag = this.availableTags.find(t => t.id === this.selectedTagId);
    return tag ? tag.name : '';
  }

  getFilteredCompanies(): string[] {
    if (!this.companySearchQuery) return this.companies;
    const query = this.companySearchQuery.toLowerCase();
    return this.companies.filter(c => c.toLowerCase().includes(query));
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadContacts();
  }

  nextPage() {
    this.goToPage(this.currentPage + 1);
  }

  prevPage() {
    this.goToPage(this.currentPage - 1);
  }

  openContact(contactId: string) {
    this.router.navigate(['/home'], { queryParams: { contactId } });
  }

  addContact() {
    this.showCreateModal = true;
    this.cdr.detectChanges();
  }

  closeCreateModal() {
    this.showCreateModal = false;
    this.cdr.detectChanges();
  }

  onContactCreated() {
    this.closeCreateModal();
    this.loadContacts();
  }

  editContact(contactId: string) {
    this.editingContactId = contactId;
    this.editingContactData = this.contacts.find(c => c.contactId === contactId) || null;
    this.showEditModal = true;
    this.cdr.detectChanges();
  }

  closeEditModal() {
    this.showEditModal = false;
    this.editingContactId = '';
    this.editingContactData = null;
    this.cdr.detectChanges();
  }

  onContactSaved() {
    this.closeEditModal();
    this.loadContacts();
  }

  openChat(contactId: string) {
    this.sendMessageContactId = contactId;
    this.showSendMessageModal = true;
    this.cdr.detectChanges();
  }

  closeSendMessageModal() {
    this.showSendMessageModal = false;
    this.sendMessageContactId = '';
    this.cdr.detectChanges();
  }

  onMessageSent() {
    this.closeSendMessageModal();
  }

  getAvatar(contact: ContactDetail): string {
    return contact.profilePic || `https://i.pravatar.cc/150?u=${contact.contactId}`;
  }

  formatPhone(phone: string | null): string {
    if (!phone) return '-';
    const digits = phone.replace(/\D/g, '');
    if (digits.length === 11) {
      return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
    }
    if (digits.length === 10) {
      return `(${digits.slice(0, 2)}) ${digits.slice(2, 6)}-${digits.slice(6)}`;
    }
    return phone;
  }

  hasActiveFilters(): boolean {
    return !!(this.searchQuery || this.selectedCompany || this.selectedTagId);
  }

  clearAllFilters() {
    this.searchQuery = '';
    this.selectedCompany = '';
    this.selectedTagId = '';
    this.currentPage = 1;
    this.loadContacts();
  }
}
