
module DuckTypingFS

open System
open System.Reflection
open System.Reflection.Emit
open System.Diagnostics.Contracts
open System.Collections
open System.Collections.Generic

type MethodSignature =
    | MethodSignature of returnType : Type * name : string * parameters : Type array
    
type PropertySignature =
    | PropertySignature of propertyType : Type * name : string
    
type EventSignature = 
    | EventSignature of signature : MethodSignature * name : string

type MemberSignature = 
    | Method of MethodSignature
    | Property of PropertySignature
    | Event of EventSignature
    | Other

let getMethodSignature (methodInfo : MethodInfo) =
        let parameters =  methodInfo.GetParameters() |> Array.map (fun p -> p.ParameterType)
        MethodSignature (name = methodInfo.Name,
                         returnType = methodInfo.ReturnType,
                         parameters = parameters)

let getDelegateSignature (delegateType:Type) =
    let invoke = delegateType.GetMethod("Invoke")
    getMethodSignature invoke

let getSignature (memberInfo : MemberInfo) = 
    match memberInfo.MemberType with
    | MemberTypes.Method ->
        let m = memberInfo :?> MethodInfo
        Method(getMethodSignature m)
    | MemberTypes.Property ->
        let p = memberInfo :?> PropertyInfo
        Property(PropertySignature (name = p.Name, propertyType = p.PropertyType))
    | MemberTypes.Event ->
        let e = memberInfo :?> EventInfo
        let signature = getDelegateSignature e.EventHandlerType
        Event (EventSignature (signature = signature, name = e.Name))
    | _ -> Other

let flatten<'TNode> (source : 'TNode) (extract : ('TNode->'TNode seq)) =
  seq {
    let stack = new Stack<'TNode>();
    stack.Push source

    while stack.Count > 0 do
        let item = stack.Pop()
        yield item
        for child in extract(item) do
            stack.Push(child)
  }

let findDuckMemberInThing duckMember thingMembers =
    let (dSig, dMI) = duckMember
    thingMembers |> Seq.tryFind (fun (tSig, tMI) -> true)

let getAllMembers (tp:Type) = 
    flatten tp (fun t -> t.GetInterfaces() |> Array.toSeq)
    |> Seq.distinct 
    |> Seq.map (fun i -> i.GetMembers())
    |> Seq.collect (fun x -> x)
    |> Seq.map (fun m -> (getSignature m, m))
    |> Seq.distinct

exception DuckExceptionMemberMapping of MemberSignature array

type DuckTyping =
  class
    static member Cast<'TDuck when 'TDuck: null and 'TDuck : not struct> thing =
        let duckType = typedefof<'TDuck>
        let thingType = thing.GetType()
        Contract.Requires duckType.IsInterface
        Contract.Requires (thing <> null)
        //get all members of the duck and the thing
        let duckMembers = getAllMembers duckType |> Seq.toList
        let thingMembers = getAllMembers thingType |> Seq.toList
        //try to map all duckmembers to thingmembers
        let mapping = duckMembers
                      |> Seq.map (fun x -> (x,findDuckMemberInThing x thingMembers))
                      |> Seq.toList
        //see if the mapping was successfull
        let mappingSuccess = mapping |> Seq.forall (fun (d,t) -> t.IsSome)
        if not mappingSuccess then
            let missingMembers = mapping |> Seq.where (fun (d,t) -> t.IsNone)
                                 |> Seq.map (fun ((s, mi),t)->s) |> Seq.toArray
            raise (DuckExceptionMemberMapping(missingMembers))
        //we are still alive, so the mapping was a success
        let assemblyName = sprintf "%s_%s_%s" duckType.Name thingType.Name (Guid.NewGuid().ToString())
        let asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName),
                                                                Emit.AssemblyBuilderAccess.RunAndSave)
        let dynMod = asm.DefineDynamicModule("MainModule", assemblyName + ".mod.dll", true)
        let tb = dynMod.DefineType(sprintf "__%s" duckType.Name)
        let ctor = tb.DefineConstructor(MethodAttributes.)
        let nullptr : 'TDuck = null
        nullptr
  end

