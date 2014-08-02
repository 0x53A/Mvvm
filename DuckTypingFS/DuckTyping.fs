namespace Mvvm.Codegen

open System
open System.Reflection
open System.Reflection.Emit
open System.Diagnostics.Contracts
open System.Collections
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks


type MappingKey = { TDuck : Type; TThing : Type }
type MappingValue = { T : Type; Init : Action<Object,Object> }

type DuckTyping =
  class
    static member private _mappings = new Dictionary<MappingKey, MappingValue>()
    static member private _lock = new Object()

    ///Maps a Thing-Type to an interface-Type.
    ///Returns the mapped type and a function to initialise it
    static member Map tDuck tThing =
        let key = { TDuck = tDuck; TThing = tThing }
        let mapping = lock (DuckTyping._lock) (fun () ->
            if DuckTyping._mappings.ContainsKey (key) then
                DuckTyping._mappings.[key]
            else
                let (t,init) = DuckTypingFSInternal.mapType tDuck tThing
                let value = { T = t; Init = init }
                DuckTyping._mappings.Add(key, value)
                value  )
        mapping

    ///Casts a Thing to an interface IDuck.
    ///The thing must implement all members of IDuck.
    static member Cast<'TDuck when 'TDuck: null and 'TDuck : not struct> thing =
        Contract.Requires (thing <> null)
        let tDuck = typedefof<'TDuck>
        let tThing = thing.GetType()
        
        let mapping = DuckTyping.Map tDuck tThing

        let instance = Activator.CreateInstance(mapping.T, [| thing |]) :?> 'TDuck
        mapping.Init.Invoke (instance,thing)
        instance
  end
