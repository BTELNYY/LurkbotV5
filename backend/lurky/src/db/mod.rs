use std::fmt::Debug;
pub mod mem;
pub mod postgres;
use crate::{config::LurkyConfig, query::Restriction};
use anyhow::{anyhow, Result};
use async_trait::async_trait;
use serde::{Deserialize, Serialize};
use serde_with::{serde_as, DurationSeconds};
use sqlx::FromRow;
use time::ext::NumericalDuration;

pub fn wrap_to_u64(x: i64) -> u64 {
    (x as u64).wrapping_add(u64::MAX / 2 + 1)
}
pub fn wrap_to_i64(x: u64) -> i64 {
    x.wrapping_sub(u64::MAX / 2 + 1) as i64
}

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct Flag {
    pub flag: i64,
    pub issuer: String,
    pub issued_at: time::OffsetDateTime,
    pub comment: String,
}
#[derive(Debug, Clone, FromRow)]
pub struct DbRow {
    pub id: i64,
    pub first_seen: time::OffsetDateTime,
    pub last_seen: time::OffsetDateTime,
    pub play_time: i64,
    pub last_nickname: String,
    pub nicknames: Vec<String>,
    pub flags: serde_json::Value,
    pub time_online: i64,
    pub login_amt: i64,
}

#[serde_as]
#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct DBPlayer {
    pub id: u64,
    #[serde(with = "time::serde::rfc3339")]
    pub first_seen: time::OffsetDateTime,
    #[serde(with = "time::serde::rfc3339")]
    pub last_seen: time::OffsetDateTime,
    #[serde_as(as = "DurationSeconds<i64>")]
    pub play_time: time::Duration,
    pub last_nickname: String,
    pub nicknames: Vec<String>,
    pub flags: Vec<Flag>,
    #[serde_as(as = "DurationSeconds<i64>")]
    pub time_online: time::Duration,
    pub login_amt: u64,
}

impl DBPlayer {
    pub fn to_row(self) -> DbRow {
        DbRow {
            id: wrap_to_i64(self.id),
            first_seen: self.first_seen,
            last_seen: self.last_seen,
            play_time: self.play_time.whole_seconds(),
            last_nickname: self.last_nickname,
            nicknames: self.nicknames,
            flags: serde_json::to_value(self.flags).expect("Flags to serialize"),
            time_online: self.time_online.whole_seconds(),
            login_amt: wrap_to_i64(self.login_amt),
        }
    }
    pub fn from_row(row: DbRow) -> DBPlayer {
        DBPlayer {
            id: wrap_to_u64(row.id),
            first_seen: row.first_seen,
            last_seen: row.last_seen,
            play_time: row.play_time.seconds(),
            last_nickname: row.last_nickname,
            nicknames: row.nicknames,
            flags: serde_json::from_value(row.flags).expect("Flags to deserialize"),
            time_online: row.time_online.seconds(),
            login_amt: wrap_to_u64(row.login_amt),
        }
    }
}

pub type ManagedDB = Box<dyn DB>;

#[async_trait]
pub trait DB: Send + Sync + Debug {
    async fn health(&self) -> Result<(), anyhow::Error>;
    async fn setup(&mut self) -> Result<(), anyhow::Error>;
    async fn has_player(&self, player_id: u64) -> Result<bool, anyhow::Error>;
    async fn get_player(&self, player_id: u64) -> Result<DBPlayer, anyhow::Error>;
    async fn create_player(&self, player: DBPlayer) -> Result<(), anyhow::Error>;
    async fn update_player(&self, player: DBPlayer) -> Result<(), anyhow::Error>;
    async fn get_by_latest_nickname(&self, nickname: &str) -> Result<DBPlayer, anyhow::Error>;
    async fn get_by_restriction(
        &self,
        restriction: &Restriction,
    ) -> Result<Vec<DBPlayer>, anyhow::Error>;
    async fn get_by_restriction_random(
        &self,
        restriction: &Restriction,
    ) -> Result<DBPlayer, anyhow::Error>;
    async fn leaderboard(&self, limit: u64) -> Result<Vec<DBPlayer>, anyhow::Error>;
}

pub fn create_db_from_config(config: &LurkyConfig) -> Result<ManagedDB> {
    match config.db_type.as_str() {
        "postgres" => Ok(Box::new(postgres::PostgresDB::new(config)?)),
        "memory" => Ok(Box::new(mem::MemoryDB::new())),
        _ => Err(anyhow!("Unknown DB type: {}", config.db_type)),
    }
}
