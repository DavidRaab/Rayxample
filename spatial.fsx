#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
#load "Lib/SpatialTree.fsx"
open Raylib_cs
open Helper
open SpatialTree
open System.Numerics

type Point = {
    mutable Pos: Vector2
    Radius: float32
}

let width, height = 1200, 800
let pointSize = 10f
let points = ResizeArray<_>([
    for i=1 to 10 do
        { Pos = vec2 (randf -600f 600f) (randf -400f 400f); Radius = pointSize }
])

// Allows dragging points
let mutable drag = NoDrag

// Camera with 0,0 in screen center
let mutable camera = Camera2D()
camera.Offset   <- vec2 (float32 width / 2f) (float32 height / 2f)
camera.Target   <- vec2 0f 0f
camera.Rotation <- 0f
camera.Zoom     <- 1f

let drawScreenGrid size width height color =
    for x in 0 .. size .. width  do rl.DrawLine(x, 0, x, height, color)
    for y in 0 .. size .. height do rl.DrawLine(0, y, width, y, color)

rl.InitWindow(width, height, "Hello, World!")
rl.SetMouseCursor(MouseCursor.Crosshair)
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse (Some camera)

    if mouse.Right = Pressed then
        points.Add({Pos = (worldPosition mouse); Radius = pointSize })

    // Process Drageable
    drag <- processDrag drag points (fun p -> Circle (p.Pos,p.Radius)) mouse
    match drag with
    | StartDrag (point,off)
    | InDrag    (point,off) -> point.Pos <- worldPosition mouse
    | _ -> ()

    // Populate Spartial Tree
    let spatialSize = 64
    let tree = STree.fromSeq spatialSize (seq {
        for point in points do
            point.Pos, point
    })

    rl.BeginDrawing()

    // Draw World
    rl.BeginMode2D(camera)
    rl.ClearBackground(Color.Black)

    for point in points do
        rl.DrawCircleV(point.Pos, point.Radius, Color.Red)

    match drag with
    | Hover point ->
        rl.DrawCircleLinesV(point.Pos, point.Radius, Color.RayWhite)
        STree.getRec tree point.Pos point.Radius point.Radius (fun other ->
            rl.DrawLineV(point.Pos, other.Pos, Color.Yellow)
        )
    | NoDrag ->
        // Draw a line to all points in the chunk
        match STree.get (worldPosition mouse) tree with
        | ValueNone       -> ()
        | ValueSome chunk ->
            for point in chunk do
                rl.DrawLineV(worldPosition mouse, point.Pos, Color.Green)
    | _ -> ()

    // Draw chunks that are populated
    for (x,y,w,h) in STree.getChunkRegions tree do
        rl.DrawRectangleLines(x,y,w,h, Color.Gray)

    rl.EndMode2D()

    // Draw UI
    rl.DrawLine(0,400, 1200,400, Color.Red)
    rl.DrawLine(600,0, 600,800,  Color.Green)
    rl.DrawText(sprintf "Chunk %A" (STree.calcPos (worldPosition mouse) tree), 300, 770, 24, Color.Yellow)
    onHover drag (fun point -> rl.DrawText(sprintf "Point {%.2f,%.2f}" point.Pos.X point.Pos.Y, 10, 770, 24, Color.Yellow))
    rl.DrawFPS(0,0)

    rl.EndDrawing()

rl.CloseWindow()
