#load "Bootstrapper.fsx"

open System
open System.Threading
open Akka
open Akka.Actor
open Akka.Routing
open Akka.Configuration
open Akka.FSharp

// Subscribe actor to the event stream for particular type of message.
// Publish messages of that type to the event stream.
// Unsubscribe actor - no longer receives published messages of the type.

let system = System.create "system" <| Configuration.defaultConfig()

type Message =
    | Subscribe
    | Unsubscribe
    | Msg of IActorRef * string

let subscriber =
    spawn system "subscriber" 
        (actorOf2 (fun mailbox msg ->
                let eventStream = mailbox.Context.System.EventStream
                match msg with
                | Msg (sender, content) -> 
                    printfn "%A says %s" (sender.Path) content
                | Subscribe -> subscribe typeof<Message> mailbox.Self eventStream |> ignore
                | Unsubscribe -> unsubscribe typeof<Message> mailbox.Self eventStream |> ignore 
            )
        )

let publisher =
    spawn system "publisher"
        (actorOf2 (fun mailbox msg -> publish msg mailbox.Context.System.EventStream))

subscriber <! Subscribe

// introduce artificial delay to allow subscription
// to take place 
TimeSpan.FromSeconds 1. |> Thread.Sleep

publisher  <! Msg (publisher, "hello")
subscriber <! Unsubscribe

// introduce artificial delay to allow unsubscription
// to take place 
TimeSpan.FromSeconds 1. |> Thread.Sleep

publisher  <! Msg (publisher, "hello again")

