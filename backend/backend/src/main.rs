use lurky::config::LurkyConfig;
use rocket::http::ContentType;
use rocket::tokio::spawn;
use rocket::{catch, catchers};
mod backend;
mod northwood;
use std::path::PathBuf;
use std::sync::Arc;
mod routes;
use clap::Parser;
use lurky::db;
#[derive(Debug, Clone, Parser)]
#[clap(author, version, about, long_about = None)]
struct Args {
    config: PathBuf,
}

impl Args {
    fn validate(&self) -> Result<(), String> {
        if !self.config.exists() {
            return Err(format!(
                "Config file does not exist: {}",
                self.config.display()
            ));
        }
        Ok(())
    }
}
#[catch(default)]
async fn default_error_catcher(
    status: rocket::http::Status,
    _: &rocket::Request<'_>,
) -> (rocket::http::ContentType, (rocket::http::Status, Vec<u8>)) {
    let http_cat = format!("https://http.cat/{}", status.code);
    let e = reqwest::get(http_cat).await;
    if let Ok(e) = e {
        let bytes = e.bytes().await;
        if let Ok(bytes) = bytes {
            return (ContentType::JPEG, (status, bytes.to_vec()));
        }
    }
    return (ContentType::Text, (status, Vec::new()));
}

#[rocket::main]
async fn main() -> Result<(), anyhow::Error> {
    let args = Args::parse();
    if let Err(e) = args.validate() {
        eprintln!("Error: {}", e);
        std::process::exit(1);
    }
    let config = Arc::new(LurkyConfig::parse_data(std::fs::File::open(args.config)?));
    println!("{:?}", config);
    let mut db = db::create_db_from_config(&config)?;
    db.setup().await?;
    let db = Arc::new(db);
    let backend_thread = spawn(backend(Arc::clone(&config), Arc::clone(&db)));
    let _rocket = rocket::build()
        .register("/", catchers![default_error_catcher])
        .mount("/", routes::basics::routes())
        .mount("/nw", routes::northwood::routes())
        .mount("/query", routes::query::routes())
        .manage(Arc::clone(&config))
        .manage(Arc::clone(&db))
        .manage(backend_thread)
        .launch()
        .await?;

    Ok(())
}
