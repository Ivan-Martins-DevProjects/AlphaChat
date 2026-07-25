import { Injectable, NgZone } from '@angular/core';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface MessageCreatedEvent {
  type: 'message_created';
  messageId: string;
  conversationId: string;
  senderId: string;
  content: string;
  messageType: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class WebSocketService {
  private socket: WebSocket | null = null;
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
  private userId: string = '';

  private messageCreated$ = new Subject<MessageCreatedEvent>();

  messageCreated = this.messageCreated$.asObservable();

  constructor(private zone: NgZone) {}

  connect(userId: string) {
    console.log('[WebSocket] connect chamado com userId:', userId, 'tipo:', typeof userId);

    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      console.log('[WebSocket] Já conectado, ignorando');
      return;
    }

    if (this.socket) {
      console.log('[WebSocket] Socket anterior existente, state:', this.socket.readyState, 'fechando...');
      this.socket.close();
      this.socket = null;
    }

    this.userId = userId;
    const url = `${environment.wsUrl}/ws/chat?userId=${userId}`;
    console.log('[WebSocket] Conectando a:', url);

    this.socket = new WebSocket(url);

    this.socket.onopen = () => {
      console.log('[WebSocket] ✅ Conectado com sucesso!');
    };

    this.socket.onmessage = (event) => {
      console.log('[WebSocket] 📩 Mensagem recebida:', event.data);
      this.zone.run(() => {
        try {
          const data = JSON.parse(event.data);

          if (data.type === 'message_created') {
            console.log('[WebSocket] 📨 message_created emitido:', data);
            this.messageCreated$.next(data as MessageCreatedEvent);
          }
        } catch (err) {
          console.error('[WebSocket] Erro ao processar mensagem:', err);
        }
      });
    };

    this.socket.onclose = (event) => {
      console.log(`[WebSocket] ❌ Desconectado (code: ${event.code}, reason: ${event.reason || 'nenhum'}, wasClean: ${event.wasClean})`);
      this.socket = null;
      this.scheduleReconnect();
    };

    this.socket.onerror = (err) => {
      console.error('[WebSocket] ⚠️ Erro na conexão:', err);
    };
  }

  private scheduleReconnect() {
    if (this.reconnectTimer) return;

    console.log('[WebSocket] Reconectando em 3s...');
    this.reconnectTimer = setTimeout(() => {
      this.reconnectTimer = null;
      if (this.userId) {
        this.connect(this.userId);
      }
    }, 3000);
  }

  disconnect() {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }

    if (this.socket) {
      this.socket.close();
      this.socket = null;
    }
  }
}
