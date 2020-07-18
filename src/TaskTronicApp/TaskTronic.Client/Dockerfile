FROM node:14.5.0-alpine AS builder

WORKDIR /usr/src/app
ENV PATH /app/node_modules/.bin:$PATH

COPY TaskTronic.Client/package.json .
RUN npm install

COPY . .

CMD npm run build --prod

FROM nginx:1.19-alpine

COPY --from=build /usr/src/app/dist/taskTronic /usr/share/nginx/html