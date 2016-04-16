module generated_fake_data

open generated_types
open generated_data_access

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

type CityStateZip = { City : string; State: string; Zip : string }
let words = [ "At";"vero";"eos";"et";"accusamus";"et";"iusto";"odio";"dignissimos";"ducimus";"qui";"blanditiis";"praesentium";"voluptatum";"atque";"corrupti";"quos";"dolores";"et";"quas";"molestias";"excepturi";"sint";"occaecati";"cupiditate";"non";"provident";"similique";"sunt";"in";"culpa";"qui";"officia";"deserunt";"mollitia";"animi";"id";"est";"laborum";"omnis";"dolor";"repellendus";"Temporibus";"autem";"quibusdam";"et";"aut";"officiis";"debitis";"aut";"rerum";"necessitatibus";"saepe";"eveniet";"ut";"et";"oluptates";"repudiandae";"sint";"et";"molestiae";"non";"recusandae";"Itaque";"earum";"rerum";"hic";"tenetur";"sapiente";"delectus";"ut";"aut";"reiciendis";"voluptatibus";"maiores";"alias";"consequatur";"aut";"perferendis";"doloribus";"asperiores";"repellat"; ]
let firstNames = [ "James";"Mary";"John";"Patricia";"Robert";"Jennifer";"Michael";"Elizabeth";"William";"Linda";"David";"Barbara";"Richard";"Susan";"Joseph";"Margaret";"Charles";"Jessica";"Thomas";"Sarah";"Christopher";"Dorothy";"Daniel";"Karen";"Matthew";"Nancy";"Donald";"Betty";"Anthony";"Lisa";"Mark";"Sandra";"Paul";"Ashley";"Steven";"Kimberly";"George";"Donna";"Kenneth";"Helen";"Andrew";"Carol";"Edward";"Michelle";"Joshua";"Emily";"Brian";"Amanda";"Kevin";"Melissa";"Ronald";"Deborah";"Timothy";"Laura";"Jason";"Stephanie";"Jeffrey";"Rebecca";"Ryan";"Sharon";"Gary";"Cynthia";"Nicholas";"Kathleen";"Eric";"Anna";"Jacob";"Shirley";"Stephen";"Ruth";"Jonathan";"Amy";"Larry";"Angela";"Frank";"Brenda";"Scott";"Virginia";"Justin";"Pamela";"Catherine";"Raymond";"Katherine";"Gregory";"Nicole";"Samuel";"Christine";"Benjamin";"Samantha";"Patrick";"Janet";"Jack";"Debra";"Dennis";"Carolyn";"Alexander";"Rachel";"Jerry";"Heather" ]
let lastNames = [ "Smith";"Brown";"Johnson";"Jones";"Williams";"Davis";"Miller";"Wilson";"Taylor";"Clark";"White";"Moore";"Thompson";"Allen";"Martin";"Hall";"Adams";"Thomas";"Wright";"Baker";"Walker";"Anderson";"Lewis";"Harris";"Hill";"King";"Jackson";"Lee";"Green";"Wood";"Parker";"Campbell";"Young";"Robinson";"Stewart";"Scott";"Rogers";"Roberts";"Cook";"Phillips";"Turner";"Carter";"Ward";"Foster";"Morgan";"Howard" ]
let states = [ "Alabama";   "Alaska";   "Arizona";"Arkansas";"   California"; "Colorado";"Connecticut";"Delaware";  "Florida";     "Georgia";"Hawaii"; "Idaho"; "Illinois";"Indiana";     "Iowa";      "Kansas"; "Kentucky";  "Louisiana";  "Maine";   "Maryland"; "Massachusetts";"Michigan";"Minnesota";  "Mississippi";"Missouri";   "Montana"; "Nebraska";"Nevada";   "New Hampshire";"New Jersey";"New Mexico"; "New York";     "North Carolina";"North Dakota";"Ohio";    "Oklahoma";     "Oregon";  "Pennsylvania";"Rhode Island";"South Carolina";"South Dakota";"Tennessee";"Texas";  "Utah";          "Vermont";   "Virginia";      "Washington";"West Virginia";"Wisconsin";"Wyoming"]
let cities = [ "Birmingham";"Anchorage";"Phoenix";"Little Rock";"Los Angeles";"Denver";  "Bridgeport"; "Wilmington";"Jacksonville";"Atlanta";"Honolulu";"Boise";"Chicago"; "Indianapolis";"Des Moines";"Wichita";"Louisville";"New Orleans";"Portland";"Baltimore";"Boston";       "Detroit"; "Minneapolis";"Jackson";    "Kansas City";"Billings";"Omaha";   "Las Vegas";"Manchester";   "Newark";    "Albuquerque";"New York City";"Charlotte";     "Fargo";       "Columbus";"Oklahoma City";"Portland";"Philadelphia";"Providence";  "Columbia";      "Sioux Falls"; "Memphis";  "Houston";"Salt Lake City";"Burlington";"Virginia Beach";"Seattle";   "Charleston";   "Milwaukee";"Cheyenne"]
let zips = [   "35201";     "99501";    "85001";  "72201";      "90001";      "80123";   "06601";      "19801";     "32099";       "30301";  "96813";   "83701";"60290";   "46201";       "50301";     "67201";  "40201";     "70112";      "97201";   "21117";    "02109";        "48201";   "55401";      "39201";      "64101";      "59101";   "68022";   "89101";    "03101";        "07101";     "87101";      "10001";        "28201";         "58102";       "43085";   "73101";        "97201";   "19019";       "02901";       "29201";         "57101";       "37501";    "Houston";"84101";         "05401";     "23450";         "98101";     "29401";        "53202";    "82001";]
let streetNames = [ "Second";"Third";"First";"Fourth";"Park";"Fifth";"Main";"Sixth";"Oak";"Seventh";"Pine";"Maple";"Cedar";"Eighth";"Elm";"Washington";"Ninth";"Lake";"Hill"; ]
let streetNameSuffixes = [ "Alley";"Avenue";"Bluff";"Boulevard";"Circle";"Estates";"Junction";"Road";"Lane"]

let citiesSatesZips = List.zip3 cities states zips |> List.map (fun (city, state, zip) -> { City = city; State = state; Zip = zip })
