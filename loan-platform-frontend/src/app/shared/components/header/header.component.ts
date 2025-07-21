import { Component, OnInit, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
  animations: [
    trigger('slideDown', [
      transition(':enter', [
        style({ transform: 'translateY(-100%)', opacity: 0 }),
        animate('300ms ease-in-out', style({ transform: 'translateY(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('300ms ease-in-out', style({ transform: 'translateY(-100%)', opacity: 0 }))
      ])
    ]),
    trigger('slideInOut', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('300ms ease-in-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('300ms ease-in-out', style({ transform: 'translateX(100%)', opacity: 0 }))
      ])
    ])
  ]
})
export class HeaderComponent implements OnInit {
  isMobile = false;
  mobileMenuOpen = false;
  notificationsOpen = false;
  userName = '';
  userAvatar = '';
  unreadNotifications = 0;
  notifications: any[] = [];

  constructor(
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.checkScreenSize();
    this.loadUserInfo();
    this.loadNotifications();
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this.checkScreenSize();
  }

  checkScreenSize() {
    this.isMobile = window.innerWidth < 768;
    if (!this.isMobile) {
      this.mobileMenuOpen = false;
    }
  }

  loadUserInfo() {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.userName = `${user.firstName} ${user.lastName}`;
      this.userAvatar = user.profileImageUrl || '';
    }
  }

  loadNotifications() {
    this.notificationService.getNotifications().subscribe(notifications => {
      this.notifications = notifications;
      this.unreadNotifications = notifications.filter(n => !n.isRead).length;
    });
  }

  hasRole(role: string): boolean {
    return this.authService.hasRole(role);
  }

  toggleMobileMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu() {
    this.mobileMenuOpen = false;
  }

  toggleNotifications() {
    this.notificationsOpen = !this.notificationsOpen;
  }

  markAsRead(notification: any) {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe(() => {
        notification.isRead = true;
        this.unreadNotifications--;
      });
    }
  }

  getNotificationIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'Info': 'info',
      'Success': 'check_circle',
      'Warning': 'warning',
      'Error': 'error',
      'ApplicationUpdate': 'update',
      'DocumentRequest': 'description',
      'Approval': 'thumb_up',
      'Rejection': 'thumb_down'
    };
    return icons[type] || 'notifications';
  }

  getNotificationIconClass(type: string): string {
    const classes: { [key: string]: string } = {
      'Info': 'info',
      'Success': 'success',
      'Warning': 'warning',
      'Error': 'error',
      'ApplicationUpdate': 'update',
      'DocumentRequest': 'document',
      'Approval': 'success',
      'Rejection': 'error'
    };
    return classes[type] || 'info';
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}