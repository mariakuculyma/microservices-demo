FROM golang:1.13.8-alpine AS builder
RUN apk add --no-cache ca-certificates git build-base 

ENV PROJECT github.com/GoogleCloudPlatform/microservices-demo/src/productcatalogservice
WORKDIR /go/src/$PROJECT

# Download Go dependencies
COPY go.mod .
COPY go.sum .
RUN go mod download

# Copy application code into container
COPY . .

# Build dynamically linked binary
RUN go build -ldflags '-linkmode=external' -o /productcatalogservice .

FROM alpine AS release
RUN apk add --no-cache ca-certificates
RUN GRPC_HEALTH_PROBE_VERSION=v0.2.0 && \
    wget -qO/bin/grpc_health_probe https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/${GRPC_HEALTH_PROBE_VERSION}/grpc_health_probe-linux-amd64 && \
    chmod +x /bin/grpc_health_probe
WORKDIR /productcatalogservice
COPY --from=builder /productcatalogservice ./server
COPY products.json .
EXPOSE 3550
ENTRYPOINT ["/productcatalogservice/server"]

