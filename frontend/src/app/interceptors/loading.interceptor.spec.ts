import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { LoadingInterceptor } from './loading.interceptor';
import { LoadingService } from '../services/loading.service';

describe('LoadingInterceptor', () => {
  let httpMock: HttpTestingController;
  let loadingService: LoadingService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        LoadingService,
        { provide: HTTP_INTERCEPTORS, useClass: LoadingInterceptor, multi: true }
      ]
    });
    httpMock = TestBed.inject(HttpTestingController);
    loadingService = TestBed.inject(LoadingService);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should set isLoading to true during request and false when complete', (done) => {
    const httpClient = TestBed.inject(require('@angular/common/http').HttpClient);

    expect(loadingService.isLoading()).toBe(false);

    httpClient.get('http://test.com/api').subscribe(() => {
      expect(loadingService.isLoading()).toBe(false);
      done();
    });

    expect(loadingService.isLoading()).toBe(true);

    const req = httpMock.expectOne('http://test.com/api');
    req.flush({ data: 'test' });
  });
});
