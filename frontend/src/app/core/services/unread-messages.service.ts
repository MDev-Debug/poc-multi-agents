import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UnreadMessagesService {
  private readonly counts = new BehaviorSubject<Map<string, number>>(new Map());
  readonly counts$ = this.counts.asObservable();

  increment(senderId: string): void {
    const current = new Map(this.counts.getValue());
    const prev = current.get(senderId) ?? 0;
    current.set(senderId, prev + 1);
    this.counts.next(current);
  }

  clear(userId: string): void {
    const current = new Map(this.counts.getValue());
    current.delete(userId);
    this.counts.next(current);
  }

  getCount(userId: string): number {
    return this.counts.getValue().get(userId) ?? 0;
  }
}
