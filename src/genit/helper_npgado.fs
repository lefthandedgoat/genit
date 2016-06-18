module helper_npgado

open Npgsql

let firstOrNone s = s |> Seq.tryFind (fun _ -> true)

let boolean (value : string) = System.Convert.ToBoolean(value)

let connection (connectionString : string) =
  let connection = new NpgsqlConnection(connectionString)
  connection.Open()
  connection

let command connection sql =
  let command = new NpgsqlCommand(sql, connection)
  command

let param (name:string) value (command : NpgsqlCommand) =
  command.Parameters.AddWithValue(name, value) |> ignore
  command

let executeScalar (command : NpgsqlCommand) =
  command.ExecuteScalar()

let executeNonQuery (command : NpgsqlCommand) =
  command.ExecuteNonQuery() |> ignore

let read toFunc (command : NpgsqlCommand) =
  use reader = command.ExecuteReader()
  toFunc reader

let getDouble name (reader : NpgsqlDataReader) =
  reader.GetDouble(reader.GetOrdinal(name))

let getInt16 name (reader : NpgsqlDataReader) =
  reader.GetInt16(reader.GetOrdinal(name))

let getInt32 name (reader : NpgsqlDataReader) =
  reader.GetInt32(reader.GetOrdinal(name))

let getInt64 name (reader : NpgsqlDataReader) =
  reader.GetInt64(reader.GetOrdinal(name))

let getString name (reader : NpgsqlDataReader) =
  reader.GetString(reader.GetOrdinal(name))

let getDateTime name (reader : NpgsqlDataReader) =
  reader.GetDateTime(reader.GetOrdinal(name))

let getBool name (reader : NpgsqlDataReader) =
  reader.GetBoolean(reader.GetOrdinal(name))

let getIntArray name (reader : NpgsqlDataReader) =
  reader.GetValue(reader.GetOrdinal(name)) :?> int array

let searchHowToClause how value =
  match how with
  | "Equals"      -> value
  | "Begins With" -> sprintf "%s%s" value "%"
  | _             -> value
