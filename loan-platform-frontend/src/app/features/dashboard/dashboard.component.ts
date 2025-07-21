import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { Chart, ChartConfiguration, ChartType } from 'chart.js';
import { AuthService } from '../../core/services/auth.service';
import { LoanApplicationService } from '../../core/services/loan-application.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, AfterViewInit {
  @ViewChild('statusChart') statusChartRef!: ElementRef<HTMLCanvasElement>;
  
  userName = '';
  stats: any[] = [];
  recentApplications: any[] = [];
  recentActivity: any[] = [];
  importantNotifications: any[] = [];
  chartLegend: any[] = [];
  
  totalLoanAmount = 0;
  averageProcessingTime = 0;
  approvalRate = 0;
  
  private statusChart?: Chart;

  constructor(
    private authService: AuthService,
    private loanApplicationService: LoanApplicationService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadUserInfo();
    this.loadDashboardData();
  }

  ngAfterViewInit() {
    this.initializeChart();
  }

  loadUserInfo() {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.userName = user.firstName;
    }
  }

  loadDashboardData() {
    // Load stats
    this.stats = [
      {
        id: 1,
        type: 'primary',
        icon: 'description',
        value: 12,
        label: 'Total Applications',
        change: 15
      },
      {
        id: 2,
        type: 'success',
        icon: 'check_circle',
        value: 8,
        label: 'Approved',
        change: 25
      },
      {
        id: 3,
        type: 'warning',
        icon: 'pending',
        value: 3,
        label: 'Pending Review',
        change: -5
      },
      {
        id: 4,
        type: 'info',
        icon: 'schedule',
        value: 1,
        label: 'In Progress',
        change: 0
      }
    ];

    // Load recent applications
    this.loanApplicationService.getApplications().subscribe(applications => {
      this.recentApplications = applications.slice(0, 5);
    });

    // Load notifications
    this.notificationService.getNotifications().subscribe(notifications => {
      this.importantNotifications = notifications
        .filter(n => !n.isRead)
        .slice(0, 5);
    });

    // Mock data for other sections
    this.loadMockData();
  }

  loadMockData() {
    this.recentActivity = [
      {
        id: 1,
        type: 'application',
        icon: 'description',
        title: 'Application Submitted',
        description: 'Personal loan application #LA20240115001',
        timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000)
      },
      {
        id: 2,
        type: 'approval',
        icon: 'check_circle',
        title: 'Application Approved',
        description: 'Home loan application #LA20240114002',
        timestamp: new Date(Date.now() - 6 * 60 * 60 * 1000)
      },
      {
        id: 3,
        type: 'document',
        icon: 'upload_file',
        title: 'Document Uploaded',
        description: 'Bank statement for application #LA20240113003',
        timestamp: new Date(Date.now() - 24 * 60 * 60 * 1000)
      }
    ];

    this.totalLoanAmount = 125000;
    this.averageProcessingTime = 7;
    this.approvalRate = 85;

    this.chartLegend = [
      { label: 'Approved', count: 8, color: '#22c55e' },
      { label: 'Pending', count: 3, color: '#f59e0b' },
      { label: 'In Review', count: 1, color: '#3b82f6' },
      { label: 'Declined', count: 0, color: '#ef4444' }
    ];
  }

  initializeChart() {
    const ctx = this.statusChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    const config: ChartConfiguration = {
      type: 'doughnut' as ChartType,
      data: {
        labels: ['Approved', 'Pending', 'In Review', 'Declined'],
        datasets: [{
          data: [8, 3, 1, 0],
          backgroundColor: ['#22c55e', '#f59e0b', '#3b82f6', '#ef4444'],
          borderWidth: 0,
          hoverOffset: 10
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          }
        },
        cutout: '70%'
      }
    };

    this.statusChart = new Chart(ctx, config);
  }

  viewApplication(id: string) {
    this.router.navigate(['/applications', id]);
  }

  markAsRead(notification: any) {
    this.notificationService.markAsRead(notification.id).subscribe(() => {
      notification.isRead = true;
    });
  }

  markAllAsRead() {
    this.notificationService.markAllAsRead().subscribe(() => {
      this.importantNotifications.forEach(n => n.isRead = true);
    });
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

  // TrackBy functions for performance
  trackByStatId(index: number, item: any): any {
    return item.id;
  }

  trackByAppId(index: number, item: any): any {
    return item.id;
  }

  trackByActivityId(index: number, item: any): any {
    return item.id;
  }

  trackByNotificationId(index: number, item: any): any {
    return item.id;
  }
}