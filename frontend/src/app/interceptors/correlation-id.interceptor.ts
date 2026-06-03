import { HttpInterceptorFn } from '@angular/common/http';
import { v4 as uuidv4 } from 'uuid';

export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  const correlationId = sessionStorage.getItem('correlationId') || uuidv4();
  sessionStorage.setItem('correlationId', correlationId);

  const clonedReq = req.clone({
    setHeaders: {
      'X-Correlation-ID': correlationId
    }
  });

  return next(clonedReq);
};