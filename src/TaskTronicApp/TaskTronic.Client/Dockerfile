
FROM node:14.4.0-alpine
WORKDIR /usr/src/app/app-client
COPY package*.json ./
RUN npm install -g @angular/cli @angular-devkit/build-angular && npm install
EXPOSE 4200
CMD ["npm", "start"]