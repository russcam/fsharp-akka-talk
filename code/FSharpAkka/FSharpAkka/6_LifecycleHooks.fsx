#load "Bootstrapper.fsx"

open System
open Akka
open Akka.Actor
open Akka.Routing
open Akka.Configuration
open Akka.FSharp

// Demonstrate Lifecycle Hooks and Monitoring an actor
// The default behaviour when child actor throws an exception is to restart the actor

type Command =
| Message of string
| Crash

type MyActor() =
    inherit Actor()
        override this.PreStart () = 
            printfn "Actor pre start"
        override this.PostStop () = 
            printfn "Actor post stop"
        override this.OnReceive msg = 
            match msg with
            | :? Command as command ->
                match command with
                | Message m -> printfn "Received message %s" m
                | Crash -> raise(new exn())
            | _ -> this.Unhandled msg
                         
        override this.PreRestart (reason:exn, msg:obj) = 
            printfn "Actor to be restarted because of %A" reason
        override this.PostRestart (reason:exn) = 
            printfn "Actor restarted because of %A" reason
                        
let system = System.create "system" <| Configuration.defaultConfig()

let myActor = system.ActorOf(Props(typedefof<MyActor>), "my-actor")

let monitorActor = spawn system "monitor" <| fun mailbox ->
            // monitor myActor
            monitor myActor mailbox.Context |> ignore
   
            let rec loop () = actor {          
                let! message = mailbox.Receive ()
                printfn "Monitor received %A" message

                return! loop ()
            }
            loop ()

// send some messages to myActor
for i in [1..5] do  
    myActor <! Message (sprintf "Message %d" i)

// tell him to crash!
myActor <! Crash

// send more messages to myActor.
// After myActor crashed, it restarted, so these messages
// are received by myActor
for i in [1..5] do 
    myActor <! Message (sprintf "Message %d" (i+5))

// Kill the actor by sending it a poison pill
myActor <! PoisonPill.Instance

// Send some more messages.
// These will not be received by myActor who has now stopped
// The messages will be sent to dead letter
for i in [1..5] do 
    let message =  Message (sprintf "Message %d" (i+10))
    myActor <! message