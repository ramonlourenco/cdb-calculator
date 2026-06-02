import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { CorrelationIdInterceptor } from './correlation-id.interceptor';

describe('CorrelationIdInterceptor', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: HTTP_INTERCEPTORS, useClass: CorrelationIdInterceptor, multi: true }
      ]
    });
    httpMock = TestBed.inject(HttpTestingController);
    sessionStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('should add X-Correlation-ID header to requests', () => {
    const httpClient = TestBed.inject(require('@angular/common/http').HttpClient);

    httpClient.get('http://test.com/api').subscribe();

    const req = httpMock.expectOne('http://test.com/api');
    expect(req.request.headers.has('X-Correlation-ID')).toBe(true);
    const correlationId = req.request.headers.get('X-Correlation-ID');
    expect(correlationId).toBeTruthy();
    expect(correlationId?.length).toBeGreaterThan(0);
    req.flush({});
  });

  it('should reuse the same correlation ID for multiple requests', () => {
    const httpClient = TestBed.inject(require('@angular/common/http').HttpClient);
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
