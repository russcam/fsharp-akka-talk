#load "Bootstrapper.fsx"

open System
open Akka
open Akka.Actor
open Akka.Routing
open Akka.Configuration
open Akka.FSharp

// Create a round robin router actor configured through HOCON, 
// that will create 10 echo actors and route messages to them in a round robin fashion.

// commented resizer to demonstrate dynamic resizing fo the router pool

type Command =
| Message of string

let config =  
    Configuration.parse
        """akka.actor.deployment {
            /echo-router {
                router = round-robin-pool
                nr-of-instances = 10
                # resizer {
                #   enabled = on
                #   lower-bound = 1
                #   upper-bound = 10
                #   messages-per-resize = 10
                #   rampup-rate = 0.2
                #   backoff-rate = 0.1
                #   pressure-threshold = 1
                #   backoff-threshold = 0.3
                # }
            }
        }"""

let system = System.create "system" <| config

// spawn option for router comes from config
// function passed is the routee
let router = spawnOpt system "echo-router" (fun mailbox ->               
        let rec loop () = actor {
            let! (Message message) = mailbox.Receive ()
            Console.WriteLine (sprintf "%s received by %A" message mailbox.Self.Path)
            return! loop ()
        }
        loop ()) [SpawnOption.Router(FromConfig.Instance)]
               
for i in [1..50] do 
    let message =  Message (sprintf "Message %d" i)
    router <! message
