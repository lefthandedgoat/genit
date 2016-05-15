module loadGenerator

open System
open HttpClient
open Microsoft.FSharp.Reflection

type Result =
  {
    URI : string
    Seconds : float
    Average : int
    Max : int
    Min : int
    NumberofRequests : int
    Concurrency : int
  }

type LoadTest =
  {
    URI : string
    NumberofRequests : int
    MaxConcurrentRequests : int
  }

type LoadTestForm =
  {
    URI : string
    NumberofRequests : string
    MaxConcurrentRequests : string
  }

let convertLoadTest (loadTestForm : LoadTestForm) : LoadTest =
  {
    URI = loadTestForm.URI
    NumberofRequests = int loadTestForm.NumberofRequests
    MaxConcurrentRequests = int loadTestForm.MaxConcurrentRequests
  }

let defaultLoadTest : LoadTest =
  {
    URI = "http://localhost:8083/"
    NumberofRequests = 100
    MaxConcurrentRequests = 5
  }

//actor stuff
type actor<'t> = MailboxProcessor<'t>

type Command =
  | Process of string

type Worker =
  | Do
  | Retire

type Manager =
  | Initialize of LoadTest * Command * AsyncReplyChannel<Result>
  | WorkerDone of float * actor<Worker>

let doit uri =
  let stopWatch = System.Diagnostics.Stopwatch.StartNew()
  let request = createRequest Get uri
  getResponse request |> ignore
  let responseTime = stopWatch.Elapsed.TotalMilliseconds
  responseTime

let newWorker (manager : actor<Manager>) (command : Command): actor<Worker> =
  actor.Start(fun self ->
    let rec loop () =
      async {
        let! msg = self.Receive ()
        match msg with
        | Worker.Retire ->
          return ()
        | Worker.Do ->
          match command with
          | Process uri ->
            let results = doit uri
            manager.Post(Manager.WorkerDone(results, self))
          return! loop ()
      }
    loop ())

let rec haveWorkersWork (idleWorkers : actor<Worker> list) numberOfRequests =
    match idleWorkers with
    | [] -> numberOfRequests
    | worker :: remainingWorkers ->
      worker.Post(Worker.Do)
      haveWorkersWork remainingWorkers (numberOfRequests - 1)

let newManager () : actor<Manager> =
  let sw = System.Diagnostics.Stopwatch()
  actor.Start(fun self ->
    let rec loop sendData numberOfRequests pendingRequests results channel =
      async {
        let! msg = self.Receive ()
        match msg with
        | Manager.Initialize (sendData, command, channel) ->
          //build up a list of all the work to do
          let numberOfRequests = sendData.NumberofRequests
          let workers = [ 1 .. sendData.MaxConcurrentRequests ] |> List.map (fun _ -> newWorker self command)
          let results = []
          let pendingRequests = sendData.MaxConcurrentRequests
          sw.Restart()
          let numberOfRequests = haveWorkersWork workers numberOfRequests
          return! loop sendData numberOfRequests pendingRequests results (Some channel)
        | Manager.WorkerDone(ms, worker) ->
          let results = ms :: results
          if numberOfRequests > 0 then
            let numberOfRequests = haveWorkersWork [worker] numberOfRequests
            return! loop sendData numberOfRequests pendingRequests results channel
          else if pendingRequests > 1 then //if only 1 pendingRequest, then this that pendingRequest so we are done
            let pendingRequests = pendingRequests - 1
            return! loop sendData numberOfRequests pendingRequests results channel
          else
            sw.Stop()
            let avg = results |> List.average
            let min = results |> List.min
            let max = results |> List.max
            channel.Value.Reply(
              {
                URI = sendData.URI
                Seconds = sw.Elapsed.TotalSeconds
                Average = int avg
                Min = int min
                Max = int max
                NumberofRequests = sendData.NumberofRequests;
                Concurrency = sendData.MaxConcurrentRequests;
              })
            return! loop sendData numberOfRequests pendingRequests results channel
      }
    loop defaultLoadTest 0 0 [] None)
