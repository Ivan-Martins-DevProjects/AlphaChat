import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DateTime {
  constructor() { }

  getDate() {
    const now: Date = new Date();

    return now.toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit'
    })
  }
}
