use thiserror::Error;

#[derive(Error, Debug)]
pub enum LurkyError {
    #[error("Api returned error: {}", .0)]
    ApiError(String),
    #[error("No servers found")]
    NoServers,
    #[error("Invalid player: Player {{ id: {}, nick: {:?} }}", .0, .1)]
    InvalidPlayer(String, Option<String>),
}

//
