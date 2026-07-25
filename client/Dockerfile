FROM node:22-alpine
WORKDIR /app
COPY client/package*.json ./
RUN npm ci
COPY client/ .
EXPOSE 4003
CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--port", "4003"]
