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

  it('deve gerar novo ID se sessionStorage estiver vazio (cobre o lado direito do ||)', () => {
    httpClient.get('/api').subscribe();
    const req = httpMock.expectOne('/api');
    const id = req.request.headers.get('X-Correlation-ID');
    
    expect(id).toBeDefined();
    expect(sessionStorage.getItem('correlationId')).toBe(id);
    req.flush({});
  });

  it('deve reutilizar ID existente no sessionStorage (cobre o lado esquerdo do ||)', () => {
    const existingId = 'fixed-id-123';
    sessionStorage.setItem('correlationId', existingId);
    
    httpClient.get('/api').subscribe();
    const req = httpMock.expectOne('/api');
    
    expect(req.request.headers.get('X-Correlation-ID')).toBe(existingId);
    req.flush({});
  });
});