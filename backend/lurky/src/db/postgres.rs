use super::{DBPlayer, DbRow, DB};
use crate::db::wrap_to_i64;
use anyhow::anyhow;
use async_trait::async_trait;

use sqlx::postgres::PgPoolOptions;

#[derive(Debug)]
pub struct PostgresDB {
    pool: Option<sqlx::Pool<sqlx::Postgres>>,
    db_url: String,
}

impl PostgresDB {
    pub fn new(config: &crate::config::Config) -> Result<Self, anyhow::Error> {
        let db_url = config
            .get::<String>("db_url")
            .ok_or(anyhow!("No db_url present in config file!"))?;
        Ok(PostgresDB { pool: None, db_url })
    }
    pub fn is_connected(&self) -> bool {
        self.pool.is_some()
    }
}

#[async_trait]
impl DB for PostgresDB {
    async fn health(&self) -> Result<(), anyhow::Error> {
        if let Some(db) = &self.pool {
            sqlx::query!("select").execute(db).await?;
            return Ok(());
        }
        Err(anyhow!("Not connected to database!"))
    }
    async fn setup(&mut self) -> Result<(), anyhow::Error> {
        if self.is_connected() {
            return Ok(());
        }
        let pool = PgPoolOptions::new()
            .max_connections(5)
            .connect(&self.db_url)
            .await?;
        println!("Postgres: Starting migrations...");
        sqlx::migrate!("./migrations").run(&pool).await?;
        println!("Postgres: Migrations complete!");
        self.pool = Some(pool);
        Ok(())
    }
    async fn has_player(&self, player_id: u64) -> Result<bool, anyhow::Error> {
        if let Some(db) = &self.pool {
            let result = sqlx::query!(
                r#"select id from lurkies where id = $1"#,
                wrap_to_i64(player_id)
            )
            .fetch_optional(db)
            .await?;
            return Ok(result.is_some());
        }
        Err(anyhow!("Not connected to database!"))
    }
    async fn get_player(&self, player_id: u64) -> Result<DBPlayer, anyhow::Error> {
        if let Some(db) = &self.pool {
            let result = sqlx::query_as!(
                DbRow,
                r#"select * from lurkies where id = $1"#,
                wrap_to_i64(player_id)
            )
            .fetch_optional(db)
            .await?;
            return Ok(DBPlayer::from_row(
                result.ok_or(anyhow!("Player not found!"))?,
            ));
        }
        Err(anyhow!("Not connected to database!"))
    }
    async fn update_player(&self, player: DBPlayer) -> Result<(), anyhow::Error> {
        let row = player.to_row();
        if let Some(db) = &self.pool {
            sqlx::query!(r#"update lurkies set first_seen = $2, last_seen = $3, play_time = $4, last_nickname = $5, nicknames = $6, flags = $7, time_online = $8, login_amt = $9 where id = $1"#, row.id, row.first_seen, row.last_seen, row.play_time, row.last_nickname, &row.nicknames, row.flags, row.time_online, row.login_amt)
                .execute(db)
                .await?;
            return Ok(());
        }
        Err(anyhow!("Not connected to database!"))
    }
    async fn create_player(&self, player: DBPlayer) -> Result<(), anyhow::Error> {
        let row = player.to_row();
        if let Some(db) = &self.pool {
            sqlx::query!(r#"insert into lurkies (id, first_seen, last_seen, play_time, last_nickname, nicknames, flags, time_online, login_amt) values ($1, $2, $3, $4, $5, $6, $7, $8, $9)"#, row.id, row.first_seen, row.last_seen, row.play_time, row.last_nickname, &row.nicknames, row.flags, row.time_online, row.login_amt)
                .execute(db)
                .await?;
            return Ok(());
        }
        Err(anyhow!("Not connected to database!"))
    }
    async fn get_by_latest_nickname(&self, nickname: &str) -> Result<DBPlayer, anyhow::Error> {
        if let Some(db) = &self.pool {
            let result = sqlx::query_as!(
                DbRow,
                r#"select * from lurkies where last_nickname = $1"#,
                nickname
            )
            .fetch_optional(db)
            .await?;
            return Ok(DBPlayer::from_row(result.unwrap()));
        }
        Err(anyhow!("Not connected to database!"))
    }
}
