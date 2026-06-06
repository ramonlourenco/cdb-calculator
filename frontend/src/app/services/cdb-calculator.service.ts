import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CdbCalculationRequest {
  initialValue: number;
  months: number;
}

export interface CdbCalculationResponse {
  initialValue: number;
  months: number;
  grossValue: number;
  incomeTax: number;  
  netValue: number;
}

@Injectable({
  providedIn: 'root'
})
export class CdbCalculatorService {
  private apiUrl = `${environment.apiUrl}/api/cdbcalculator`;

  constructor(private http: HttpClient) {}

  calculate(request: CdbCalculationRequest): Observable<CdbCalculationResponse> {
    return this.http.post<CdbCalculationResponse>(`${this.apiUrl}/calculate`, request);
  }
}