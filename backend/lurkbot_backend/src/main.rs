use std::{
    collections::HashMap, error::Error, net::SocketAddr, path::PathBuf, process::ExitCode,
    sync::Arc,
};

use axum::{routing::get, Router};
use clap::Parser;
use lurkbot_common::*;
use tracing::{debug, error, info, instrument};
mod backend_task;
#[derive(Parser)]
struct CommandLineArgs {
    configpath: PathBuf,
}

#[tokio::main]
async fn main() -> ExitCode {
    tracing_subscriber::fmt::init();
    if let Err(e) = lb().await {
        error!("Error while running LurkbotBackend: {}", e);
        return ExitCode::FAILURE;
    }
    return ExitCode::SUCCESS;
}

#[instrument]
async fn lb() -> Result<(), LurkbotError> {
    info!("LurkbotBackend v{}", env!("CARGO_PKG_VERSION"));
    info!("May god have mercy on our souls.");
    info!(
        r"
    ⠄⠄⠄⠄⠄⠄⠄⠄⠄⢀⣤⣶⣿⣿⣿⣿⣿⣿⣿⣶⣄⠄⠄⠄⠄⠄⠄⠄⠄⠄
    ⠄⠄⠄⠄⠄⠄⠄⢀⣴⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⠄⠄⠄⠄⠄⠄⠄⠄
    ⠄⠄⠄⠄⠄⠄⢀⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⠄⠄⠄⠄⠄⠄⠄
    ⠄⠄⠄⠄⠄⣴⡿⠛⠉⠁⠄⠄⠄⠄⠈⢻⣿⣿⣿⣿⣿⣿⣿⠄⠄⠄⠄⠄⠄⠄
    ⠄⠄⠄⠄⢸⣿⡅⠄⠄⠄⠄⠄⠄⠄⣠⣾⣿⣿⣿⣿⣿⣿⣿⣷⣶⣶⣦⠄⠄⠄
    ⠄⠄⠄⠄⠸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣇⠄⠄
    ⠄⠄⠄⠄⠄⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄
    ⠄⠄⠄⠄⠄⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄
    ⠄⠄⠄⠄⠄⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄
    ⠄⠄⠄⠄⠄⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄
    ⠄⠄⠄⠄⠄⠘⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄
    ⠄⠄⠄⠄⠄⠄⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⠛⠛⠛⠃⠄⠄
    ⠄⠄⠄⠄⠄⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄⠄⠄⠄⠄
    ⠄⠄⠄⠄⢰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠄⠄⠄⠄⠄
    ⠄⠄⠄⢀⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄⠄⠄⠄
    ⠄⠄⠄⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡆⠄⠄⠄⠄
    ⠄⠄⢠⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠃⠄⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠇⠄⠄⠄⠄
    ⠄⠄⢸⣿⣿⣿⣿⣿⣿⣿⡿⠟⠁⠄⠄⠄⠻⣿⣿⣿⣿⣿⣿⣿⡿⠄⠄⠄⠄⠄
    ⠄⠄⢸⣿⣿⣿⣿⣿⡿⠋⠄⠄⠄⠄⠄⠄⠄⠙⣿⣿⣿⣿⣿⣿⡇⠄⠄⠄⠄⠄
    ⠄⠄⢸⣿⣿⣿⣿⣿⣧⡀⠄⠄⠄⠄⠄⠄⠄⢀⣾⣿⣿⣿⣿⣿⡇⠄⠄⠄⠄⠄
    ⠄⠄⢸⣿⣿⣿⣿⣿⣿⣿⡄⠄⠄⠄⠄⠄⠄⣿⣿⣿⣿⣿⣿⣿⣷⠄⠄⠄⠄⠄
    ⠄⠄⠸⣿⣿⣿⣿⣿⣿⣿⣷⠄⠄⠄⠄⠄⢰⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄⠄⠄⠄
    ⠄⠄⠄⢿⣿⣿⣿⣿⣿⣿⡟⠄⠄⠄⠄⠄⠸⣿⣿⣿⣿⣿⣿⣿⠏⠄⠄⠄⠄⠄
    ⠄⠄⠄⠈⢿⣿⣿⣿⣿⠏⠄⠄⠄⠄⠄⠄⠄⠙⣿⣿⣿⣿⣿⠏⠄⠄⠄⠄⠄⠄
    ⠄⠄⠄⠄⠘⣿⣿⣿⣿⡇⠄⠄⠄⠄⠄⠄⠄⠄⣿⣿⣿⣿⡏⠄⠄⠄⠄⠄⠄⠄
    ⠄⠄⠄⠄⠄⢸⣿⣿⣿⣧⠄⠄⠄⠄⠄⠄⠄⢀⣿⣿⣿⣿⡇⠄⠄⠄⠄⠄⠄⠄
    ⠄⠄⠄⠄⠄⣸⣿⣿⣿⣿⣆⠄⠄⠄⠄⠄⢀⣾⣿⣿⣿⣿⣿⣄⠄⠄⠄⠄⠄⠄
    ⠄⣀⣀⣤⣾⣿⣿⣿⣿⡿⠟⠄⠄⠄⠄⠄⠸⣿⣿⣿⣿⣿⣿⣿⣷⣄⣀⠄⠄⠄
    ⠸⠿⠿⠿⠿⠿⠿⠟⠁⠄⠄⠄⠄⠄⠄⠄⠄⠄⠉⠉⠛⠿⢿⡿⠿⠿⠿⠃⠄⠄                      
    "
    );

    // parse command line arguments
    let args = CommandLineArgs::parse();

    if !args.configpath.exists() {
        return Err(LurkbotError::FileDoesntExist(args.configpath));
    }

    if !args.configpath.is_file() {
        return Err(LurkbotError::GenericConfigError(
            "Config is not a file!".to_string(),
        ));
    }

    // read that shit
    let config_bytes = std::fs::read(args.configpath)
        .map_err(|e| LurkbotError::GenericConfigError(format!("{}", e)))?;

    let config_text = String::from_utf8(config_bytes).map_err(|e| {
        LurkbotError::GenericConfigError(format!("Config is not valid utf8! {}", e))
    })?;

    let config = Arc::new(Config::parse(config_text)?);
    info!("Config loaded successfully!");
    tokio::spawn(backend_task::backend_task(Arc::clone(&config)));

    // construct app

    let app = Router::new().route("/", get(home));

    let addr = SocketAddr::from(([0, 0, 0, 0], 8080));
    tracing::debug!("listening on {}", addr);
    axum::Server::bind(&addr)
        .serve(app.into_make_service())
        .await
        .map_err(|e| LurkbotError::ServerError(format!("{}", e)))
}

async fn home() -> &'static str {
    concat!(
        "Lurkbot backend v",
        env!("CARGO_PKG_VERSION"),
        " is running!"
    )
}

#[derive(Debug)]
struct Config {
    auth_code: String,
    default_refresh_time: u64,
}

impl Config {
    #[instrument]
    /// Parses the config data into a Config struct
    ///
    /// # Arguments
    ///
    /// * `data` - The config file contents as a String
    ///
    /// # Returns
    ///
    /// A Config struct if parsing succeeded
    ///
    /// # Errors
    ///
    /// May return a LurkbotError if:
    ///
    /// - A config line is invalid
    /// - A required key is missing
    ///
    /// # Example
    ///
    /// ```
    /// use lurkbot_common::*;
    ///
    /// let data = r#"
    /// auth_code: abc123
    /// refresh_cooldown: 60
    /// "#;
    ///
    /// let config = Config::parse(data)?;
    /// ```
    fn parse(data: String) -> Result<Self, LurkbotError> {
        // Parse each line of the config
        let mut hash_data = HashMap::new();
        for (num, line) in data.split("\n").enumerate() {
            // skip comments and blank lines
            if line.starts_with("#") || line.trim().is_empty() {
                continue;
            }

            // split line into key/value and insert into hash
            let (key, value) = line.split_once(":").ok_or_else(|| {
                LurkbotError::GenericConfigError(format!(
                    "Invalid config line: \"{}\" at {} ",
                    line, num
                ))
            })?;
            hash_data.insert(key.trim(), value.trim());
        }

        // Extract the required auth_code
        let auth_code = hash_data
            .get("auth_code")
            .ok_or_else(|| LurkbotError::MissingKeyError("auth_code".to_string()))?
            .to_string();

        // Extract default_refresh_time
        let default_refresh_time: u64 = hash_data
            .get("refresh_cooldown")
            .ok_or_else(|| LurkbotError::MissingKeyError("refresh_cooldown".to_string()))?
            .parse::<u64>()
            .map_err(|err| {
                LurkbotError::InvalidConfigValueError(
                    hash_data.get("refresh_cooldown").unwrap().to_string(),
                    "refresh_cooldown".to_string(),
                    err.to_string(),
                )
            })?;

        Ok(Config {
            auth_code,
            default_refresh_time,
        })
    }
}
