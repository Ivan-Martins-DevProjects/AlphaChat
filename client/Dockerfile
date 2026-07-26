FROM node:22-alpine AS build
WORKDIR /app
COPY client/package*.json ./
RUN npm ci
COPY client/ .
ARG BUILD_CONFIG=production
RUN npx ng build --configuration $BUILD_CONFIG

FROM nginx:alpine
COPY client/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/alpha-chat-client/browser /usr/share/nginx/html
EXPOSE 4003
CMD ["nginx", "-g", "daemon off;"]
