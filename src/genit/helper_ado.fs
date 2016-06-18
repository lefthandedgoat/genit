module helper_ado

open System.Data.SqlClient

let firstOrNone s = s |> Seq.tryFind (fun _ -> true)

let boolean (value : string) = System.Convert.ToBoolean(value)

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

let executeScalar (command : SqlCommand) =
  command.ExecuteScalar()

let executeNonQuery (command : SqlCommand) =
  command.ExecuteNonQuery() |> ignore

let read toFunc (command : SqlCommand) =
  use reader = command.ExecuteReader()
  toFunc reader

let getDouble name (reader : SqlDataReader) =
  reader.GetDouble(reader.GetOrdinal(name))

let getInt16 name (reader : SqlDataReader) =
  reader.GetInt16(reader.GetOrdinal(name))

let getInt32 name (reader : SqlDataReader) =
  reader.GetInt32(reader.GetOrdinal(name))

let getInt64 name (reader : SqlDataReader) =
  reader.GetInt64(reader.GetOrdinal(name))

let getString name (reader : SqlDataReader) =
  reader.GetString(reader.GetOrdinal(name))

let getDateTime name (reader : SqlDataReader) =
  reader.GetDateTime(reader.GetOrdinal(name))

let getBool name (reader : SqlDataReader) =
  reader.GetBoolean(reader.GetOrdinal(name))

let getIntArray name (reader : SqlDataReader) =
  reader.GetValue(reader.GetOrdinal(name)) :?> int array

let searchHowToClause how value =
  match how with
  | "Equals"      -> value
  | "Begins With" -> sprintf "%s%s" value "%"
  | _             -> value
