import { TestBed } from '@angular/core/testing';
import { WebSocketService } from './websocket';
import { NgZone } from '@angular/core';
import { vi } from 'vitest';

describe('WebSocketService', () => {
  let service: WebSocketService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        WebSocketService,
        { provide: NgZone, useValue: { run: (fn: Function) => fn() } },
      ],
    });
    service = TestBed.inject(WebSocketService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should expose messageCreated observable', () => {
    expect(service.messageCreated).toBeDefined();
  });

  it('should start with no socket', () => {
    expect((service as any).socket).toBeNull();
  });

  describe('disconnect', () => {
    it('should set socket to null', () => {
      service.disconnect();
      expect((service as any).socket).toBeNull();
    });

    it('should clear reconnect timer', () => {
      (service as any).reconnectTimer = setTimeout(() => {}, 9999);
      service.disconnect();
      expect((service as any).reconnectTimer).toBeNull();
    });
  });
});
