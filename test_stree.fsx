#!/usr/bin/env -S dotnet fsi
#load "../Lib/Test.fsx"
#load "Lib/SpatialTree.fsx"
open System.Numerics
open SpatialTree
open Test

type GO = { Name: string }
let go name = { Name = name }
let vec2 x y = Vector2(x,y)

/// checks content of an chunk
let chunkIs name (chunk:voption<ResizeArray<_>>) data =
    match chunk with
    | ValueNone       -> Test.fail "No chunk"
    | ValueSome chunk ->
        let data = Seq.toArray data
        if chunk.Count = data.Length then
            let mutable pass = true
            for idx=0 to chunk.Count-1 do
                if chunk.[idx] <> data.[idx] then
                    pass <- false
                    printfn "# Mismatch on %d. Expected: %A Got: %A" idx data.[idx] chunk.[idx]
            if pass
            then Test.pass name
            else Test.fail name
        else
            Test.fail (sprintf "Chunk has not correct Size. Expected: %d Got: %d" data.Length chunk.Count)

let tree = STree.create 32

let data1 = [
    (vec2 10f   10f), (go "Foo");
    (vec2 100f  10f), (go "Bar");
    (vec2 100f 100f), (go "Baz")
]
data1 |> List.iter (fun (v,p) -> STree.add v p tree)

chunkIs "Chunk(0,0) contains foo" (STree.get (vec2  5f  5f) tree) [data1.[0]]
chunkIs "Chunk(3,0) contains bar" (STree.get (vec2 97f  1f) tree) [data1.[1]]
chunkIs "Chunk(3,3) contains baz" (STree.get (vec2 97f 97f) tree) [data1.[2]]

Test.is (STree.length tree) 3 "Tree has 3 elements"

// Generate 32 position,go entries
let data2 = [
    for x=1 to 32 do
        let v = (vec2 (float32 x+16f) (float32 x+16f))
        let g = (go (sprintf "gen %d" x))
        v,g
]

data2 |> List.iter (fun (p,g) -> STree.add p g tree)
Test.is (STree.length tree) 35 "Tree has 35 elements"

chunkIs "Chunk(0,0) contains foo and first 15 from data2"
    (STree.get (vec2 1f 1f) tree) ([data1.[0]] @ data2.[0..14])

chunkIs "Chunk(1,1) contains 15..31 from data2 "
    (STree.get (vec2 32f 32f) tree) (data2.[15..31])
