use rocket::request::{self, FromRequest, Outcome, Request};
use rocket::State;
use std::sync::Arc;

use lurky::config::Config;

pub mod basics;
pub mod northwood;
pub mod query;
pub type ConfigArgument = State<Arc<Config>>;

pub struct Authenticated;
#[rocket::async_trait]
impl<'r> FromRequest<'r> for Authenticated {
    type Error = anyhow::Error;
    async fn from_request(req: &'r Request<'_>) -> request::Outcome<Self, Self::Error> {
        let conf = req
            .rocket()
            .state::<Arc<Config>>()
            .expect("The config to be present??");
        let key = req.headers().get_one("Authorization");
        let valid_key: Result<String, _> = conf
            .get("auth_key")
            .ok_or(anyhow::anyhow!("No key present in config file!"));
        match valid_key {
            Ok(valid_key) => match key {
                Some(passed_key) => {
                    if format!("Bearer {}", valid_key) == passed_key {
                        Outcome::Success(Authenticated)
                    } else {
                        Outcome::Failure((
                            rocket::http::Status::Unauthorized,
                            anyhow::anyhow!("Invalid key!"),
                        ))
                    }
                }
                None => Outcome::Failure((
                    rocket::http::Status::Unauthorized,
                    anyhow::anyhow!("No key provided!"),
                )),
            },
            Err(e) => return Outcome::Failure((rocket::http::Status::InternalServerError, e)),
        }
    }
}
