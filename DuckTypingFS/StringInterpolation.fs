namespace Mvvm.FSharp

type StringInterpolationException =
    inherit System.Exception
    new () = {inherit System.Exception()}
    new (msg) = {inherit System.Exception(msg)}
    new (msg, inner:System.Exception) = {inherit System.Exception(msg, inner)}

module Internal =
    type FormatStringElement =
    | String of string
    | Hole of expression : string * formatter : string option

    let fail_if<'a when 'a :> System.Exception> condition (exn:unit->'a) =
        if condition then
            raise (exn())

    let rec parseFormatString (format : string) iStart iCurrent isInHole = [|
        match isInHole with
        | true ->
            // in a hole, so search for the end,  ...
            let iEnd = format.IndexOf('}', iCurrent)
            fail_if (iEnd = -1) (fun () -> StringInterpolationException(""))
            fail_if (iEnd = iCurrent) (fun () -> StringInterpolationException(""))
            // ... get the string between ...
            let between = format.Substring(iCurrent, iEnd-iCurrent).Trim()
            // ... and parse it. (either {expr} or {expr:format})
            let nColons = between |> Seq.where (fun c -> c = ':') |> Seq.length
            let (expr, formatter) =
                match nColons with
                | 0 -> (between, None)
                | 1 ->
                    let splitted = between.Split(':')
                    (splitted.[0].Trim(), Some (splitted.[1].Trim()))
                |_ -> raise(StringInterpolationException( "more than one ':'"))
            yield Hole(expr, formatter)
            yield! parseFormatString format (iEnd + 1) (iEnd + 1) false
        | false ->
            if format.Length = iCurrent then
                if iStart < iCurrent then
                    yield String(format.Substring(iStart))
            else
                let next = if format.Length > iCurrent + 1 then Some format.[iCurrent+1] else None
                let current = format.[iCurrent]
                let getUntilNow() = String(format.Substring(iStart, iCurrent-iStart))
                match (current, next) with
                | ('{',Some('{')) ->
                    if iCurrent > iStart then
                        yield getUntilNow()
                    yield String("{")
                    yield! parseFormatString format (iCurrent+2) (iCurrent+2) false
                | ('{', _) ->
                    if iCurrent > iStart then
                        yield getUntilNow()
                    yield! parseFormatString format (iCurrent+1) (iCurrent+1) true
                | ('}', Some('}')) ->
                    if iCurrent > iStart then
                        yield getUntilNow()
                    yield String("}")
                    yield! parseFormatString format (iCurrent+2) (iCurrent+2) false
                | ('}', _) -> raise (StringInterpolationException("unexpected '}'"))
                | _ -> yield! parseFormatString format iStart (iCurrent+1) false
    |]

open Internal
open System

type StringInterpolation =
    static member Do format (o:Object) =
        fail_if (format = null) (fun ()->ArgumentNullException("format"))
        fail_if (o = null) (fun ()->ArgumentNullException("o"))
        let info = parseFormatString format 0 0 false
        let outSb = System.Text.StringBuilder()
        for x in info do
            match x with
            | String(s) -> outSb.Append(s) |> ignore
            | Hole(expr, fmtopt) ->
                let value =
                        match fmtopt with
                        | Some(fmt) -> System.Web.UI.DataBinder.Eval(o, expr, fmt)
                        | None -> System.Web.UI.DataBinder.Eval(o, expr).ToString()
                outSb.Append(value) |> ignore

        outSb.ToString()
