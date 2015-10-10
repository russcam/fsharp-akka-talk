namespace Common

module Actors =

    open System
    open Akka.FSharp
    open Akka.Actor
    open Akka.Remote
    open Akka.Configuration

    let someActor (mailbox:Actor<obj>) = 
        let rec loop () : Cont<obj,obj> = actor {
            let! message = mailbox.Receive ()

            let senderAddress = mailbox.Self.Path.ToStringWithAddress()
            
            // changing console colour is not thread safe, so for purposes
            // of demo, lock on the output
            lock Console.Out (fun () ->        
                let originalColor = Console.ForegroundColor

                Console.ForegroundColor <-
                    match mailbox.Self.Path.Parent.Name with
                    | "localactor" -> ConsoleColor.Red
                    | _ -> ConsoleColor.Yellow 

                Console.WriteLine (sprintf "%s got %A" senderAddress message)
                Console.ForegroundColor <- originalColor
            )

            return! loop()
        }
        loop()