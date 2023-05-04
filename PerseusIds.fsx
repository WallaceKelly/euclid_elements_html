module PerseusIds

// converts Perseus reference ids, like elem.1.42
// into HTML file references, like book01prop42

open System
open System.Text.RegularExpressions

let private IdRegex = "^elem\.(\d+)\.(\d+|def|p|l|c\.n\.|post)\.*(\d+)*$"

let isElementId (s: string) =
    (not (isNull s)) && Regex.IsMatch(s, IdRegex)

let parseElementId (s: string) =
    if not <| isElementId s then
        failwith $"{s} is not an element id."
    else
        let m = Regex.Match(s, IdRegex)
        let bookNum = Int32.Parse(m.Groups[1].Value)
        let (isProposition, elemNum) = Int32.TryParse(m.Groups[2].Value)
        let sectId = if isProposition then "prop" else m.Groups[2].Value
        let (_, sectNum) = Int32.TryParse(m.Groups[3].Value)
        (bookNum, elemNum, sectId, sectNum)

let toHtmlRef (s: string) =

    let proposition (m: Match) =
        let book = Int32.Parse(m.Groups[1].Value)
        let prop = Int32.Parse(m.Groups[2].Value)
        $"book%02d{book}prop%02d{prop}"

    let commonNotion (m: Match) =
        let book = Int32.Parse(m.Groups[1].Value)
        let cn = Int32.Parse(m.Groups[2].Value)
        $"book%02d{book}cn%02d{cn}"

    let postulate (m: Match) =
        let book = Int32.Parse(m.Groups[1].Value)
        let post = Int32.Parse(m.Groups[2].Value)
        $"book%02d{book}post%02d{post}"

    let definition (m: Match) =
        let book = Int32.Parse(m.Groups[1].Value)
        let def = Int32.Parse(m.Groups[2].Value)
        $"book%02d{book}def%02d{def}"

    let definition2 (m: Match) =
        let book = Int32.Parse(m.Groups[1].Value)
        let def = Int32.Parse(m.Groups[2].Value)
        $"book%02d{book}def%02d{def}"

    // TODO: create a link to the porism within the document
    let porism (m: Match) =
        let book = Int32.Parse(m.Groups[1].Value)
        let prop = Int32.Parse(m.Groups[2].Value)
        $"book%02d{book}prop%02d{prop}"

    // TODO: create a link to the lemma within the document
    let lemma (m: Match) =
        let book = Int32.Parse(m.Groups[1].Value)
        let prop = Int32.Parse(m.Groups[2].Value)
        $"book%02d{book}prop%02d{prop}"

    let regexes =
        [ ("^elem\.(\d+)\.(\d+)$", proposition)
          ("^elem\.(\d+)\.c.n.(\d+)$", commonNotion)
          ("^elem\.(\d+)\.post\.(\d+)$", postulate)
          ("^elem\.(\d+)\.def\.(\d+)$", definition)
          ("^elem\.(\d+)\.def\.(\d+).(\d+)$", definition2)
          ("^elem\.(\d+)\.(\d+).p.(\d+)$", porism) 
          ("^elem\.(\d+)\.(\d+).l.(\d+)$", lemma) ]

    regexes
    |> Seq.map (fun (regex, convert) ->
        let m = Regex.Match(s, regex)
        if m.Success then Some(convert m) else None)
    |> Seq.choose id
    |> Seq.tryExactlyOne
    |> Option.defaultWith (fun _ -> failwith $"Cannot convert '{s}' to an HTML reference.")
