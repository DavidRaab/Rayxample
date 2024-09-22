#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
open Raylib_cs
open System.Numerics

type rl = Raylib

let circlePoints (steps:int) (radius:float32) =
    let rad = (360f / float32 steps) * (System.MathF.PI / 180f)
    let rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rad)
    [
        let mutable start = Vector2(1f,0f)
        for i=1 to steps do
            start <- Vector2.Transform(start, rot)
            yield start * radius
    ]

let drawCircle (steps:int) center (radius:float32) color =
    let points = circlePoints steps radius

    let start = List.head points
    let mutable previous = start
    for point in points do
        rl.DrawLineV(center + previous, center + point, color)
        previous <- point
    rl.DrawLineV(center + start, center + previous, color)


rl.InitWindow(1200, 800, "Rotation")
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do

    rl.BeginDrawing()
    let center = Vector2(150f, 400f)
    for point in circlePoints 24 100f do
        rl.DrawCircleV(center + point, 10f, Color.Red)

    drawCircle  4 (Vector2( 100f,100f)) 100f Color.Red
    drawCircle  8 (Vector2( 250f,100f)) 100f Color.DarkBlue
    drawCircle 12 (Vector2( 450f,100f)) 100f Color.DarkGreen
    drawCircle 16 (Vector2( 650f,100f)) 100f Color.Purple
    drawCircle 20 (Vector2( 850f,100f)) 100f Color.SkyBlue
    drawCircle 24 (Vector2(1050f,100f)) 100f Color.Violet

    rl.EndDrawing()

rl.CloseWindow()
