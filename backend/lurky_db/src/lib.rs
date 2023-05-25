// i dont even know

use std::error::Error;
use std::fmt::Debug;

// ill probably use diesel but idfk
// memory -> sqlite(:memory:)
// postgres -> postgres (obv)
use deadpool_diesel::postgres;
use deadpool_diesel::sqlite;
pub enum Database {
    Sqlite(sqlite::Pool),
    Postgres(postgres::Pool),
}

impl Debug for Database {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Postgres(_) => f.write_str("Database::Postgres"),
            Self::Sqlite(_) => f.write_str("Database::Sqlite"),
        }
    }
}

impl Database {
    pub async fn check(&self) -> Result<(), Box<dyn Error>> {
        match self {
            Self::Sqlite(pool) => pool.get().await?.interact(|_| {}).await?,
            Self::Postgres(pool) => pool.get().await?.interact(|_| {}).await?,
        }
        Ok(())
    }
    pub async fn create_sqlite() -> Result<Self, Box<dyn Error>> {
        let man = sqlite::Manager::new(":memory:", deadpool_diesel::Runtime::Tokio1);
        let pool = sqlite::Pool::builder(man).max_size(8).build()?;
        // test the connection
        let db = Database::Sqlite(pool);
        db.check().await?;
        return Ok(db);
    }
    pub async fn create_postgres(url: String) -> Result<Self, Box<dyn Error>> {
        let man = postgres::Manager::new(url, deadpool_diesel::Runtime::Tokio1);
        let pool = postgres::Pool::builder(man).max_size(8).build()?;
        // test the connection
        let db = Database::Postgres(pool);
        db.check().await?;
        return Ok(db);
    }
}
