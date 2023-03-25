use chrono::Utc;

use crate::db::{wrap_to_i64, DBPlayer};

pub enum Operator {
    LessThan,
    GreaterThan,
    EqualTo,
    NotEqualTo,
    LessThanEqualTo,
    GreaterThanEqualTo,
}

impl ToString for Operator {
    fn to_string(&self) -> String {
        match self {
            Operator::LessThan => "<".to_string(),
            Operator::GreaterThan => ">".to_string(),
            Operator::EqualTo => "=".to_string(),
            Operator::NotEqualTo => "!=".to_string(),
            Operator::LessThanEqualTo => "<=".to_string(),
            Operator::GreaterThanEqualTo => ">=".to_string(),
        }
    }
}
pub struct Query<T: Ord + Eq> {
    pub operator: Operator,
    pub val: T,
}

impl<T: Ord + Eq> Query<T> {
    fn matches(&self, val: &T) -> bool {
        match self.operator {
            Operator::LessThan => val < &self.val,
            Operator::GreaterThan => val > &self.val,
            Operator::EqualTo => val == &self.val,
            Operator::NotEqualTo => val != &self.val,
            Operator::LessThanEqualTo => val <= &self.val,
            Operator::GreaterThanEqualTo => val >= &self.val,
        }
    }
}
pub struct Restriction {
    pub flags: Vec<i64>,
    pub play_time: Vec<Query<chrono::Duration>>,
    pub time_online: Vec<Query<chrono::Duration>>,
    pub login_amt: Vec<Query<u64>>,
    pub first_seen: Vec<Query<chrono::DateTime<Utc>>>,
    pub last_seen: Vec<Query<chrono::DateTime<Utc>>>,
}
impl Restriction {
    pub fn matches(&self, player: &DBPlayer) -> bool {
        for flag in &self.flags {
            if !player.flags.iter().any(|f| f.flag == *flag) {
                return false;
            }
        }
        for query in &self.play_time {
            if !query.matches(&player.play_time) {
                return false;
            }
        }
        for query in &self.time_online {
            if !query.matches(&player.time_online) {
                return false;
            }
        }
        for query in &self.login_amt {
            if !query.matches(&player.login_amt) {
                return false;
            }
        }
        for query in &self.first_seen {
            if !query.matches(&player.first_seen) {
                return false;
            }
        }
        for query in &self.last_seen {
            if !query.matches(&player.last_seen) {
                return false;
            }
        }
        true
    }
    pub fn generate_postgres(&self) -> String {
        //let mut query = String::new();
        let mut queries = vec![];
        queries.extend(
            self.flags
                .iter()
                .map(|flag| format!("jsonb_path_query_array(flags, '$[*].flag') @> '{}'", flag)),
        );
        queries.extend(self.play_time.iter().map(|query| {
            format!(
                "play_time {} '{} seconds'::interval",
                query.operator.to_string(),
                query.val.num_seconds()
            )
        }));
        queries.extend(self.time_online.iter().map(|query| {
            format!(
                "time_online {} '{} seconds'::interval",
                query.operator.to_string(),
                query.val.num_seconds()
            )
        }));
        queries.extend(self.login_amt.iter().map(|query| {
            format!(
                "login_amt {} {}",
                query.operator.to_string(),
                wrap_to_i64(query.val)
            )
        }));
        queries.extend(self.first_seen.iter().map(|query| {
            format!(
                "first_seen {} '{}'",
                query.operator.to_string(),
                query.val.to_rfc3339()
            )
        }));
        queries.extend(self.last_seen.iter().map(|query| {
            format!(
                "last_seen {} '{}'",
                query.operator.to_string(),
                query.val.to_rfc3339()
            )
        }));
        queries.join(" AND ")
    }
}
