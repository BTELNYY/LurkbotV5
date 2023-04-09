use parking_lot::RwLock;

use super::{DBPlayer, DB};
use crate::query::Restriction;
use rand::prelude::SliceRandom;
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
    async fn get_by_restriction(
        &self,
        restriction: &Restriction,
    ) -> Result<Vec<DBPlayer>, anyhow::Error> {
        Ok(self
            .data
            .read()
            .iter()
            .filter(|player| restriction.matches(player))
            .cloned()
            .collect())
    }
    async fn get_by_restriction_random(
        &self,
        restriction: &Restriction,
    ) -> Result<DBPlayer, anyhow::Error> {
        let players = self.get_by_restriction(restriction).await?;
        let mut rng = rand::thread_rng();
        Ok(players
            .choose(&mut rng)
            .cloned()
            .ok_or_else(|| anyhow::anyhow!("No players found"))?)
    }
    async fn leaderboard(&self, limit: u64) -> Result<Vec<DBPlayer>, anyhow::Error> {
        let mut players = self.data.read().clone();
        players.sort_by(|a, b| b.play_time.cmp(&a.play_time));
        Ok(players.into_iter().take(limit as usize).collect())
    }
}
