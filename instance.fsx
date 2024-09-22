#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
open Raylib_cs
open System.Numerics

type rl = Raylib
let screenWidth, screenHeight = 1200, 800

rl.InitWindow(screenWidth, screenHeight, "Hello, World!")
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt = rl.GetFrameTime()

    let vbo = Rlgl.LoadVertexArray()

    let mem = [| 0f  |]
    let vao = Rlgl.LoadVertexBuffer()

    Rlgl.Begin(DrawMode.Triangles)
    Rlgl.DrawVertexArrayInstanced(0, 3, 1)

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)



    let text     = "Hello, World!"
    let textSize = rl.MeasureText(text, 24)
    rl.DrawText("Hello, World!", (800/2 - textSize/2), 400-12, 24, Color.White)
    rl.EndDrawing ()

rl.CloseWindow()
