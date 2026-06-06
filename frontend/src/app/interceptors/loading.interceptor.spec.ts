import { TestBed, fakeAsync, tick } from '@angular/core/testing';
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

  it('deve setar loading true durante requisição e false quando completa com sucesso', fakeAsync(() => {
    httpClient.get('/api').subscribe();
    expect(loadingService.isLoading()).toBe(true);

    const req = httpMock.expectOne('/api');
    req.flush({});
    tick(0); 

    expect(loadingService.isLoading()).toBe(false);
  }));

  it('deve garantir loading false mesmo se a requisição falhar', fakeAsync(() => {
    // Usamos o catchError do subscribe apenas para evitar que o erro suba para o Jasmine
    httpClient.get('/api').subscribe({
      error: (err) => expect(err).toBeDefined() 
    });

    expect(loadingService.isLoading()).toBe(true);

    const req = httpMock.expectOne('/api');
    req.error(new ProgressEvent('error'));
    tick(0); 

    expect(loadingService.isLoading()).toBe(false);
  }));
});