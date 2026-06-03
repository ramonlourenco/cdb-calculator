import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { loadingInterceptor } from './loading.interceptor';
import { LoadingService } from '../services/loading.service';

describe('loadingInterceptor', () => {
  let httpMock: HttpTestingController;
  let loadingService: LoadingService;
  let httpClient: HttpClient;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        LoadingService,
        provideHttpClient(withInterceptors([loadingInterceptor])),
        provideHttpClientTesting()
      ]
    });
    httpMock = TestBed.inject(HttpTestingController);
    loadingService = TestBed.inject(LoadingService);
    httpClient = TestBed.inject(HttpClient);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should set isLoading to true during request and false when complete', (done) => {
    // 1. Antes da requisição, precisa ser false
    expect(loadingService.isLoading()).toBe(false);

    httpClient.get('http://test.com/api').subscribe({
      next: () => {
        // Dados recebidos, mas o fluxo ainda não completou aqui
      },
      complete: () => {
        // 4. O fluxo completou com sucesso! O operador 'finalize' foi executado.
        expect(loadingService.isLoading()).toBe(false);
        done();
      },
      error: () => {
        done();
      }
    });

    // 2. Com a requisição ativa no ar, precisa ser true
    expect(loadingService.isLoading()).toBe(true);

    // 3. Simula a resposta do servidor chegando
    const req = httpMock.expectOne('http://test.com/api');
    req.flush({ data: 'test' });
  });
});