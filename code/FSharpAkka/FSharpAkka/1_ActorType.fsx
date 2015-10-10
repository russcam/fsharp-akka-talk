#load "Bootstrapper.fsx"

open System
open System.Threading
open Akka.Actor
open Akka.Configuration
open Akka.FSharp

// Create an actor using a type inheriting from Actor
// and pass a message to it

type EchoActor () =
    inherit Actor ()

    override x.OnReceive message =
        match message with
        | :? string as msg -> printfn "%s" msg
        | _ -> printfn "Received a non-string message"

let system = System.create "system" <| Configuration.defaultConfig()
let echoActor = system.ActorOf(Props(typedefof<EchoActor>))

echoActor.Tell "Hello F# Sydney!"