#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib/Helper.fsx"
open Raylib_cs
open System.Numerics
open Helper

let screenWidth, screenHeight = 800, 800

// Definition of a Moveable, also could just be a Rectangle, but then a function
// needs to pass a byref. So instead of this i turn it into its own reference
// type. At least assigning an additional Color makes it somehow more useful.
type MoveableRect = {
    mutable Rect: Rectangle
    Color:        Color
}

let moveables = [
    { Rect = rect 100f 100f 100f 100f; Color = Color.Yellow }
    { Rect = rect 200f 200f 100f 100f; Color = Color.Red    }
    { Rect = rect 300f 300f 100f 100f; Color = Color.Blue   }
]

let mutable movingRect = None
let mutable selection  = NoDrag

rl.InitWindow(screenWidth, screenHeight, "Hello, World!")
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse None

    selection <- processDrag selection moveables (fun m -> Rect m.Rect) mouse
    match selection with
    | NoDrag   -> ()
    | Hover  _ -> ()
    | StartDrag (mov,offset)   -> movingRect <- Some (mov.Rect,offset)
    | InDrag (moveable,offset) ->
        let r = moveable.Rect
        moveable.Rect <- rect (mouse.Position.X-offset.X) (mouse.Position.Y-offset.Y) r.Width r.Height
    | EndDrag _ -> movingRect <- None


    // Begin Drawing
    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    match movingRect with
    | None               -> ()
    | Some (rect,offset) ->
        rl.DrawRectangleLinesEx(rect, 1f, Color.Gray)
        let w,h = rect.Width / 2f, rect.Height / 2f
        rl.DrawLine(
            int (rect.X + w),
            int (rect.Y + h),
            int (mouse.Position.X - offset.X + w),
            int (mouse.Position.Y - offset.Y + w),
            Color.Lime
        )

    for mov in moveables do
        rl.DrawRectangleRec(mov.Rect, mov.Color)

    match selection with
    | Hover drag -> rl.DrawRectangleLinesEx(drag.Rect, 1f, Color.RayWhite)
    | _          -> ()

    rl.EndDrawing ()
rl.CloseWindow()
