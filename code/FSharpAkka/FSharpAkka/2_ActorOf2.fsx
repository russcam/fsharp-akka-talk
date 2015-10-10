#load "Bootstrapper.fsx"

open System
open System.Threading
open Akka.Actor
open Akka.Configuration
open Akka.FSharp

// Spawn actors using actorOf2. This wraps the passed function
// in an actor computation expression and invokes it, passing the 
// the message received from the actor mailbox, as well as the actor

let system = System.create "system" <| Configuration.defaultConfig()

let echo repeater (mailbox:Actor<_>) message =
    printfn "%A %s" mailbox.Self.Path message
    let cancelable = 
         mailbox.Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(500, 1000, repeater, message, mailbox.Self)
    cancelable.CancelAfter(TimeSpan.FromSeconds 10.)

let repeater (mailbox:Actor<_>) message =
    printfn "%A - %s from repeater!" mailbox.Self.Path message

let repeaterActor = spawn system "repeater" (actorOf2 repeater)
let echoActor = spawn system "echo" (actorOf2 (echo repeaterActor))

echoActor <! "Hello F# Sydney!"

echoActor <! "I say, hello again!"
