use std::fmt::Debug;
pub mod postgres;
pub mod mem;
use crate::config::Config;
use anyhow::{anyhow, Result};
use async_trait::async_trait;
use chrono::Utc;
use serde::{Deserialize, Serialize};
use sqlx::postgres::types::PgInterval;

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
    pub issued_at: chrono::DateTime<Utc>,
    pub comment: String,
}
#[derive(Debug, Clone)]
pub struct DbRow {
    pub id: i64,
    pub first_seen: chrono::DateTime<Utc>,
    pub last_seen: chrono::DateTime<Utc>,
    pub play_time: PgInterval,
    pub last_nickname: String,
    pub nicknames: Vec<String>,
    pub flags: serde_json::Value,
    pub time_online: PgInterval,
    pub login_amt: i64,
}
use serde_with::{serde_as, DurationSeconds};
#[serde_as]
#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct DBPlayer {
    pub id: u64,
    pub first_seen: chrono::DateTime<Utc>,
    pub last_seen: chrono::DateTime<Utc>,
    #[serde_as(as = "DurationSeconds<i64>")]
    pub play_time: chrono::Duration,
    pub last_nickname: String,
    pub nicknames: Vec<String>,
    pub flags: Vec<Flag>,
    #[serde_as(as = "DurationSeconds<i64>")]
    pub time_online: chrono::Duration,
    pub login_amt: u64,
}

impl DBPlayer {
    pub fn to_row(self) -> DbRow {
        DbRow {
            id: wrap_to_i64(self.id),
            first_seen: self.first_seen,
            last_seen: self.last_seen,
            play_time: PgInterval::try_from(
                self.play_time
                    .to_std()
                    .expect("Failed to convert duration to std::time::Duration"),
            )
            .expect("Failed to convert duration to PgInterval"),
            last_nickname: self.last_nickname,
            nicknames: self.nicknames,
            flags: serde_json::to_value(self.flags).expect("Flags to serialize"),
            time_online: PgInterval::try_from(
                self.time_online
                    .to_std()
                    .expect("Failed to convert duration to std::time::Duration"),
            )
            .expect("Failed to convert duration to PgInterval"),
            login_amt: wrap_to_i64(self.login_amt),
        }
    }
    pub fn from_row(row: DbRow) -> DBPlayer {
        DBPlayer {
            id: wrap_to_u64(row.id),
            first_seen: row.first_seen,
            last_seen: row.last_seen,
            play_time: convert_duration(row.play_time),
            last_nickname: row.last_nickname,
            nicknames: row.nicknames,
            flags: serde_json::from_value(row.flags).expect("Flags to deserialize"),
            time_online: convert_duration(row.time_online),
            login_amt: wrap_to_u64(row.login_amt),
        }
    }
}

pub fn convert_duration(dur: PgInterval) -> chrono::Duration {
    chrono::Duration::microseconds(dur.microseconds)
        + chrono::Duration::days((dur.days + (dur.months * 31)).into())
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
}

pub fn create_db_from_config(config: &Config) -> Result<ManagedDB> {
    match config.get::<String>("db_type") {
        Some(db_type) => match db_type.as_str() {
            "postgres" => Ok(Box::new(postgres::PostgresDB::new(config)?)),
            "memory" => Ok(Box::new(mem::MemoryDB::new())),
            _ => Err(anyhow!("Unknown DB type: {}", db_type)),
        },
        None => Err(anyhow!("No DB type present in config file!")),
    }
}
