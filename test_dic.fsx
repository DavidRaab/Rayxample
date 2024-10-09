#!/usr/bin/env -S dotnet fsi
#load "Lib/Dic.fsx"
#load "Lib/Test.fsx"
open Dic
open Test

/// compares a dictionary with a specified definition
let dic_is name dic ds =
    let ds = Array.ofSeq ds
    if Dic.count dic = ds.Length then
        let rec loop idx =
            let k,v = ds.[idx]
            match Dic.get k dic with
            | ValueNone       -> Test.fail (sprintf "%s - Dictionary does not contain key '%A'" name k)
            | ValueSome value ->
                if value = v then
                    if idx < ds.Length-2 then
                        loop (idx+1)
                    else
                        Test.pass name
                else
                    Test.fail (sprintf "%s - values for key '%A' does not match" name k)
        loop 0
    else
        Test.fail "Dictionary not same size"

let a = Dic.empty
Test.is (Dic.count a) 0 "a is empty"

Dic.add "foo" 1 a
Test.is (Dic.count a) 1 "a contains 1 item"

let b = Dic.empty<string,int>
Test.is (Dic.count b) 0 "b is empty"

dic_is "check a" a ["foo", 1]

Dic.add "maz" 10 a
dic_is
    "concat"
    (Dic.concat
        a
        (Dic.ofSeq ["foo", 2; "bar", 3]))
    ["foo",2; "bar",3; "maz",10]

Test.is (Dic.count Dic.empty) 0 "Dic.empty is empty"


let count = Dic.empty
Dic.increment "foo" count
Dic.increment "bar" count
Dic.increment "baz" count
Dic.increment "foo" count
dic_is "count" count [
    "foo", 2
    "bar", 1
    "baz", 1
]

let folders = Dic.empty
Dic.push "etc"  "fstab"   folders
Dic.push "etc"  "crontab" folders
Dic.push "home" "sid"     folders
Dic.push "home" "david"   folders

Test.is (List.ofSeq folders.["etc"])  ["fstab"; "crontab"] "etc folder"
Test.is (List.ofSeq folders.["home"]) ["sid";   "david"]   "home folder"