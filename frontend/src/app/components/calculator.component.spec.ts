import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { CalculatorComponent } from './calculator.component';
import { CdbCalculatorService } from '../services/cdb-calculator.service';
import { LoadingService } from '../services/loading.service';
import { of, throwError } from 'rxjs';

describe('CalculatorComponent', () => {
  let component: CalculatorComponent;
  let fixture: ComponentFixture<CalculatorComponent>;
  let calculatorServiceSpy: jasmine.SpyObj<CdbCalculatorService>;

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('CdbCalculatorService', ['calculate']);
    
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, CalculatorComponent],
      providers: [
        { provide: CdbCalculatorService, useValue: spy },
        LoadingService
      ]
    }).compileComponents();

    calculatorServiceSpy = TestBed.inject(CdbCalculatorService) as jasmine.SpyObj<CdbCalculatorService>;
    fixture = TestBed.createComponent(CalculatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('deve criar o componente', () => {
    expect(component).toBeTruthy();
  });

  it('não deve chamar serviço se formulário for inválido', () => {
    component.form.controls['initialValue'].setValue(null);
    component.onSubmit();
    expect(calculatorServiceSpy.calculate).not.toHaveBeenCalled();
  });

  it('deve chamar o serviço com sucesso', () => {
    const mockResponse = { initialValue: 1000, months: 2, grossValue: 1010, incomeTax: 1, netValue: 1009 };
    calculatorServiceSpy.calculate.and.returnValue(of(mockResponse));
    
    component.form.controls['initialValue'].setValue(1000);
    component.form.controls['months'].setValue(2);
    component.onSubmit();
    
    expect(calculatorServiceSpy.calculate).toHaveBeenCalled();
    expect(component.result()).toEqual(mockResponse);
  });

  it('deve logar erro se serviço falhar', () => {
    spyOn(console, 'error');
    calculatorServiceSpy.calculate.and.returnValue(throwError(() => new Error('API Error')));
    
    component.form.controls['initialValue'].setValue(1000);
    component.form.controls['months'].setValue(2);
    component.onSubmit();
    
    expect(console.error).toHaveBeenCalled();
  });

  it('deve formatar valor corretamente (cobrir os ramos do ternário)', () => {
    expect(component.formatarBrTruncado(1000)).toContain('1.000');
    expect(component.formatarBrTruncado(1000.55)).toContain('1.000,55');
    expect(component.formatarBrTruncado(null)).toBe('');
  });
});