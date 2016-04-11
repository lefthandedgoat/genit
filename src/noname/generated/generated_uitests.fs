module generated_uitests

open generated_forms
open generated_validation
open canopy

let run () =
  start firefox

  canopy.runner.run()

  quit()
