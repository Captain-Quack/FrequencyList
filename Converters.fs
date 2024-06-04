module Transforms 

open FSharp.Json
open System.Security.Cryptography
open System.Text.Json.Nodes

open Unions
open System
open System.Globalization

type OidConverter() = 
    interface  ITypeTransform with
        member _.targetType() = (fun _ -> typeof<Map<_, string>>)()
        member _.fromTargetType (value: obj) = 
            (value :?>Map<_, string>) 
            |> Map.toSeq
            |> Seq.exactlyOne
            |> snd
            |> string
            |> fun x -> new Oid(x)

        member _.toTargetType (value: obj) =
             NotImplementedException()


type IntConverter() = 
    interface  ITypeTransform with
        member _.targetType() = (fun _ -> typeof<Map<string, string>>)()

        member _.fromTargetType (value: obj) = 
            (value:?> Map<string, string>) 
            |> Map.toSeq
            |> Seq.exactlyOne
            |> snd
            |> fun x -> Convert.ToInt32(x)
        member _.toTargetType (value: obj) =
             NotImplementedException()

type SByteConverter() = 
    interface  ITypeTransform with
        member _.targetType() = (fun _ -> typeof<Map<string, string>>)()

        member _.fromTargetType (value: obj) = 
            (value:?> Map<string, string>) 
            |> Map.toSeq
            |> Seq.exactlyOne
            |> snd
            |> fun x -> Convert.ToSByte(x)
        member _.toTargetType (value: obj) =
             NotImplementedException()

type DateConverter() = 
    interface  ITypeTransform with
        member _.targetType() = (fun _ -> typeof<Map<string, Map<string, string>>>)()

        member _.fromTargetType (value: obj) = 

            (value:?> Map<string, Map<string, string>>) 
            |> Map.toSeq
            |> Seq.exactlyOne
            |> snd 
            |> Seq.exactlyOne
            |> _.Value
            |> fun x -> DateTimeOffset.FromUnixTimeMilliseconds(Int64.Parse(x)).DateTime
              

        member _.toTargetType (value: obj) =
             NotImplementedException()


type CategoryConverter() =
    interface ITypeTransform with
        member _.targetType() = typeof<Category>
        member _.fromTargetType (value: obj) =  box<Category> (value :?> Category)
        member _.toTargetType (value: obj) = NotImplementedException()
            
type SubcategoryConverter() = 
    interface ITypeTransform with
        member _.targetType() = typeof<Subcategory>
        member _.fromTargetType (value: obj) = box<Subcategory> (value :?> Subcategory)
        member _.toTargetType (value: obj) = NotImplementedException()
type AlternateSubcategoryConverter() = 
    interface ITypeTransform with
        member _.targetType() = typeof<AlternateSubcategory>
        member _.fromTargetType (value: obj) = box<AlternateSubcategory> (value :?> AlternateSubcategory)
        member _.toTargetType (value: obj) = NotImplementedException()