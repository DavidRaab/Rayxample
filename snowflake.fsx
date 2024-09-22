#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
open Raylib_cs
open System.Numerics

let vec2 x y = Vector2(x,y)
type Line = Line of start:Vector2 * stop:Vector2

let start (Line (start,_)) = start
let stop  (Line (_,stop))  = stop
let line start stop = Line (start,stop)

let normalize vec = Vector2.Normalize(vec)
let length line   = Vector2.Distance(start line, stop line)

/// returns the midpoint of a line
let midpoint (Line (start,stop)) =
    (vec2 ((start.X + stop.X) / 2f) ((start.Y + stop.Y) / 2f))

/// the center point is the new tip. it is calculated from the mid point and
/// goes orthogonal up from the line
let centerPoint line =
    let d = (stop line) - (start line)
    (midpoint line) + (normalize (vec2 d.Y -d.X)) * ((length line) / 3f)

/// returns the left and right point where a line has to be splited
let lrPoint line =
    let dir = (normalize ((stop line) - (start line)))
    let l   = dir * ((length line) / 3f)
    let r   = dir * ((length line) / 3f * 2f)
    (start line) + l,(start line) + r

/// turns a single line into 4 new lines
let splitLine input =
    let s      = start input
    let e      = stop input
    let center = centerPoint input
    let l,r    = lrPoint input
    [(line s l); (line l center); (line center r); (line r e)]

// Lines to Draw
let steps = ResizeArray<_>()

// Start with a diamond
steps.Add([
    line (vec2 100f 400f) (vec2 400f 100f)
    line (vec2 400f 100f) (vec2 700f 400f)
    line (vec2 700f 400f) (vec2 400f 700f)
    line (vec2 400f 700f) (vec2 100f 400f)
])

// add 6 recursions
for i=1 to 6 do
    let last = steps.[ steps.Count-1 ]
    steps.Add(List.collect splitLine last)


// Generetas a Koch Fractal Snowflake
Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint)
Raylib.InitWindow(800, 800, "Snowflake")
Rlgl.EnableSmoothLines()
Raylib.SetTargetFPS(60)

let mutable elapsed  = 0f
let mutable showStep = 0
while not <| CBool.op_Implicit (Raylib.WindowShouldClose()) do
    // switch every second to new step
    elapsed <- elapsed + Raylib.GetFrameTime()
    if elapsed >= 1f then
        elapsed <- elapsed - 1f
        showStep <- showStep + 1
        if showStep > steps.Count-1 then showStep <- 0

    Raylib.BeginDrawing()
    Raylib.ClearBackground(Color.Black)

    Raylib.DrawText((sprintf "Step: %d" (showStep+1)), 10, 10, 24, Color.Yellow)
    for line in steps.[showStep] do
        Raylib.DrawLineEx((start line), (stop line), 1f, Color.Blue)

    Raylib.EndDrawing()

Raylib.CloseWindow ()

