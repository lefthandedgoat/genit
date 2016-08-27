module generated_types

type Register =
  {
    UserID : int64
    FirstName : string
    LastName : string
    Email : string
    Password : string
  }
  
type Login =
  {
    UserID : int64
    Email : string
    Password : string
  }
  
type Self =
  {
    SelfID : int64
    FirstName : string
    LastName : string
    Title : string
    Email : string
    PositionObjective1 : string
    PositionObjective2 : string
    PositionObjective3 : string
    Supervisor : int16
    CoworkerorClient1 : int16
    CoworkerorClient2 : int16
    CoworkerorClient3 : int16
    CoworkerorClient4 : int16
  }
  
type Customer =
  {
    CustomerID : int64
    CustomerName : string
    ContactName : string
    Email : string
  }
  
type EmployeeSurvey =
  {
    EmployeeSurveyID : int64
    EmployeeName : string
    PositionObjective1 : string
    PositionObjective2 : string
    PositionObjective3 : string
    Rating : int16
  }
  
type CustomerSurvey =
  {
    CustomerSurveyID : int64
    Customer : string
    Comment : string
    HowLikely : int16
  }
  