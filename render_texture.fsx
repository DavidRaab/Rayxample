#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
open Raylib_cs
open Helper
open System.Numerics

rl.InitWindow(800, 800, "Render Texture")

let rt  = rl.LoadRenderTexture(800, 800)
// Size should be 800,800, but Height must be flipped
let src = Rectangle(0f, 0f, float32 rt.Texture.Width, float32 -rt.Texture.Height)
let dst = Rectangle(0f, 0f, 800f,  800f)

rl.BeginTextureMode rt
rl.DrawText("Mouse. Left=Draw Right=Clear", 0, 0, 20, Color.Yellow)
rl.EndTextureMode ()

rl.SetTargetFPS(60)
while not <| toBool (rl.WindowShouldClose()) do
    let mouse = getMouse None
    // Now all drawing operations draws to an Texture on the GPU
    rl.BeginTextureMode(rt)
    if mouse.Left = Down then
        rl.DrawCircleV(mouse.Position, 2f, Color.RayWhite)
    if mouse.Right = Pressed then
        rl.ClearBackground(color 0 0 0 0)
    rl.EndTextureMode()

    // draw texture to screen
    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)
    rl.DrawTexturePro(rt.Texture, src, dst, Vector2.Zero, 0f, Color.White)
    rl.EndDrawing ()

rl.CloseWindow()
