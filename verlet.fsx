#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
#load "Lib/SpatialTree.fsx"
#load "Lib/Verlet.fsx"
open Raylib_cs
open Helper
open SpatialTree
open Verlet
open System.Numerics

// Some Resources to Watch:
// https://www.youtube.com/watch?v=-GWTDhOQU6M  --  Verlet Integration
// https://www.youtube.com/watch?v=lS_qeBy3aQI  --  Writing a Physics Engine from Scratch

// Some constants / Game state
let screenWidth, screenHeight = 1400, 900
let circleAmount              = 200
let circleSizes               = [| 3f; 5f; 7f; 9f |]
let gravity                   = vec2 0f 1000f
let mutable showVelocity      = false

// Data-structures
[<NoComparison; NoEquality>]
type Circle = {
    VPoint: VerletPoint
    Color:  Color
}

module Circle =
    let randomCircle pos = {
        VPoint = {
            OldPosition  = pos
            Position     = pos
            Acceleration = Vector2.Zero
            Radius       = randomOf circleSizes
        }
        Color =
            match randi 1 5 with
            | 1 -> Color.DarkBlue
            | 2 -> Color.Orange
            | 3 -> Color.Purple
            | 4 -> Color.SkyBlue
            | 5 -> Color.DarkGreen
    }

    let draw circle =
        let inline ir x = int (round x)
        let pos = circle.VPoint.Position
        rl.DrawCircle (ir pos.X, ir pos.Y, float32 circle.VPoint.Radius, circle.Color)
        if showVelocity then
            let velocity = Verlet.velocity circle.VPoint
            rl.DrawLine (
                int pos.X, int pos.Y,
                int (pos.X + velocity.X),
                int (pos.Y + velocity.Y),
                Color.RayWhite
            )

    let resolveCollision circle other =
        if not (isSame circle other) then
            let toOther        = other.Position - circle.Position
            let distance       = toOther.Length ()
            let neededDistance = circle.Radius + other.Radius
            if distance < neededDistance then
                let toOther     = toOther / distance // normalize vector
                let overlap     = neededDistance - distance
                // The simulation becomes better when the collision is not
                // fully resolved in one step. The idea is to just move
                // the object into the target position, but not set the
                // target position. But this can require multiple subSteps
                // in the gameLoop.
                let correction  = 0.5f * overlap * toOther
                circle.Position <- circle.Position - correction
                other.Position  <- other.Position  + correction

    let w, h = float32 screenWidth, float32 screenHeight
    let resolveScreenBoundaryCollision circle =
        // Collision with Bottom Axis
        if circle.Position.Y > (h - circle.Radius) then
            circle.Position.Y <- h - circle.Radius
        // Collision with left Axis
        if circle.Position.X < circle.Radius then
            circle.Position.X <- circle.Radius
        // Collision with Right Axis
        if circle.Position.X > (w - circle.Radius) then
            circle.Position.X <- w - circle.Radius
        // Collision with Up Axis
        if circle.Position.Y < circle.Radius then
            circle.Position.Y <- circle.Radius

// Circles to draw.
let circles =
    ResizeArray<_>(
        Seq.init circleAmount (fun i -> Circle.randomCircle (vec2 (randf 0f (float32 screenWidth)) (randf 0f 100f)))
    )

// Game Loop
rl.SetConfigFlags(ConfigFlags.Msaa4xHint)
rl.InitWindow(screenWidth, screenHeight, "Verlet Integration")
rl.SetTargetFPS(60)

while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse None

    // Spawn circles
    if mouse.Left = Down then
        circles.Add(Circle.randomCircle mouse.Position)

    // Build Spatial Tree
    let tree = STree.create 64
    for circle in circles do
        STree.add circle.VPoint.Position circle tree

    // Update Circles
    let subSteps = 4
    let dt       = dt / float32 subSteps
    for i=1 to subSteps do
        for circle in circles do
            let vp = circle.VPoint
            Verlet.addForce gravity vp
            Verlet.updatePoint vp dt
            // Resolve Collision with Spatial Tree
            STree.getRec tree vp.Position vp.Radius vp.Radius (fun other ->
                Circle.resolveScreenBoundaryCollision vp
                Circle.resolveCollision vp other.VPoint
            )

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)
    // draw always should be in its own loop. even when every circle is iterated
    // once it can be processed multiple times. Because collision detection can
    // move the same circle multiple times, even inside a single loop iteration.
    // So it only makes sense to draw all circles once they all have been processed
    // completely. Think of it like in Conways Game of Life. The whole State of
    // all circles has to be advanced forward until the final position of a circle
    // is really known.
    for circle in circles do
        Circle.draw circle

    // Draw GUI
    rl.DrawFPS(0,0)
    rl.DrawText(System.String.Format("Circles: {0}", circles.Count), 1000, 10, 24, Color.Yellow)
    if guiButton (rect 100f 10f 200f 30f) (if showVelocity then "Hide Velocity" else "Show Velocity") then
        showVelocity <- not showVelocity
    if guiButton (rect 325f 10f 150f 30f) "New Circles" then
        circles.Clear()
        circles.AddRange(
            Seq.init circleAmount (fun i ->
                Circle.randomCircle (vec2 (randf 0f (float32 screenWidth)) (randf 0f 100f))
        ))
    if guiButton (rect 500f 10f 150f 30f) "Add 100" then
        circles.AddRange(
            Seq.init 100 (fun i -> Circle.randomCircle (vec2 (randf 0f (float32 screenWidth)) (randf 0f 100f)))
        )

    rl.EndDrawing ()

rl.CloseWindow()
