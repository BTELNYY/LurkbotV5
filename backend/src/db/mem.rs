use parking_lot::RwLock;

use super::{DBPlayer, DB};
#[derive(Debug)]
pub struct MemoryDB {
    data: RwLock<Vec<DBPlayer>>,
}

impl Clone for MemoryDB {
    /// can be very expensive;
    fn clone(&self) -> Self {
        Self {
            data: RwLock::new(self.data.read().clone()),
        }
    }
}

impl MemoryDB {
    pub fn new() -> Self {
        eprintln!("Using in-memory database (no persistence)");
        eprintln!("This is not recommended for production use");
        eprintln!("May god have mercy on your soul");
        Self {
            data: RwLock::new(Vec::new()),
        }
    }
}
#[async_trait::async_trait]
impl DB for MemoryDB {
    async fn health(&self) -> Result<(), anyhow::Error> {
        Ok(())
    }
    async fn setup(&mut self) -> Result<(), anyhow::Error> {
        Ok(())
    }
    async fn has_player(&self, player_id: u64) -> Result<bool, anyhow::Error> {
        Ok(self.data.read().iter().any(|player| player.id == player_id))
    }
    async fn get_player(&self, player_id: u64) -> Result<DBPlayer, anyhow::Error> {
        self.data
            .read()
            .iter()
            .find(|player| player.id == player_id)
            .cloned()
            .ok_or_else(|| anyhow::anyhow!("Player not found"))
    }
    async fn create_player(&self, player: DBPlayer) -> Result<(), anyhow::Error> {
        self.data.write().push(player);
        Ok(())
    }
    async fn update_player(&self, player: DBPlayer) -> Result<(), anyhow::Error> {
        let mut data = self.data.write();
        let index = data
            .iter()
            .position(|p| p.id == player.id)
            .ok_or_else(|| anyhow::anyhow!("Player not found"))?;
        data[index] = player;
        Ok(())
    }
    async fn get_by_latest_nickname(&self, nickname: &str) -> Result<DBPlayer, anyhow::Error> {
        self.data
            .read()
            .iter()
            .find(|player| player.last_nickname == nickname)
            .cloned()
            .ok_or_else(|| anyhow::anyhow!("Player not found"))
    }
}
