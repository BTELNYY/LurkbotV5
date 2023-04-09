use std::{collections::HashMap, error::Error, ops::{RangeBounds, Range}, io::{Read, BufReader, BufRead}, str::FromStr, process::exit, hash::Hash, fmt::Debug};

// Btelnyy config format!!
#[derive(Debug, Clone)]
pub struct RawConfig {
    data: HashMap<String, String>,
    full: String,
    offsets: HashMap<String, usize>
}

use anyhow::{anyhow, Context};
impl RawConfig {
    pub fn parse<T: Read>(file: T) -> Result<Self, anyhow::Error> {
        let mut data = HashMap::new();
        let mut raw = String::new();
        let mut reader = BufReader::new(file);
        let mut offset = 0;
        let mut offsets = HashMap::new();
        loop {
            let mut line = String::new();
            let read = match reader.read_line(&mut line) {
                Ok(0) => break,
                Ok(e) => e,
                Err(e) => return Err(e.into()),
            };
            raw.push_str(&line);
            if line.starts_with("#") || line.trim().is_empty() {
                offset += read;
                continue;
            }
            let mut parts = line.splitn(2, ':');
            let key = parts.next().ok_or(anyhow!("Unable to find key!"))?.trim();
            let value = parts.next().ok_or(anyhow!("Unable to find value!"))?.trim();
            data.insert(key.to_string(), value.to_string());
            offsets.insert(key.to_string(), offset);
            offset += read;
        }

        Ok(RawConfig { data, full: raw, offsets })
    }
    pub fn get<T: BCFValue>(&self, key: &str) -> BCFParseResult<T> {
        self.data.get(key)
        .ok_or_else(|| {
            BCFParseError {
                error: anyhow!("Missing key {}", key),
                span: 0..0
            }
        })
        .and_then(|s| {
            T::parse_bcf(s)
        })
    }
} 
#[derive(Debug)]
pub struct BCFParseError {
    pub span: Range<usize>,
    pub error: anyhow::Error
}

pub type BCFParseResult<T> = Result<T, BCFParseError>;

pub trait BCFValue where Self: Sized {
    fn parse_bcf(value: &str) -> BCFParseResult<Self>;
}

impl BCFValue for u64 {
    fn parse_bcf(value: &str) -> BCFParseResult<Self> {
        value.parse().map_err(|err| {
            BCFParseError { span: 
                0..value.len(), error: anyhow::Error::new(err) 
            }
        })
    }
}
impl BCFValue for u16 {
    fn parse_bcf(value: &str) -> BCFParseResult<Self> {
        value.parse().map_err(|err| {
            BCFParseError { span: 
                0..value.len(), error: anyhow::Error::new(err) 
            }
        })
    }
}
// thank you rossetta code
const ESCAPE: char = '\\';
fn tokenize(string: &str, sep: char) -> Vec<(String, usize)> {
    let mut token = String::new();
    let mut tokens: Vec<(String, usize)> = Vec::new();
    let mut chars = string.chars();
    let mut escapes_hit = 0;
    while let Some(ch) = chars.next() {
        match ch {
            t if t == sep => {
                tokens.push((token, escapes_hit));
                escapes_hit = 0;
                token = String::new();
            },
            ESCAPE => {
                if let Some(next) = chars.next() {
                    escapes_hit += 1;
                    token.push(next);
                }
            },
            _ => token.push(ch),
        }
    }
    tokens.push((token, escapes_hit));
    tokens
}

impl<T: BCFValue> BCFValue for Vec<T> {
    fn parse_bcf(value: &str) -> BCFParseResult<Self> {
        let vals = tokenize(value, ',');
        let mut resulting: Vec<T> = Vec::with_capacity(vals.len());
        let mut offset: usize = 0; // into value
        for (val, escapes_hit) in vals {
            //println!("{:?}, {}", val, escapes_hit);
            let full_len = val.len() + escapes_hit;
            
            let value = T::parse_bcf(&val);
            match value {
                Ok(v) => resulting.push(v),
                Err(e) => {
                    let new = BCFParseError {
                        span: e.span.start+offset..e.span.end+offset+escapes_hit,
                        ..e
                    };
                    return Err(new)
                }
            }
            offset += full_len + 1;
        }        
        Ok(resulting)
    }
}
// there is some fuckery with errors with this one but i dont think im ever going to use it/touch it again so it stays
impl<K: Hash + Eq + BCFValue, V: BCFValue> BCFValue for HashMap<K, V> {
    fn parse_bcf(value: &str) -> BCFParseResult<Self> {
        let vals = <Vec<String>>::parse_bcf(value)?;
        let mut resulting: HashMap<K, V> = HashMap::with_capacity(vals.len());
        let mut offset: usize = 0; // into value
        for pval in vals {
            let val = tokenize(&pval, '|');
            //println!("{:?}", val);
            if val.len() != 2 {
                return Err(BCFParseError {
                    span: offset..offset+pval.len(),
                    error: anyhow!("Invalid key value pair")
                })
            }
            let (key, key_escapes) = val[0].clone();
            let (value, value_escapes) = val[1].clone();
            let key_len = key.len();
            let full_len = key.len() + key_escapes + value.len() + value_escapes;
            let key = K::parse_bcf(&key);
            let value = V::parse_bcf(&value);
            //println!("{:?}, {:?}, {:?}, {:?}", key, value, key_escapes, value_escapes);
            match (key, value) {
                (Ok(k), Ok(v)) => {resulting.insert(k, v);},
                (Err(e), _) => {
                    let new = BCFParseError {
                        span: e.span.start+offset+1..e.span.end+offset+key_escapes+1,
                        ..e
                    };
                    return Err(new)
                },
                (_, Err(e)) => {
                    let new = BCFParseError {
                        span: e.span.start+offset+key_len+key_escapes+2..e.span.end+offset+key_len+key_escapes+2+value_escapes,
                        ..e
                    };
                    return Err(new)
                }
            }
            offset += full_len + 1;
        }        
        Ok(resulting)
    }
}

impl BCFValue for String {
    fn parse_bcf(value: &str) -> BCFParseResult<Self> {
        Ok(value.to_string())
    }
}

use codespan_reporting::{diagnostic::{Diagnostic, Label}, term};
use codespan_reporting::files::SimpleFiles;
use codespan_reporting::term::termcolor::{ColorChoice, StandardStream};

pub fn bcf_parse_into<T: BCFValue>(conf: &RawConfig, key: &str) -> T {
    match conf.get::<T>(key) {
        Ok(a) => a,
        Err(e) => {
            // oh shit
            // we got an error
            let offset = conf.offsets.get(key).unwrap_or(&0);
            let mut files = SimpleFiles::new();
            let conf_file = files.add("config", conf.full.clone());
            let err_message = format!("{:?}", e.error);
            let err_span = e.span.start+offset+key.len()+1..e.span.end+offset+key.len()+1;
            let diag = Diagnostic::error()
                .with_message(err_message.clone())
            .with_code("ERROR")
            .with_labels(vec![
                Label::primary(conf_file, err_span).with_message(err_message)
            ]);
            let writer = StandardStream::stderr(ColorChoice::Always);
            let config = codespan_reporting::term::Config::default();
                
            term::emit(&mut writer.lock(), &config, &files, &diag).unwrap();
            exit(1)
        }
    }
}       


#[cfg(test)]
mod tests {
    // Note this useful idiom: importing names from outer (for mod tests) scope.
    use super::*;
    use indoc::indoc;
    #[derive(Debug)]
    struct TestConfig {
        val1: u64,
        val2: u64,
        banned_mfs: Vec<u64>,
        escape_test: Vec<String>,
        test_map: HashMap<String, String>,
        test_map_two: HashMap<u64, u64>
    }

    impl TestConfig {
        fn parse_from_bcf(conf: RawConfig) -> Self {
            Self {
                val1: bcf_parse_into(&conf, "val1"),
                val2: bcf_parse_into(&conf, "val2"),
                banned_mfs: bcf_parse_into(&conf, "banned_mfs"),
                escape_test: bcf_parse_into(&conf, "escape_test"),
                test_map: bcf_parse_into(&conf, "test_map"),
                test_map_two: bcf_parse_into(&conf, "test_map_two")
            }
        }
    }

    #[test]
    pub fn test_thing() {
        let data = indoc! {"
        val1:65535
        val2:1234
        banned_mfs:6\\96\\9,420,5923
        escape_test:hello\\, world,hey!
        test_map:hello|world,hey|there,test_escape|test\\,ok!\\\\|ok!
        test_map_two:567|1\\23,123|5\\67
        "};

        let conf = RawConfig::parse(data.as_bytes()).expect("Failed to parse config");
        let dt = TestConfig::parse_from_bcf(conf);
        assert_eq!(dt.val1, 65535);
        assert_eq!(dt.val2, 1234);
        assert_eq!(dt.banned_mfs, vec![6969, 420, 5923]);
        assert_eq!(dt.escape_test, vec!["hello, world".to_string(), "hey!".to_string()]);
        assert_eq!(dt.test_map, HashMap::from_iter(vec![("hello".to_string(), "world".to_string()), ("hey".to_string(), "there".to_string()), ("test_escape".to_string(), "test,ok!|ok!".to_string())]));
        assert_eq!(dt.test_map_two, HashMap::from_iter(vec![(567, 123), (123, 567)]));
        println!("{:#?}", dt);
    }
    
}