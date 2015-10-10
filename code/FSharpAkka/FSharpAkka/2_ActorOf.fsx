#load "Bootstrapper.fsx"

open System
open System.Threading
open Akka.Actor
open Akka.Configuration
open Akka.FSharp

// Spawn an actor using actorOf. This wraps the passed function
// in an actor computation expression and invokes it, passing the 
// the message received from the actor mailbox

let system = System.create "system" <| Configuration.defaultConfig()

let echo (message:obj) =
    match message with
    | :? string as msg -> printfn "%s" msg
    | _ -> printfn "Received a non-string message"

let echoActor = spawn system "echo" (actorOf echo)

echoActor <! "Hello F# Sydney!"
