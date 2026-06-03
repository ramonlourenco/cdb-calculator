import { Component, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CdbCalculatorService, CdbCalculationResponse } from '../services/cdb-calculator.service';
import { LoadingService } from '../services/loading.service';

@Component({
  selector: 'app-calculator',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="container">
      <h1>Calculadora de CDB B3</h1>

      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="form-group">
          <label for="initialValue">Valor Inicial (R$)</label>
          <input
            type="number"
            id="initialValue"
            formControlName="initialValue"
            placeholder="Digite o valor inicial"
            step="0.01"
            min="0"
          />
          <span class="error" *ngIf="isFieldInvalid('initialValue')">
            O valor inicial deve ser maior que zero
          </span>
        </div>

        <div class="form-group">
          <label for="months">Meses</label>
          <input
            type="number"
            id="months"
            formControlName="months"
            placeholder="Digite a quantidade de meses"
            min="1"
          />
          <span class="error" *ngIf="isFieldInvalid('months')">
            A quantidade de meses deve ser pelo menos 1
          </span>
        </div>

        <button
          type="submit"
          [disabled]="!form.valid || isLoading()"
          class="btn-primary"
        >
          {{ isLoading() ? 'Calculando...' : 'Calcular' }}
        </button>
      </form>

      <div class="loading-overlay" *ngIf="isLoading()">
        <div class="spinner"></div>
      </div>

      <div class="result-card" *ngIf="result()">
        <h2>Resultados</h2>
        <div class="result-item">
          <span class="label">Valor Inicial:</span>
          <span class="value">{{ formatarBrTruncado(result()!.initialValue) }}</span>
        </div>
        <div class="result-item">
          <span class="label">Valor Bruto:</span>
          <span class="value">{{ formatarBrTruncado(result()!.grossValue) }}</span>
        </div>
        <div class="result-item">
          <span class="label">Imposto de Renda:</span>
          <span class="value" style="color: #dc3545;">- {{ formatarBrTruncado(result()!.incomeTax) }}</span>
        </div>
        <div class="result-item highlight">
          <span class="label">Valor Líquido:</span>
          <span class="value" style="color: #28a745;">{{ formatarBrTruncado(result()!.netValue) }}</span>
        </div>
        <div class="result-item">
          <span class="label">Prazo:</span>
          <span class="value">{{ result()!.months }} {{ result()!.months === 1 ? 'mês' : 'meses' }}</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 600px;
      margin: 0 auto;
      padding: 20px;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
    }

    h1 {
      text-align: center;
      color: #333;
      margin-bottom: 30px;
    }

    form {
      background: #f5f5f5;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 30px;
    }

    .form-group {
      margin-bottom: 20px;
    }

    label {
      display: block;
      margin-bottom: 8px;
      font-weight: 600;
      color: #333;
    }

    input {
      width: 100%;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 16px;
      box-sizing: border-box;
    }

    input:focus {
      outline: none;
      border-color: #007bff;
      box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.25);
    }

    .error {
      display: block;
      color: #dc3545;
      font-size: 12px;
      margin-top: 5px;
    }

    .btn-primary {
      width: 100%;
      padding: 12px;
      background-color: #007bff;
      color: white;
      border: none;
      border-radius: 4px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: background-color 0.3s;
    }

    .btn-primary:hover:not(:disabled) {
      background-color: #0056b3;
    }

    .btn-primary:disabled {
      background-color: #ccc;
      cursor: not-allowed;
    }

    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1000;
    }

    .spinner {
      border: 4px solid #f3f3f3;
      border-top: 4px solid #007bff;
      border-radius: 50%;
      width: 40px;
      height: 40px;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .result-card {
      background: #f9f9f9;
      padding: 20px;
      border-radius: 8px;
      border-left: 4px solid #28a745;
    }

    .result-card h2 {
      margin-top: 0;
      color: #333;
      font-size: 20px;
    }

    .result-item {
      display: flex;
      justify-content: space-between;
      padding: 10px 0;
      border-bottom: 1px solid #eee;
    }

    .result-item.highlight {
      font-weight: 600;
      font-size: 18px;
      padding: 15px 0;
      border-top: 2px solid #28a745;
      border-bottom: 2px solid #28a745;
      margin: 10px 0;
    }

    .label {
      color: #666;
    }

    .value {
      color: #333;
      font-weight: 600;
    }
  `]
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
      months: [null, [Validators.required, Validators.min(1)]]
    });
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

  /**
   * Trunca o valor estritamente em 2 casas decimais (sem arredondar) 
   * e formata no padrão de moeda Brasileiro (pt-BR).
   */
  formatarBrTruncado(valor: number | null | undefined): string {
    if (valor === null || valor === undefined) return '';

    // Separando parte inteira da decimal usando string para evitar imprecisões do tipo float do JS
    const partes = valor.toString().split('.');
    const inteiro = partes[0];
    
    // Pega somente os 2 primeiros caracteres após o ponto (truncamento cirúrgico)
    const decimal = partes[1] ? partes[1].substring(0, 2).padEnd(2, '0') : '00';
    
    // Reconstrói o número limpo
    const valorTruncado = parseFloat(`${inteiro}.${decimal}`);

    // Aplica a máscara financeira pt-BR (R$)
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(valorTruncado);
  }
}