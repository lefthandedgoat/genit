module helper_ado

open System.Data
open System.Data.SqlClient

let firstOrNone s = s |> Seq.tryFind (fun _ -> true)

let boolean (value : string) = System.Convert.ToBoolean(value)

let executeScalar (command : IDbCommand) =
  command.ExecuteScalar()

let executeNonQuery (command : IDbCommand) =
  command.ExecuteNonQuery() |> ignore

let read toFunc (command : IDbCommand) =
  use reader = command.ExecuteReader()
  toFunc reader

let getDouble name (reader : IDataReader) =
  reader.GetDouble(reader.GetOrdinal(name))

let getDoubleOption name (reader : IDataReader) =
  let ordinal = reader.GetOrdinal(name)
  if reader.IsDBNull(ordinal)
  then None
  else Some <| reader.GetDouble(ordinal)

let getInt16 name (reader : IDataReader) =
  reader.GetInt16(reader.GetOrdinal(name))

let getInt16Option name (reader : IDataReader) =
  let ordinal = reader.GetOrdinal(name)
  if reader.IsDBNull(ordinal)
  then None
  else Some <| reader.GetInt16(ordinal)

let getInt32 name (reader : IDataReader) =
  reader.GetInt32(reader.GetOrdinal(name))

let getInt32Option name (reader : IDataReader) =
  let ordinal = reader.GetOrdinal(name)
  if reader.IsDBNull(ordinal)
  then None
  else Some <| reader.GetInt32(ordinal)

let getInt64 name (reader : IDataReader) =
  reader.GetInt64(reader.GetOrdinal(name))

let getInt64Option name (reader : IDataReader) =
  let ordinal = reader.GetOrdinal(name)
  if reader.IsDBNull(ordinal)
  then None
  else Some <| reader.GetInt64(ordinal)

let getString name (reader : IDataReader) =
  reader.GetString(reader.GetOrdinal(name))

let getDateTime name (reader : IDataReader) =
  reader.GetDateTime(reader.GetOrdinal(name))

let getDateTimeOption name (reader : IDataReader) =
  let ordinal = reader.GetOrdinal(name)
  if reader.IsDBNull(ordinal)
  then None
  else Some <| reader.GetDateTime(ordinal)

let getBool name (reader : IDataReader) =
  reader.GetBoolean(reader.GetOrdinal(name))

let getBoolOption name (reader : IDataReader) =
  let ordinal = reader.GetOrdinal(name)
  if reader.IsDBNull(ordinal)
  then None
  else Some <| reader.GetBoolean(ordinal)

let searchHowToClause how value =
  match how with
  | "Equals"      -> value
  | "Begins With" -> sprintf "%s%s" value "%"
  | _             -> value

module helper_npgado =

  open Npgsql
  open System.Data

  let connection (connectionString : string) =
    let connection = new NpgsqlConnection(connectionString)
    connection.Open()
    connection :> IDbConnection

  let command (connection : IDbConnection) sql =
    let connection = connection :?> NpgsqlConnection
    let command = new NpgsqlCommand(sql, connection)
    command :> IDbCommand

  let param (name:string) value (command : IDbCommand) =
    let command = command :?> NpgsqlCommand
    command.Parameters.AddWithValue(name, value) |> ignore
    command :> IDbCommand

  let paramOption (name:string) value (command : IDbCommand) =
    let command = command :?> NpgsqlCommand
    match value with
    | None       -> command.Parameters.AddWithValue(name, null) |> ignore
    | Some value -> command.Parameters.AddWithValue(name, value) |> ignore
    command :> IDbCommand

module helper_sqlado =

  open System.Data
  open System.Data.SqlClient

  let connection (connectionString : string) =
    let connection = new SqlConnection(connectionString)
    connection.Open()
    connection

  let command connection sql =
    let command = new SqlCommand(sql, connection)
    command

  let param (name:string) value (command : SqlCommand) =
    command.Parameters.AddWithValue(name, value) |> ignore
    command

  let paramOption (name:string) value (command : SqlCommand) =
    match value with
    | None       -> command.Parameters.AddWithValue(name, null) |> ignore
    | Some value -> command.Parameters.AddWithValue(name, value) |> ignore
    command
