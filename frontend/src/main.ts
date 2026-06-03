import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { CalculatorComponent } from './app/components/calculator.component';
import { correlationIdInterceptor } from './app/interceptors/correlation-id.interceptor';
import { loadingInterceptor } from './app/interceptors/loading.interceptor';

bootstrapApplication(CalculatorComponent, {
  providers: [
    // Configuração moderna para interceptores baseados em funções
    provideHttpClient(
      withInterceptors([
        correlationIdInterceptor,
        loadingInterceptor
      ])
    )
  ]
}).catch(err => console.error(err));