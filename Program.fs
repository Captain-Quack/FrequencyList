open System
open System.IO
open FSharp.Json
open System.Security.Cryptography
open Unions
open CleanAnswer
open System.Text
open System.Text.RegularExpressions
open Utils


// close excel file
System.Diagnostics.Process.GetProcessesByName("EXCEL") |> Array.iter (fun x -> x.Kill())

type Packet = {
    [<JsonField(Transform=typeof<Transforms.OidConverter>)>] _id: Oid
    name: string
    [<JsonField(Transform=typeof<Transforms.SByteConverter>)>] number: sbyte
}

type Set = {
    [<JsonField(Transform=typeof<Transforms.OidConverter>)>] _id: Oid
    name: string
    [<JsonField(Transform=typeof<Transforms.IntConverter>)>] year: int
    standard: bool
}

type JsonObject = {
    [<JsonField(Transform=typeof<Transforms.OidConverter>)>] _id: Oid
    question: string
    answer: string
    [<JsonField(Transform=typeof<Transforms.SubcategoryConverter>)>] subcategory: Subcategory
    [<JsonField(Transform=typeof<Transforms.CategoryConverter>)>] category: Category
    [<JsonField(Transform=typeof<Transforms.AlternateSubcategoryConverter>)>] alternate_subcategory: AlternateSubcategory option
    packet: Packet
    set: Set
    [<JsonField(Transform=typeof<Transforms.DateConverter>)>] createdAt: DateTime
    [<JsonField(Transform=typeof<Transforms.DateConverter>)>] updatedAt: DateTime
    [<JsonField(Transform=typeof<Transforms.SByteConverter>)>] difficulty: sbyte
    [<JsonField(Transform=typeof<Transforms.SByteConverter>)>] number: sbyte
    answer_sanitized: string
    question_sanitized: string
}

// Function to process a single line of JSON and parse it into a JsonObject
let processLineToObject (line: string) = Json.deserializeEx<JsonObject> (JsonConfig.create()) line

           
let [<Literal>] path = @"C:\Users\mason\Downloads\tossups.json"

let getAllObjects (filePath: string) = File.ReadAllLines(filePath) |> Array.map processLineToObject
let getAllObjectsFiltered (filePath: string) (field: string) =
    File.ReadLines(filePath)
    |> Seq.filter (fun x -> x.Contains(field))
    |> Seq.map processLineToObject

             

let jsonObjects = getAllObjectsFiltered path "alternate_subcategory"


let mutable all = 
    jsonObjects
    |> Seq.filter (fun x -> Option.get x.alternate_subcategory = Architecture) 
    |> Seq.map (fun x -> (x.answer_sanitized |> stripSanitized |>(omit NonAlphaNumeric >> _.Trim()) |> omit directivesContent), x.answer |> formatAnswer) 
open FSharpPlus
open System.Collections.Generic

let mutable referance = 
    all
    |> Seq.map (fun (x, y) -> (x, (getEquivalents x y) |> List.map formatAnswer |> List.distinct)) 
    |> List.ofSeq
let flattenedRef = referance |> Seq.collect (fun (mainName, aliases) ->  aliases |> Seq.map (fun alias -> (alias, mainName)))
        
let mutable finalFrequency =  new Collections.Generic.Dictionary<string, int>()
for (_, aliases) in referance do
    // find the first item in aliases that is equal to the first item in flattenedRef
    let found: string = flattenedRef |> Seq.find (fun (x, y) -> List.contains x aliases) |> snd
    Console.WriteLine(found)
    match finalFrequency.TryGetValue found with
    | true, count -> finalFrequency.[found] <- count + 1
    | false, _ -> finalFrequency.[found] <- 1

           

open ClosedXML.Excel
open System.Collections.Generic

let writeDictionaryToExcel (data : Dictionary<string, int>) (filePath : string) (title : string) =
    // Create a new Excel workbook
    let workbook : XLWorkbook = new XLWorkbook()
    let worksheet = workbook.Worksheets.Add("Sheet1")
    
    // Set the title
    let titleCell = worksheet.Cell("A1")
    titleCell.Value <- title
    titleCell.Style.Font.Bold <- true
    titleCell.Style.Font.FontSize <- 18.0
    titleCell.Style.Fill.BackgroundColor <- XLColor.Aqua
    titleCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
    worksheet.Range("A1:B1").Merge()  // Merge cells for the title to span
    
    // Set the headers
    worksheet.Cell("A2").Value <- "Key"
    worksheet.Cell("B2").Value <- "Value"
    
    // Iterate over the dictionary and write data to the worksheet
    let mutable row = 3  // Start from row 3 to accommodate the title
    for kvp in data do
        worksheet.Cell(row, 1).Value <- kvp.Key
        worksheet.Cell(row, 2).Value <- kvp.Value
        row <- row + 1
    
    // Adjust column widths
    worksheet.Column(1).AdjustToContents()
    worksheet.Column(2).AdjustToContents()
    
    // Save the workbook
    workbook.SaveAs(filePath)

open System.Linq
writeDictionaryToExcel (finalFrequency.OrderByDescending(_.Value).ToDictionary()) "C:/Users/mason/Downloads/Architecture.xlsx" "Architecture"
// open excel
