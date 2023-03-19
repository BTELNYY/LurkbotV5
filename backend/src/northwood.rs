use anyhow::anyhow;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "PascalCase")]
pub struct SLResponse {
    pub cooldown: u64,
    pub servers: Vec<SLServer>,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "PascalCase")]
pub struct SLServer {
    #[serde(rename = "ID")]
    pub id: u64,
    pub port: u16,
    pub online: bool,
    pub players_list: Vec<Player>,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "PascalCase")]
pub struct Player {
    #[serde(rename = "ID")]
    pub id: String,
    pub nickname: String,
}

pub struct SlServer {
    key: String,
    sid: u64,
}

impl SlServer {
    pub fn parse(p: &str) -> Self {
        let mut parts = p.split('|');
        let sid = parts.next().expect("Invalid server id");
        let key = parts.next().expect("Invalid key");
        let sid = sid.parse::<u64>().expect("Unable to parse id");
        Self {
            key: key.to_string(),
            sid,
        }
    }
    fn api_url(&self) -> String {
        format!("https://api.scpslgame.com/serverinfo.php?id={}&key={}&list=true&nicknames=true&online=true", self.sid, self.key)
    }
    pub async fn get(&self) -> Result<SLResponse, anyhow::Error> {
        let resp = reqwest::get(self.api_url()).await?.text().await?;
        //println!("{}", resp);
        let resp: serde_json::Value = serde_json::from_str(&resp)?;
        if resp["Success"].as_bool().unwrap_or(false) {
            let resp: SLResponse = serde_json::from_value(resp)?;
            Ok(resp)
        } else {
            Err(anyhow!(
                "API returned error: {}",
                resp["Error"].as_str().unwrap_or("Unknown error")
            ))
        }
    }
}
