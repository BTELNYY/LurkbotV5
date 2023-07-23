use std::{sync::Arc, time::Duration};

use tokio::select;
use tracing::{info, instrument};

use crate::Config;
#[instrument]
pub(crate) async fn backend_task(conf: Arc<Config>) {
    let interval = Duration::from_secs(32);
    info!("Backend task here!");
    loop {
        info!("Backend tick!");
        select! {
            _ = tokio::time::sleep(interval) => {}
        }
    }
}
