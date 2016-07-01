# genit

A cross-platform website generator and server using [F#](http://fsharp.org/), [Suave](https://suave.io/) and [PostgreSQL](http://www.postgresql.org/) or [SQLServer].

## Introduction

genit provides a simple <acronym title="domain specific language">DSL</acronym> for generating forms-over-data web sites.
The DSL includes built-in types and data validations that produces both the UI and database scripts necessary to deliver a working web site.

genit was developed to increase iterative interactions with project stakeholders. With genit, you can quickly generate a working web site and begin iterating on the functionality while working out the finer points of the design. genit generates F# using [Suave](https://suave.io/) and SQL scripts for use with PostgreSQL. You can easily migrate the generated Suave code once you have formed a solid baseline for the project and continue to iterate outside of genit. Suave provides a fast, stable, cross-platform web server and integrates nicely with the [OWIN](http://owin.org/) components with which you may be familiar from using other .NET web frameworks such as [ASP.NET](http://www.asp.net/).

Creating a genit application is easy. Clone this repository into a new folder, modify the [web site specification](src/genit/generator/script.fs), and run `./build.sh` (or `.\build.cmd` on Windows). Follow the steps below to generate, create the database, run the site and run tests.

1. `Generate`: generates the site
1. `CreateDB`: creates -- or drops and re-creates -- the database (PostgreSQL/SQLServer must be installed and running)
1. `RunSite`: runs the generated site (defaults to port 8083)
1. `Test`: runs tests against the site using [canopy](http://lefthandedgoat.github.io/canopy/)

## Data Types and Validations

TODO

## Generating Test Data

genit can generate test data according to your specified data types and validations. To see this in action using the default "Bob's Burgers" sample, build and run the site, then navigate to the create page.

![navigate to Create Order page](https://raw.githubusercontent.com/lefthandedgoat/genit/master/docs/img/nav-to-create-order.png?token=AAsQ6S9w_PS2OEWyi2-Yh4AbETBdB7wjks5XQjhKwA%3D%3D)

You should initially see an empty order form. Now, change the url to `<page>/generate/1` instead of `<page>/create`. You should see the order form populated with test data.

![Empty Create Order page](https://raw.githubusercontent.com/lefthandedgoat/genit/master/docs/img/empty-create-order.png?token=AAsQ6TNlj_o4PhfGTX5hxJN6YYbtk2RDks5XQjgIwA%3D%3D)

genit generates valid data based on your defined data types and validation rules.

![Create Order page with generated test data](https://raw.githubusercontent.com/lefthandedgoat/genit/master/docs/img/generated-create-order.png?token=AAsQ6f9w2LLaZ4woS3mUix4VnKHvG_r0ks5XQjgswA%3D%3D)

You can generate more than one record by using `<page>/generate/<number>`. genit will generate and insert the requested number of records and redirect you to the `/<page>/list` page.

## Load Testing

genit comes with a built-in load testing tool. Navigate to http://localhost:8083/loadtest to see the form. Type the url you wish to hit, the number of requests, and the number of concurrent requests to run, then click Submit.

## UI Testing

TODO
