module loadGeneratorView

open Suave.Html
open helper_html
open helper_bootstrap
open loadGenerator

let resultsTable (results : Result list) =
  let toTd (result : Result) =
    [
      td [ text (string result.Seconds) ]
      td [ text (string result.Average) ]
      td [ text (string result.Min) ]
      td [ text (string result.Max) ]
      td [ text (string result.URI) ]
      td [ text (string result.NumberofRequests) ]
      td [ text (string result.Concurrency) ]
    ]
  block_flat [
    content [
      table_bordered
        [
          "Total Seconds"
          "Average ms"
          "Min ms"
          "Max ms"
          "URI"
          "# Requests"
          "# Concurrent"
        ]
        results toTd
    ]
  ]

let formWithResults description results formElements =
  container [
    row [
      form_wrapper [
        form_title [ h3 description ]
        form_content [
          div [
            form_horizontal [
              content (formElements @ [form_group [ sm12 [ button_submit_right ] ] ])
            ]
          ]
        ]
      ]
    ]
    row [ form_wrapper [ resultsTable results ] ]
  ]

let html (data : LoadTest) results =
  base_html
    "Load Test"
    (base_header "Load Test")
    [
      formWithResults "Load Test" results [
        label_text "URI" data.URI
        label_text "Number of Requests" (string data.NumberofRequests)
        label_text "Max Concurrent Requests" (string data.MaxConcurrentRequests)
      ]
    ]
    scripts.common
