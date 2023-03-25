# running the backend locally ( with in memory db )

## Step 0: dependancies
* rust


## Step 2: Setup config
The backend requires these config values to be set:
* servers:serverid1|serverkey1,serverid2|serverkey2
* db_type:memory
* auth_key:\<auth key\>

## Step 3: Run the backend
```
cargo run -p backend <config location>
```
this will automatically download and build all dependancies

# Routes

   * (index) GET /
   * (test_auth) GET /test (REQUIRES AUTH)
   * (health) GET /health
   * (nw) GET /nw/
   * (nw_api_all) GET /nw/all (REQUIRES AUTH)
   * (nw_api) GET /nw/\<id\> (REQUIRES AUTH)
   * (nw_api_servers) GET /nw/servers (REQUIRES AUTH)
   * (index) GET /query/
   * (query_by_id) GET /query/id/\<id\>
   * (query_by_name) GET /query/last_nick/\<last_nick\>
   * (query_db) GET /query/db?<flags>&<login_amt>&<play_time>&<time_online>&<first_seen>&<last_seen> (REQUIRES AUTH)
   * (query_db_random) GET /query/random?<flags>&<login_amt>&<play_time>&<time_online>&<first_seen>&<last_seen> (REQUIRES AUTH)

# Querying
For the routes query_db and query_db_random, here are some examples

* ?login_amt=>100 (login_amt greater than 100)
* ?login_amt=>100,<200 (login_amt greater than 100 and less than 200)
* ?flags=1 (has a flag with id 1)
* ?flags=1,2 (has a flags with id 1 and 2)
* ?flags=1,2&login_amt=>100 (has a flags with id 1 and 2 and login_amt greater than 100)
* ?play_time=>=3600 (play_time greater than or equal to 3600 in seconds)
* ?first_seen=>=2023-02-25T12:23:38-07:00 (first_seen greater than or this rfc 3339 date (it has to be rfc 3339))
* ?first_seen=>=2023-01-25T12:23:38-07:00&last_seen=<2023-03-25T12:23:38-07:00 (first_seen greater than or this rfc 3339 date and last_seen less than this rfc 3339 date)

The layout is \<op1\>\<val1\>,\<op2\>\<val2\>

Valid ops are:
* = (equal to)
* \> (greater than)
* < (less than)
* \>= (greater than or equal to)
* <= (less than or equal to)
* != (not equal to)


All query params are optional, and if they are not provided, they will not be used in the query.
query_db_random will return a random player that matches the query params, and query_db will return at most 20 players that match the query params.