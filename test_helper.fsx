#!/usr/bin/env -S dotnet fsi
#load "Lib/Helper.fsx"
#load "../Lib/Test.fsx"
open Helper
open Test

Test.is
    (List.map (wrap 0f 10f) [2f .. 7f])
    [2f .. 7f]
    "No Wrapping"

Test.is
    (List.map (wrap 0f 10f) [5f .. 15f])
    [5f; 6f; 7f; 8f; 9f; 0f; 1f; 2f; 3f; 4f; 5f]
    "Wrapping if greater"

Test.is
    (List.map (wrap 0f 10f) [-5f .. 5f])
    [5f; 6f; 7f; 8f; 9f; 0f; 1f; 2f; 3f; 4f; 5f]
    "Wrapping if smaller"

Test.is
    (List.map (wrap -16f -8f) [-20f .. -15f])
    [-12f; -11f; -10f; -9f; -16f; -15f]
    "negatives"

Test.is
    (List.map (wrap -16f -8f) [-16f; -16.1f])
    [-16f; -8.1f]
    "small step"

let t1 =
    let xs     = [1..10]
    let length = List.length xs
    Test.is
        (List.map (fun idx -> xs.[int (wrap 0f (float32 length) (float32 idx))]) [-5 .. 5])
        [6;7;8;9;10;1;2;3;4;5;6]
        "negative indexing in array"

    Test.is
        (List.map (fun idx -> xs.[int (wrap 0f (float32 length) (float32 idx))]) [8 .. 12])
        [9;10;1;2;3]
        "indexing above maximum"

let t2 =
    let xs     = [1..10]
    let length = List.length xs
    Test.is
        (List.map (fun idx -> xs.[wrapi 0 length idx]) [-5 .. 5])
        [6;7;8;9;10;1;2;3;4;5;6]
        "negative indexing in array"

    Test.is
        (List.map (fun idx -> xs.[wrapi 0 length idx]) [8 .. 12])
        [9;10;1;2;3]
        "indexing above maximum"

let cos_and_sin =
    Test.float32 (cosd  0f)   1f "cosd of  0"
    Test.float32 (cosd 60f) 0.5f "cosd of 60"
    Test.float32 (cosd 90f)   0f "cosd of 90"

    Test.float32 (sind  0f) 0f        "sind of  0"
    Test.float32 (sind 60f) 0.866025f "sind of 60"
    Test.float32 (sind 90f) 1f        "sind of 90"

    // This back and forth only works properly with degrees between 0° - 90°
    Test.withAccuracy32 0.001f (fun () ->
        for i=1 to 10 do
            let degree = randf 0f 90f
            Test.float32 (acosd (cosd degree)) degree "acosd of cosd"
            Test.float32 (asind (sind degree)) degree "asind of sind"
    )

let cos_and_sin_as_radiant =
    // This back and forth only works properly with degrees between 0° - 90°
    // This has a lot better accuracy compared to cosd and sind. Most of the time
    // works even good with 0.000001f accuracy while cosd and sind shows many
    // failing tests with this accuracy.
    Test.withAccuracy32 0.0001f (fun () ->
        for i=1 to 10 do
            let rad = randf 0f System.MathF.PI / 2f
            Test.float32 (acos (cos rad)) rad "acos of cos"
            Test.float32 (asin (sin rad)) rad "asin of sin"
    )

Test.doneTesting ()
