import { TestBed } from '@angular/core/testing';
import { CreateContactModal } from './create-contact-modal';
import { ChatService } from '../../../services/chat';
import { vi } from 'vitest';

describe('CreateContactModal', () => {
  let component: CreateContactModal;
  let chatServiceMock: any;

  beforeEach(async () => {
    chatServiceMock = {
      getTags: vi.fn().mockResolvedValue([
        { id: '1', name: 'VIP', colorCode: '#fff' },
        { id: '2', name: 'Lead', colorCode: '#000' },
      ]),
    };

    await TestBed.configureTestingModule({
      imports: [CreateContactModal],
      providers: [{ provide: ChatService, useValue: chatServiceMock }],
    }).compileComponents();

    const fixture = TestBed.createComponent(CreateContactModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with empty fields', () => {
    expect(component.name).toBe('');
    expect(component.phone).toBe('');
    expect(component.email).toBe('');
  });

  describe('addTag', () => {
    it('should add a tag to selectedTags', () => {
      component.addTag({ id: '3', name: 'Novo', colorCode: '#111' });
      expect(component.selectedTags.length).toBe(1);
      expect(component.selectedTags[0].id).toBe('3');
    });

    it('should not add duplicate tag', () => {
      component.addTag({ id: '3', name: 'Novo', colorCode: '#111' });
      component.addTag({ id: '3', name: 'Novo', colorCode: '#111' });
      expect(component.selectedTags.length).toBe(1);
    });
  });

  describe('removeTag', () => {
    it('should remove a tag by id', () => {
      component.addTag({ id: '3', name: 'Novo', colorCode: '#111' });
      component.removeTag('3');
      expect(component.selectedTags.length).toBe(0);
    });
  });

  describe('getAvailableTags', () => {
    it('should exclude already selected tags', () => {
      component.addTag({ id: '1', name: 'VIP', colorCode: '#fff' });
      const available = component.getAvailableTags();
      expect(available.every(t => t.id !== '1')).toBe(true);
    });
  });

  it('should emit close on onClose', () => {
    let emitted = false;
    component.close.subscribe(() => (emitted = true));
    component.onClose();
    expect(emitted).toBe(true);
  });
});
