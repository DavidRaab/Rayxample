#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
open Raylib_cs
open Helper

let width, height = 1200, 800

// Camera with 0,0 in screen center
let mutable camera = Camera2D()
camera.Offset   <- vec2 (float32 width / 2f) (float32 height / 2f)
camera.Target   <- vec2 0f 0f
camera.Rotation <- 0f
camera.Zoom     <- 1f

rl.InitWindow(width, height, "Sine Wave")
rl.SetMouseCursor(MouseCursor.Crosshair)
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    rl.BeginDrawing()

    rl.BeginMode2D(camera)
    rl.ClearBackground(Color.Black)

    let scaling = 100f
    for x = -600 to 600 do
        let nx = (float32 x) / scaling
        let  y = (sin nx)    * scaling
        rl.DrawPixel(int x, int y, Color.RayWhite)

    rl.EndMode2D()

    // Draw XY Grid
    rl.DrawLine(0,400, 1200,400, Color.Red)
    rl.DrawLine(600,0, 600,800,  Color.Green)
    rl.DrawFPS(0,0)

    rl.EndDrawing()

rl.CloseWindow()
