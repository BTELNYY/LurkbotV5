use crate::{
    northwood::SlServer,
    northwood::{Player, SLResponse},
};
use futures::future::join_all;
use lazy_static::lazy_static;
use lurky::{
    config::LurkyConfig,
    db::{DBPlayer, ManagedDB},
};
use parking_lot::RwLock;
use std::{hash::Hasher, sync::Arc, time::Duration};
lazy_static! {
    pub static ref CACHED_NW_REQ: RwLock<Vec<Option<SLResponse>>> = RwLock::new(Vec::new());
}

/// this function runs in a seperate thread, it really shouldnt return
pub async fn backend(conf: Arc<LurkyConfig>, db: Arc<ManagedDB>) {
    let refresh = conf.refresh_cooldown;
    println!("Backend: Refresh cooldown: {}", refresh);
    println!("Parsing servers...");
    let servers = conf
        .servers
        .iter()
        .map(|s| SlServer::parse(s))
        .collect::<Vec<SlServer>>();
    println!("Parsed {} servers", servers.len());
    for _ in servers.iter() {
        CACHED_NW_REQ.write().push(None);
    }
    let mut intv = rocket::tokio::time::interval(Duration::from_secs(refresh));
    intv.set_missed_tick_behavior(rocket::tokio::time::MissedTickBehavior::Delay);
    let mut old_plr_list: Vec<Player> = vec![];
    loop {
        // do shit
        intv.tick().await;
        println!("Backend refresh!");
        let mut player_list: Vec<Player> = vec![];
        for (id, server) in servers.iter().enumerate() {
            let resp = server.get().await;
            println!("{:#?}", resp);
            if let Ok(resp) = resp {
                CACHED_NW_REQ.write()[id] = Some(resp.clone());
                for server in resp.servers {
                    player_list.extend(server.players_list)
                }
            }
        }
        // do the db things

        join_all(
            player_list
                .iter()
                .map(|e| update_player(e, Arc::clone(&db), refresh, old_plr_list.clone())),
        )
        .await;
        old_plr_list = player_list;
    }
}

async fn update_player(
    player: &Player,
    db: Arc<ManagedDB>,
    refresh: u64,
    old_plr_list: Vec<Player>,
) {
    // identify id

    let mut id_parts = player.id.split("@");
    let raw_id = id_parts.next();
    let identif = id_parts.next();
    if raw_id.is_none() || identif.is_none() {
        eprintln!("Invalid player id: {}", player.id);
        return;
    }
    let raw_id = raw_id.unwrap();
    let identif = identif.unwrap();

    let (id, nick) = match identif {
        "steam" => (
            raw_id
                .parse::<u64>()
                .expect("steam player to have valid u64 id"),
            player
                .nickname
                .clone()
                .expect("Steam player to have nickname"),
        ),
        "northwood" => {
            let mut hasher = std::collections::hash_map::DefaultHasher::new();
            hasher.write(raw_id.as_bytes());
            (hasher.finish(), raw_id.to_string())
        }
        _ => {
            eprintln!("Invalid indicator: {}", identif);
            return;
        }
    };

    println!("{}: {}", id, nick);
    if (db.has_player(id).await).unwrap_or(false) {
        // update the player
        println!("Updating player: {} ({})", nick, id);
        let mut dbplayer = db.get_player(id).await.unwrap();
        if dbplayer.last_nickname != nick {
            dbplayer.nicknames.push(nick.clone());
        }
        dbplayer.last_nickname = nick;
        dbplayer.last_seen = time::OffsetDateTime::now_utc();
        dbplayer.play_time = dbplayer.play_time + time::Duration::seconds(refresh as i64);
        if !old_plr_list.iter().any(|e| e.id == player.id) {
            // this player just logged in
            dbplayer.time_online = time::Duration::seconds(refresh as i64);
            dbplayer.login_amt += 1;
        } else {
            dbplayer.time_online = dbplayer.time_online + time::Duration::seconds(refresh as i64);
        }
        //player.time_online = player.time_online + time::Duration::seconds(refresh as i64);
        //player.login_amt += 1;
        db.update_player(dbplayer).await.unwrap();
    } else {
        // add the player!
        println!("Adding player: {} ({})", nick, id);
        let r = db
            .create_player(DBPlayer {
                id,
                last_nickname: nick.clone(),
                nicknames: vec![nick],
                last_seen: time::OffsetDateTime::now_utc(),
                first_seen: time::OffsetDateTime::now_utc(),
                play_time: time::Duration::seconds(refresh as i64),
                flags: vec![],
                time_online: time::Duration::seconds(refresh as i64),
                login_amt: 1,
            })
            .await;
        if let Err(e) = r {
            eprintln!("Error adding player: {}", e);
        }
    }
}
