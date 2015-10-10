namespace Common

open System
open Akka.FSharp
open Akka.Actor
open Akka.Remote
open Akka.Configuration

type SomeActor() =
     inherit Actor()

     override x.OnReceive message =
        let senderAddress = base.Self.Path.ToStringWithAddress()
        let path = base.Self.Path.Name

        // changing console colour is not thread safe, so for purposes
        // of demo, lock on the output
        lock Console.Out (fun () ->
            let originalColor = Console.ForegroundColor

            Console.ForegroundColor <-
                match path with
                | "localactor" -> ConsoleColor.Red
                | _ -> ConsoleColor.Green 

            printfn "%s got %A" senderAddress message
            Console.ForegroundColor <- originalColor
        )

