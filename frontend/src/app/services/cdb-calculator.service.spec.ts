import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CdbCalculatorService } from './services/cdb-calculator.service';

describe('CdbCalculatorService', () => {
  let service: CdbCalculatorService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CdbCalculatorService]
    });
    service = TestBed.inject(CdbCalculatorService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call calculate endpoint with correct parameters', () => {
    const mockResponse = {
      initialValue: 1000,
      months: 12,
      grossValue: 1109.02,
      incomeTax: 21.80,
      netValue: 1087.22
    };

    service.calculate({ initialValue: 1000, months: 12 }).subscribe(result => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne('http://localhost:8080/api/cdbcalculator/calculate');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ initialValue: 1000, months: 12 });
    req.flush(mockResponse);
  });

  it('should handle calculation error', () => {
    service.calculate({ initialValue: 1000, months: 12 }).subscribe(
      () => fail('should have failed'),
      (error) => {
        expect(error.status).toBe(400);
      }
    );

    const req = httpMock.expectOne('http://localhost:8080/api/cdbcalculator/calculate');
    req.flush('Invalid input', { status: 400, statusText: 'Bad Request' });
  });
});
