# noname

A cross-platform website generator and server using [F#](http://fsharp.org/), [Suave](https://suave.io/) and [PostgreSQL](http://www.postgresql.org/).

## Introduction

noname provides a simple <acronym title="domain specific language">DSL</acronym> for generating forms-over-data web sites.
The DSL includes built-in types and data validations that produces both the UI and database scripts necessary to deliver a working web site.

noname was developed to increase iterative interactions with project stakeholders. With noname, you can quickly generate a working web site and begin iterating on the functionality while working out the finer points of the design. noname generates F# using [Suave](https://suave.io/) and SQL scripts for use with PostgreSQL. You can easily migrate the generated Suave code once you have formed a solid baseline for the project and continue to iterate outside of noname. Suave provides a fast, stable, cross-platform web server and integrates nicely with the [OWIN](http://owin.org/) components with which you may be familiar from using other .NET web frameworks such as [ASP.NET](http://www.asp.net/).

Creating a noname application is easy. Clone this repository into a new folder, modify the [web site specification](https://github.com/panesofglass/noname/blob/docs/src/noname/generator/script.fs), and run `./build.sh` (or `.\build.cmd` on Windows). Follow the steps below to generate, create the database, run the site and run tests.

1. `Generate`: generates the site
1. `CreateDB`: creates -- or drops and re-creates -- the database (PostgreSQL must be installed and running)
1. `RunSite`: runs the generated site (defaults to port 8083)
1. `Test`: runs tests against the site using [canopy](http://lefthandedgoat.github.io/canopy/)

## Data Types and Validations

TODO

## Generating Test Data

noname can generate test data according to your specified data types and validations. To see this in action using the default "Bob's Burgers" sample, build and run the site, then navigate to the create page.

![navigate to Create Order page](https://raw.githubusercontent.com/panesofglass/noname/docs/docs/img/nav-to-create-order.png?token=AAAiYz_2t2lzjr6OEvHInnTTXygPPAxXks5XMr5SwA%3D%3D)

You should initially see an empty order form. Now, add a query string to the url with `?generate=true` and hit enter. You should see the order form populated with test data.

![Empty Create Order page](https://raw.githubusercontent.com/panesofglass/noname/docs/docs/img/empty-create-order.png?token=AAAiY4mCrxiYnR-aO3-EJhviSY4W9hDDks5XMr3uwA%3D%3D)

noname generates valid data based on your defined data types and validation rules.

![Create Order page with generated test data](https://raw.githubusercontent.com/panesofglass/noname/docs/docs/img/generated-create-order.png?token=AAAiYxpzZPdQFbbYrRP-FpqkptzkfZlRks5XMr49wA%3D%3D)

## Load Testing

TODO

## UI Testing

TODO

