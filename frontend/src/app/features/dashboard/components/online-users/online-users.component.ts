import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OnlineUser } from '../../../../core/services/presence-hub.service';

@Component({
  selector: 'app-online-users',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './online-users.component.html',
  styleUrl: './online-users.component.scss'
})
export class OnlineUsersComponent {
  @Input() users: OnlineUser[] = [];

  getTimeAgo(lastSeenAt: string): string {
    const now = new Date();
    const last = new Date(lastSeenAt);
    const diffMs = now.getTime() - last.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    if (diffMin < 1) return 'agora';
    if (diffMin === 1) return 'há 1 min';
    if (diffMin < 60) return `há ${diffMin} min`;
    const diffH = Math.floor(diffMin / 60);
    return `há ${diffH}h`;
  }
}
