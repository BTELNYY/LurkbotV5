use std::sync::Arc;

use crate::db::ManagedDB;
use rocket::{get, response::status::NotFound, routes, serde::json::Json, Route, State};
use serde::Serialize;

use crate::db::DBPlayer;

#[derive(Serialize)]
pub struct DBError {
    pub err: String,
}

type DBResult<T> = Result<Json<T>, NotFound<Json<DBError>>>;

#[get("/")]
pub fn index() -> &'static str {
    "query time"
}

#[get("/id/<id>")]
pub async fn query_by_id(id: u64, db: &State<Arc<ManagedDB>>) -> DBResult<DBPlayer> {
    let player = db.get_player(id).await;
    match player {
        Ok(p) => Ok(Json(p)),
        Err(_) => Err(NotFound(Json(DBError {
            err: "Player not found!".to_string(),
        }))),
    }
}

#[get("/last_nick/<last_nick>")]
pub async fn query_by_name(
    last_nick: String,
    db: &State<Arc<ManagedDB>>,
) -> Result<Json<DBPlayer>, NotFound<&'static str>> {
    let player = db.get_by_latest_nickname(&last_nick).await;
    match player {
        Ok(p) => Ok(Json(p)),
        Err(_) => Err(NotFound("Player not found!")),
    }
}

pub fn routes() -> Vec<Route> {
    routes![index, query_by_id, query_by_name]
}
