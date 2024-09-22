#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
open Raylib_cs
open System.Numerics
open Helper

// This programs allows drawing lines that then can be splited with
// the Koch Fractal up to a certain limit. The order in which lines are
// drawn is important in which direction the line is split up.
//
// The line is split up to the left. So when you draw a line from left
// to right. think of it as looking from the starting point where you
// started drawing to the end point. Then the left side gets extended.

module Line =
    // Helper function
    let private normalize vec = Vector2.Normalize(vec)

    // Line.T
    type T = Line of start:Vector2 * stop:Vector2

    let create start stop      = Line (start,stop)
    let start (Line (start,_)) = start
    let stop  (Line (_,stop))  = stop
    let length line            = Vector2.Distance(start line, stop line)

    /// returns the midpoint of a line
    let midpoint (Line (start,stop)) =
        (vec2 ((start.X + stop.X) / 2f) ((start.Y + stop.Y) / 2f))

    /// turns a single line into 4 new lines
    let splitLine input =
        /// the center point is the new tip. it is calculated from the mid point and
        /// goes orthogonal up from the line
        let centerPoint line =
            let d = (stop line) - (start line)
            (midpoint line) + (normalize (vec2 d.Y -d.X)) * ((length line) / 3f)

        /// returns the left and right point where a line has to be splited
        let lrPoint line =
            let dir = (normalize ((stop line) - (start line)))
            let l   = dir * ((length line) / 3f)
            let r   = dir * ((length line) * (2f / 3f))
            (start line) + l, (start line) + r

        let s      = start input
        let e      = stop input
        let center = centerPoint input
        let l,r    = lrPoint input
        [(create s l); (create l center); (create center r); (create r e)]

let line = Line.create

// Lines to Draw
// Note: For Beginners. This creates an immutable List. List stays immutable.
//       adding `mutable` only makes the variable itself mutable. So we can
//       swap out one immutable list with another immutable list.
let mutable lines = []

// Again a DU. Basically works like a State Machine.
type MouseSelection =
    | NotStarted
    | Start  of Vector2
    | Drag   of Vector2 * Vector2
    | Finish of Vector2 * Vector2
let mutable selection = NotStarted

// Generetas a Koch Fractal Snowflake
Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint)
Raylib.InitWindow(800, 800, "Snowflake")
Rlgl.EnableSmoothLines()
Raylib.SetTargetFPS(60)

// Game Loop
while not <| CBool.op_Implicit (Raylib.WindowShouldClose()) do
    let mouse = getMouse None

    // Handle mouse state for drawing lines
    selection <-
        match selection, mouse.Left with
        | NotStarted,   Pressed  -> Start mouse.Position
        | NotStarted,   Down     -> Start mouse.Position
        | NotStarted,   Released -> NotStarted
        | NotStarted,   Up       -> NotStarted
        | Start _   ,   Pressed  -> Start mouse.Position
        | Start s,      Down     -> Drag (s,mouse.Position)
        | Start s,      Released -> Finish (s,mouse.Position)
        | Start s,      Up       -> Finish (s,mouse.Position)
        | Drag  (_,_),  Pressed  -> Start mouse.Position
        | Drag  (s,_),  Down     -> Drag (s,mouse.Position)
        | Drag  (s,_),  Released -> Finish (s,mouse.Position)
        | Drag  (s,_),  Up       -> Finish (s,mouse.Position)
        | Finish (_,_), Pressed  -> Start mouse.Position
        | Finish (_,_), Down     -> Start mouse.Position
        | Finish (_,_), Released -> NotStarted
        | Finish (_,_), Up       -> NotStarted

    Raylib.BeginDrawing()
    Raylib.ClearBackground(Color.Black)

    // Draws the lines
    for line in lines do
        Raylib.DrawLineEx((Line.start line), (Line.stop line), 1f, Color.Blue)

    // Draw Mouse Cursor Line
    match selection with
    | NotStarted  -> ()
    | Start _     -> ()
    | Drag (s,e)  -> Raylib.DrawLineEx(s, e, 1f, Color.RayWhite)
    | Finish(s,e) ->
        let line = line s e
        if Line.length line > 3f then
            lines <- line :: lines

    // Draw UI
    if guiButton (rect 250f 10f 100f 30f) "Split" then
        lines <- lines |> List.collect (fun line ->
            // only split lines into more segments if line is longer than 10px
            if Line.length line > 10f then Line.splitLine line else [line]
        )
    if guiButton (rect 400f 10f 100f 30f) "Clear" then
        lines <- []
    Raylib.DrawText((sprintf "Lines %d" (List.length lines)), 600, 10, 24, Color.Yellow)
    Raylib.DrawFPS(0,0)

    Raylib.EndDrawing()

Raylib.CloseWindow ()

