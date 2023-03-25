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
