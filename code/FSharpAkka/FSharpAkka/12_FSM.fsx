#load "Bootstrapper.fsx"

open System
open System.Threading
open Akka
open Akka.Actor
open Akka.Routing
open Akka.Configuration
open Akka.FSharp

// Demonstrate Finite State Machine with mutually recursive functions.
// Simplified version of https://github.com/rikace/AkkaActorModel/blob/master/src/1.%20Mailbox%20Processor/11.StateMachine.fsx

type ClimateControl =
    | HeatUp
    | CoolDown
    | GetStatus of string

let system = System.create "temperature-control" <| Configuration.defaultConfig()

let actor = 
    spawn system "MyActor"
    <| fun mailbox ->
        let rec heat() = actor {
                let! message = mailbox.Receive()
                
                match message with
                | CoolDown -> 
                    printfn "Cooling down"
                    return! normal()
                | HeatUp ->
                    printfn "It's already hot!"
                    return! heat()
                | GetStatus(s) ->
                    printfn "Hey %s - It's hot!" s
                    return! heat()
            }
        and cool() = actor {
                let! message = mailbox.Receive()
                
                match message with
                | CoolDown -> 
                    printfn "It's already cold!"
                    return! cool()
                | HeatUp ->
                    printfn "Heating up!"
                    return! normal()
                | GetStatus(s) ->
                    printfn "Hey %s - It's cold!" s
                    return! cool()
            }
        and normal() = actor {
                let! message = mailbox.Receive()
                
                match message with
                | CoolDown -> 
                    printfn "Cooling down!"
                    return! cool()
                | HeatUp ->
                    printfn "Heating up!"
                    return! heat()
                | GetStatus(s) ->
                    printfn "Hey %s - It's nice!" s
                    return! normal()
            }
        normal()

actor <! GetStatus("Guys")
actor <! HeatUp
actor <! GetStatus("Guys")
actor <! HeatUp
actor <! CoolDown
actor <! GetStatus("Guys")
actor <! CoolDown
actor <! GetStatus("Guys")
actor <! CoolDown
