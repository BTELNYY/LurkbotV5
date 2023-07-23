// @generated automatically by Diesel CLI.

diesel::table! {
    lurkies (id) {
        id -> BigInt,
        first_seen -> Timestamp,
        last_seen -> Timestamp,
        play_time -> BigInt,
        last_nickname -> Text,
        nicknames -> Text,
        flags -> Text,
        time_online -> BigInt,
        login_amt -> BigInt,
    }
}
