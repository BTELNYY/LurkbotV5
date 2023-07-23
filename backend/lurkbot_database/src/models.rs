use diesel::prelude::*;

#[derive(Queryable, Selectable)]
#[diesel(table_name = crate::schema::lurkies)]
#[diesel(check_for_backend(diesel::sqlite::Sqlite))]
pub struct Lurkie {
    pub id: i64,
    pub first_seen: time::PrimitiveDateTime,
    pub last_seen: time::PrimitiveDateTime,
    pub play_time: i64,
    pub last_nickname: String,
    pub nicknames: String,
    pub flags: String,
    pub time_online: i64,
    pub login_amt: i64,
}
use serde::{Deserialize, Serialize};
use serde_with::serde_as;
use serde_with::DurationSeconds;
#[serde_as]
#[derive(Serialize, Deserialize)]
pub struct LurkiePlayer {
    pub id: u64,
    #[serde(with = "time::serde::rfc3339")]
    pub first_seen: time::OffsetDateTime,
    #[serde(with = "time::serde::rfc3339")]
    pub last_seen: time::OffsetDateTime,
    #[serde_as(as = "DurationSeconds<i64>")]
    pub play_time: time::Duration,
    pub last_nickname: String,
    pub nicknames: Vec<String>,
    pub flags: serde_json::Value,
    #[serde_as(as = "DurationSeconds<i64>")]
    pub time_online: time::Duration,
    pub login_amt: u64,
}
use time::ext::NumericalDuration;
impl From<Lurkie> for LurkiePlayer {
    fn from(value: Lurkie) -> Self {
        LurkiePlayer {
            id: wrap_to_u64(value.id),
            first_seen: value.first_seen.assume_utc(),
            last_seen: value.last_seen.assume_utc(),
            play_time: value.play_time.seconds(),
            last_nickname: value.last_nickname,
            nicknames: serde_json::from_str(&value.nicknames)
                .expect("Failed to deserialize nicknames!"),
            flags: serde_json::from_str(&value.flags).expect("Failed to deserialize flags!"),
            time_online: value.time_online.seconds(),
            login_amt: wrap_to_u64(value.login_amt),
        }
    }
}

impl From<LurkiePlayer> for Lurkie {
    fn from(value: LurkiePlayer) -> Self {
        Lurkie {
            id: wrap_to_i64(value.id),
            first_seen: todo!("First seen offset -> primitive"), //value.first_seen,
            last_seen: todo!("Last seen offset -> primitive"),
            play_time: value.play_time.whole_seconds(),
            last_nickname: value.last_nickname,
            nicknames: serde_json::to_string(&value.nicknames).expect("Failed to serialize nicknames"),
            flags: serde_json::to_string(&value.flags).expect("Failed to serialize flags"),
            time_online: value.time_online.whole_seconds(),
            login_amt: wrap_to_i64(value.login_amt),
        }
    }
}

pub fn wrap_to_u64(x: i64) -> u64 {
    (x as u64).wrapping_add(u64::MAX / 2 + 1)
}
pub fn wrap_to_i64(x: u64) -> i64 {
    x.wrapping_sub(u64::MAX / 2 + 1) as i64
}
#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn wrap_test() {
        let m_int: u64 = 1264534;
        assert_eq!(wrap_to_u64(wrap_to_i64(m_int)), m_int)
    }
}
