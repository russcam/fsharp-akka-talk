#load "Bootstrapper.fsx"

open System
open Akka
open Akka.Actor
open Akka.Configuration
open Akka.FSharp

type CustomException() = inherit Exception()
type Message = | Echo of string | Crash
let system = System.create "system" <| Configuration.defaultConfig()

// create parent actor to watch over jobs delegated to it's child
let child (childMailbox:Actor<Message>) = 
    childMailbox.Defer (fun () -> printfn "Child stopping")
    printfn "Child started"

    let rec childLoop() = actor {
        let! msg = childMailbox.Receive()
        match msg with
        | Echo info -> 
            let response = sprintf "Child %s received: %s" (childMailbox.Self.Path.ToStringWithAddress()) info
            childMailbox.Sender() <! response
        | Crash -> 
            printfn "Child %A received crash order" (childMailbox.Self.Path)
            raise (CustomException())
        return! childLoop()
    }
    childLoop()    

let parent (parentMailbox:Actor<Message>) =
    let child = spawn parentMailbox "child" child
    let rec parentLoop() = actor {
        let! msg = parentMailbox.Receive()
        child.Forward msg  
        return! parentLoop()
    }
    parentLoop()

let parentChild = 
    spawnOpt system "parent" parent
        <| [ SpawnOption.SupervisorStrategy (
                Strategy.OneForOne(fun e ->
                    match e with
                    | :? CustomException -> Directive.Restart 
                    | _ -> SupervisorStrategy.DefaultDecider(e))); ]

async {
    let! response = parentChild <? Echo "hello world"
    printfn "%s" response
    // after this one child should crash
    parentChild <! Crash
    System.Threading.Thread.Sleep 200
        
    // actor should be restarted
    let! response = parentChild <? Echo "hello world2"
    printfn "%s" response
} |> Async.RunSynchronously
