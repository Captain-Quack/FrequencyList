module Utils
open FSharp.Text.RegexProvider
open type System.Text.RegularExpressions.RegexOptions    
open System.Text
open FSharpPlus



open System.Text.RegularExpressions
open FSharpPlus.Control

let WeirdQuotes = Regex("""["“‟❝”❞]""", Compiled)
let CombiningDiacriticals = Regex("""[\u0300-\u036f]""", Compiled)
let ParenthesesS = Regex(@"\(s\)", Compiled)
let DashClass = Regex(@"\p{Pd}", Compiled)
let BracketContent = Regex(@"(?<=\[)[^\]]*(?=\])", Compiled)
let ParenthesesContent = Regex(@"(?<=\()[^)]*(?=\))", Compiled)
let Parentheses = Regex(@"\([^)]*\)", Compiled)
let Brackets = Regex(@"\[[^\]]*\]", Compiled)
let ItalicsContent = Regex(@"<i>(.*?)<\/i>", Compiled)
let AngleBraces = Regex(@"<[^>]*>", Compiled)
let MultipleSpaces = Regex(@"\s{2,}", Compiled)
let HtmlTags = Regex(@"<.*?>", Compiled)
let ExtractQuotes = Regex("(?<=\")[^\"]*(?=\")", Compiled)
let ExtractUnderlining = Regex("(?<=<u>)[^<]*(?=</u>)", Compiled)
let NonAlphaNumeric = Regex(@"[^a-zA-Z0-9\s\\.']", Compiled)
let getMain = Regex(@"<b><u>(.*)</u></b>" , RegexOptions.Compiled ||| RegexOptions.IgnoreCase)                               
let orOrAccept = Regex(@"^\s*((or)|(accept))", RegexOptions.Compiled)
let orPart = Regex(@",? or |, ", Compiled ) 
let either = Regex(@"[[(]accept ((either)|(any))")
let directives = Regex(@"^((or)|(prompt)|(prompt on)|(antiprompt)|(antiprompt on)|(anti-prompt)|(anti-prompt on)|(accept)|(do not accept)|(do not accept or prompt on)|(do not accept))", Compiled ||| ECMAScript)
let directivesContent = Regex(@"\s+((or)|(prompt)|(prompt on)|(antiprompt)|(antiprompt on)|(anti-prompt)|(anti-prompt on)|(accept)|(do not accept)|(do not accept or prompt on)|(do not accept)).*", Compiled)


let inline replace (pattern: Regex) (replacement: string) (input: string) = pattern.Replace(input, replacement)
let inline omit (pattern: Regex) (input: string) = pattern.Replace(input, System.String.Empty)



let sb = StringBuilder()  
                                                  
let extractUnderlining (input: string) =
    let matches = Regex.Matches(input, "(?<=<u>)[^<]*(?=</u>)")
    if matches.Count > 0 then
        matches
        |> Seq.cast<Match>
        |> Seq.map (fun m -> m.Value)
        |> String.concat " "
        |> omit HtmlTags
        |> _.Trim()

    else
        omit HtmlTags input


let extractKeyWords (input: string) : string =
    let requiredWords = 
        extractUnderlining(input).Split(' ') 
        |> Array.toList

    let keywords =
        input.Split(' ')
        |> Array.filter (fun token -> token.Length > 0)
        |> Array.filter (fun token -> Regex.IsMatch(token, @"</?u>", RegexOptions.Compiled) || List.contains token requiredWords)
        |> String.concat " "

    HtmlTags.Replace(keywords.Trim(), "")

let extractQuotes (input: string) : string =
    let matches =
        ExtractQuotes.Matches(input)
        |> Seq.map (fun m -> m.Value.Trim())
        |> Seq.toList

    if matches.IsEmpty then
        input
    else
        String.concat " " matches

let getAbbreviation (input: string) : string =
    sb.Clear() |> ignore   
    input.Split(' ')
    |> Array.filter (fun token -> token.Length > 0)
    |> Array.iter (fun token ->
        let cleanToken = HtmlTags.Replace(token, "")
        match cleanToken |> Seq.tryHead with
        | Some ch -> sb.Append(ch) |> ignore
        | None -> ())
    sb.ToString().Trim()


