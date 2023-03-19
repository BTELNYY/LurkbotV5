use rocket::{get, http::Status, routes, tokio::task::JoinHandle, Route, State};

use super::Authenticated;

#[get("/")]
pub fn index() -> &'static str {
    "lurkbot v5 backend here, the fuck do you want?"
}

#[get("/test")]
pub fn test_auth(_auth: Authenticated) -> &'static str {
    "Auth OK!"
}

#[get("/health")]
pub fn health(g: &State<JoinHandle<()>>) -> Result<&'static str, Status> {
    if g.is_finished() {
        Err(Status::InternalServerError)
    } else {
        Ok("OK")
    }
}

pub fn routes() -> Vec<Route> {
    routes![index, test_auth, health]
}
