use backend::backend;
use config::Config;
use rocket::tokio::spawn;
mod backend;
mod config;
mod northwood;
use std::path::PathBuf;
use std::sync::Arc;
mod routes;
use clap::Parser;
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

#[rocket::main]
async fn main() -> Result<(), anyhow::Error> {
    let args = Args::parse();
    if let Err(e) = args.validate() {
        eprintln!("Error: {}", e);
        std::process::exit(1);
    }
    let config = Arc::new(Config::parse(std::fs::File::open(args.config)?)?);
    if let Err(e) = config.validate() {
        eprintln!("Config Error: {}", e);
        std::process::exit(1);
    }
    println!("{:?}", config);
    let backend_thread = spawn(backend(Arc::clone(&config)));
    let _rocket = rocket::build()
        .mount("/", routes::basics::routes())
        .mount("/nw", routes::northwood::routes())
        .manage(Arc::clone(&config))
        .manage(backend_thread)
        .launch()
        .await?;

    Ok(())
}
