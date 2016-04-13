module sql

let createTemplate dbname =
  sprintf """
DROP DATABASE IF EXISTS %s;
CREATE DATABASE %s;""" dbname dbname

let initialSetupTemplate (dbname : string) = System.String.Format("""
DROP OWNED BY {0};
DROP USER IF EXISTS {0};

DROP SCHEMA IF EXISTS {0};
CREATE SCHEMA {0};

CREATE USER {0} WITH ENCRYPTED PASSWORD 'secure123';
GRANT USAGE ON SCHEMA {0} to {0};
ALTER DEFAULT PRIVILEGES IN SCHEMA {0} GRANT SELECT ON TABLES TO {0};
GRANT CONNECT ON DATABASE "{0}" to {0};""", dbname)

let createTablesTemplate = "--placeholder"

let createTableTemplate2 (dbname : string) table = System.String.Format("""
CREATE TABLE {0}.{1}(
  user_id        SERIAL       PRIMARY KEY NOT NULL,
  name           varchar(64)  NOT NULL UNIQUE,
  email          varchar(256) NOT NULL UNIQUE,
  password       varchar(60)  NOT NULL,
  scheme         smallint     NOT NULL);

CREATE INDEX users_name ON turtletest.Users (name);
CREATE INDEX users_email ON turtletest.Users (email);
""", dbname, table)
