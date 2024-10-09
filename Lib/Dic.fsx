type Dic<'a,'b> = System.Collections.Generic.Dictionary<'a,'b>

module Dic =
    /// An empty Dictionary
    let empty<'Key,'Value when 'Key:equality> = Dic<'Key,'Value>()

    /// The amount of entries in the dictionary
    let inline count (dic:Dic<'Key,'Value>) =
        dic.Count

    /// get a 'Key from Dic. When the 'Key does not exists then the `init` function
    /// is called to create the 'Value.
    let getOrInit init key (dic:Dic<'Key,'Value>) =
        match dic.TryGetValue(key) with
        | true , value -> value
        | false, _     ->
            let value = init ()
            dic.Add(key, value)
            value

    /// Gets key from a Dictionary and assumes it is an array. Creates
    /// array if key was not populated before.
    let getArray key dic =
        getOrInit (fun () -> ResizeArray()) key dic

    /// adds key,value to dictionary, either creating or overwriting value in dictionary.
    let add key value (dic:Dic<'Key,'Value>) =
        match dic.ContainsKey key with
        | true  -> dic.[key] <- value
        | false -> dic.Add(key,value)

    /// Turns a sequence into a dictionary
    let ofSeq seq =
        let d = empty
        for k,v in seq do
            add k v d
        d

    /// removes key and its value from the dictionary
    let remove key (dic:Dic<'Key,'Value>) =
        dic.Remove(key) |> ignore

    // get a key from dic and stores new value
    let change f key (dic:Dic<'Key,'Value>) =
        match dic.TryGetValue(key) with
        | true, value -> dic.[key] <- f (ValueSome value)
        | false, _    -> dic.Add(key, f ValueNone)

    /// The keys in a Dictionary
    let inline keys (dic:Dic<'Key,'Value>) =
        dic.Keys :> seq<'Key>

    /// The values in a Dictionary
    let inline values (dic:Dic<'Key,'Value>) =
        dic.Values :> seq<'Value>

    // Increments value of key by one. When key does not exists, key will be added and set to 1
    let increment key (dic:Dic<'Key,int>) =
        let inline incr x =
            match x with
            | ValueNone   -> 1
            | ValueSome x -> x + 1
        change incr key dic

    /// Filters a dictionary only returning key & value of matching predicate
    let filter predicate (dic:Dic<'Key,'Value>) = seq {
        for kv in dic do
            if predicate kv.Key kv.Value then
                struct (kv.Key,kv.Value)
    }

    /// Assumes that `key` contains a ResizeArray and pushes a value onto it.
    /// Creates an empty ResizeArray when the key was not populated before.
    let push (key:'Key) (value:'Value) (dic:Dic<_,_>) : unit =
        let ra = getArray key dic
        ra.Add(value)

    /// try to get a key from a dictionary
    let get key (dic:Dic<'Key,'Value>) =
        match dic.TryGetValue(key) with
        | false,_ -> ValueNone
        | true, x -> ValueSome x

    /// Iterates through each element of a dictionary
    let inline iter ([<InlineIfLambda>] f) (dic:Dic<'Key,'Value>) =
        for kv in dic do
            f kv.Key kv.Value

    /// concatenates two dictionaries and returns a new dictionary.
    /// key and values of second dictionary overwrites first ones.
    let concat (dicA:Dic<'Key,'Value>) (dicB:Dic<_,_>) =
        let d = empty
        dicA |> iter (fun k v -> add k v d)
        dicB |> iter (fun k v -> add k v d)
        d
