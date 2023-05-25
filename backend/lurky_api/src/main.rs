use axum::response::Response;
use axum::{response::IntoResponse, routing::get, Router};
use hyper::{StatusCode, Uri};
use lurky_common::tokio;
use lurky_common::tracing;
use lurky_common::tracing::info;
use lurky_common::tracing::instrument;
use tower_http::auth::require_authorization::Bearer;
use tower_http::validate_request::{ValidateRequestHeader, ValidateRequestHeaderLayer};

use std::net::SocketAddr;
use tower::{ServiceBuilder, Layer};
use tracing_subscriber::prelude::*;
use tracing_subscriber::{fmt, EnvFilter};
#[tokio::main]
#[instrument]
async fn main() {
    let fmt_layer = fmt::layer();
    let filter_layer = EnvFilter::try_from_default_env()
        .or_else(|_| {
            if cfg!(debug_assertions) {
                EnvFilter::try_new("debug")
            } else {
                EnvFilter::try_new("info")
            }
        })
        .unwrap();

    tracing_subscriber::registry()
        .with(filter_layer)
        .with(fmt_layer)
        .init();
    let middleware = ServiceBuilder::new();

    let app = Router::new()
        .route("/", get(index))
        .route("/auth", get(auth_test).layer(ValidateRequestHeaderLayer::bearer("gaming")))
        .fallback(fallback)
        .layer(middleware);

    let addr = SocketAddr::from(([0, 0, 0, 0], 3000));
    info!(
        "Starting Lurky v{} ({}-{}) on {}",
        env!("CARGO_PKG_VERSION"),
        env!("TARGET"),
        env!("PROFILE"),
        addr
    );
    axum::Server::bind(&addr)
        .serve(app.into_make_service())
        .await
        .unwrap();
}
#[instrument]
async fn fallback(url: Uri) -> impl IntoResponse {
    (StatusCode::NOT_FOUND, format!("Not found: {}", url.path()))
}
#[instrument]
async fn index() -> impl IntoResponse {
    format!(
        "Lurky v{} ({}-{})",
        env!("CARGO_PKG_VERSION"),
        env!("TARGET"),
        env!("PROFILE")
    )
}
#[instrument]
async fn auth_test() -> impl IntoResponse {
    "auth ok!"
}
