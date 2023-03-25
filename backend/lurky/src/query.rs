use serde::{Deserialize, Serialize};

use crate::db::DBPlayer;

#[derive(Debug, Serialize, Deserialize)]
pub enum ConstraintOn {
    FirstSeen,
    LastSeen,
    Playtime,
    TimeOnline,
    LoginAmt,
}

impl CompType {
    pub fn comp<T: Ord + Eq>(&self, one: &T, two: &T) -> bool {
        match self {
            Self::Equal => one == two,
            Self::Greater => one > two,
            Self::GreaterEQ => one >= two,
            Self::Less => one < two,
            Self::LessEQ => one <= two,
        }
    }
}

#[derive(Debug, Serialize, Deserialize)]
pub enum CompType {
    Greater,
    GreaterEQ,
    Less,
    LessEQ,
    Equal,
}

impl ToString for CompType {
    fn to_string(&self) -> String {
        match self {
            Self::Greater => ">".to_string(),
            Self::GreaterEQ => ">=".to_string(),
            Self::Less => "<".to_string(),
            Self::LessEQ => "<=".to_string(),
            Self::Equal => "=".to_string(),
        }
    }
}

#[derive(Debug, Serialize, Deserialize)]
pub struct Constraint {
    pub on: ConstraintOn,
    pub value: String,
    pub compare_type: CompType,
}

impl Constraint {
    pub fn matches(&self, plr: &DBPlayer) -> Option<bool> {
        match self.on {
            ConstraintOn::FirstSeen => {
                let value: chrono::DateTime<chrono::Utc> = self.value.parse().ok()?;
                Some(self.compare_type.comp(&plr.first_seen, &value))
            }
            ConstraintOn::LastSeen => {
                let value: chrono::DateTime<chrono::Utc> = self.value.parse().ok()?;
                Some(self.compare_type.comp(&plr.last_seen, &value))
            }
            ConstraintOn::Playtime => {
                let value: i64 = self.value.parse().ok()?;
                let value = chrono::Duration::seconds(value);
                Some(self.compare_type.comp(&plr.play_time, &value))
            }
            ConstraintOn::TimeOnline => {
                let value: i64 = self.value.parse().ok()?;
                let value = chrono::Duration::seconds(value);
                Some(self.compare_type.comp(&plr.time_online, &value))
            }
            ConstraintOn::LoginAmt => {
                let value: u64 = self.value.parse().ok()?;
                Some(self.compare_type.comp(&plr.login_amt, &value))
            }
        }
    }
    // probably works?
    pub fn generate_postgres(&self) -> String {
        match self.on {
            ConstraintOn::FirstSeen => {
                let value: chrono::DateTime<chrono::Utc> = self.value.parse().unwrap();
                format!(
                    "first_seen {} '{}'",
                    self.compare_type.to_string(),
                    value.format("%Y-%m-%d %H:%M:%S")
                )
            }
            ConstraintOn::LastSeen => {
                let value: chrono::DateTime<chrono::Utc> = self.value.parse().unwrap();
                format!(
                    "last_seen {} '{}'",
                    self.compare_type.to_string(),
                    value.format("%Y-%m-%d %H:%M:%S")
                )
            }
            ConstraintOn::Playtime => {
                let value: i64 = self.value.parse().unwrap();
                let value = chrono::Duration::seconds(value);
                format!(
                    "play_time {} '{}'",
                    self.compare_type.to_string(),
                    value.num_seconds()
                )
            }
            ConstraintOn::TimeOnline => {
                let value: i64 = self.value.parse().unwrap();
                let value = chrono::Duration::seconds(value);
                format!(
                    "time_online {} '{}'",
                    self.compare_type.to_string(),
                    value.num_seconds()
                )
            }
            ConstraintOn::LoginAmt => {
                let value: u64 = self.value.parse().unwrap();
                format!("login_amt {} '{}'", self.compare_type.to_string(), value)
            }
        }
    }
}

/*trait ConstraintFilter {
    type Item;
    fn filter_constraint(&self, constraint: Constraint<Item>) -> impl Iterator<Item = Item>;
}*/
/*
pub trait ConstraintParsible where Self: Sized {
    fn constraint_parse(dt: &str) -> Option<Self>;
}

/*impl<T: Ord + Eq + FromStr> ConstraintParsible for T {
    fn constraint_parse(dt: &str) -> Option<Self> {
        dt.parse().ok()
    }
}*/

impl ConstraintParsible for chrono::Duration {
    fn constraint_parse(dt: &str) -> Option<Self> {
        let secs: i64 = dt.parse().ok()?;
        Some(chrono::Duration::seconds(secs))
    }
}

impl<T: Ord + Eq + ConstraintParsible> Constraint<T> {
    pub fn parse_from(val: &str, on: ConstraintOn, comp_type: CompType) -> Option<Self> {
        let val = T::constraint_parse(val)?;
        Some(Constraint::new(val, on, comp_type))
    }
}

impl<T: Ord + Eq + FromStr> Constraint<T> {

}

impl<T: Ord + Eq> Constraint<T> {
    pub fn new(value: T, on: ConstraintOn, compare_type: CompType) -> Self {
        Self {
            value, on, compare_type
        }
    }
    /*pub fn parse(on: ConstraintOn, stringy: String) -> Option<Self> {
        match on {
            ConstraintOn::FirstSeen => {
                Some(Self::new(1234_u64, on, CompType::Equal))
            }
        }
    }*/
    pub fn valid_for(&self, val: &T) -> bool {
        match self.compare_type {
            CompType::Equal => *val == self.value,
            CompType::Greater => *val > self.value,
            CompType::GreaterEQ => *val >= self.value,
            CompType::Less => *val < self.value,
            CompType::LessEQ => *val <= self.value
        }
    }
    pub fn all_match(constraints: &Vec<Constraint<T>>, value: &T) -> bool {
        constraints.iter().all(|e| e.valid_for(value))
    }
    pub fn filter_constraint<'a>(&'a self, iter: impl Iterator<Item = T> + 'a) -> impl Iterator<Item = T> + '_ + 'a {
        iter.filter(|e| self.valid_for(e))
    }
    pub fn filter_constraints<'a>(constraints: &'a Vec<Constraint<T>>, iter: impl Iterator<Item = T> + 'a) -> impl Iterator<Item = T> + '_ + 'a {
        iter.filter(|e| Constraint::all_match(constraints, e))
    }
}
*/
