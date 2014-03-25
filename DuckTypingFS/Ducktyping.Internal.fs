
module DuckTypingFSInternal

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
    yield source
    for child in extract(source) do
        yield! flatten child extract
  }

let areMembersEqual a b =
    match a,b with
    | Method m1, Method m2 -> 
        let p1 = m1.GetParameters() |> Array.map (fun p -> p.ParameterType)
        let p2 = m2.GetParameters() |> Array.map (fun p -> p.ParameterType)
        m1.Name = m2.Name  && m1.ReturnType = m2.ReturnType && (Seq.compareWith (fun aa bb -> if aa = bb then 0 else 5)  p2 p1) = 0
    | Property p1, Property p2 ->
        p1.PropertyType = p2.PropertyType && p1.Name = p2.Name
    | Field f1, Field f2 ->
        f1.FieldType = f2.FieldType && f1.Name = f2.Name
    | _ -> false

let findDuckMemberInThing (duckMember : Member) thingMembers =
    let d = duckMember
    thingMembers |> Seq.tryFind (fun t -> areMembersEqual t d)

let rec getAllMembers_r (tp : Type) =  seq{
    let membersOfThis = tp.GetMembers(BindingFlags.FlattenHierarchy ||| BindingFlags.Instance ||| BindingFlags.NonPublic ||| BindingFlags.Public)
                        |> Seq.map (fun m -> (toMember m))
    yield! membersOfThis
    for intf in tp.GetInterfaces() do
        yield! getAllMembers_r intf
    }

let getAllMembers (tp : Type) =
    getAllMembers_r tp
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
        let bound = unbound.MakeGenericType(genericTypes)
        bound


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

let mapMember ((tb:TypeBuilder),(ctorIL:ILGenerator),(methods:MethodBuilder list),(duckField:FieldBuilder),delegates,isTypePublic) (duck, thing) =
    match thing with
    | Method (m) ->
        let methodProps = InterfaceImplementationAttributes
        let paramTypes = m.GetParameters() |> Seq.map (fun p -> p.ParameterType) |> Seq.toArray
        let mb = tb.DefineMethod(m.Name, methodProps, m.ReturnType,  paramTypes)
        let mIL = mb.GetILGenerator()
        mIL.Emit(OpCodes.Ldarg_0)
        if not m.IsPublic || not isTypePublic then
            //implement a backing delegate and call it
            let delegateType = getDelegateType m
            let fb = tb.DefineField("_backing_" + m.Name, delegateType, FieldAttributes.Public)
            let invoke = fb.FieldType.GetMethod("Invoke")
            for i in 1 .. paramTypes.Length do
                mIL.Emit(OpCodes.Ldarg, i)
            mIL.Emit(OpCodes.Ldfld, fb)
            mIL.EmitCall(OpCodes.Callvirt, invoke, null)
            mIL.Emit(OpCodes.Ret)
            (tb, ctorIL, mb :: methods, duckField, (fb,m) :: delegates, isTypePublic)
        else
            //directly call the method
            mIL.Emit(OpCodes.Ldfld, duckField)
            for i in 1 .. paramTypes.Length do
                mIL.Emit(OpCodes.Ldarg, i)
            mIL.Emit(OpCodes.Call, m)
            mIL.Emit(OpCodes.Ret)
            (tb, ctorIL, mb :: methods, duckField, delegates, isTypePublic)
    | Event (e) ->
        raise (NotImplementedException())
    | Property (p) ->
        let pb = tb.DefineProperty(p.Name, PropertyAttributes.None, p.PropertyType, null)
        if p.CanRead then
            let getterName = "get_" + p.Name
            let getter = methods |> Seq.find (fun m -> m.Name = getterName)
            pb.SetGetMethod(getter)
        if p.CanWrite then
            let setterName = "set_" + p.Name
            let setter = methods |> Seq.find (fun m -> m.Name = setterName)
            pb.SetSetMethod(setter)
        (tb, ctorIL, methods, duckField, delegates, isTypePublic)
    | Field(_)
    | Other -> raise (InvalidOperationException())

exception DuckExceptionMemberMapping of Member array

let sanitizeFilename (file : string) =
    let invalidChars = System.IO.Path.GetInvalidFileNameChars()
    let mutable xx = file
    for c in invalidChars do
        xx <- xx.Replace(c, '_')
    xx

let toField f =
    match f with
    | Field f -> f
    | _ -> raise (InvalidOperationException())

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
                       |> sanitizeFilename
    let asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName),
                                                            Emit.AssemblyBuilderAccess.RunAndSave)
    let dynMod = asm.DefineDynamicModule("MainModule", assemblyName + ".mod.dll", true)
    let tb = dynMod.DefineType(sprintf "__%s_duck" duckType.Name, GeneratedTypeAttributes, typedefof<Object>, [| duckType |])
    let ctor = tb.DefineConstructor(ConstructorAttributes, CallingConventions.HasThis, [| thingType |])
    let ctorIL = ctor.GetILGenerator()

    //implement the duck field
    let duckField = tb.DefineField("__duck", duckType, FieldAttributes.Private)

    //implement all members
    //methods must be implemented prior to properties, as a property depends on the get_ set_ methods
    let sorted = mapping |> Seq.sortBy (fun (m,_) -> match m with | Method (_) -> 0 | Event (_) -> 1 | Field (_) -> 2 | Property (_) -> 3 | Other -> raise (InvalidOperationException()) )
    let (_,_,_,_,delegates,_) = sorted |> Seq.fold mapMember (tb, ctorIL, List.empty, duckField, List.empty, thingType.IsPublic)

    //emit the constructor
    ctorIL.Emit(OpCodes.Ldarg_0)
    ctorIL.Emit(OpCodes.Call, typedefof<Object>.GetConstructor(Type.EmptyTypes))
    ctorIL.Emit(OpCodes.Ldarg_0)
    ctorIL.Emit(OpCodes.Ldarg_1)
    ctorIL.Emit(OpCodes.Stfld, duckField)
    ctorIL.Emit(OpCodes.Ret)

    let generatedType = tb.CreateType()
    asm.Save(sprintf "%s.dll" assemblyName)
    let membersOfRealType = getAllMembers generatedType
    let realDelegates = delegates |> List.map (fun (a,b) -> (toField (findDuckMemberInThing (Field(a)) membersOfRealType).Value, b))
    (generatedType, realDelegates)
    

