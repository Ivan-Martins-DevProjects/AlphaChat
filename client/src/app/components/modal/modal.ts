import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  imports: [CommonModule],
  templateUrl: './modal.html',
  styleUrl: './modal.css',
  encapsulation: ViewEncapsulation.None,
})
export class Modal implements OnChanges {
  @Input() isOpen = false;
  @Input() title = 'Erro';
  @Input() message = '';
  @Input() type: 'error' | 'success' | 'warning' = 'error';
  @Output() close = new EventEmitter<void>();

  ngOnChanges(changes: SimpleChanges) {
    console.log('[Modal] Input mudou:', changes);
  }

  onClose() {
    this.close.emit();
  }

  onBackdropClick(event: Event) {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }
}
