

use std::path::PathBuf;
use thiserror::Error;

/// An enum representing various error conditions in Lurkbot.
///
/// This enum is marked as non-exhaustive so new variants can be added in the future.
#[derive(Debug, Error, PartialEq, Eq)]
#[non_exhaustive]
pub enum LurkbotError {
    /// Indicates a referenced file does not exist.
    ///
    /// # Arguments
    ///
    /// * `path` - The PathBuf pointing to the missing file
    #[error("File {0} does not exist!")]
    FileDoesntExist(PathBuf),

    /// Indicates a general server error
    ///
    /// # Arguments
    ///
    /// * `err` - The error
    #[error("Server error: {0}")]
    ServerError(String),
    /// Indicates a generic serde serialization/deserialization error.
    ///
    /// # Arguments
    ///
    /// * `error` - The error message   
    /// * `data` - The data that failed to serialize/deserialize
    #[error("Generic serde error: {0} while parsing {1}")]
    SerdeError(String, String),

    /// Indicates a generic error parsing the config file.
    ///
    /// # Arguments
    ///
    /// * `error` - The error message
    #[error("Config error: {0}")]
    GenericConfigError(String),

    /// Indicates an invalid config value
    ///
    /// # Arguments
    ///
    /// * `error` - The error message
    #[error("Value {0} is invalid for key {1}: {2}")]
    InvalidConfigValueError(String, String, String),

    /// Indicates a required config key is missing.
    ///
    /// # Arguments
    ///
    /// * `key` - The name of the missing key
    #[error("Key {0} is required but is missing")]
    MissingKeyError(String),

    /// Indicates an invalid player ID from Northwood API.
    ///
    /// # Arguments
    ///
    /// * `id` - The invalid ID
    #[error("Player ID invalid: {0}")]
    InvalidID(String),

    /// Indicates an invalid player ID type from Northwood API.
    ///
    /// # Arguments
    ///
    /// * `id_type` - The invalid ID type
    #[error("Player ID_TYPE invalid: {0}")]
    InvalidIDType(String),

    /// Indicates an error response from the Northwood API.
    ///
    /// # Arguments
    ///
    /// * `error` - The error message  
    #[error("Northwood API Error: {0}")]
    NorthwoodRequestFailed(String),
}
