module generated_data_access

open System.Data
open generated_types
open helper_general
open helper_ado
open helper_npgado
open Npgsql
open dsl
open BCrypt.Net

let toChartData (reader : IDataReader) : ChartData =
  let temp =
    [ while reader.Read() do
      yield getString "description" reader, getInt32 "count" reader
    ]
  {
    Descriptions = temp |> List.map (fun data -> fst data)
    Data =  temp |> List.map (fun data -> snd data)
  }

[<Literal>]
let connectionString = "Server=127.0.0.1;User Id=defi_pays; Password=NOTSecure1234;Database=defi_pays;"


let insert_register (register : Register) =
  let sql = "
INSERT INTO defi_pays.users
  (
    user_id,
    first_name,
    last_name,
    email,
    password
  ) VALUES (
    DEFAULT,
    :first_name,
    :last_name,
    :email,
    :password
  ) RETURNING user_id;
"

  let bCryptScheme = getBCryptScheme currentBCryptScheme
  let salt = BCrypt.GenerateSalt(bCryptScheme.WorkFactor)
  let password = BCrypt.HashPassword(register.Password, salt)
    
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "first_name" register.FirstName
  |> param "last_name" register.LastName
  |> param "email" register.Email
  |> param "password" password
  |> executeScalar
  |> string |> int64
  
let toLogin (reader : IDataReader) : Login list =
  [ while reader.Read() do
    yield {
      UserID = getInt64 "user_id" reader
      Email = getString "email" reader
      Password = getString "password" reader
    }
  ]
  

let authenticate (login : Login) =
  let sql = "
SELECT * FROM defi_pays.users
WHERE email = :email
"
  use connection = connection connectionString
  use command = command connection sql
  let user =
    command
    |> param "email" login.Email
    |> read toLogin
    |> firstOrNone
  match user with
    | None -> None
    | Some(user) ->
      let verified = BCrypt.Verify(login.Password, user.Password)
      if verified
      then Some(user)
      else None
  
let toSelf (reader : IDataReader) : Self list =
  [ while reader.Read() do
    yield {
      SelfID = getInt64 "self_id" reader
      FirstName = getString "first_name" reader
      LastName = getString "last_name" reader
      Title = getString "title" reader
      Email = getString "email" reader
      PositionObjective1 = getString "position_objective_1" reader
      PositionObjective2 = getString "position_objective_2" reader
      PositionObjective3 = getString "position_objective_3" reader
      Supervisor = getInt16 "supervisor" reader
      CoworkerorClient1 = getInt16 "coworker_or_client_1" reader
      CoworkerorClient2 = getInt16 "coworker_or_client_2" reader
      CoworkerorClient3 = getInt16 "coworker_or_client_3" reader
      CoworkerorClient4 = getInt16 "coworker_or_client_4" reader
    }
  ]
  

let insert_self (self : Self) =
  let sql = "
INSERT INTO defi_pays.selfs
  (
    self_id,
    first_name,
    last_name,
    title,
    email,
    position_objective_1,
    position_objective_2,
    position_objective_3,
    supervisor,
    coworker_or_client_1,
    coworker_or_client_2,
    coworker_or_client_3,
    coworker_or_client_4
  ) VALUES (
    DEFAULT,
    :first_name,
    :last_name,
    :title,
    :email,
    :position_objective_1,
    :position_objective_2,
    :position_objective_3,
    :supervisor,
    :coworker_or_client_1,
    :coworker_or_client_2,
    :coworker_or_client_3,
    :coworker_or_client_4
  ) RETURNING self_id;
"

  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "first_name" self.FirstName
  |> param "last_name" self.LastName
  |> param "title" self.Title
  |> param "email" self.Email
  |> param "position_objective_1" self.PositionObjective1
  |> param "position_objective_2" self.PositionObjective2
  |> param "position_objective_3" self.PositionObjective3
  |> param "supervisor" self.Supervisor
  |> param "coworker_or_client_1" self.CoworkerorClient1
  |> param "coworker_or_client_2" self.CoworkerorClient2
  |> param "coworker_or_client_3" self.CoworkerorClient3
  |> param "coworker_or_client_4" self.CoworkerorClient4
  |> executeScalar
  |> string |> int64
  

let update_self (self : Self) =
  let sql = "
UPDATE defi_pays.selfs
SET
  self_id = :self_id,
  first_name = :first_name,
  last_name = :last_name,
  title = :title,
  email = :email,
  position_objective_1 = :position_objective_1,
  position_objective_2 = :position_objective_2,
  position_objective_3 = :position_objective_3,
  supervisor = :supervisor,
  coworker_or_client_1 = :coworker_or_client_1,
  coworker_or_client_2 = :coworker_or_client_2,
  coworker_or_client_3 = :coworker_or_client_3,
  coworker_or_client_4 = :coworker_or_client_4
WHERE self_id = :self_id;
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "self_id" self.SelfID
  |> param "first_name" self.FirstName
  |> param "last_name" self.LastName
  |> param "title" self.Title
  |> param "email" self.Email
  |> param "position_objective_1" self.PositionObjective1
  |> param "position_objective_2" self.PositionObjective2
  |> param "position_objective_3" self.PositionObjective3
  |> param "supervisor" self.Supervisor
  |> param "coworker_or_client_1" self.CoworkerorClient1
  |> param "coworker_or_client_2" self.CoworkerorClient2
  |> param "coworker_or_client_3" self.CoworkerorClient3
  |> param "coworker_or_client_4" self.CoworkerorClient4
  |> executeNonQuery
  

let tryById_self id =
  let sql = "
SELECT * FROM defi_pays.selfs
WHERE self_id = :self_id
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "self_id" id
  |> read toSelf
  |> firstOrNone

let getMany_self () =
  let sql = "
SELECT * FROM defi_pays.selfs
LIMIT 500
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> read toSelf
  

let getManyWhere_self field how value =
  let field = to_postgres_dbColumn field
  let search = searchHowToClause how value
  let sql =
    sprintf "SELECT * FROM defi_pays.selfs
WHERE lower(%s) LIKE lower(:search)
LIMIT 500" field

  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "search" search
  |> read toSelf
  
let toCustomer (reader : IDataReader) : Customer list =
  [ while reader.Read() do
    yield {
      CustomerID = getInt64 "customer_id" reader
      CustomerName = getString "customer_name" reader
      ContactName = getString "contact_name" reader
      Email = getString "email" reader
    }
  ]
  

let insert_customer (customer : Customer) =
  let sql = "
INSERT INTO defi_pays.customers
  (
    customer_id,
    customer_name,
    contact_name,
    email
  ) VALUES (
    DEFAULT,
    :customer_name,
    :contact_name,
    :email
  ) RETURNING customer_id;
"

  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "customer_name" customer.CustomerName
  |> param "contact_name" customer.ContactName
  |> param "email" customer.Email
  |> executeScalar
  |> string |> int64
  

let update_customer (customer : Customer) =
  let sql = "
UPDATE defi_pays.customers
SET
  customer_id = :customer_id,
  customer_name = :customer_name,
  contact_name = :contact_name,
  email = :email
WHERE customer_id = :customer_id;
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "customer_id" customer.CustomerID
  |> param "customer_name" customer.CustomerName
  |> param "contact_name" customer.ContactName
  |> param "email" customer.Email
  |> executeNonQuery
  

let tryById_customer id =
  let sql = "
SELECT * FROM defi_pays.customers
WHERE customer_id = :customer_id
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "customer_id" id
  |> read toCustomer
  |> firstOrNone

let getMany_customer () =
  let sql = "
SELECT * FROM defi_pays.customers
LIMIT 500
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> read toCustomer
  
let toEmployeeSurvey (reader : IDataReader) : EmployeeSurvey list =
  [ while reader.Read() do
    yield {
      EmployeeSurveyID = getInt64 "employee_survey_id" reader
      EmployeeName = getString "employee_name" reader
      PositionObjective1 = getString "position_objective_1" reader
      PositionObjective2 = getString "position_objective_2" reader
      PositionObjective3 = getString "position_objective_3" reader
      Rating = getInt16 "rating" reader
    }
  ]
  

let insert_employeeSurvey (employeeSurvey : EmployeeSurvey) =
  let sql = "
INSERT INTO defi_pays.employee_surveys
  (
    employee_survey_id,
    employee_name,
    position_objective_1,
    position_objective_2,
    position_objective_3,
    rating
  ) VALUES (
    DEFAULT,
    :employee_name,
    :position_objective_1,
    :position_objective_2,
    :position_objective_3,
    :rating
  ) RETURNING employee_survey_id;
"

  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "employee_name" employeeSurvey.EmployeeName
  |> param "position_objective_1" employeeSurvey.PositionObjective1
  |> param "position_objective_2" employeeSurvey.PositionObjective2
  |> param "position_objective_3" employeeSurvey.PositionObjective3
  |> param "rating" employeeSurvey.Rating
  |> executeScalar
  |> string |> int64
  

let update_employeeSurvey (employeeSurvey : EmployeeSurvey) =
  let sql = "
UPDATE defi_pays.employee_surveys
SET
  employee_survey_id = :employee_survey_id,
  employee_name = :employee_name,
  position_objective_1 = :position_objective_1,
  position_objective_2 = :position_objective_2,
  position_objective_3 = :position_objective_3,
  rating = :rating
WHERE employee_survey_id = :employee_survey_id;
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "employee_survey_id" employeeSurvey.EmployeeSurveyID
  |> param "employee_name" employeeSurvey.EmployeeName
  |> param "position_objective_1" employeeSurvey.PositionObjective1
  |> param "position_objective_2" employeeSurvey.PositionObjective2
  |> param "position_objective_3" employeeSurvey.PositionObjective3
  |> param "rating" employeeSurvey.Rating
  |> executeNonQuery
  

let tryById_employeeSurvey id =
  let sql = "
SELECT * FROM defi_pays.employee_surveys
WHERE employee_survey_id = :employee_survey_id
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "employee_survey_id" id
  |> read toEmployeeSurvey
  |> firstOrNone

let getMany_employeeSurvey () =
  let sql = "
SELECT * FROM defi_pays.employee_surveys
LIMIT 500
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> read toEmployeeSurvey
  

let getManyWhere_employeeSurvey field how value =
  let field = to_postgres_dbColumn field
  let search = searchHowToClause how value
  let sql =
    sprintf "SELECT * FROM defi_pays.employee_surveys
WHERE lower(%s) LIKE lower(:search)
LIMIT 500" field

  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "search" search
  |> read toEmployeeSurvey
  
let toCustomerSurvey (reader : IDataReader) : CustomerSurvey list =
  [ while reader.Read() do
    yield {
      CustomerSurveyID = getInt64 "customer_survey_id" reader
      Customer = getString "customer" reader
      Comment = getString "comment" reader
      HowLikely = getInt16 "how_likely" reader
    }
  ]
  

let insert_customerSurvey (customerSurvey : CustomerSurvey) =
  let sql = "
INSERT INTO defi_pays.customer_surveys
  (
    customer_survey_id,
    customer,
    comment,
    how_likely
  ) VALUES (
    DEFAULT,
    :customer,
    :comment,
    :how_likely
  ) RETURNING customer_survey_id;
"

  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "customer" customerSurvey.Customer
  |> param "comment" customerSurvey.Comment
  |> param "how_likely" customerSurvey.HowLikely
  |> executeScalar
  |> string |> int64
  

let update_customerSurvey (customerSurvey : CustomerSurvey) =
  let sql = "
UPDATE defi_pays.customer_surveys
SET
  customer_survey_id = :customer_survey_id,
  customer = :customer,
  comment = :comment,
  how_likely = :how_likely
WHERE customer_survey_id = :customer_survey_id;
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "customer_survey_id" customerSurvey.CustomerSurveyID
  |> param "customer" customerSurvey.Customer
  |> param "comment" customerSurvey.Comment
  |> param "how_likely" customerSurvey.HowLikely
  |> executeNonQuery
  

let tryById_customerSurvey id =
  let sql = "
SELECT * FROM defi_pays.customer_surveys
WHERE customer_survey_id = :customer_survey_id
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "customer_survey_id" id
  |> read toCustomerSurvey
  |> firstOrNone

let getMany_customerSurvey () =
  let sql = "
SELECT * FROM defi_pays.customer_surveys
LIMIT 500
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> read toCustomerSurvey
  

let getManyWhere_customerSurvey field how value =
  let field = to_postgres_dbColumn field
  let search = searchHowToClause how value
  let sql =
    sprintf "SELECT * FROM defi_pays.customer_surveys
WHERE lower(%s) LIKE lower(:search)
LIMIT 500" field

  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "search" search
  |> read toCustomerSurvey
  