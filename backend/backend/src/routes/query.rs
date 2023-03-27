use std::{str::FromStr, sync::Arc};

use crate::db::ManagedDB;
use lurky::query::{Operator, Query, Restriction};
use rocket::{get, response::status::NotFound, routes, serde::json::Json, Route, State};
use serde::Serialize;
use time::{format_description::well_known::Rfc3339, OffsetDateTime};

use crate::db::DBPlayer;

use super::Authenticated;

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

fn create_query_from_str<T: FromStr + Ord + Eq>(s: &str) -> Vec<Query<T>> {
    s.split(",")
        .filter_map(|f| {
            if f.len() < 2 {
                return None;
            }
            // first 1-2 chars are the operator
            let op = if let Some('=') = f.chars().nth(1) {
                &f[0..2]
            } else {
                &f[0..1]
            };
            let op = match op {
                "=" => Operator::EqualTo,
                ">=" => Operator::GreaterThanEqualTo,
                "<=" => Operator::LessThanEqualTo,
                ">" => Operator::GreaterThan,
                "<" => Operator::LessThan,
                "!=" => Operator::NotEqualTo,
                _ => return None,
            };
            let val = f[op.to_string().len()..].parse::<T>().ok()?;
            Some(Query { operator: op, val })
        })
        .collect()
}

pub fn create_duration_query_from_str(s: &str) -> Vec<Query<time::Duration>> {
    s.split(",")
        .filter_map(|f| {
            // first 1-2 chars are the operator
            if f.len() < 2 {
                return None;
            }
            let op = if let Some('=') = f.chars().nth(1) {
                &f[0..2]
            } else {
                &f[0..1]
            };
            let op = match op {
                "=" => Operator::EqualTo,
                ">=" => Operator::GreaterThanEqualTo,
                "<=" => Operator::LessThanEqualTo,
                ">" => Operator::GreaterThan,
                "<" => Operator::LessThan,
                "!=" => Operator::NotEqualTo,
                _ => return None,
            };
            let val = f[op.to_string().len()..].parse::<i64>().ok()?;
            Some(Query {
                operator: op,
                val: time::Duration::seconds(val),
            })
        })
        .collect()
}

fn create_date_query_from_str(s: &str) -> Vec<Query<OffsetDateTime>> {
    s.split(",")
        .filter_map(|f| {
            if f.len() < 2 {
                return None;
            }
            // first 1-2 chars are the operator
            let op = if let Some('=') = f.chars().nth(1) {
                &f[0..2]
            } else {
                &f[0..1]
            };
            let op = match op {
                "=" => Operator::EqualTo,
                ">=" => Operator::GreaterThanEqualTo,
                "<=" => Operator::LessThanEqualTo,
                ">" => Operator::GreaterThan,
                "<" => Operator::LessThan,
                "!=" => Operator::NotEqualTo,
                _ => return None,
            };
            let str = &f[op.to_string().len()..];
            let val = OffsetDateTime::parse(&str, &Rfc3339).ok()?;
            Some(Query { operator: op, val })
        })
        .collect()
}

#[get("/db?<flags>&<login_amt>&<play_time>&<time_online>&<first_seen>&<last_seen>")]
pub async fn query_db(
    flags: Option<String>,
    login_amt: Option<String>,
    play_time: Option<String>,
    time_online: Option<String>,
    first_seen: Option<String>,
    last_seen: Option<String>,
    _auth: Authenticated,
    db: &State<Arc<ManagedDB>>,
) -> DBResult<Vec<DBPlayer>> {
    let flags =
        flags.and_then(|f| Some(f.split(",").filter_map(|f| f.parse::<i64>().ok()).collect()));
    //println!("flags: {:?}", flags);
    let rest = Restriction {
        flags: flags.unwrap_or_default(),
        play_time: create_duration_query_from_str(&play_time.unwrap_or_default()),
        time_online: create_duration_query_from_str(&time_online.unwrap_or_default()),
        login_amt: create_query_from_str(&login_amt.unwrap_or_default()),
        first_seen: create_date_query_from_str(&first_seen.unwrap_or_default()),
        last_seen: create_date_query_from_str(&last_seen.unwrap_or_default()),
    };
    let players = db.get_by_restriction(&rest).await;
    match players {
        Ok(p) => {
            if p.is_empty() {
                Err(NotFound(Json(DBError {
                    err: "No players found!".to_string(),
                })))
            } else {
                Ok(Json(p))
            }
        }
        Err(e) => Err(NotFound(Json(DBError { err: e.to_string() }))),
    }
}

#[get("/random?<flags>&<login_amt>&<play_time>&<time_online>&<first_seen>&<last_seen>")]
pub async fn query_db_random(
    flags: Option<String>,
    login_amt: Option<String>,
    play_time: Option<String>,
    time_online: Option<String>,
    first_seen: Option<String>,
    last_seen: Option<String>,
    _auth: Authenticated,
    db: &State<Arc<ManagedDB>>,
) -> DBResult<DBPlayer> {
    let flags =
        flags.and_then(|f| Some(f.split(",").filter_map(|f| f.parse::<i64>().ok()).collect()));
    //println!("flags: {:?}", flags);
    let rest = Restriction {
        flags: flags.unwrap_or_default(),
        play_time: create_duration_query_from_str(&play_time.unwrap_or_default()),
        time_online: create_duration_query_from_str(&time_online.unwrap_or_default()),
        login_amt: create_query_from_str(&login_amt.unwrap_or_default()),
        first_seen: create_date_query_from_str(&first_seen.unwrap_or_default()),
        last_seen: create_date_query_from_str(&last_seen.unwrap_or_default()),
    };
    let players = db.get_by_restriction_random(&rest).await;
    match players {
        Ok(p) => Ok(Json(p)),
        Err(e) => Err(NotFound(Json(DBError { err: e.to_string() }))),
    }
}

pub fn routes() -> Vec<Route> {
    routes![index, query_by_id, query_by_name, query_db, query_db_random]
}
