#load "Bootstrapper.fsx"

open System
open Akka
open Akka.Actor
open Akka.Routing
open Akka.Configuration
open Akka.FSharp

// Create a round robin router actor in code, that will create 10 routees
// and route messages to them in a round robin fashion

type Command =
| Message of string

let system = ActorSystem.Create "system"

// function passed to spawnOpt is the routee
let router = spawnOpt system "echo-router" (fun mailbox ->               
        let rec loop () = actor {
            let! (Command.Message message) = mailbox.Receive ()

            Console.WriteLine (sprintf "%s received by %A" message mailbox.Self.Path)
            return! loop ()
        }
        loop ()) [SpawnOption.Router(RoundRobinPool 10)]
               
for i in [1..50] do 
    let message =  Message (sprintf "Message %d" i)
    router <! message
