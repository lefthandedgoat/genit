module database

open forms

let getCars () =
  [
    { Model = "Honda"; Make = "Civic"; Year = 2015; Price = 21250 }
    { Model = "Ford"; Make = "F150"; Year = 2016; Price = 31950 }
    { Model = "Chevrolet"; Make = "1500"; Year = 2015; Price = 34290 }
    { Model = "Toyota"; Make = "Camry"; Year = 2014; Price = 24050 }
    { Model = "Honda"; Make = "Accord"; Year = 2013; Price = 19250 }
    { Model = "BMW"; Make = "M4"; Year = 2015; Price = 54250 }
    { Model = "BMW"; Make = "328i"; Year = 2015; Price = 44650 }
    { Model = "Ford"; Make = "Taurus"; Year = 2016; Price = 31250 }
  ]
