import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardView } from '../../dashboard.component';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
  @Input() userEmail = '';
  @Input() userInitial = '';
  @Input() currentView: DashboardView = 'chat';
  @Output() viewChange = new EventEmitter<DashboardView>();
  @Output() logoutClick = new EventEmitter<void>();
}
