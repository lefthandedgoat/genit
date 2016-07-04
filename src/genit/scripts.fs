module scripts

open dsl
open helper_html
open helper_general
open Suave.Html

let jquery_1_11_3_min = """<script src="//code.jquery.com/jquery-1.11.3.min.js"></script>"""
let bootstrap = """<script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js"></script>"""
let datatable_jquery_1_10_9_min = """<script src="//cdn.datatables.net/1.10.9/js/jquery.dataTables.min.js"></script>"""
let chartjs_2_1_6_min = """<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.1.6/Chart.min.js"></script>"""
let datatable_adapter = """<script src="/js/datatables.js"></script>"""
let bootstrap_datepicker = """<script src="/js/bootstrap-datetimepicker.min.js"></script>"""

let generic_onready = """
<script type="text/javascript">
  $(document).ready(function(){
    //enable datepickers
    $('.datetime').datetimepicker();
  });
</script>
"""

let generic_datatable = """
<script type="text/javascript">
  $(document).ready(function(){
    //Basic Instance
    $(".table.table-bordered").dataTable();

    //Search input style
    $('.dataTables_filter input').addClass('form-control').attr('placeholder','Search');
    $('.dataTables_length select').addClass('form-control').attr('size','1');

    //make a tr into a link on paging
    $(".table.table-bordered").on("draw.dt", function () {
        $('tr[data-link]').on("click", function() { document.location = $(this).data('link'); });
    } );

    //make a tr into a link intial load
    $('tr[data-link]').on("click", function() { document.location = $(this).data('link'); });
  });
</script>
"""

let none = emptyText

let common =
  [
    jquery_1_11_3_min
    bootstrap
    bootstrap_datepicker
    generic_onready
  ]
  |> List.map (fun script -> text script) |> flatten

let datatable_bundle =
  [
    jquery_1_11_3_min
    bootstrap
    datatable_jquery_1_10_9_min
    datatable_adapter
    bootstrap_datepicker
    generic_onready
    generic_datatable
  ]
  |> List.map (fun script -> text script) |> flatten

(*

Charting

*)

let contextTemplate item =
  let template type' index = sprintf """var %sContext%i = $("#%s%i");""" type' index type' index |> pad 2
  match item.ChartType with
  | Line -> template "line" item.Index
  | Pie  -> template "pie" item.Index
  | Bar  -> template "bar" item.Index

let chartInstanceTemplate item =
  let template type' index =
    sprintf """
    var %sChart%i = new Chart(%sContext%i, {
      type: '%s',
      data: data,
      options: {}
    });""" type' index type' index type'
  match item.ChartType with
  | Line -> template "line" item.Index
  | Pie  -> template "pie" item.Index
  | Bar  -> template "bar" item.Index

let formatDescriptions descriptions =
  descriptions
  |> List.map trimEnd
  |> List.reduce (sprintf """%s","%s""")
  |> fun reduced -> sprintf """["%s"]""" reduced

let formatData (data : int list) =
  data
  |> List.map string
  |> List.reduce (sprintf "%s, %s")
  |> fun reduced -> "[" + reduced + "]"

let chartTemplate (chartData, chart) =
  sprintf """
%s
    var data = {
      labels: %s,
      datasets: [
        {
          label: "%s",
          fill: false,
          lineTension: 0.1,
          backgroundColor: "rgba(26,179,148,0.5)",
          borderColor: "rgba(26,179,148,0.7)",
          borderCapStyle: 'butt',
          borderDash: [],
          borderDashOffset: 0.0,
          borderJoinStyle: 'miter',
          pointBorderColor: "rgba(75,192,192,1)",
          pointBackgroundColor: "#fff",
          pointBorderWidth: 1,
          pointHoverRadius: 5,
          pointHoverBackgroundColor: "rgba(26,179,148,0.7)",
          pointHoverBorderColor: "rgba(26,179,148,1)",
          pointHoverBorderWidth: 2,
          pointRadius: 1,
          pointHitRadius: 10,
          data: %s,
        }
      ]
    };
%s
  """ (contextTemplate chart) (formatDescriptions chartData.Descriptions) chart.Field (formatData chartData.Data) (chartInstanceTemplate chart)

let chartjs_onready items =
  sprintf """
<script type="text/javascript">
  $(document).ready(function(){
%s
  });
</script>
""" (items |> List.map chartTemplate |> helper_general.flatten)

let chartjs_bundle charts =
  [
    jquery_1_11_3_min
    bootstrap
    chartjs_2_1_6_min
    chartjs_onready charts
  ]
  |> List.map (fun script -> text script) |> flatten
