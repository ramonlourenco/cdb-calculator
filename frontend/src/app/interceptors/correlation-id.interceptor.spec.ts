import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { correlationIdInterceptor } from './correlation-id.interceptor';

describe('correlationIdInterceptor', () => {
  let httpMock: HttpTestingController;
  let httpClient: HttpClient;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([correlationIdInterceptor])),
        provideHttpClientTesting()
      ]
    });
    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
    sessionStorage.clear();
  });

  it('should create new correlation ID if none exists', () => {
    httpClient.get('/api').subscribe();
    const req = httpMock.expectOne('/api');
    expect(req.request.headers.get('X-Correlation-ID')).not.toBeNull();
    req.flush({});
  });

  it('should reuse existing correlation ID from session storage', () => {
    sessionStorage.setItem('correlationId', 'abc-123');
    httpClient.get('/api').subscribe();
    const req = httpMock.expectOne('/api');
    expect(req.request.headers.get('X-Correlation-ID')).toBe('abc-123');
    req.flush({});
  });
});