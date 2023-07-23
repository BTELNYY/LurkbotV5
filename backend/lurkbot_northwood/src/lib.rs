use std::hash::Hasher;

use lurkbot_common::*;
use siphasher::sip::SipHasher13;
pub enum NorthwoodPlayer {
    NorthwoodStaff(String),
    Steam { id: u64, name: String },
}
impl NorthwoodPlayer {
    /// Parses a NorthwoodPlayerResponse into a NorthwoodPlayer enum.
    ///
    /// # Arguments
    ///
    /// * `resp` - The NorthwoodPlayerResponse to parse
    ///
    /// # Returns
    ///
    /// Returns a Result containing the parsed NorthwoodPlayer or a LurkbotError.
    ///
    /// # Errors
    ///
    /// May return the following errors:
    ///
    /// - LurkbotError::InvalidID if the ID is invalid
    /// - LurkbotError::InvalidIDType if the ID type is invalid
    ///
    /// # Example
    ///
    /// ```
    /// use lurkbot_northwood::*;
    ///
    /// let resp = NorthwoodPlayerResponse {
    ///     ID: "123@steam".to_string(),
    ///     Nickname: Some("JohnDoe".to_string()),
    /// };
    ///
    /// let player = NorthwoodPlayer::parse(resp).unwrap();
    /// ```
    pub fn parse(resp: NorthwoodPlayerResponse) -> Result<NorthwoodPlayer, LurkbotError> {
        let (id, id_type) = resp
            .ID
            .split_once("@")
            .ok_or_else(|| LurkbotError::InvalidID(resp.ID.clone()))?;
        match id_type {
            "steam" => Ok(NorthwoodPlayer::Steam {
                id: id
                    .parse()
                    .map_err(|_| LurkbotError::InvalidID(resp.ID.clone()))?,
                name: resp
                    .Nickname
                    .clone()
                    .ok_or_else(|| LurkbotError::InvalidID(resp.ID.clone()))?,
            }),
            "northwood" => Ok(NorthwoodPlayer::NorthwoodStaff(id.to_string())),
            _ => Err(LurkbotError::InvalidIDType(id_type.to_string())),
        }
    }
    /// Returns the unique ID for this player.
    ///
    /// For NorthwoodStaff, this is a hash of the name.
    /// For Steam players, this is the 64-bit Steam ID.
    ///
    /// # Returns
    ///
    /// The unique u64 ID for this player
    pub fn id(&self) -> u64 {
        match self {
            NorthwoodPlayer::NorthwoodStaff(name) => {
                let mut hasher = SipHasher13::new_with_keys(0, 0);
                hasher.write(name.as_bytes());
                hasher.finish()
            }
            NorthwoodPlayer::Steam { id, .. } => *id,
        }
    }
    /// Returns the display name for this player.
    ///
    /// For NorthwoodStaff, this is the name string.
    /// For Steam players, this is the Steam nickname.
    ///
    /// # Returns
    ///
    /// A string slice reference to the display name.
    pub fn name(&self) -> &str {
        match self {
            NorthwoodPlayer::NorthwoodStaff(name) => name,
            NorthwoodPlayer::Steam { name, .. } => name,
        }
    }
}
use serde::Deserialize;
#[derive(Deserialize, PartialEq, Eq)]
#[allow(non_snake_case)]
pub struct NorthwoodPlayerResponse {
    pub ID: String,
    pub Nickname: Option<String>,
}
#[allow(non_snake_case)]
#[derive(Deserialize, PartialEq, Eq)]
pub struct NorthwoodServer {
    ID: usize,
    Port: u16,
    Online: bool,
    #[serde(default)]
    PlayersList: Vec<NorthwoodPlayerResponse>,
}
/// Gathers all the players from a list of Northwood servers into a single vector.
///
/// Flattens the player lists from each server, parses each player, and collects
/// them into a vector.
///
/// # Arguments
///
/// * `servers` - The list of NorthwoodServer structs
///
/// # Returns  
///
/// A vector containing NorthwoodPlayer variants parsed from each server.
///
/// # Errors
///
/// Returns any errors encountered while parsing players.
///
/// # Example
///
/// ```ignore
/// use lurkbot_northwood::*;
///
/// let servers = vec![NorthwoodServer { /* ... */ }];
/// let players = gather_players(servers).unwrap();
/// ```
pub fn gather_players(servers: Vec<NorthwoodServer>) -> Result<Vec<NorthwoodPlayer>, LurkbotError> {
    servers
        .into_iter()
        .flat_map(|e| {
            e.PlayersList
                .into_iter()
                .map(|plr| NorthwoodPlayer::parse(plr))
        })
        .collect()
}

/// Represents the response from the Northwood API.
///
/// This enum has two variants:
///
/// - Error: Contains an error message string if the API returned an error.
///
/// - Success: Contains a vector of NorthwoodServer structs if the API returned successfully.
///
/// This allows the API response to be represented in a structured way instead
/// of just returning the raw JSON string.
#[allow(non_snake_case)]
pub enum NorthwoodResponse {
    Error { Error: String },
    Success { Servers: Vec<NorthwoodServer> },
}
/// Represents a Northwood API client.
///
/// Used to make requests to the Northwood API for a specific server.
///
/// # Fields
///
/// * `id` - The Northwood server ID for this API client
/// * `api_key` - The API key to authenticate requests  
pub struct NorthwoodAPI {
    pub id: u64,
    api_key: String,
}

impl NorthwoodAPI {
    /// Creates a new NorthwoodAPI instance.
    ///
    /// # Arguments
    ///
    /// * `id` - The Northwood server ID
    /// * `api_key` - The API key for the Northwood server
    ///
    /// # Returns
    ///  
    /// A new NorthwoodAPI instance  
    pub fn new(id: u64, api_key: String) -> Self {
        NorthwoodAPI { id, api_key }
    }
    /// Generates the API URL for this NorthwoodAPI instance.
    ///
    /// # Returns
    ///
    /// The API URL string for requesting data from this API instance.
    pub fn url(&self) -> String {
        format!("https://api.scpslgame.com/serverinfo.php?id={}&key={}&list=true&nicknames=true&online=true", self.id, self.api_key)
    }
    /// Makes an async request to the Northwood API and parses the response.
    ///
    /// Requests player data from the Northwood API URL and parses the
    /// response into a vector of NorthwoodServer structs.
    ///
    /// # Returns
    ///
    /// A vector of NorthwoodServer structs if successful.
    ///
    /// # Errors
    ///
    /// Returns a NorthwoodRequestFailed error if the request fails.
    /// Returns a SerdeError if deserializing the response fails.
    pub async fn get(&self) -> Result<Vec<NorthwoodServer>, LurkbotError> {
        let url = self.url();
        let req = reqwest::get(url)
            .await
            .map_err(|err| LurkbotError::NorthwoodRequestFailed(format!("{}", err)))?;
        let txt = req
            .text()
            .await
            .map_err(|err| LurkbotError::NorthwoodRequestFailed(format!("{}", err)))?;
        Self::parse_raw_response(&txt)
    }
    /// Parses a raw JSON response from the Northwood API.
    ///
    /// # Arguments
    ///
    /// * `resp` - The raw JSON response string
    ///
    /// # Returns
    ///
    /// A vector of NorthwoodServer structs if successful.
    ///
    /// # Errors
    ///
    /// Returns a NorthwoodRequestFailed error if the API response indicates failure.
    /// Returns a SerdeError if deserializing the JSON fails.
    pub fn parse_raw_response(resp: &str) -> Result<Vec<NorthwoodServer>, LurkbotError> {
        let mut resp: serde_json::Value = serde_json::from_str(resp)
            .map_err(|err| LurkbotError::SerdeError(format!("{}", err), resp.to_string()))?;
        if Some(true) == resp["Success"].as_bool() {
            serde_json::from_value(resp["Servers"].take())
                .map_err(|err| LurkbotError::SerdeError(format!("{}", err), resp.to_string()))
        } else {
            Err(LurkbotError::NorthwoodRequestFailed(
                resp["Error"]
                    .as_str()
                    .unwrap_or("Error not found")
                    .to_string(),
            ))
        }
    }
    /// Gathers all players from the Northwood servers.
    ///
    /// Calls the `get()` method to fetch the servers,
    /// then passes the result to `gather_players()` to collect
    /// all players into a single vector.
    ///
    /// # Returns
    ///
    /// A vector containing all players from all servers.
    ///
    /// # Errors
    ///
    /// Returns any error from the `get()` call or `gather_players()`.
    pub async fn gather_players(&self) -> Result<Vec<NorthwoodPlayer>, LurkbotError> {
        let servers = self.get().await?;
        gather_players(servers)
    }
}

#[cfg(test)]
mod tests {

    use super::*;

    const EXAMPLE_TEST_DATA: &str = r#"{"Success":true,"Cooldown":18,"Servers":[{"ID":58759,"Port":7877,"Online":false},{"ID":63783,"Port":7777,"Online":true,"PlayersList":[{"ID":"76561198049114278@steam","Nickname":"Spree"},{"ID":"76561199092254625@steam","Nickname":"Kat"},{"ID":"76561198145901134@steam","Nickname":"This is Dominos"},{"ID":"76561198839479062@steam","Nickname":"Boredom"},{"ID":"76561198236100704@steam","Nickname":"SCP-420-J"},{"ID":"76561199227177606@steam","Nickname":"indigoprism9"},{"ID":"76561199063496606@steam","Nickname":"TotallyOriginalName"},{"ID":"76561199037624522@steam","Nickname":"animatedprotootype"},{"ID":"76561198943539499@steam","Nickname":"Cod2Fish"},{"ID":"76561199133933091@steam","Nickname":"madkingmedusa"},{"ID":"76561198956493815@steam","Nickname":"Charles Chuck McGil"},{"ID":"76561198846133005@steam","Nickname":"Scientist?!"},{"ID":"76561198061259271@steam","Nickname":"BigFrank423"},{"ID":"76561198083421376@steam","Nickname":"TrevorPoor"},{"ID":"76561198158953925@steam","Nickname":"The Grubbler"},{"ID":"76561198084172896@steam","Nickname":"Isaac"},{"ID":"mikel@northwood","Nickname":null},{"ID":"76561198167841632@steam","Nickname":"Big Phrog"},{"ID":"76561198362574711@steam","Nickname":"Captain Kenny"},{"ID":"76561199075774572@steam","Nickname":"TheHolyCrusader"},{"ID":"76561199389354610@steam","Nickname":"Insufferable"},{"ID":"76561198370807683@steam","Nickname":"GalaxyBac"},{"ID":"76561198941382203@steam","Nickname":"FroggyGames"},{"ID":"76561198868583308@steam","Nickname":"Ezavas"},{"ID":"76561199050819457@steam","Nickname":"butteryjeff"},{"ID":"76561198261679098@steam","Nickname":"LePiromano"},{"ID":"76561198860021363@steam","Nickname":"ArmzRed"},{"ID":"76561199076058581@steam","Nickname":"TV Man"},{"ID":"76561197993588087@steam","Nickname":"Torpedo423"},{"ID":"76561198188708898@steam","Nickname":"HyperKdog05"}]},{"ID":63784,"Port":7778,"Online":true,"PlayersList":[]}]}"#;
    #[test]
    fn test_parse() {
        let steam_resp = NorthwoodPlayerResponse {
            ID: "123@steam".to_string(),
            Nickname: Some("JohnDoe".to_string()),
        };
        let staff_resp = NorthwoodPlayerResponse {
            ID: "admin@northwood".to_string(),
            Nickname: None,
        };

        let steam_player = NorthwoodPlayer::parse(steam_resp).unwrap();
        assert_eq!(steam_player.id(), 123);
        assert_eq!(steam_player.name(), "JohnDoe");

        let staff_player = NorthwoodPlayer::parse(staff_resp).unwrap();

        assert_eq!(staff_player.name(), "admin");

        let invalid_id = NorthwoodPlayerResponse {
            ID: "invalid".to_string(),
            Nickname: None,
        };
        assert!(NorthwoodPlayer::parse(invalid_id).is_err());
    }
    #[test]
    fn test_parse_invalid_steam_id() {
        let invalid_steam_resp = NorthwoodPlayerResponse {
            ID: "invalid@steam".to_string(),
            Nickname: Some("JohnDoe".to_string()),
        };

        let result = NorthwoodPlayer::parse(invalid_steam_resp);

        assert_eq!(
            result.err().unwrap(),
            LurkbotError::InvalidID("invalid@steam".to_string())
        );
    }

    #[test]
    fn test_parse_raw_response() {
        let response = NorthwoodAPI::parse_raw_response(EXAMPLE_TEST_DATA);

        let result = response.unwrap();

        assert_eq!(result.len(), 3);
        assert_eq!(result[0].ID, 58759);
        assert_eq!(result[0].PlayersList.len(), 0);
    }

    #[test]
    fn test_gather_players() {
        let response = NorthwoodAPI::parse_raw_response(EXAMPLE_TEST_DATA);

        let result = response.unwrap();

        let players = gather_players(result).unwrap();
        assert_eq!(players.len(), 30);
        assert_eq!(players[0].id(), 76561198049114278);
    }
}
