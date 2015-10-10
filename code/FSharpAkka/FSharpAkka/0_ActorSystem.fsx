#load "Bootstrapper.fsx"

open Akka.FSharp

// Create an actor system, using a default akka configuration
let system = System.create "system" <| Configuration.defaultConfig()
