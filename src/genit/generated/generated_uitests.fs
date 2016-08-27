module generated_uitests

open generated_forms
open generated_validation
open canopy

let run () =
  start firefox

  context "Self"

  once (fun _ -> url "http://localhost:8083/self/create"; click ".btn") 

  "First Name is required" &&& fun _ ->
    displayed "First Name is required"
    
  "Last Name is required" &&& fun _ ->
    displayed "Last Name is required"
    
  "Title is required" &&& fun _ ->
    displayed "Title is required"
    
  "Email is required" &&& fun _ ->
    displayed "Email is required"
    
  "Email must be a valid email" &&& fun _ ->
    displayed "Email is not a valid email"
    
  "Position Objective 1 is required" &&& fun _ ->
    displayed "Position Objective 1 is required"
    
  "Supervisor is required" &&& fun _ ->
    displayed "Supervisor is required"
    
  "Coworker or Client 1 is required" &&& fun _ ->
    displayed "Coworker or Client 1 is required"
    
  "Coworker or Client 2 is required" &&& fun _ ->
    displayed "Coworker or Client 2 is required"
    
  "Coworker or Client 3 is required" &&& fun _ ->
    displayed "Coworker or Client 3 is required"
    
  "Coworker or Client 4 is required" &&& fun _ ->
    displayed "Coworker or Client 4 is required"
    
    
  context "Customer"

  once (fun _ -> url "http://localhost:8083/customer/create"; click ".btn") 

  "Customer Name is required" &&& fun _ ->
    displayed "Customer Name is required"
    
  "Contact Name is required" &&& fun _ ->
    displayed "Contact Name is required"
    
  "Email is required" &&& fun _ ->
    displayed "Email is required"
    
  "Email must be a valid email" &&& fun _ ->
    displayed "Email is not a valid email"
    
    
  context "Employee Survey"

  once (fun _ -> url "http://localhost:8083/employeeSurvey/create"; click ".btn") 

  "Rating is required" &&& fun _ ->
    displayed "Rating is required"
    
    
  context "Customer Survey"

  once (fun _ -> url "http://localhost:8083/customerSurvey/create"; click ".btn") 

  "Comment is required" &&& fun _ ->
    displayed "Comment is required"
    
  "How Likely is required" &&& fun _ ->
    displayed "How Likely is required"
    
    

  canopy.runner.run()

  quit()
