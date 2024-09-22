#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
open Raylib_cs
open Helper
open System.Numerics

let screenWidth, screenHeight = 1200, 800
let pointSize = 5f
let poly = [
    vec2 -100f -100f
    vec2  -50f  -80f
    vec2  -80f -200f
    vec2    0f  -80f
    vec2  100f -150f
    vec2  150f    0f
    vec2  100f  200f
    vec2  -30f  150f
    vec2  -50f  200f
    vec2  -80f  150f
    vec2 -100f  150f
    vec2 -130f  180f
    vec2 -180f    0f
]

/// Like Array.item but allows negative indexing
let arrayItem idx (array:'a array) =
    if idx < 0
    then array.[array.Length + idx]
    else array.[idx]

let drawPoly points =
    let points = Array.ofSeq points
    for j=0 to points.Length-1 do
        let i = j - 1
        rl.DrawLineV(arrayItem i points, arrayItem j points, Color.Red)

// How the alogorithm works:
//  The idea is to cast a ray from the point we want to check. We can pick any direction
//  here the direction is just right. Then we need to check with how many lines the ray will
//  intersect. If the intersection count is uneven than the point must be inside a concave
//  mesh, when the intersection count is even than it is outside a concave mesh.
//
// 1. we get two points start and stop and this constructs a line.
// 2. we check if height is inside the height of the line.
// 3. when point is inside the height we need to calculate the x position on the line
// 4. when the real x point is smaller than x intersection point, than the ray would hit the line
// 5. whenever an intersection happens we flip the boolean `inside`.
//    It is also possible to just count the amount of intersections and check if number is uneven.
let insidePoly (point:Vector2) (poly:seq<Vector2>) =
    let poly  = Array.ofSeq poly
    let (x,y) = point.X, point.Y

    let mutable inside = false
    for i=0 to poly.Length-2 do
        let start = Array.item  i    poly
        let stop  = Array.item (i+1) poly

        let insideHeight = (y < start.Y && y > stop.Y) || (y < stop.Y && y > start.Y)
        if insideHeight then
            let diffX       = start.X - stop.X
            let diffY       = start.Y - stop.Y
            let n           = diffX / diffY     // x movement per 1 y unit
            let h           = y - start.Y       // height of y relative to start
            let x_collision = start.X + (h * n) // x point for y value on line start to stop
            if x < x_collision then
                inside <- not inside

    // Explicitly check again line from first to last point
    let start = Array.item  0              poly
    let stop  = Array.item (poly.Length-1) poly

    let insideHeight = (y < start.Y && y > stop.Y) || (y < stop.Y && y > start.Y)
    if insideHeight then
        let diffX       = start.X - stop.X
        let diffY       = start.Y - stop.Y
        let n           = diffX / diffY     // x movement per 1 y unit
        let h           = y - start.Y       // height of y relative to start
        let x_collision = start.X + (h * n) // x point for y value on line start to stop
        if x < x_collision then
            inside <- not inside

    inside

let drawPoints points =
    for point in points do
        let color = if insidePoly point poly then Color.Red else Color.Yellow
        rl.DrawCircleV(point, pointSize, color)

let points = ResizeArray([
    vec2 -200f -100f
    vec2 -100f -200f
])
let mutable drag = NoDrag

rl.InitWindow(screenWidth, screenHeight, "Hello, World!")
rl.SetTargetFPS(60)

let mutable camera = Camera2D()
camera.Offset   <- vec2 (float32 screenWidth/2f) (float32 screenHeight /2f)
camera.Target   <- vec2 0f 0f
camera.Zoom     <- 1f
camera.Rotation <- 0f

while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse (Some camera)

    rl.BeginDrawing()
    rl.ClearBackground(Color.Black)
    rl.BeginMode2D(camera)

    drag <- processDrag drag points (fun p -> Circle(p,pointSize)) mouse
    match drag with
    | NoDrag ->
        if mouse.Left = Pressed then
            points.Add (worldPosition mouse)
    | Hover point ->
        rl.DrawCircleLinesV(point, pointSize, Color.RayWhite)
        if mouse.Right = Pressed then
            points.Remove point |> ignore
    | _ -> ()

    drawPoly   poly
    drawPoints points

    rl.EndMode2D()
    rl.DrawFPS(0,0)
    rl.EndDrawing()

rl.CloseWindow()
