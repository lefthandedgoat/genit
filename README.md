# noname

A cross-platform website generator using [F#](http://fsharp.org/), [Suave](https://suave.io/) and [PostgreSQL](http://www.postgresql.org/).

## Building the Generator

Build the `noname.exe` generator by running `./build.sh all`. This will produce the `noname.exe` in the `bin` folder.

## Usage

Once you have a working `noname.exe`, you can use it to take the following actions:

* Generate a site: `noname.exe generate`
* Generate the site's database: `./build.sh CreateDB` (currently supports only PostgreSQL)
* Test the site: `noname.exe test`
* Run the site: `noname.exe`

> NOTE: on Linux or Mac OS X, you will need to run these commands with `mono noname.exe *`.

If working from the `noname` repository, you can use the following FAKE targets:

* `Generate`: generates the site
* `CreateDB`: creates the database (PostgreSQL must be running)
* `Test`: runs tests
* `RunSite`: runs the generated site

