import { TestBed } from '@angular/core/testing';
import { DateTime } from './date-time';

describe('DateTime', () => {
  let service: DateTime;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DateTime);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return a time string in pt-BR format', () => {
    const result = service.getDate();
    expect(result).toMatch(/^\d{2}:\d{2}$/);
  });

  it('should return current time approximately', () => {
    const now = new Date();
    const result = service.getDate();
    const [hours, minutes] = result.split(':').map(Number);

    expect(hours).toBe(now.getHours());
    expect(minutes).toBe(now.getMinutes());
  });
});
