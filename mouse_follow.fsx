#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
open Raylib_cs
open Helper
open System.Numerics

let mutable current  = Vector2(400f, 400f)
let mutable previous = current
let mutable target   = Vector2(400f, 400f)

let width, height = 1200, 900
rl.InitWindow(width, height, "Mouse Follow")

// When circle moves it draws something like "footsteps" onto a render texture
let rt  = rl.LoadRenderTexture(width, height)
let src = Rectangle(0f, 0f, float32 rt.Texture.Width, float32 -rt.Texture.Height)

rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt = rl.GetFrameTime()
    target <- rl.GetMousePosition()

    let t = 1.0 - System.Math.Pow(0.1, float dt)
    current <- Vector2.Lerp(current,target,(float32 t))

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    if current <> previous then
        rl.BeginTextureMode(rt)
        rl.DrawCircleV(current, 2f, Color.RayWhite)
        rl.EndTextureMode()
        previous <- current

    // Draw footsteps
    rl.DrawTextureRec(rt.Texture, src, (vec2 0f 0f), Color.White)

    // Draw circle that follows mouse cursor
    rl.DrawCircleV(current, 30f, Color.DarkBlue)
    rl.DrawCircleV(current,  2f, Color.DarkBrown)

    // Draw mouse cursor and line
    rl.DrawCircleV(target, 5f, Color.Yellow)
    rl.DrawLineV(current, target, Color.RayWhite)
    rl.EndDrawing ()

rl.CloseWindow()
