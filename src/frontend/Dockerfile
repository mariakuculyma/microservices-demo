FROM golang:1.13.8-alpine as builder
RUN apk add --no-cache ca-certificates git build-base

ENV PROJECT github.com/GoogleCloudPlatform/microservices-demo/src/frontend
WORKDIR /go/src/$PROJECT

# Download Go dependencies
COPY go.mod .
COPY go.sum .
RUN go mod download

# Copy application code into container
COPY . .

# Build dynamically linked binary
RUN go install -ldflags '-linkmode=external' 

FROM alpine as release
RUN apk add --no-cache ca-certificates \
    busybox-extras net-tools bind-tools
WORKDIR /frontend
COPY --from=builder /go/bin/frontend /frontend/server
COPY ./templates ./templates
COPY ./static ./static
EXPOSE 8080
ENTRYPOINT ["/frontend/server"]
