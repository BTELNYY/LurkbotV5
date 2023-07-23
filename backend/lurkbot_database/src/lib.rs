use std::error::Error;
use std::f32::consts::PI;

use deadpool_diesel::sqlite::{Pool, Manager};
use lurkbot_common::*;
pub mod schema;
pub mod models;
pub struct Database {
    inner: Pool
}

impl Database {
    pub async fn interact(&self) {
        
    }
}

impl Database {
    pub fn create(addr: &str) -> Result<Self, Box<dyn Error>> {
        let manager = Manager::new(addr, deadpool_diesel::Runtime::Tokio1);
        let pool = Pool::builder(manager).max_size(16).build()?;
        Ok(Database {
            inner: pool
        })
    }
    pub fn inner(&self) -> &Pool {
        &self.inner
    }
}

