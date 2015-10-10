#load "Bootstrapper.fsx"

open System
open System.Text.RegularExpressions
open System.Collections.Generic
open System.Threading
open Akka.Actor
open Akka.Configuration
open Akka.FSharp

// Spawn an actor by wrapping a recursive function 
// with an actor computation expression. 

let system = System.create "system" <| Configuration.defaultConfig()

let splitIntoWords sentence =
    Regex.Matches(sentence, @"[\w#!]+")
    |> Seq.cast
    |> Seq.map (fun (m:Match) -> m.Value)

let incrementCounts (dict:Dictionary<string,int>) (word:string) =
    match dict.TryGetValue word with
    | true, v -> dict.[word] <- v + 1
    | false, _ -> dict.[word] <- 1

let wordCounter  =
    spawn system "counter"
        (fun mailbox ->
            let rec loop (dict:Dictionary<string,int>) = actor {
                let! message = mailbox.Receive ()
                printfn "Received '%s'" message
                              
                message
                |> splitIntoWords
                |> Seq.iter (incrementCounts dict)

                dict
                |> Seq.iter (fun kv -> 
                        let k, v = kv.Key, kv.Value
                        match v with
                        | 1 -> printfn "Passed '%s' %i time" k v
                        | _ -> printfn "Passed '%s' %i times" k v
                    )

                return! loop (dict)
            }
            loop (Dictionary<string,int>(StringComparer.InvariantCultureIgnoreCase))
        )

wordCounter <! "Hello F# Sydney!"
wordCounter <! "F# is cool"
wordCounter <! "So Is Sydney!"
