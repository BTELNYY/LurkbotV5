use anyhow::{anyhow, Result};
use std::{
    collections::HashMap,
    io::{BufRead, BufReader, Read},
    str::FromStr,
};

#[derive(Debug, Clone)]
pub struct Config {
    data: HashMap<String, String>,
}

impl Config {
    pub fn parse<T: Read>(file: T) -> Result<Self> {
        let mut data = HashMap::new();

        let mut reader = BufReader::new(file);

        loop {
            let mut line = String::new();
            match reader.read_line(&mut line) {
                Ok(0) => break,
                Ok(_) => (),
                Err(e) => return Err(e.into()),
            }
            if line.starts_with("#") || line.trim().is_empty() {
                continue;
            }
            let mut parts = line.splitn(2, ':');
            let key = parts.next().ok_or(anyhow!("Unable to find key!"))?.trim();
            let value = parts.next().ok_or(anyhow!("Unable to find value!"))?.trim();
            data.insert(key.to_string(), value.to_string());
        }

        Ok(Config { data })
    }
    pub fn get<T: FromStr>(&self, key: &str) -> Option<T> {
        self.data.get(key).and_then(|s| s.parse().ok())
    }
    pub fn validate(&self) -> Result<()> {
        if let None = self.get::<String>("auth_key") {
            return Err(anyhow!("No auth key present in config file!"));
        }
        Ok(())
    }
}
