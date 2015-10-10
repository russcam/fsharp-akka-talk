open System
open System.Linq
open Akka
open Akka.FSharp
open Akka.Actor
open Akka.Remote
open Akka.Configuration
open Akka.Routing
open Common

[<EntryPoint>]
let main argv = 
    Console.Title <- "System 1"   
    let config = Configuration.parse """
                akka {  
                    log-config-on-start = on
                    stdout-loglevel = DEBUG
                    loglevel = ERROR
                    actor {
                        provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"

                        deployment {
                            /localactor {
                                router = round-robin-pool
                                nr-of-instances = 5
                            }
                            /remoteactor {
                                router = round-robin-pool
                                nr-of-instances = 5
                            }  
                            /remoteactor2 {
                                router = round-robin-pool
                                nr-of-instances = 5
                            }
                        }
                    }
                    remote {
                        helios.tcp {
                            transport-class = "Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote"
		                    applied-adapters = []
		                    transport-protocol = tcp
		                    port = 8090
		                    hostname = localhost
                        }
                    }
                }"""

    let system = System.create "system1" <| config

    let localActor (mailbox:Actor<obj>) = 
        let rec loop () : Cont<obj,obj> = actor {
            let! message = mailbox.Receive ()

            let senderAddress = mailbox.Self.Path.ToStringWithAddress()
    
            lock Console.Out (fun () ->
                let originalColor = Console.ForegroundColor

                Console.ForegroundColor <-
                    match mailbox.Self.Path.Parent.Name with
                    | "localactor" -> ConsoleColor.Red
                    | _ -> ConsoleColor.Green 

                Console.WriteLine (sprintf "%s got %A" senderAddress message)
                Console.ForegroundColor <- originalColor
            )

            return! loop()
        }
        loop() 

    let local =  spawnOpt system "localactor" localActor [SpawnOption.Router(FromConfig.Instance)]

    Console.WriteLine("Press Enter to send messages to the Local Actor")
    Console.ReadLine() |> ignore 

    for i in [1..10] do
        local <! sprintf "Local message %i" i

    TimeSpan.FromSeconds 2.
    |> System.Threading.Thread.Sleep  
    |> ignore

    Console.WriteLine("Deploy remote actor using F# Quotation")

    let deployRemotely address = Deploy(RemoteScope (Address.Parse address))  

    // remote deploy actor using F# Quotation
    let remote = spawne system "remoteactor" (<@ fun (mailbox:Actor<obj>) -> 
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
                    | _ -> ConsoleColor.Green 

                Console.WriteLine (sprintf "%s got %A" senderAddress message)
                Console.ForegroundColor <- originalColor
            )

            return! loop()
        }
        loop() @>) [ SpawnOption.Router(FromConfig.Instance); 
                     SpawnOption.Deploy(deployRemotely "akka.tcp://system2@localhost:8080")]

    Console.WriteLine("Press Enter to send messages to the Remote Actor")
    Console.ReadLine() |> ignore

    for i in [1..10] do
        remote <! sprintf "Remote message %i" i

    TimeSpan.FromSeconds 2.
    |> System.Threading.Thread.Sleep  
    |> ignore

    Console.WriteLine("Deploy remote actor using F# Quotation from a shared assembly")

    // remote deploy actor using F# Quotation in a shared assembly
    let remote2 = spawne system "remoteactor2" 
                    (<@ Actors.someActor @>) 
                    [ SpawnOption.Router(FromConfig.Instance); 
                      SpawnOption.Deploy(deployRemotely "akka.tcp://system2@localhost:8080")]

    Console.WriteLine("Press Enter to send messages to the Remote Actor 2")
    Console.ReadLine() |> ignore 

    for i in [1..10] do
        remote2 <! sprintf "Remote message %i" i

    Console.ReadLine () |> ignore 
    system.Shutdown ()
    0
