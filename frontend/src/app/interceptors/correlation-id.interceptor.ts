import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';

@Injectable()
export class CorrelationIdInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const correlationId = sessionStorage.getItem('correlationId') || uuidv4();
    sessionStorage.setItem('correlationId', correlationId);

    const clonedReq = req.clone({
      setHeaders: {
        'X-Correlation-ID': correlationId
      }
    });

    return next.handle(clonedReq);
  }
}
