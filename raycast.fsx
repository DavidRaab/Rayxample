#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
open Raylib_cs
open Helper
open System.Numerics

// Coding Challenge from Coding Train : https://www.youtube.com/watch?v=TOEi6T2mtHo

type Line = {
    Start: Vector2
    End:   Vector2
}
module Line =
    let create sx sy ex ey = {
        Start = Vector2(sx,sy)
        End   = Vector2(ex,ey)
    }

    let draw thickness color line =
        Raylib.DrawLineEx(line.Start, line.End, thickness, color)

type Ray = {
    Position:  Vector2
    Direction: Vector2
}
module Ray =
    let fromLine line = {
        Position  = line.Start
        Direction = Vector2.Normalize((line.End - line.Start))
    }

    let draw length ray =
        Raylib.DrawLineEx(ray.Position, (ray.Position+ray.Direction*length), 1f, Color.RayWhite)

    let intersectsLine ray line =
        let x1,y1,x2,y2 = line.Start.X, line.Start.Y, line.End.X, line.End.Y
        let x3,y3       = ray.Position.X, ray.Position.Y
        let x4,y4       = ray.Position.X + ray.Direction.X, ray.Position.Y + ray.Direction.Y

        let den  = ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4))
        if den = 0f then ValueNone
        else
            let t =   ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den
            let u = -(((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den)

            if t >= 0.0f && t <= 1.0f && u >= 0.0f then
                ValueSome (Vector2(
                    (x1 + t * (x2 - x1)),
                    (y1 + t * (y2 - y1))
                ))
            else
                ValueNone

let rng = System.Random ()
let generateWorld amount = [
    let num scale = rng.NextSingle() * scale
    let scale = 800f
    for i=1 to amount do
        yield Line.create (num scale) (num scale) (num scale) (num scale)
]

// Game State
let mutable sun                  = Vector2(400f,400f)
let mutable drawRaycasts         = true
let mutable rayToMouse           = false
let mutable showAllIntersections = false
let mutable lines                = generateWorld 5

// unfuck it so it doesnt throw exceptions and is usuable
let unfuckMinBy f xs =
    match xs with
    | [] -> ValueNone
    | xs -> ValueSome (List.minBy f xs)

Raylib.InitWindow(800, 800, "Raycast")
Raylib.SetTargetFPS(60)
while not <| CBool.op_Implicit (Raylib.WindowShouldClose()) do
    let mouse = getMouse None

    Raylib.BeginDrawing ()
    Raylib.ClearBackground(Color.Black)

    // Draw World
    List.iter (Line.draw 1f Color.DarkBlue) lines

    // Set and draw sun
    if mouse.Right = Down then
        sun <- mouse.Position
    Raylib.DrawCircleV(sun, 10f, Color.Yellow)

    // Create and show Raycast
    let rays =
        if rayToMouse then [
            { Position = sun; Direction = Vector2.Normalize(mouse.Position-sun) }
        ]
        else [
            for i=0 to 359 do
                let x = float32 i * System.MathF.PI / 180f
                yield { Position = sun; Direction = Vector2(cos x, sin x) }
        ]

    // Show intersections
    for ray in rays do
        if showAllIntersections then
            for line in lines do
                match Ray.intersectsLine ray line with
                | ValueNone       -> ()
                | ValueSome point ->
                    Raylib.DrawCircleV(point, 4f, Color.Yellow)
                    match drawRaycasts, showAllIntersections with
                    | true, true  -> Ray.draw 400f ray
                    | true, false -> Line.draw 1f Color.RayWhite { Start = sun; End = point }
                    | false, _    -> ()
        else
            let nearest =
                List.map (Ray.intersectsLine ray) lines
                |> List.collect ValueOption.toList
                |> unfuckMinBy (fun p -> Vector2.DistanceSquared(sun,p))
            match nearest with
            | ValueNone         -> ()
            | ValueSome nearest ->
                Raylib.DrawCircleV(nearest, 4f, Color.Yellow)
                match drawRaycasts, showAllIntersections with
                | true, true  -> Ray.draw 400f ray
                | true, false -> Line.draw 1f Color.RayWhite { Start = sun; End = nearest }
                | false, _    -> ()

    // Draw UI
    Raylib.DrawText("Right-Click changes position of Sun", 10, 760, 24, Color.Yellow)
    if guiButton (Rectangle(10f, 10f, 120f, 30f)) (if rayToMouse then "Many" else "To Mouse") then
        rayToMouse <- not rayToMouse
    if guiButton (Rectangle(140f, 10f, 140f, 30f)) (if drawRaycasts then "Hide Rays" else "Show Rays" ) then
        drawRaycasts <- not drawRaycasts
    if guiButton (Rectangle(300f, 10f, 200f, 30f)) "New World" then
        lines <- generateWorld 5
    if guiButton (Rectangle(650f, 10f, 120f, 30f)) (if showAllIntersections then "Nearest" else "All") then
        showAllIntersections <- not showAllIntersections

    Raylib.EndDrawing ()

Raylib.CloseWindow()
