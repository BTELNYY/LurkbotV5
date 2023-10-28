FROM clux/muslrust:stable AS chef
USER root
RUN cargo install cargo-chef
WORKDIR /app


FROM chef AS planner
COPY . .
RUN cargo chef prepare --recipe-path recipe.json

FROM chef AS builder
COPY --from=planner /app/recipe.json recipe.json
# Build dependencies - this is the caching Docker layer!
RUN cargo chef cook --bin backend --release --target x86_64-unknown-linux-musl --recipe-path recipe.json
# Build application
COPY . .
RUN cargo build --bin backend --target x86_64-unknown-linux-musl --release

# We do not need the Rust toolchain to run the binary!
FROM alpine AS runtime
WORKDIR /app
RUN apk update && apk add --no-cache curl
COPY --from=builder /app/target/x86_64-unknown-linux-musl/release/backend /usr/local/bin/backend

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s CMD curl --fail http://localhost:8000/health || exit 1   

ENTRYPOINT ["/usr/local/bin/backend"]