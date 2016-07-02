module scripts

open Suave.Html
open helper_html

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

let chartjs_onready = """
<script type="text/javascript">
  $(document).ready(function(){
    var lineContext = $("#line");

    var data = {
      labels: ["January", "February", "March", "April", "May", "June", "July"],
      datasets: [
        {
          label: "My First dataset",
          fill: false,
          lineTension: 0.1,
          backgroundColor: "rgba(75,192,192,0.4)",
          borderColor: "rgba(75,192,192,1)",
          borderCapStyle: 'butt',
          borderDash: [],
          borderDashOffset: 0.0,
          borderJoinStyle: 'miter',
          pointBorderColor: "rgba(75,192,192,1)",
          pointBackgroundColor: "#fff",
          pointBorderWidth: 1,
          pointHoverRadius: 5,
          pointHoverBackgroundColor: "rgba(75,192,192,1)",
          pointHoverBorderColor: "rgba(220,220,220,1)",
          pointHoverBorderWidth: 2,
          pointRadius: 1,
          pointHitRadius: 10,
          data: [65, 59, 80, 81, 56, 55, 40],
        }
      ]
    };
    var lineChart = new Chart(lineContext, {
      type: 'line',
      data: data,
      options: {}
    });
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

let chartjs_bundle =
  [
    jquery_1_11_3_min
    bootstrap
    chartjs_2_1_6_min
    chartjs_onready
  ]
  |> List.map (fun script -> text script) |> flatten
