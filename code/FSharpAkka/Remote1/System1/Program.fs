open System
open Akka
open System.Linq
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
                                remote = "akka.tcp://system2@localhost:8080"
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
    let local = system.ActorOf<SomeActor>("localactor")
    let remote = system.ActorOf<SomeActor>("remoteactor")

    Console.WriteLine("Press Enter to send messages to the Local Actor")
    Console.ReadLine() |> ignore 

    local <! "Local message 1"
    local <! "Local message 2"
    local <! "Local message 3"
    local <! "Local message 4"
    local <! "Local message 5"

    local <! "Local message 6"
    local <! "Local message 7"
    local <! "Local message 8"
    local <! "Local message 9"
    local <! "Local message 10"

    Console.WriteLine("Press Enter to send messages to the Remote Actor")
    Console.ReadLine() |> ignore 

    remote <! "Remote message 1"
    remote <! "Remote message 2"
    remote <! "Remote message 3"
    remote <! "Remote message 4"
    remote <! "Remote message 5"

    remote <! "Remote message 6"
    remote <! "Remote message 7"
    remote <! "Remote message 8"
    remote <! "Remote message 9"
    remote <! "Remote message 10"

    Console.ReadLine () |> ignore 
    system.Shutdown ()
    0
