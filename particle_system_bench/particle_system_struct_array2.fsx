#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "../Lib/Helper.fsx"
open Raylib_cs
open System.Numerics
open Helper

// This is a modification of particle_system_struct.fsx.
// It uses two arrays to updated and push particles into new array
// and swaps them after finishing all operations

let screenWidth, screenHeight = 1200, 800

// This shows only 1000 of the particles. Activating is for benchmarking the
// underlying particle system and its performance.
let showOnly1000 = false

type Sprite = {
    Texture: Texture2D
    Source:  Rectangle
}

// A single particle is usually spawned by an emiter. It lifes for some
// time and during lifetime applies some movement, rotation to it.
[<Struct>]
type Particle = {
    mutable Sprite:      Sprite
    mutable Position:    Vector2
    mutable Rotation:    float32
    mutable ElapsedTime: float32
    mutable Velocity:    Vector2
    mutable Torque:      float32
    mutable LifeTime:    float32
}

// An Emiter spawn particles from its Position into a direction
type Emiter = {
    Position:  Vector2
    Direction: Vector2
    FOV:       float32 // Degree
}

module Particles =
    let maxParticles            = 100_000
    let mutable activeParticles = 0

    // The idea is to use two arrays instead of just one. While iterating
    // through one array and updating the particles the previous one is
    // populated with only the active elements. This is only useful when
    // using structs.
    let mutable previous = Array.init maxParticles (fun i -> {
        Sprite      = Unchecked.defaultof<Sprite>
        Position    = vec2 0f 0f
        Rotation    = 0f
        ElapsedTime = 0f
        Velocity    = vec2 0f 0f
        Torque      = 0f
        LifeTime    = 1f
    })
    let mutable current = Array.init maxParticles (fun i -> {
        Sprite      = Unchecked.defaultof<Sprite>
        Position    = vec2 0f 0f
        Rotation    = 0f
        ElapsedTime = 0f
        Velocity    = vec2 0f 0f
        Torque      = 0f
        LifeTime    = 1f
    })

    // Only iterates through the active particles
    let inline iter ([<InlineIfLambda>] f) =
        if activeParticles > 0 then
            for idx=0 to activeParticles-1 do
                f idx

    /// Initialize a new particle. Every field should be explicitly set. No
    /// cleanup or reset is done.
    let initParticle f =
        if activeParticles < maxParticles then
            f activeParticles
            activeParticles <- activeParticles + 1

    let updateParticles (dt:float32) =
        let mutable newActive = 0

        // go through current and update every particle. When its ElapsedTime
        // for idx=0 to activeParticles-1 do
        System.Threading.Tasks.Parallel.For(0, activeParticles, (fun idx _ ->
            current.[idx].ElapsedTime <- current.[idx].ElapsedTime + dt
            current.[idx].Position    <- current.[idx].Position + (current.[idx].Velocity * dt)
            current.[idx].Rotation    <- current.[idx].Rotation + (current.[idx].Torque * dt)
        ))
        |> ignore

        // push into previous when active
        for idx=0 to activeParticles-1 do
            if current.[idx].ElapsedTime < current.[idx].LifeTime then
                previous.[newActive] <- current.[idx]
                newActive <- newActive + 1

        // swap current and previous
        let tmp = current
        current  <- previous
        previous <- tmp

        // update new active element
        activeParticles <- newActive

rl.InitWindow(screenWidth, screenHeight, "Hello, World!")

// Genereates a Texture Atlas in Memory. This emulates later behaviour and
// performance better than calling DrawCircle() and DrawRectangle()
let sprites =
    let atlas = rl.LoadRenderTexture(21, 12)
    rl.BeginTextureMode(atlas)
    rl.DrawRectangle(0, 0, 10, 10, Color.DarkBlue)
    rl.DrawCircle(15, 5, 4f, Color.Yellow)
    rl.EndTextureMode()
    [|
        { Texture = atlas.Texture; Source = rect  0f 0f 10f 10f } // Rect
        { Texture = atlas.Texture; Source = rect 10f 0f 11f 11f } // Circle
    |]

// rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt = rl.GetFrameTime()
    rl.DrawFPS(0,0)

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    // Update particles
    Particles.updateParticles dt

    // Initialize x Particles each frame
    for i=0 to 100 do
        let sprite = if rng.NextSingle () < 0.5f then sprites.[0] else sprites.[1]
        Particles.initParticle (fun idx ->
            let p = Particles.current
            p.[idx].Sprite      <- sprite
            p.[idx].Position    <- vec2 (float32 screenWidth / 2f) (float32 screenHeight / 2f)
            p.[idx].ElapsedTime <- 0f
            p.[idx].LifeTime    <- randf 10f 20f
            p.[idx].Rotation    <- 0f
            p.[idx].Torque      <- randf -45f 45f
            p.[idx].Velocity    <- (vec2 (randf -1f 1f) (randf -1f 1f)) * 200f
        )

    // Draw particles
    if showOnly1000 then
        for idx=0 to (min 1000 (Particles.activeParticles-1)) do
            let p = Particles.current
            rl.DrawTexturePro(
                p.[idx].Sprite.Texture,
                p.[idx].Sprite.Source,
                (rect p.[idx].Position.X p.[idx].Position.Y 10f 10f),
                Vector2.Zero,
                p.[idx].Rotation,
                Color.White
            )
    else
        Particles.iter (fun idx ->
            let p = Particles.current
            rl.DrawTexturePro(
                p.[idx].Sprite.Texture,
                p.[idx].Sprite.Source,
                (rect p.[idx].Position.X p.[idx].Position.Y 10f 10f),
                Vector2.Zero,
                p.[idx].Rotation,
                Color.White
            )
        )

    Raylib.DrawText(System.String.Format("Particles {0}", Particles.activeParticles), 1000, 10, 24, Color.Yellow)
    rl.EndDrawing ()

rl.CloseWindow()
