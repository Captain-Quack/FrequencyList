module CleanAnswer
open FSharpPlus
open System.Text
open Utils
open System.Text.RegularExpressions

let formatAnswer =   replace WeirdQuotes "\""
                  >> replace ParenthesesS "s"
                  >> omit Brackets
                  >> replace DashClass "-"
                  >> omit CombiningDiacriticals
                  >> String.normalize NormalizationForm.FormKD
                  >> String.removeDiacritics
                  >> replace MultipleSpaces " "
                  >> String.trimWhiteSpaces


let stripSanitized = replace WeirdQuotes "\""
                  >> replace CombiningDiacriticals ""
                  >> replace ParenthesesS "s"
                  >> replace DashClass "-"       
                  >> String.normalize NormalizationForm.FormKD
                  >> String.removeDiacritics
                  >> replace Brackets ""
                  >> replace Parentheses ""
                  >> replace AngleBraces ""
                  >> replace MultipleSpaces " "  
                  >> String.trimWhiteSpaces



let splitMainAnswer (input: string) : (string * string) =

    let bracketsSubAnswer =
        BracketContent.Matches(input)
        |> Seq.map _.Value
        |> Seq.tryLast
        |> Option.defaultValue ""

    let parenthesesSubAnswer = 
        ParenthesesContent.Matches(input)
        |> Seq.map _.Value
        |> Seq.tryLast
        |> Option.defaultValue ""
    
    let mainAnswer = (omit Parentheses >> omit Brackets) input 
    
    if bracketsSubAnswer.Length <> 0 then
        mainAnswer, bracketsSubAnswer
    else
        let directives = ["or"; "prompt"; "antiprompt"; "anti-prompt"; "accept"; "reject"; "do not accept"]
        if directives |> List.exists (parenthesesSubAnswer.StartsWith) then
            mainAnswer, parenthesesSubAnswer
        else
            mainAnswer, ""



let getEquivalents (actual: string) (str: string) : string list = 
     
     let equivalents = ResizeArray<string>()
     equivalents.Add((formatAnswer >> (omit HtmlTags)) actual)
     let (mainAnswer, subAnswer) = splitMainAnswer str
     let mainAnswers = 
        let main = (mainAnswer.Split(" or ")) |> Array.map _.Trim() |> Array.filter (fun x -> x.Length <> 0)
        // generate alternate for answers with periods, one with a period in between and one without
        let woPeriod = mainAnswer.Replace(".", "")

        if woPeriod <> mainAnswer then
            Array.append main [|mainAnswer.Replace(".", ""); mainAnswer.Replace(".", " ")|]
        else
            main

     
     Array.iter (fun x -> equivalents.AddRange([extractUnderlining x; extractKeyWords x; extractQuotes x])) mainAnswers



     // can we make an abbreviation
     let abbreviation = (getAbbreviation mainAnswer)
     if abbreviation.Length >= 3 then
         equivalents.Add(getAbbreviation mainAnswer)

     // can we make an abbriviation out of the underlined
     let underlinedAbbreviation = (getAbbreviation (extractUnderlining mainAnswer)) 
     if underlinedAbbreviation.Length >= 3 then
         equivalents.Add(underlinedAbbreviation)
     

     if either.IsMatch(str) then 
       equivalents.AddRange(equivalents[0].Split(' '))
      

     for subClause in subAnswer |> String.split [";"] |> Seq.map String.trimWhiteSpaces do
       
        if subClause.StartsWith("accept") then
           

           let clause = directives.Replace(subClause, "").Trim()
           
           let answers = orPart.Split(clause) 
                         |> Array.map _.Trim() 
                         |> Array.filter (System.String.IsNullOrEmpty >> not)
           Array.iter (fun x -> equivalents.AddRange([extractUnderlining x; extractKeyWords x; extractQuotes x])) answers 
        
     let e = equivalents
            // split " or " into two 
            |> Seq.map _.Trim()
            |> Seq.distinct    
            
            |> Seq.toList
     e

        


            
    
     



    



