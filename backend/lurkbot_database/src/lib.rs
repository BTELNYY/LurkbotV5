use std::error::Error;


use deadpool_diesel::sqlite::{Manager, Pool};

pub mod models;
pub mod schema;
pub struct Database {
    inner: Pool,
}

impl Database {
    pub async fn interact(&self) {}
}

impl Database {
    pub fn create(addr: &str) -> Result<Self, Box<dyn Error>> {
        let manager = Manager::new(addr, deadpool_diesel::Runtime::Tokio1);
        let pool = Pool::builder(manager).max_size(16).build()?;
        Ok(Database { inner: pool })
    }
    pub fn inner(&self) -> &Pool {
        &self.inner
    }
}
