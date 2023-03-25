use rocket::{get, response::status::NotFound, routes, Route};

use super::Authenticated;
use crate::{
    backend::CACHED_NW_REQ,
    northwood::{SLResponse, SLServer},
};
use rocket::serde::json::Json;

#[get("/<id>")]
pub fn nw_api(id: usize, _auth: Authenticated) -> Result<Json<SLResponse>, NotFound<String>> {
    let nw_ca = CACHED_NW_REQ.read();
    nw_ca
        .get(id)
        .and_then(|e| e.clone())
        .and_then(|e| Some(Json(e.clone())))
        .ok_or(NotFound(format!("Server with id {} not found", id)))
}

#[get("/all")]
pub fn nw_api_all(_auth: Authenticated) -> Json<Vec<SLResponse>> {
    Json(
        CACHED_NW_REQ
            .read()
            .iter()
            .filter_map(|e| e.clone())
            .collect::<Vec<SLResponse>>(),
    )
}

#[get("/servers")]
pub fn nw_api_servers(_auth: Authenticated) -> Json<Vec<SLServer>> {
    Json(
        CACHED_NW_REQ
            .read()
            .iter()
            .filter_map(|e| e.clone())
            .flat_map(|e| e.servers)
            .collect::<Vec<SLServer>>(),
    )
}

#[get("/")]
pub fn nw() -> &'static str {
    "Northwood API wrapper. /all for all servers, /<id> for specific server. All routes require auth."
}

pub fn routes() -> Vec<Route> {
    routes![nw_api, nw_api_all, nw, nw_api_servers]
}
