import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CdbCalculatorService } from './cdb-calculator.service';

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
    const mockResponse = { initialValue: 1000, months: 12, grossValue: 1125, incomeTax: 25, netValue: 1100 };
    service.calculate({ initialValue: 1000, months: 12 }).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne('http://localhost:8080/api/cdbcalculator/calculate');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should handle calculation error', (done) => {
    service.calculate({ initialValue: 1000, months: 12 }).subscribe({
      error: (err) => {
        expect(err.status).toBe(500);
        done();
      }
    });

    const req = httpMock.expectOne('http://localhost:8080/api/cdbcalculator/calculate');
    req.error(new ProgressEvent('error'), { status: 500 });
  });
});