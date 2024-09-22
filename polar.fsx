#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
open Raylib_cs
open Helper
open System.Numerics

let screenWidth, screenHeight = 1200, 800

[<Struct>]
type Polar = {
    Angle:    float32
    Distance: float32
}

/// angle is in degree
let polar angle distance = {
    Angle    = angle
    Distance = distance
}

let toCartesian polar =
    vec2
        (cosd polar.Angle * polar.Distance)
        (sind polar.Angle * polar.Distance)

let mutable drawPixel = true

rl.InitWindow(screenWidth, screenHeight, "Polar Coordinates")
Rlgl.EnableSmoothLines()
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt = rl.GetFrameTime()

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    // Draw Grid
    for x in 0 .. 50 .. screenWidth do
        rl.DrawLine(x, 0, x, screenHeight, Color.RayWhite)
    for y in 0 .. 50 .. screenHeight do
        rl.DrawLine(0, y, screenWidth, y, Color.RayWhite)
    let center = vec2 (float32 screenWidth / 2f) (float32 screenHeight / 2f)
    rl.DrawLine(0, int center.Y, screenWidth, int center.Y, Color.Red)
    rl.DrawLine(int center.X, 0, int center.X, screenHeight, Color.Green)
    rl.DrawCircleV(center, 5f, Color.Red)

    // Draw Points
    let mutable previous = center
    let drawLine polar   =
        let pos = center + (toCartesian polar)
        rl.DrawLineV(previous, pos, Color.Yellow)
        previous <- pos

    let drawPoint polar =
        rl.DrawPixelV(center + toCartesian polar, Color.Yellow)

    let mutable angle    = 0f
    let mutable distance = 0f
    let drawFunc         = if drawPixel then drawPoint else drawLine
    for i=1 to 2_000 do
        drawFunc (polar angle distance)
        angle    <- angle + 22.34f
        distance <- wrap 0f 600f (distance + 22.5f)

    // Draw UI
    rl.DrawFPS(0,0)
    if guiButton (rect 100f 10f 150f 30f) (if drawPixel then "Draw Lines" else "Draw Pixel") then
        drawPixel <- not drawPixel

    rl.EndDrawing ()

rl.CloseWindow()
