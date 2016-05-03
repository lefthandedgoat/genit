module generalHelpers

open System
open System.Text.RegularExpressions

let onlyIfValues values func =
  let values = values |> List.filter (fun str -> str <> "")
  if values = []
  then ""
  else values |> func

let flatten values = onlyIfValues values (List.reduce (fun value1 value2 -> sprintf "%s%s%s" value1 Environment.NewLine value2))

let flattenWith delimeter values = onlyIfValues values (List.reduce (fun value1 value2 -> sprintf "%s%s%s%s" value1 delimeter Environment.NewLine value2))

let concat values = onlyIfValues values (List.reduce (fun value1 value2 -> sprintf "%s %s" value1 value2))

let repeat (value : string) times = [1..times] |> List.map (fun _ -> value) |> List.reduce (+)
let pad tabs field = sprintf "%s%s" (repeat "  " tabs) field
let rightPad upto field = sprintf "%s%s" field (repeat " " (upto - field.Length))
let clean (value : string) = Regex.Replace(value, "[^0-9a-zA-Z ]+", "")
let lower (value : string) = value.ToLower()
let trimEnd (value : string) = value.TrimEnd()
let upperFirst (value : string) = Char.ToUpper(value.[0]).ToString() + value.Substring(1)
let lowerFirst (value : string) = Char.ToLower(value.[0]).ToString() + value.Substring(1)
let spaceToNothing (value : string) = value.Replace(" ", "")
let spaceToUnderscore (value : string) = value.Replace(" ", "_")
let camelCase = spaceToNothing >> lowerFirst
let typeCase = spaceToNothing >> upperFirst
let form value = sprintf "%sForm" value
let pluralize = sprintf "%ss" //trrrrble

let to_val = camelCase
let to_type = typeCase
let to_formVal = camelCase >> form
let to_formType = typeCase >> form
let to_href = camelCase >> sprintf "/%s"
let to_createHref = camelCase >> sprintf "/%s/create"
let to_viewHref = camelCase >> (fun page -> sprintf "/%s/view/%s" page "%i")
let to_apiViewHref = camelCase >> (fun page -> sprintf "/api/%s/view/%s" page "%i")
let to_editHref = camelCase >> (fun page -> sprintf "/%s/edit/%s" page "%i")
let to_listHref = camelCase >> sprintf "/%s/list"
let to_searchHref = camelCase >> sprintf "/%s/search"

let to_property = typeCase

let to_database = clean >> lower >> spaceToUnderscore
let to_tableName = clean >> lower >> spaceToUnderscore >> pluralize
let to_dbColumn = clean >> lower >> spaceToUnderscore

//data_access
type BCryptScheme =
  {
    Id : int
    WorkFactor : int
  }

let bCryptSchemes : BCryptScheme list = [ { Id = 1; WorkFactor = 8; } ]
let getBCryptScheme id = bCryptSchemes |> List.find (fun scheme -> scheme.Id = id)
let currentBCryptScheme = 1

//fake_data
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

//bundles

type Bundle<'a> =
  {
    single_fake : unit -> 'a
    many_fake : int -> unit
    getMany : unit -> 'a list
    getManyWhere : string -> string -> string -> 'a list
    get_list : 'a list -> string
    get_edit : 'a -> string
    get_create : string
    get_search : string option -> string option -> string -> 'a list -> string
    searchHref : string
  }
