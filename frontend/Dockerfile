FROM node:20-alpine AS builder

WORKDIR /app

# O asterisco (*) torna a cópia do package-lock.json opcional
COPY package*.json ./

# Mudamos para npm install para gerar o mapeamento caso o lock não exista
RUN npm install

COPY . .
RUN npm run build

FROM nginx:alpine

# Ajuste fino: no Angular 17+, o build costuma ir para a pasta /browser
# Se o seu Nginx apresentar tela branca, mude o caminho abaixo para: /app/dist/cdb-calculator-frontend/browser
COPY --from=builder /app/dist/cdb-calculator-frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]