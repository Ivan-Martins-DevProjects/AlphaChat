import { TestBed } from '@angular/core/testing';
import { Home } from './home';
import { provideRouter } from '@angular/router';
import { ChatService } from '../../services/chat';
import { WebSocketService } from '../../services/websocket';
import { vi } from 'vitest';

describe('Home', () => {
  let chatServiceMock: any;
  let wsServiceMock: any;

  beforeEach(async () => {
    chatServiceMock = {
      selectedChat$: { subscribe: vi.fn() },
      highlightChanged: { subscribe: vi.fn() },
      newMessageFromWs: { subscribe: vi.fn() },
      setConversation: vi.fn(),
    };

    wsServiceMock = {
      connect: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [
        provideRouter([]),
        { provide: ChatService, useValue: chatServiceMock },
        { provide: WebSocketService, useValue: wsServiceMock },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(Home);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should start with loading state', () => {
    const fixture = TestBed.createComponent(Home);
    const home = fixture.componentInstance;
    expect(home.isLoading).toBe(true);
  });

  it('should start with offset 0', () => {
    const fixture = TestBed.createComponent(Home);
    const home = fixture.componentInstance;
    expect((home as any).offset).toBe(0);
  });
});
