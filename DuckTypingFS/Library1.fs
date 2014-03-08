
module DuckTypingFS

open System
open System.Reflection

type MemberSignature = 
    | MethodSignature of returnType : Type * name : string * parameters : Type array
    | PropertySignature of propertyType : Type * name : string
    | EventSignature of signature : MemberSignature * name : string
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
        getMethodSignature m
    | MemberTypes.Property ->
        let p = memberInfo :?> PropertyInfo
        PropertySignature (name = p.Name, propertyType = p.PropertyType)
    | MemberTypes.Event ->
        let e = memberInfo :?> EventInfo
        let signature = getDelegateSignature e.EventHandlerType
        EventSignature (signature = signature, name = e.Name)
    | _ -> Other


