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
type MappingValue = { T : Type; Delegates : (FieldInfo*MethodInfo) list }

type DuckTyping =
  class
    static member private _mappings = new Dictionary<MappingKey, MappingValue>()
    static member private _lock = new Object()

    //Casts a Thing to an interface IDuck.
    //The thing must implement all members of IDuck.
    static member Cast<'TDuck when 'TDuck: null and 'TDuck : not struct> thing =
        Contract.Requires (thing <> null)
        let tDuck = typedefof<'TDuck>
        let tThing = thing.GetType()
        

        let key = { TDuck = tDuck; TThing = tThing }
        let mv = lock (DuckTyping._lock) (fun x ->
            if DuckTyping._mappings.ContainsKey (key) then
                DuckTyping._mappings.[key]
            else
                let (t,del) = DuckTypingFSInternal.mapType tDuck tThing
                let value = { T = t; Delegates = del }
                DuckTyping._mappings.Add(key, value)
                value  
           )

        let instance = Activator.CreateInstance(mv.T, [| thing |]) :?> 'TDuck
              
        for (a,b) in mv.Delegates do
            let deleg = b.CreateDelegate(a.FieldType, thing)
            a.SetValue(instance, deleg)
        instance
  end

