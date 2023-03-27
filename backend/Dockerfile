FROM lukemathwalker/cargo-chef:latest-rust-1 as chef
WORKDIR /app
ENV CARGO_REGISTRIES_CRATES_IO_PROTOCOL=sparse

FROM chef AS planner
COPY . .
RUN cargo chef prepare --recipe-path recipe.json

FROM chef AS builder 
COPY --from=planner /app/recipe.json recipe.json
# Build dependencies - this is the caching Docker layer!
RUN cargo chef cook --release --recipe-path recipe.json
# Build application
COPY . .
RUN cargo build --release -p backend

# We do not need the Rust toolchain to run the binary!
FROM debian:buster-slim AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y libssl-dev openssl libssl1.1 ca-certificates
COPY --from=builder /app/target/release/http /usr/local/bin/backend

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s CMD curl --fail http://localhost:8000/health || exit 1   

ENTRYPOINT ["/usr/local/bin/backend"]