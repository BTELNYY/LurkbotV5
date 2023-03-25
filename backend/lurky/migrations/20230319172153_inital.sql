-- Add migration script here

CREATE TABLE lurkies (
    id bigint PRIMARY KEY,
    first_seen timestamp with time zone NOT NULL,
    last_seen timestamp with time zone NOT NULL,
    play_time interval NOT NULL,
    last_nickname varchar(32) NOT NULL,
    nicknames varchar(32)[] NOT NULL,
    flags jsonb NOT NULL,
    time_online interval NOT NULL,
    login_amt BIGINT NOT NULL
);