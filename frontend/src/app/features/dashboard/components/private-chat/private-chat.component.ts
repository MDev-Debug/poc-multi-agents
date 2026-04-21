import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  AfterViewChecked,
  ViewChild,
  ElementRef,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { OnlineUser } from '../../../../core/services/presence-hub.service';
import { ChatHubService, ChatMessage } from '../../../../core/services/chat-hub.service';
import { MessageApiService } from '../../../../core/services/message-api.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-private-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './private-chat.component.html',
  styleUrl: './private-chat.component.scss'
})
export class PrivateChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @Input({ required: true }) otherUser!: OnlineUser;
  @Output() closed = new EventEmitter<void>();

  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;

  private readonly chatHub = inject(ChatHubService);
  private readonly messageApi = inject(MessageApiService);
  private readonly auth = inject(AuthService);

  messages = signal<ChatMessage[]>([]);
  loading = signal(true);
  error = signal(false);
  sending = signal(false);
  sendError = signal(false);

  newMessage = '';
  myUserId: string | null = null;

  private subs = new Subscription();
  private shouldScrollToBottom = false;

  ngOnInit(): void {
    this.myUserId = this.auth.getUserId();
    this.loadHistory();

    this.subs.add(
      this.chatHub.messages$.subscribe(msg => {
        if (msg.senderId === this.otherUser.userId) {
          this.messages.update(list => [...list, msg]);
          this.shouldScrollToBottom = true;
        }
      })
    );
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  loadHistory(): void {
    this.loading.set(true);
    this.error.set(false);

    this.subs.add(
      this.messageApi.getHistory(this.otherUser.userId).subscribe({
        next: (msgs) => {
          this.messages.set(msgs);
          this.loading.set(false);
          this.shouldScrollToBottom = true;
        },
        error: () => {
          this.loading.set(false);
          this.error.set(true);
        }
      })
    );
  }

  async sendMessage(): Promise<void> {
    const content = this.newMessage.trim();
    if (!content || this.sending()) return;

    this.sending.set(true);
    this.sendError.set(false);

    const optimistic: ChatMessage = {
      messageId: crypto.randomUUID(),
      senderId: this.myUserId ?? '',
      senderEmail: this.auth.getUserEmail() ?? '',
      content,
      sentAt: new Date().toISOString()
    };

    this.messages.update(list => [...list, optimistic]);
    this.newMessage = '';
    this.shouldScrollToBottom = true;

    try {
      await this.chatHub.sendMessage(this.otherUser.userId, content);
    } catch {
      this.sendError.set(true);
      // Remove the optimistic message on failure
      this.messages.update(list => list.filter(m => m.messageId !== optimistic.messageId));
      this.newMessage = content;
    } finally {
      this.sending.set(false);
    }
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  getInitial(email: string): string {
    return email.charAt(0).toUpperCase();
  }

  formatTime(isoDate: string): string {
    const d = new Date(isoDate);
    return d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
  }

  isOwnMessage(msg: ChatMessage): boolean {
    return msg.senderId === this.myUserId;
  }

  private scrollToBottom(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
  }
}
