-- Your SQL goes here
CREATE TABLE lurkies (
    id BIGINT PRIMARY KEY NOT NULL,
    first_seen TIMESTAMP NOT NULL,
    last_seen TIMESTAMP NOT NULL,
    play_time BIGINT NOT NULL,
    last_nickname TEXT NOT NULL,
    nicknames TEXT NOT NULL,
    flags TEXT NOT NULL,
    time_online BIGINT NOT NULL,
    login_amt BIGINT NOT NULL
);