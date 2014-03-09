
module DuckTypingFS

open System
open System.Reflection
open System.Reflection.Emit
open System.Diagnostics.Contracts
open System.Collections
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

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

/// the attributes of the generated type
let GeneratedTypeAttributes = TypeAttributes.Public ||| TypeAttributes.Class ||| TypeAttributes.AutoClass |||
                              TypeAttributes.AnsiClass ||| TypeAttributes.BeforeFieldInit ||| TypeAttributes.AutoLayout

/// the attributes of a normal public .ctor
let ConstructorAttributes = MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.RTSpecialName ||| MethodAttributes.HideBySig

/// Attributes for a method implementing an interface
let InterfaceImplementationAttributes = MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.NewSlot |||
                                        MethodAttributes.HideBySig ||| MethodAttributes.Virtual ||| MethodAttributes.Final

/// Attributes for a method/property overriding a virtual/abstract base
let OverrideAttributes = MethodAttributes.Public ||| MethodAttributes.HideBySig ||| MethodAttributes.SpecialName ||| MethodAttributes.Virtual

/// the Attributes for a normal, nonvirtual instance method
let PrivateInstanceMethodAttributes = MethodAttributes.Private ||| MethodAttributes.HideBySig

/// the Attributes for a normal, nonvirtual instance method
let PrivateStaticMethodAttributes = MethodAttributes.Private ||| MethodAttributes.HideBySig ||| MethodAttributes.Static

let mapType (duckType: Type) (thingType: Type) =
    Contract.Requires duckType.IsInterface
    //get all members of the duck and the thing
    let duckMembers = getAllMembers duckType |> Seq.toList
    let thingMembers = getAllMembers thingType |> Seq.toList
    //try to map all duckmembers to thingmembers
    let mapping = duckMembers
                    |> Seq.map (fun x -> (x, findDuckMemberInThing x thingMembers))
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
    let tb = dynMod.DefineType(sprintf "__%s_duck" duckType.Name, GeneratedTypeAttributes)
    let ctor = tb.DefineConstructor(ConstructorAttributes, CallingConventions.HasThis, [| thingType |])
    let ctorIL = ctor.GetILGenerator()



    //emit the default constructor
    ctorIL.Emit(OpCodes.Ldarg_0)
    ctorIL.Emit(OpCodes.Call, typedefof<Object>.GetConstructor(Type.EmptyTypes))
    ctorIL.Emit(OpCodes.Nop)
    ctorIL.Emit(OpCodes.Ret)

    let generatedType = tb.CreateType()
    asm.Save(sprintf "%s.dll" assemblyName)
    generatedType


type DuckTyping =
  class
    static member private _mappings = new Dictionary<Tuple<Type,Type>, Type>()
    static member private _lock = new Object()

    ///Casts a Thing to an interfae IDuck.
    ///The thing must implement all members of IDuck.
    static member Cast<'TDuck when 'TDuck: null and 'TDuck : not struct> thing =
        Contract.Requires (thing <> null)
        let tDuck = typedefof<'TDuck>
        let tThing = thing.GetType()
        Monitor.Enter (DuckTyping._lock)
        try
            let key : Tuple<Type,Type> = new System.Tuple<Type,Type>(tDuck, tThing)
            let mappedType =
                if DuckTyping._mappings.ContainsKey (key) then
                    DuckTyping._mappings.[key]
                else
                    let mt = mapType  tDuck tThing
                    DuckTyping._mappings.Add(key, mt)
                    mt
            Activator.CreateInstance(mappedType, [| thing |]) :?> 'TDuck
        finally
            Monitor.Exit (DuckTyping._lock)
  end

