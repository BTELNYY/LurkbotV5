use axum::{response::IntoResponse, routing::get, Router};
use hyper::{StatusCode, Uri};
use lurky_db::Database;
use tower_http::validate_request::ValidateRequestHeaderLayer;
use tracing::info;
use tracing::instrument;
mod backend;
use crate::backend::backend;
use std::net::SocketAddr;
use std::sync::Arc;
use tower::ServiceBuilder;
use tracing_subscriber::prelude::*;
use tracing_subscriber::{fmt, EnvFilter};
#[tokio::main]
#[instrument]
async fn main() {
    let fmt_layer = fmt::layer().with_line_number(true).with_file(true);
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

    info!("Creating DB...");
    let db = Arc::new(
        Database::create_sqlite()
            .await
            .expect("Failed to create database"),
    ); // todo: make this based on config
    info!("DB: {:?}", db);
    let addr = SocketAddr::from(([0, 0, 0, 0], 3000));

    info!("Starting backend task");
    let back_thread = tokio::task::spawn(backend(Arc::clone(&db)));

    let middleware = ServiceBuilder::new();

    let app = Router::new()
        .route("/", get(index))
        .route(
            "/auth",
            get(auth_test).layer(ValidateRequestHeaderLayer::bearer("gaming")),
        )
        .fallback(fallback)
        .with_state(db)
        .with_state(Arc::new(back_thread))
        .layer(middleware);

    info!(
        "Starting Lurky v{} ({}-{}) on {}",
        env!("CARGO_PKG_VERSION"),
        env!("TARGET"),
        env!("PROFILE"),
        addr
    );
    let server = axum::Server::bind(&addr)
        .serve(app.into_make_service())
        .with_graceful_shutdown(shutdown_signal());
    if let Err(e) = server.await {
        eprintln!("server error: {}", e);
    }
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

async fn shutdown_signal() {
    // Wait for the CTRL+C signal
    tokio::signal::ctrl_c()
        .await
        .expect("failed to install CTRL+C signal handler");
}
