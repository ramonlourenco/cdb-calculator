import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http'; // Importado 'withInterceptorsFromDi'
import { CalculatorComponent } from './app/components/calculator.component';
import { CorrelationIdInterceptor } from './app/interceptors/correlation-id.interceptor';
import { LoadingInterceptor } from './app/interceptors/loading.interceptor';

bootstrapApplication(CalculatorComponent, {
  providers: [
    // ATENÇÃO: Adicionado o metódo explicitamente aqui dentro
    provideHttpClient(withInterceptorsFromDi()), 
    
    { provide: HTTP_INTERCEPTORS, useClass: CorrelationIdInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: LoadingInterceptor, multi: true }
  ]
}).catch(err => console.error(err));