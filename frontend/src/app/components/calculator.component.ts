import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CdbCalculatorService, CdbCalculationResponse } from '../services/cdb-calculator.service';
import { LoadingService } from '../services/loading.service';

@Component({
  selector: 'app-calculator',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './calculator.component.html',
  styleUrl: './calculator.component.css'
})
export class CalculatorComponent implements OnInit {
  form!: FormGroup;
  result = signal<CdbCalculationResponse | null>(null);
  isLoading = signal(false);

  constructor(
    private fb: FormBuilder,
    private calculatorService: CdbCalculatorService,
    private loadingService: LoadingService
  ) {
    this.isLoading = this.loadingService.isLoading;
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(): void {
    this.form = this.fb.group({
      initialValue: [null, [Validators.required, Validators.min(0.01)]],
      months: [null, [Validators.required, Validators.min(2)]]
    });
  }

  // Bloqueia a digitação de caracteres inválidos
  onKeyDown(event: KeyboardEvent): void {
    const invalidChars = ['.', ',', 'e', '-', '+'];
    if (invalidChars.includes(event.key)) {
      event.preventDefault();
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  onSubmit(): void {
    if (this.form.valid) {
      const { initialValue, months } = this.form.value;
      this.calculatorService.calculate({
        initialValue: parseFloat(initialValue),
        months: parseInt(months, 10)
      }).subscribe({
        next: (response: CdbCalculationResponse) => {
          this.result.set(response);
        },
        error: (error: any) => {
          console.error('Erro na requisição do cálculo:', error);
        }
      });
    }
  }

  formatarBrTruncado(valor: number | null | undefined): string {
    if (valor === null || valor === undefined) return '';
    const partes = valor.toString().split('.');
    const inteiro = partes[0];
    const decimal = partes[1] ? partes[1].substring(0, 2).padEnd(2, '0') : '00';
    return `${inteiro.replace(/\B(?=(\d{3})+(?!\d))/g, '.')},${decimal}`;
  }
}