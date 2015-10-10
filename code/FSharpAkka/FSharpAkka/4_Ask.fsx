#load "Bootstrapper.fsx"

open System
open System.Threading
open Akka.Actor
open Akka.Configuration
open Akka.FSharp


type Operation =
    | Add of float * float
    | Subtract of float * float
    | Divide of float * float
    | Multiply of float * float
    | Exp of float * float
    
let system = System.create "system"  <| Configuration.defaultConfig()

let calculatorActor = 
    spawn system "calculator" 
        <| fun mailbox ->
            let rec loop () = actor {
                
                let! message = mailbox.Receive ()

                match message with
                | Add(num1, num2) -> 
                    mailbox.Sender() <! num1 + num2
                | Subtract(num1, num2) ->
                    mailbox.Sender() <! num1 - num2
                | Divide(num1, num2) ->
                    mailbox.Sender() <! num1 / num2
                | Multiply(num1, num2) -> 
                    mailbox.Sender() <! num1 * num2
                | Exp(num1, num2) -> 
                    mailbox.Sender() <! num1 ** num2

                return! loop ()
            }
            loop ()

// Send some messages to calculatorActor using Ask
async {
    let! addResponse = calculatorActor <? Operation.Add (10., 8.)
    printfn "%f" addResponse

    let! subtractResponse = calculatorActor <? Operation.Subtract (15., 8.)
    printfn "%f" subtractResponse

    let! multiplyResponse = calculatorActor <? Operation.Multiply (2., 4.)
    printfn "%f" multiplyResponse 

    let! divideResponse = calculatorActor <? Operation.Divide (2., 4.)
    printfn "%f" divideResponse 

    let! expResponse = calculatorActor <? Operation.Exp(2., 5.)
    printfn "%f" expResponse 
} |> Async.RunSynchronously