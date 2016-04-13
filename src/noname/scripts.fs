module scripts

open Suave.Html
open htmlHelpers

let jquery_1_11_3_min = """<script src="//code.jquery.com/jquery-1.11.3.min.js"></script>"""
let bootstrap = """<script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js"></script>"""
let datatable_jquery_1_10_9_min = """<script src="//cdn.datatables.net/1.10.9/js/jquery.dataTables.min.js"></script>"""
//todo dont hotlink
let datatable_adapater = """<script src="http://chronogram.co/js/jquery.datatables/bootstrap-adapter/js/datatables.js"></script>"""

let generic_datatable = """
<script type="text/javascript">
  $(document).ready(function(){
    //Basic Instance
    $(".table.table-bordered").dataTable();

    //Search input style
    $('.dataTables_filter input').addClass('form-control').attr('placeholder','Search');
    $('.dataTables_length select').addClass('form-control').attr('size','1');

    //make a tr into a link
    $('tr[data-link]').on("click", function() { document.location = $(this).data('link'); });
  });
</script>
"""

let none = emptyText

let common =
  [
    jquery_1_11_3_min
    bootstrap
  ]
  |> List.map (fun script -> text script) |> flatten

let datatable_bundle =
  [
    jquery_1_11_3_min
    bootstrap
    datatable_jquery_1_10_9_min
    datatable_adapater
    generic_datatable
  ]
  |> List.map (fun script -> text script) |> flatten
