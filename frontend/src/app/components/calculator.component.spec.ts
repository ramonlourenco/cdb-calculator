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
      providers: [{ provide: CdbCalculatorService, useValue: spy }, LoadingService]
    }).compileComponents();

    calculatorServiceSpy = TestBed.inject(CdbCalculatorService) as jasmine.SpyObj<CdbCalculatorService>;
    fixture = TestBed.createComponent(CalculatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('deve impedir caracteres inválidos no teclado', () => {
    const event = new KeyboardEvent('keydown', { key: '.' });
    spyOn(event, 'preventDefault');
    component.onKeyDown(event);
    expect(event.preventDefault).toHaveBeenCalled();
  });

  it('deve permitir números no teclado', () => {
    const event = new KeyboardEvent('keydown', { key: '5' });
    spyOn(event, 'preventDefault');
    component.onKeyDown(event);
    expect(event.preventDefault).not.toHaveBeenCalled();
  });

  it('deve calcular com sucesso', () => {
    const mock = { initialValue: 1000, months: 2, grossValue: 1010, incomeTax: 1, netValue: 1009 };
    calculatorServiceSpy.calculate.and.returnValue(of(mock));
    component.form.setValue({ initialValue: 1000, months: 2 });
    component.onSubmit();
    expect(component.result()).toEqual(mock);
  });

  it('deve lidar com erro na API', () => {
    spyOn(console, 'error');
    calculatorServiceSpy.calculate.and.returnValue(throwError(() => 'Erro'));
    component.form.setValue({ initialValue: 1000, months: 2 });
    component.onSubmit();
    expect(console.error).toHaveBeenCalled();
  });

  it('deve formatar valor corretamente', () => {
    expect(component.formatarBrTruncado(1000.55)).toBe('1.000,55');
    expect(component.formatarBrTruncado(1000)).toBe('1.000,00');
    expect(component.formatarBrTruncado(null)).toBe('');
  });

  it('deve detectar campo inválido', () => {
    component.form.controls['initialValue'].setValue(null);
    component.form.controls['initialValue'].markAsTouched();
    expect(component.isFieldInvalid('initialValue')).toBeTrue();
  });
});