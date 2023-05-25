use std::{sync::Arc, time::Duration};

use lurky_db::Database;
use tracing::{info, instrument};

#[instrument(skip(db))]
pub async fn backend(db: Arc<Database>) {
    let intv = Duration::from_secs(60);
    info!("Backend thread here! Refreshing every {:?}", intv);
    let mut ticker = tokio::time::interval(intv);
    ticker.set_missed_tick_behavior(tokio::time::MissedTickBehavior::Delay);
    loop {
        info!("Refreshing...");
        ticker.tick().await;
    }
}
