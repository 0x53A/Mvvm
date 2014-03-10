
module DuckTypingFS

open System
open System.Reflection
open System.Reflection.Emit
open System.Diagnostics.Contracts
open System.Collections
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks


type Member = 
    | Method of MethodInfo
    | Property of PropertyInfo
    | Event of EventInfo
    | Field of FieldInfo
    | Other

let getDelegateSignature (delegateType:Type) =
    let invoke = delegateType.GetMethod("Invoke")
    Method(invoke)

let toMember (memberInfo : MemberInfo) =
    match memberInfo.MemberType with
    | MemberTypes.Method ->
        let m = memberInfo :?> MethodInfo
        Method(m)
    | MemberTypes.Property ->
        let p = memberInfo :?> PropertyInfo
        Property(p)
    | MemberTypes.Event ->
        let e = memberInfo :?> EventInfo
        Event (e)
    | MemberTypes.Field ->
        let f = memberInfo :?> FieldInfo
        Field (f)
    | _ -> Other

let rec flatten<'TNode> (source : 'TNode) (extract : ('TNode->'TNode seq)) =
  seq {
    for child in extract(source) do
        yield child
        yield! flatten child extract
  }

let findDuckMemberInThing (duckMember : Member) thingMembers =
    let d = duckMember
    thingMembers |> Seq.tryFind (fun t -> raise (NotImplementedException()))

let getAllMembers (tp : Type) = 
    flatten tp (fun t -> t.GetInterfaces() |> Array.toSeq)
    |> Seq.distinct 
    |> Seq.map (fun i -> i.GetMembers())
    |> Seq.collect (fun x -> x)
    |> Seq.map (fun m -> (toMember m))
    |> Seq.distinct

let voidType = Type.GetType("System.Void")

let getDelegateType (mi : MethodInfo) =
    let parameters = mi.GetParameters() |> Array.map (fun p -> p.ParameterType)
    if mi.ReturnType = voidType then
        let unbound = match parameters.Length with
                        | 0 -> typedefof<Action>
                        | 1 -> typedefof<Action<_>>
                        | 2 -> typedefof<Action<_,_>>
                        | 3 -> typedefof<Action<_,_,_>>
                        | 4 -> typedefof<Action<_,_,_,_>>
                        | 5 -> typedefof<Action<_,_,_,_,_>>
                        | 6 -> typedefof<Action<_,_,_,_,_,_>>
                        | 7 -> typedefof<Action<_,_,_,_,_,_,_>>
                        | 8 -> typedefof<Action<_,_,_,_,_,_,_,_>>
                        | 9 -> typedefof<Action<_,_,_,_,_,_,_,_,_>>
                        | 10 ->typedefof<Action<_,_,_,_,_,_,_,_,_,_>>
                        | _ -> raise (InvalidOperationException())
        let bound = unbound.MakeGenericType(parameters)
        bound
    else
        let retVal = mi.ReturnType
        let unbound = match parameters.Length with
                        | 0 -> typedefof<Func<_>>
                        | 1 -> typedefof<Func<_,_>>
                        | 2 -> typedefof<Func<_,_,_>>
                        | 3 -> typedefof<Func<_,_,_,_>>
                        | 4 -> typedefof<Func<_,_,_,_,_>>
                        | 5 -> typedefof<Func<_,_,_,_,_,_>>
                        | 6 -> typedefof<Func<_,_,_,_,_,_,_>>
                        | 7 -> typedefof<Func<_,_,_,_,_,_,_,_>>
                        | 8 -> typedefof<Func<_,_,_,_,_,_,_,_,_>>
                        | 9 -> typedefof<Func<_,_,_,_,_,_,_,_,_,_>>
                        | _ -> raise (InvalidOperationException())
        let genericTypes = Array.concat [ parameters; [| retVal |] ] // the return value is the last generic parameter
        let bound = unbound.MakeGenericType(parameters)
        bound

let mapMember (tb,ctorIL) (duck, thing) =
    match duck with
    | Method (m) ->
        raise (NotImplementedException())
        (tb, ctorIL)       
    | Event (e) ->
        raise (NotImplementedException())
        (tb, ctorIL)
    | Property (p) ->
        raise (NotImplementedException())
        (tb, ctorIL)
    | Field(_)
    | Other -> raise (InvalidOperationException())

exception DuckExceptionMemberMapping of Member array

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
    let tryMapping = duckMembers
                  |> Seq.map (fun x -> (x, findDuckMemberInThing x thingMembers))
                  |> Seq.toList
    //see if the mapping was successfull
    let mappingSuccess = tryMapping |> Seq.forall (fun (d,t) -> t.IsSome)
    if not mappingSuccess then
        let missingMembers = tryMapping |> Seq.where (fun (d,t) -> t.IsNone)
                             |> Seq.map (fun (d,t) -> d) |> Seq.toArray
        raise (DuckExceptionMemberMapping(missingMembers))
    //we are still alive, so the mapping was a success
    let mapping = tryMapping |> Seq.map (fun (a,b)->(a,b.Value))
    let assemblyName = sprintf "%s_%s_%s" duckType.Name thingType.Name (Guid.NewGuid().ToString())
    let asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName),
                                                            Emit.AssemblyBuilderAccess.RunAndSave)
    let dynMod = asm.DefineDynamicModule("MainModule", assemblyName + ".mod.dll", true)
    let tb = dynMod.DefineType(sprintf "__%s_duck" duckType.Name, GeneratedTypeAttributes)
    let ctor = tb.DefineConstructor(ConstructorAttributes, CallingConventions.HasThis, [| thingType |])
    let ctorIL = ctor.GetILGenerator()

    //implement all members
    //methods must be implemented prior to properties, as a property depends on the get_ set_ methods
    let sorted = mapping |> Seq.sortBy (fun (m,_) -> match m with | Method (_) -> 0 | Event (_) -> 1 | Field (_) -> 2 | Property (_) -> 3 | Other -> raise (InvalidOperationException()) )
    let finalState = sorted |> Seq.fold mapMember (tb, ctorIL)

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

