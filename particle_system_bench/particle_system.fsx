#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "../Lib/Helper.fsx"
open Raylib_cs
open Helper
open System.Numerics

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
    let maxParticles = 100_000
    let mutable activeParticles = 0
    let particles = Array.init maxParticles (fun i -> {
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
                f particles.[idx]

    /// Initialize a new particle. Every field should be explicitly set. No
    /// cleanup or reset is done.
    let initParticle f =
        if activeParticles < maxParticles then
            f particles.[activeParticles]
            activeParticles <- activeParticles + 1

    let inline swap x y =
        let tmp = particles.[x]
        particles.[x] <- particles.[y]
        particles.[y] <- tmp

    /// Deactivates a particle. Usually called when its ElapsedTime reached its lifetime
    let inline deactivateParticle idx =
        swap idx (activeParticles-1)
        activeParticles <- activeParticles - 1

    let updateParticles (dt:float32) =
        if activeParticles > 0 then
            let mutable idx     = 0
            let mutable running = true
            while running do
                if idx >= activeParticles then
                    running <- false
                else
                    // Update particle
                    let p = particles.[idx]
                    p.ElapsedTime <- p.ElapsedTime + dt

                    // when particle lifetime is reached, we swap with last element.
                    // But then we need to recheck current idx again.
                    if p.ElapsedTime >= p.LifeTime then
                        deactivateParticle idx
                    else
                        p.Position <- p.Position + (p.Velocity * dt)
                        p.Rotation <- p.Rotation + (p.Torque * dt)
                        idx <- idx + 1


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
        Particles.initParticle (fun p ->
            p.Sprite      <- sprite
            p.Position    <- vec2 (float32 screenWidth / 2f) (float32 screenHeight / 2f)
            p.ElapsedTime <- 0f
            p.LifeTime    <- randf 10f 20f
            p.Rotation    <- 0f
            p.Torque      <- randf -45f 45f
            p.Velocity    <- (vec2 (randf -1f 1f) (randf -1f 1f)) * 200f
        )

    // Draw particles
    if showOnly1000 then
        for idx=0 to (min 1000 (Particles.activeParticles-1)) do
            let p = Particles.particles.[idx]
            rl.DrawTexturePro(
                p.Sprite.Texture, p.Sprite.Source, (rect p.Position.X p.Position.Y 10f 10f), Vector2.Zero, p.Rotation, Color.White
            )
    else
        Particles.iter (fun p ->
            rl.DrawTexturePro(
                p.Sprite.Texture, p.Sprite.Source, (rect p.Position.X p.Position.Y 10f 10f), Vector2.Zero, p.Rotation, Color.White
            )
        )

    Raylib.DrawText(System.String.Format("Particles {0}", Particles.activeParticles), 1000, 10, 24, Color.Yellow)
    rl.EndDrawing ()

rl.CloseWindow()
