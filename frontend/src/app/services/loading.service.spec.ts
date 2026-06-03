import { TestBed } from '@angular/core/testing';
import { LoadingService } from './loading.service';

describe('LoadingService', () => {
  let service: LoadingService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LoadingService]
    });
    service = TestBed.inject(LoadingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize isLoading as false', () => {
    expect(service.isLoading()).toBe(false);
  });

  it('should update isLoading signal', () => {
    service.isLoading.set(true);
    expect(service.isLoading()).toBe(true);

    service.isLoading.set(false);
    expect(service.isLoading()).toBe(false);
  });
});
