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

type Result =
    | Result of Operation * float * int

// Run an async workflow inside of an actor and pipe the result 
// back to the original sender
    
let system = ActorSystem.Create "system" 

let calculator (mailbox:Actor<Operation>) =
    let random = Random()

    let rec loop () = actor {
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()

        // introduce a random delay up to 5 seconds
        let sleepyTime = (random.NextDouble() * 5000.) |> int

        Console.WriteLine((sprintf "received %A" message))

        match message with
        | Add(num1, num2) ->                     
            async {
                do! Async.Sleep(sleepyTime)
                return Result(message, num1 + num2, sleepyTime)
            } |!> sender
        | Subtract(num1, num2) ->
            async {
                do! Async.Sleep(sleepyTime)
                return Result(message, num1 - num2, sleepyTime)
            } |!> sender
        | Divide(num1, num2) ->
            async {
                do! Async.Sleep(sleepyTime)
                return Result(message, num1 / num2, sleepyTime)
            } |!> sender
        | Multiply(num1, num2) as multi -> 
            async {
                do! Async.Sleep(sleepyTime)
                return Result(message, num1 * num2, sleepyTime)
            } |!> sender
        | Exp(num1, num2) -> 
            async {
                do! Async.Sleep(sleepyTime)
                return Result(message, (num1 ** num2), sleepyTime)
            } |!> sender

        return! loop ()
    }
    loop ()

let calculate calculatorActor (mailbox:Actor<obj>) =
    let rec loop () = actor {
        let! (message:obj) = mailbox.Receive ()

        match message with
        | :? Operation as operation ->
            calculatorActor <! operation
        | :? Result as result ->
            let (Result (o, r, s)) = result
            printfn "%A (slept for %i milliseconds). result of %A: %.2f" mailbox.Self.Path s o r
        | _ -> mailbox.Unhandled message
                
        return! loop ()
    }
    loop ()            

let calculatorActor = spawn system "calculator" calculator
let calculateActor1 = spawn system "calculate1" <| calculate calculatorActor
let calculateActor2 = spawn system "calculate2" <| calculate calculatorActor

// send a few messages using ActorRefs
calculateActor1 <! Add (10., 8.)
calculateActor2 <! Subtract (15., 8.)
calculateActor1 <! Multiply (2., 4.)
calculateActor2 <! Divide (2., 4.)
calculateActor1 <! Exp(2., 5.)
calculateActor2 <! Exp(2., 16.)

// send a message with ActorSelection
// This will send the message to all Actors selected with ActorSelection.
// In this case, this will be calculateActor1 and calculateActor2
select "/user/calculate*" system <! Exp(2., 16.)