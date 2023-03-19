# running the backend locally

## Step 0: dependancies
* docker
* rust

## Step 1: Start up a postgres database
Using docker:
```
docker run -i -t --rm --name some-postgres -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 postgres:alpine
```
Change the password to whatever you want. The default username is postgres.

## Step 2: Setup config
The backend requires these config values to be set:
* servers:serverid1|serverkey1,serverid2|serverkey2
* db_type:postgres
* db_url:postgres://postgres:\<password\>@localhost/lbv5
* auth_key:\<auth key\>

## Step 3 (optional): Run migrations manually
I'm pretty sure the migrations are built into the binary and run automatically, but just in case, you can use the following commands to run the migrations manually:
```
cargo install sqlx-cli
sqlx database create
sqlx migrate run
```

## Step 4: Run the backend
```
cargo run <config location>
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
