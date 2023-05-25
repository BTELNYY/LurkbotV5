use std::{error::Error, hash::Hasher};

use lurky_common::LurkyError;
use serde::{Deserialize, Serialize};
use tracing::{debug, instrument};
use siphasher::sip::{SipHasher13};
#[derive(Serialize, Deserialize, Debug, Clone, Eq, PartialEq)]
#[serde(rename_all = "PascalCase")]
pub struct SLResponse {
    pub cooldown: u64,
    pub servers: Vec<SLServerResponse>,
}

#[derive(Serialize, Deserialize, Debug, Clone, Eq, PartialEq)]
#[serde(rename_all = "PascalCase")]
pub struct SLServerResponse {
    #[serde(rename = "ID")]
    pub id: u64,
    pub port: u16,
    pub online: bool,
    #[serde(default)]
    pub players_list: Vec<Player>,
}

#[derive(Serialize, Deserialize, Debug, Clone, Eq, PartialEq)]
#[serde(rename_all = "PascalCase")]
pub struct Player {
    #[serde(rename = "ID")]
    id: String,
    nickname: Option<String>,
}
#[derive(Debug)]
pub enum Identifier {
    Steam,
    Northwood,
}

impl Player {
    pub fn info(&self) -> Result<(u64, String), LurkyError> {
        Ok((self.id()?, self.nickname()?))
    }
    pub fn id(&self) -> Result<u64, LurkyError> {
        match self.identifier()? {
            Identifier::Steam => Ok(self
                .id
                .split("@")
                .next()
                .and_then(|e| e.parse().ok())
                .ok_or_else(|| {
                    LurkyError::InvalidPlayer(self.id.clone(), self.nickname.clone())
                })?),
            Identifier::Northwood => {
                let id = self.id.split("@").next().ok_or_else(|| {
                    LurkyError::InvalidPlayer(self.id.clone(), self.nickname.clone())
                })?;
                let mut hasher = SipHasher13::new_with_keys(0,0); // explicitly state siphasher13, this might reset northwood players but eh
                hasher.write(id.as_bytes());
                Ok(hasher.finish())
            }
        }
    }
    pub fn identifier(&self) -> Result<Identifier, LurkyError> {
        match self.id.split("@").last() {
            Some("steam") => Ok(Identifier::Steam),
            Some("northwood") => Ok(Identifier::Northwood),
            _ => Err(LurkyError::InvalidPlayer(
                self.id.clone(),
                self.nickname.clone(),
            )),
        }
    }
    pub fn nickname(&self) -> Result<String, LurkyError> {
        match self.identifier()? {
            Identifier::Steam => Ok(self.nickname.clone().ok_or_else(|| {
                LurkyError::InvalidPlayer(self.id.clone(), self.nickname.clone())
            })?),
            Identifier::Northwood => {
                let id = self.id.split("@").next().unwrap_or("northwood"); // this is probably fine
                Ok(id.to_string())
            }
        }
    }
}

pub struct SLServer {
    pub key: String,
    pub sid: u64,
}

impl std::fmt::Debug for SLServer {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_fmt(format_args!("SLServer<{}>", self.sid))
    }
}

impl SLServer {
    fn api_url(&self) -> String {
        format!("https://api.scpslgame.com/serverinfo.php?id={}&key={}&list=true&nicknames=true&online=true", self.sid, self.key)
    }
    #[instrument]
    pub async fn fetch(&self) -> Result<SLResponse, Box<dyn Error>> {
        let resp = reqwest::get(self.api_url()).await?.text().await?;
        debug!("{}", resp);
        let resp: serde_json::Value = serde_json::from_str(&resp)?;
        if resp["Success"].as_bool().unwrap_or(false) {
            let resp: SLResponse = serde_json::from_value(resp)?;
            Ok(resp)
        } else {
            Err(LurkyError::ApiError(
                resp["Error"]
                    .as_str()
                    .unwrap_or("Unknown error")
                    .to_string(),
            ))?
        }
    }
}
/// fetches multiple sl servers, and unifies their responses, the cooldown is the max cooldown for all servers
pub async fn fetch_multiple(
    things: &Vec<SLServer>,
    ignore_errors: bool,
) -> Result<SLResponse, Box<dyn Error>> {
    let raw_resps = futures::future::join_all(things.iter().map(|server| server.fetch())).await;
    let mut resps = Vec::with_capacity(things.len());
    for resp in raw_resps {
        match resp {
            Ok(server) => resps.push(server),
            Err(e) => {
                if !ignore_errors {
                    return Err(e);
                }
            }
        }
    }
    let max_cooldown = resps
        .iter()
        .max_by_key(|e| e.cooldown)
        .ok_or_else(|| LurkyError::NoServers)?; // def not empty, probably
    Ok(SLResponse {
        cooldown: max_cooldown.cooldown,
        servers: resps.into_iter().flat_map(|e| e.servers).collect(),
    })
}
