#[derive(Debug)]
pub struct LurkyConfig {
    pub servers: Vec<String>,
    pub auth_key: String,
    pub db_type: String,
    pub db_url: String,
    pub refresh_cooldown: u64,
}
use std::io::Read;

use BCF::{bcf_parse_into, RawConfig};
impl LurkyConfig {
    // please automate this with a macro
    pub fn parse_data<T: Read>(data: T) -> Self {
        let conf = RawConfig::parse(data).expect("Failed to parse config!");
        Self {
            servers: bcf_parse_into(&conf, "servers"),
            db_type: bcf_parse_into(&conf, "db_type"),
            db_url: bcf_parse_into(&conf, "db_url"),
            refresh_cooldown: bcf_parse_into(&conf, "refresh_cooldown"),
            auth_key: bcf_parse_into(&conf, "auth_key"),
        }
    }
}
