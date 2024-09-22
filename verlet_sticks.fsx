#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
#load "Lib/Verlet.fsx"
open Raylib_cs
open Helper
open Verlet
open System.Numerics

// Some Resources to Watch:
// https://www.youtube.com/watch?v=-GWTDhOQU6M  --  Verlet Integration
// https://www.youtube.com/watch?v=lS_qeBy3aQI  --  Writing a Physics Engine from Scratch

// Some constants / Game state
let screenWidth, screenHeight = 1200, 800
let mutable useGravity        = false
let gravity                   = vec2 0f 1000f
let mutable showVelocity      = false

// Data-structures
[<NoComparison; NoEquality>]
type Verlet = {
    VStruct: VerletStructure
    Color:   Color
}

let w, h = float32 screenWidth, float32 screenHeight
let applyScreen point =
    // Collision with Bottom Axis
    if point.Position.Y > (h - point.Radius) then
        point.Position.Y <- h - point.Radius
        // Adds a friction to the ground by moving the position 5% against
        // the velocity (this is not frame-rate independent) but this kind
        // of simulation anyway should be run in a fixed update loop. So
        // i don't care for that demo here.
        if useGravity then
            let velocity = -(Verlet.velocity point)
            point.Position <- point.Position + (velocity * 0.05f)
    // Collision with left Axis
    if point.Position.X < point.Radius then
        point.Position.X <- point.Radius
    // Collision with Right Axis
    if point.Position.X > (w - point.Radius) then
        point.Position.X <- w - point.Radius
    // Collision with Up Axis
    if point.Position.Y < point.Radius then
        point.Position.Y <- point.Radius

// The World to Draw
let mutable structs = ResizeArray<_>()
let mutable points  = ResizeArray<_>()
let mutable sticks  = ResizeArray<_>()
let mutable bsticks = ResizeArray<_>()
let mutable pinned  = ResizeArray<_>()

let addStructure color vstruct =
    structs.Add({
        VStruct = vstruct
        Color   = color
    })
    points.AddRange(vstruct.Points)
    sticks.AddRange(vstruct.Sticks)

let resetWorld () =
    structs.Clear()
    points.Clear()
    sticks.Clear()
    bsticks.Clear()
    pinned.Clear()

    // Some basic shapes
    addStructure Color.Yellow   <| Verlet.triangle (vec2 400f 400f) (vec2 600f 200f) (vec2 500f 500f)
    addStructure Color.Brown    <| Verlet.triangle (vec2 100f 100f) (vec2 100f 200f) (vec2 200f 300f)
    addStructure Color.Blue     <| Verlet.quad     (vec2 300f 300f) (vec2 400f 300f) (vec2 500f 500f) (vec2 200f 500f)
    addStructure Color.DarkGray <| Verlet.rectangle 600f 300f 100f 250f

    // Generates two boxes sticked together and pinned at a place
    let r1 = Verlet.rectangle 600f 200f 100f 100f
    let r2 = Verlet.rectangle 740f 340f  50f  50f
    bsticks.Add({
        Stick  = Verlet.stick r1.Points.[3] r2.Points.[0] |> Verlet.newLength 50f
        Factor = 3f
    })
    addStructure Color.DarkGreen  r1
    addStructure Color.DarkPurple r2
    pinned.Add({
        Point          = r1.Points.[0]
        PinnedPosition = vec2 600f 200f
    })

    // Testing placeAt
    let tri = Verlet.triangle (vec2 0f 0f) (vec2 100f 0f) (vec2 50f 100f)
    Verlet.placeAt (vec2 800f 100f) tri
    addStructure Color.Gold tri

    // Generate ropes
    let ropes = [
        Verlet.rope 5f  0 (vec2 100f 100f) (vec2 300f 100f)
        Verlet.rope 5f  2 (vec2 150f 100f) (vec2 350f 100f)
        Verlet.rope 5f  4 (vec2 200f 100f) (vec2 400f 100f)
        Verlet.rope 5f  6 (vec2 250f 100f) (vec2 450f 100f)
        Verlet.rope 5f  8 (vec2 300f 100f) (vec2 500f 100f)
        Verlet.rope 5f 10 (vec2 350f 100f) (vec2 550f 100f)
    ]

    // Pins the first point of every rope
    List.iter (Option.iter pinned.Add << Verlet.pinFirst) ropes

    let lc = lerpColor Color.Green Color.Maroon
    let mutable value = 0f
    let increment     = 1f / float32 (List.length ropes - 1)
    for rope in ropes do
        addStructure (lc value) rope
        value <- value + increment

    // One free rope to play with
    addStructure Color.Lime (Verlet.rope 5f 16 (vec2 600f 100f) (vec2 1100f 100f))

resetWorld()

let mutable currentDrag = NoDrag


// Game Loop
rl.SetConfigFlags(ConfigFlags.Msaa4xHint)
rl.InitWindow(screenWidth, screenHeight, "Verlet Integration")
Rlgl.EnableSmoothLines()
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse None

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    // Handles Drag of Points
    currentDrag <- processDrag currentDrag points (fun p -> Circle (p.Position,p.Radius)) mouse
    match currentDrag with
    | NoDrag                   -> ()
    | Hover _                  -> ()
    | StartDrag (point,offset)
    | InDrag    (point,offset) -> point.Position <- mouse.Position
    | EndDrag _                -> ()

    // Update VerletPoint
    for point in points do
        if useGravity then
            Verlet.addForce gravity point
        applyScreen point
        Verlet.updatePoint point dt

    // Update Sticks constraints
    for i=1 to 2 do
        if bsticks.Count > 0 then
            for idx=bsticks.Count-1 downto 0 do
                if Verlet.shouldBreak bsticks.[idx] then
                    bsticks.RemoveAt(idx)
                else
                    Verlet.updateStick bsticks.[idx].Stick

        // update sticks
        for stick in sticks do
            Verlet.updateStick stick

    // Force pinned position
    for pin in pinned do
        pin.Point.Position    <- pin.PinnedPosition
        pin.Point.OldPosition <- pin.PinnedPosition

    // Draw Point & Sticks
    for stick in sticks do
        let a,b = stick.Start, stick.End
        rl.DrawLine(int a.Position.X, int a.Position.Y, int b.Position.X, int b.Position.Y, Color.DarkGray)

    for bstick in bsticks do
        let a,b = bstick.Stick.Start, bstick.Stick.End
        let n =
            let len = (a.Position - b.Position).Length()  - bstick.Stick.Length
            let max = bstick.Stick.Length * bstick.Factor - bstick.Stick.Length
            len / max
        let c = smoothstepColor Color.DarkGray Color.Red n
        rl.DrawLine(int a.Position.X, int a.Position.Y, int b.Position.X, int b.Position.Y, c)

    for s in structs do
        let color = s.Color
        for p in s.VStruct.Points do
            rl.DrawCircle(int p.Position.X, int p.Position.Y, p.Radius, color)

    // Highlight current hovered element
    match currentDrag with
    | Hover point -> rl.DrawCircleLinesV(point.Position, point.Radius, Color.RayWhite)
    | _           -> ()

    // Draw GUI
    rl.DrawFPS(0,0)
    rl.DrawText(System.String.Format("Points: {0} Sticks: {1}", points.Count, sticks.Count), 800, 10, 24, Color.Yellow)
    if guiButton (rect 100f 10f 200f 30f) (if useGravity then "Disable Gravity" else "Enable Gravity") then
        useGravity <- not useGravity
    if guiButton (rect 325f 10f 150f 30f) "Reset World" then
        resetWorld ()

    rl.EndDrawing ()

rl.CloseWindow()
