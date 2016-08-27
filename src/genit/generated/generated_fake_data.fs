module generated_fake_data

open generated_types
open generated_data_access
open helper_general

let fake_self () =
  {
    SelfID = -1L 
    FirstName = randomItem firstNames 
    LastName = randomItem lastNames 
    Title = randomItems 6 words 
    Email = sprintf "%s@%s.com" (randomItem words) (randomItem words) 
    PositionObjective1 = randomItems 6 words 
    PositionObjective2 = randomItems 6 words 
    PositionObjective3 = randomItems 6 words 
    Supervisor = 1s 
    CoworkerorClient1 = 1s 
    CoworkerorClient2 = 1s 
    CoworkerorClient3 = 1s 
    CoworkerorClient4 = 1s 
  }

let fake_many_self number =
  [| 1..number |]
  |> Array.map (fun _ -> fake_self ()) //no parallel cause of RNG
  |> Array.Parallel.map insert_self
  |> ignore
 
 
let fake_customer () =
  {
    CustomerID = -1L 
    CustomerName = (randomItem firstNames) + " " + (randomItem lastNames) 
    ContactName = (randomItem firstNames) + " " + (randomItem lastNames) 
    Email = sprintf "%s@%s.com" (randomItem words) (randomItem words) 
  }

let fake_many_customer number =
  [| 1..number |]
  |> Array.map (fun _ -> fake_customer ()) //no parallel cause of RNG
  |> Array.Parallel.map insert_customer
  |> ignore
 
 
let fake_employeeSurvey () =
  {
    EmployeeSurveyID = -1L 
    EmployeeName = (randomItem firstNames) + " " + (randomItem lastNames) 
    PositionObjective1 = randomItems 40 words 
    PositionObjective2 = randomItems 40 words 
    PositionObjective3 = randomItems 40 words 
    Rating = 1s 
  }

let fake_many_employeeSurvey number =
  [| 1..number |]
  |> Array.map (fun _ -> fake_employeeSurvey ()) //no parallel cause of RNG
  |> Array.Parallel.map insert_employeeSurvey
  |> ignore
 
 
let fake_customerSurvey () =
  {
    CustomerSurveyID = -1L 
    Customer = randomItems 6 words 
    Comment = randomItems 40 words 
    HowLikely = 1s 
  }

let fake_many_customerSurvey number =
  [| 1..number |]
  |> Array.map (fun _ -> fake_customerSurvey ()) //no parallel cause of RNG
  |> Array.Parallel.map insert_customerSurvey
  |> ignore
 
 
  