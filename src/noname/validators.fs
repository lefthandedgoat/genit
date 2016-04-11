module validators

open System
open System.Net.Mail
open System.Text.RegularExpressions

let required property value =
  if String.IsNullOrWhiteSpace value
  then Some (property, sprintf "%s is required" property)
  else None

let equal property1 property2 value1 value2  =
  if value1 <> value2
  then Some (property2, sprintf "%s must be the same as %s" property2 property1)
  else None

let email property value =
  let isEmail = try MailAddress(value)|> ignore; true with | _ -> false
  if not isEmail
  then Some (property, sprintf "%s is not a valid email" value)
  else None

let private passwordPattern = @"(\w){6,100}"
let password property value =
  if not <| Regex(passwordPattern).IsMatch(value)
  then Some (property, sprintf "%s must be between 6 and 100 characters" property)
  else None
