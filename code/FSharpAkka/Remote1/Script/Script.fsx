// After compiling the projects, use this script to launch the console applications

let start (filePath:string) =
    System.Diagnostics.Process.Start(filePath) |> ignore

let system2 = __SOURCE_DIRECTORY__ + "/../System2/bin/Debug/System2.exe"
let system1 = __SOURCE_DIRECTORY__ + "/../System1/bin/Debug/System1.exe"

system2 |> start
system1 |> start


