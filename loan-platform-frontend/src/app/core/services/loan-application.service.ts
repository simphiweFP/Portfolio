import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoanApplication {
  id: string;
  applicationNumber: string;
  userId: string;
  loanTypeId: string;
  requestedAmount: number;
  loanTermMonths: number;
  purpose: string;
  monthlyIncome: number;
  monthlyExpenses: number;
  employmentStatus: string;
  employerName: string;
  yearsEmployed: number;
  status: string;
  createdAt: Date;
  submittedAt?: Date;
  reviewedAt?: Date;
  approvedAt?: Date;
  reviewerUserId?: string;
  reviewNotes?: string;
  approvedAmount?: number;
  interestRate?: number;
  monthlyPayment?: number;
  loanType?: any;
  user?: any;
  documents?: any[];
  history?: any[];
}

export interface CreateLoanApplicationRequest {
  loanTypeId: string;
  requestedAmount: number;
  loanTermMonths: number;
  purpose: string;
  monthlyIncome: number;
  monthlyExpenses: number;
  employmentStatus: string;
  employerName: string;
  yearsEmployed: number;
}

@Injectable({
  providedIn: 'root'
})
export class LoanApplicationService {
  private apiUrl = `${environment.apiUrl}/loanapplications`;

  constructor(private http: HttpClient) {}

  getApplications(status?: string): Observable<LoanApplication[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    
    return this.http.get<LoanApplication[]>(this.apiUrl, { params });
  }

  getApplication(id: string): Observable<LoanApplication> {
    return this.http.get<LoanApplication>(`${this.apiUrl}/${id}`);
  }

  createApplication(application: CreateLoanApplicationRequest): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(this.apiUrl, application);
  }

  updateApplication(id: string, application: Partial<LoanApplication>): Observable<LoanApplication> {
    return this.http.put<LoanApplication>(`${this.apiUrl}/${id}`, application);
  }

  submitApplication(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/submit`, {});
  }

  reviewApplication(id: string, review: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/review`, review);
  }

  deleteApplication(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}