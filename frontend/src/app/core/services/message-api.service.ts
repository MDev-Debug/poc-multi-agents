import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChatMessage } from './chat-hub.service';

@Injectable({ providedIn: 'root' })
export class MessageApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000';

  getHistory(otherUserId: string, page = 1, pageSize = 50): Observable<ChatMessage[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ChatMessage[]>(
      `${this.baseUrl}/api/messages/${otherUserId}`,
      { params }
    );
  }
}
