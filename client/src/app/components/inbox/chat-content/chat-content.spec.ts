import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatContent } from './chat-content';

describe('ChatContent', () => {
  let component: ChatContent;
  let fixture: ComponentFixture<ChatContent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChatContent],
    }).compileComponents();

    fixture = TestBed.createComponent(ChatContent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
