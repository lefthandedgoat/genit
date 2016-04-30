module generated_data_access

open generated_types
open generated_forms
open adoHelper
open Npgsql
open dsl
open BCrypt.Net

type BCryptScheme =
  {
    Id : int
    WorkFactor : int
  }

let bCryptSchemes : BCryptScheme list = [ { Id = 1; WorkFactor = 8; } ]
let getBCryptScheme id = bCryptSchemes |> List.find (fun scheme -> scheme.Id = id)
let currentBCryptScheme = 1
