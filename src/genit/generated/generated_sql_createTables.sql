

CREATE TABLE defi_pays.users(
  user_id                   SERIAL                    PRIMARY KEY NOT NULL,
  first_name                varchar(128)              NOT NULL,
  last_name                 varchar(128)              NOT NULL,
  email                     varchar(128)              NOT NULL,
  password                  varchar(60)               NOT NULL
);
  

CREATE TABLE defi_pays.selfs(
  self_id                   SERIAL                    PRIMARY KEY NOT NULL,
  first_name                varchar(1024)             NOT NULL,
  last_name                 varchar(1024)             NOT NULL,
  title                     varchar(1024)             NOT NULL,
  email                     varchar(128)              NOT NULL,
  position_objective_1      varchar(1024)             NOT NULL,
  position_objective_2      varchar(1024)             NULL,
  position_objective_3      varchar(1024)             NULL,
  supervisor                smallint                  NOT NULL,
  coworker_or_client_1      smallint                  NOT NULL,
  coworker_or_client_2      smallint                  NOT NULL,
  coworker_or_client_3      smallint                  NOT NULL,
  coworker_or_client_4      smallint                  NOT NULL
);
  

CREATE TABLE defi_pays.customers(
  customer_id               SERIAL                    PRIMARY KEY NOT NULL,
  customer_name             varchar(1024)             NOT NULL,
  contact_name              varchar(1024)             NOT NULL,
  email                     varchar(128)              NOT NULL
);
  

CREATE TABLE defi_pays.employee_surveys(
  employee_survey_id        SERIAL                    PRIMARY KEY NOT NULL,
  employee_name             varchar(1024)             NULL,
  position_objective_1      text                      NULL,
  position_objective_2      text                      NULL,
  position_objective_3      text                      NULL,
  rating                    smallint                  NOT NULL
);
  

CREATE TABLE defi_pays.customer_surveys(
  customer_survey_id        SERIAL                    PRIMARY KEY NOT NULL,
  customer                  varchar(1024)             NULL,
  comment                   text                      NOT NULL,
  how_likely                smallint                  NOT NULL
);
  


GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA defi_pays TO defi_pays;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA defi_pays TO defi_pays;
  
  