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
  let loadingService: LoadingService;

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
    loadingService = TestBed.inject(LoadingService);
    fixture = TestBed.createComponent(CalculatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('deve criar o componente', () => {
    expect(component).toBeTruthy();
  });

  it('deve verificar se o loadingService está instanciado', () => {
    // Isso utiliza a variável 'loadingService' e resolve o erro do Lint
    expect(loadingService).toBeDefined();
  });

  it('deve invalidar o formulário se months for 1 (regra > 1)', () => {
    component.form.controls['initialValue'].setValue(1000);
    component.form.controls['months'].setValue(1);
    expect(component.form.valid).toBeFalse();
  });

  it('deve validar o formulário se months for 2', () => {
    component.form.controls['initialValue'].setValue(1000);
    component.form.controls['months'].setValue(2);
    expect(component.form.valid).toBeTrue();
  });

  it('deve chamar o serviço quando o formulário for submetido', () => {
    const mockResponse = {
      initialValue: 1000, 
      months: 2, 
      grossValue: 1010, 
      incomeTax: 1, 
      netValue: 1009
    };
    
    calculatorServiceSpy.calculate.and.returnValue(of(mockResponse));
    
    component.form.controls['initialValue'].setValue(1000);
    component.form.controls['months'].setValue(2);
    component.onSubmit();
    
    expect(calculatorServiceSpy.calculate).toHaveBeenCalled();
    expect(component.result()).toEqual(mockResponse);
  });

  it('deve logar erro no console quando o serviço falhar', () => {
    spyOn(console, 'error');
    calculatorServiceSpy.calculate.and.returnValue(throwError(() => new Error('API Error')));
    
    component.form.controls['initialValue'].setValue(1000);
    component.form.controls['months'].setValue(2);
    component.onSubmit();
    
    expect(console.error).toHaveBeenCalled();
    expect(component.result()).toBeNull();
  });

  it('deve formatar valor corretamente', () => {
    const formatado = component.formatarBrTruncado(1000.567);
    expect(formatado).toContain('1.000,56');
  });
});