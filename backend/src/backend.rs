use std::{sync::Arc, time::Duration};

use crate::{config::Config, northwood::SLResponse, northwood::SlServer};
use lazy_static::lazy_static;
use parking_lot::RwLock;
lazy_static! {
    pub static ref CACHED_NW_REQ: RwLock<Vec<Option<SLResponse>>> = RwLock::new(Vec::new());
}

/// this function runs in a seperate thread, it really shouldnt return
pub async fn backend(conf: Arc<Config>) {
    let refresh = conf.get::<u64>("refresh_cooldown").unwrap_or(60);
    println!("Backend: Refresh cooldown: {}", refresh);
    println!("Parsing servers...");
    let servers: String = conf.get("servers").expect("No servers defined");
    let servers = servers
        .split(',')
        .map(|s| SlServer::parse(s))
        .collect::<Vec<SlServer>>();
    println!("Parsed {} servers", servers.len());
    for _ in servers.iter() {
        CACHED_NW_REQ.write().push(None);
    }
    let mut intv = rocket::tokio::time::interval(Duration::from_secs(refresh));
    intv.set_missed_tick_behavior(rocket::tokio::time::MissedTickBehavior::Delay);
    loop {
        // do shit
        intv.tick().await;
        println!("Backend refresh!");
        for (id, server) in servers.iter().enumerate() {
            let resp = server.get().await;
            println!("{:#?}", resp);
            CACHED_NW_REQ.write()[id] = resp.ok();
        }
    }
}
