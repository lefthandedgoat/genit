module generated_fake_data

open generated_types

let concat values =
  if values = []
  then ""
  else values |> List.reduce (fun value1 value2 -> value1 + " " + value2)

let random = System.Random()
let randomItem (items : 'a list) = items.[random.Next(items.Length)]
let randomItems number items =
  if number = 1
  then randomItem items
  else
    let number = random.Next(number)
    [ 1 .. (number + 1 ) ] |> List.map (fun _ -> items.[random.Next(items.Length)]) |> concat

let words = ["At"; "vero"; "eos"; "et"; "accusamus"; "et"; "iusto"; "odio"; "dignissimos"; "ducimus"; "qui"; "blanditiis"; "praesentium"; "voluptatum"; "atque"; "corrupti"; "quos"; "dolores"; "et"; "quas"; "molestias"; "excepturi"; "sint"; "occaecati"; "cupiditate"; "non"; "provident"; "similique"; "sunt"; "in"; "culpa"; "qui"; "officia"; "deserunt"; "mollitia"; "animi"; "id"; "est"; "laborum"; "omnis"; "dolor"; "repellendus"; "Temporibus"; "autem"; "quibusdam"; "et"; "aut"; "officiis"; "debitis"; "aut"; "rerum"; "necessitatibus"; "saepe"; "eveniet"; "ut"; "et"; "oluptates"; "repudiandae"; "sint"; "et"; "molestiae"; "non"; "recusandae"; "Itaque"; "earum"; "rerum"; "hic"; "tenetur"; "sapiente"; "delectus"; "ut"; "aut"; "reiciendis"; "voluptatibus"; "maiores"; "alias"; "consequatur"; "aut"; "perferendis"; "doloribus"; "asperiores"; "repellat"; ]
let names = [ "James";"Mary";"John";"Patricia";"Robert";"Jennifer";"Michael";"Elizabeth";"William";"Linda";"David";"Barbara";"Richard";"Susan";"Joseph";"Margaret";"Charles";"Jessica";"Thomas";"Sarah";"Christopher";"Dorothy";"Daniel";"Karen";"Matthew";"Nancy";"Donald";"Betty";"Anthony";"Lisa";"Mark";"Sandra";"Paul";"Ashley";"Steven";"Kimberly";"George";"Donna";"Kenneth";"Helen";"Andrew";"Carol";"Edward";"Michelle";"Joshua";"Emily";"Brian";"Amanda";"Kevin";"Melissa";"Ronald";"Deborah";"Timothy";"Laura";"Jason";"Stephanie";"Jeffrey";"Rebecca";"Ryan";"Sharon";"Gary";"Cynthia";"Nicholas";"Kathleen";"Eric";"Anna";"Jacob";"Shirley";"Stephen";"Ruth";"Jonathan";"Amy";"Larry";"Angela";"Frank";"Brenda";"Scott";"Virginia";"Justin";"Pamela";"Catherine";"Raymond";"Katherine";"Gregory";"Nicole";"Samuel";"Christine";"Benjamin";"Samantha";"Patrick";"Janet";"Jack";"Debra";"Dennis";"Carolyn";"Alexander";"Rachel";"Jerry";"Heather"]
let zips = [ 75028; 75220; 75233; 76701; 76531 ]
let cities = [ "Dallas";"Fort Worth";"Arlington";"Plano";"Garland";"Irving";"Grand Prairie";"McKinney";"Mesquite";"Frisco";"Carrollton";"Denton";"Richardson";"Lewisville" ]
