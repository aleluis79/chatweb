### STAGE 1: Build ###

# We label our stage as ‘builder’
FROM node:20-alpine as builder

ARG BUILD_VERSION
# If BUILD_VERSION is set/non-empty, use it, otherwise use a default value
ARG VERSION=${BUILD_VERSION:-1.0.0}

ARG CONFIGURATION=production

COPY package.json package-lock.json ./

## Storing node modules on a separate layer will prevent unnecessary npm installs at each build
RUN npm i && mkdir /app && mv ./node_modules ./app

WORKDIR /app

COPY . .

## Build the angular app in production mode and store the artifacts in dist folder
RUN npm run build -- --output-path=dist --base-href / --configuration=${CONFIGURATION}

### STAGE 2: Setup ###

FROM nginx:1.25-alpine

# Copy custom nginx config
COPY ./nginx.conf /etc/nginx/nginx.conf

## Remove default nginx website
RUN rm -rf /usr/share/nginx/html/*

## From ‘builder’ stage copy over the artifacts in dist folder to default nginx public folder
COPY --from=builder /app/dist/browser /usr/share/nginx/html

EXPOSE 80 443

ENTRYPOINT ["nginx", "-g", "daemon off;"]
