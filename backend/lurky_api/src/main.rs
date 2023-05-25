use axum::{response::IntoResponse, Router};
use hyper::{StatusCode, Uri};
use lurky_common::tokio;
use lurky_common::tracing;
use lurky_common::tracing::info;
use lurky_common::tracing::instrument;

use std::net::SocketAddr;
use tower::ServiceBuilder;
use tracing_subscriber::prelude::*;
use tracing_subscriber::{fmt, EnvFilter};
#[tokio::main]
#[instrument]
async fn main() {
    let fmt_layer = fmt::layer();
    let filter_layer = EnvFilter::try_from_default_env()
        .or_else(|_| EnvFilter::try_new("info"))
        .unwrap();

    tracing_subscriber::registry()
        .with(filter_layer)
        .with(fmt_layer)
        .init();
    let middleware = ServiceBuilder::new();

    let app = Router::new().fallback(fallback).layer(middleware);

    let addr = SocketAddr::from(([0, 0, 0, 0], 3000));
    info!("Starting Lurky v{} on {}", env!("CARGO_PKG_VERSION"), addr);
    axum::Server::bind(&addr)
        .serve(app.into_make_service())
        .await
        .unwrap();
}
#[instrument]
async fn fallback(url: Uri) -> impl IntoResponse {
    (StatusCode::NOT_FOUND, format!("Not found: {}", url.path()))
}
