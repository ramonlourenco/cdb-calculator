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

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('should add X-Correlation-ID header to requests', () => {
    httpClient.get('http://test.com/api').subscribe();

    const req = httpMock.expectOne('http://test.com/api');
    expect(req.request.headers.has('X-Correlation-ID')).toBe(true);
    const correlationId = req.request.headers.get('X-Correlation-ID');
    expect(correlationId).toBeTruthy();
    expect(correlationId?.length).toBeGreaterThan(0);
    req.flush({});
  });

  it('should reuse the same correlation ID for multiple requests', () => {
    let firstCorrelationId: string | null = null;

    httpClient.get('http://test.com/api1').subscribe();
    let req = httpMock.expectOne('http://test.com/api1');
    firstCorrelationId = req.request.headers.get('X-Correlation-ID');
    req.flush({});

    httpClient.get('http://test.com/api2').subscribe();
    req = httpMock.expectOne('http://test.com/api2');
    const secondCorrelationId = req.request.headers.get('X-Correlation-ID');
    req.flush({});

    expect(firstCorrelationId).toBe(secondCorrelationId);
  });
});