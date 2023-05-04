#load "Books.fsx"
#load "Sections.fsx"
#load "Elements.fsx"
#load "PerseusXmlParsing.fsx"
#load "PerseusIds.fsx"

open System.IO
open System.Text.RegularExpressions
open System.Linq
open System.Xml.Linq
open System.Xml

open Books
open Elements
open Sections

let private regexReplace (pattern: string) (replacement: string) (input: string) =
    Regex.Replace(input, pattern, replacement)

let private regexReplaceWith (pattern: string) (replacement: MatchEvaluator) (input: string) =
    Regex.Replace(input, pattern, replacement)

let private replaceWithReference (m: Match) =
    let target = m.Groups[1].Value
    let ref = m.Groups[2].Value
    let htmlRef = PerseusIds.toHtmlRef target
    $"<a class=\"perseus-ref\" href=\"{htmlRef}\">{ref}</a>"

let private replaceWithForeignSpan (m: Match) =
    let lang = m.Groups[1].Value
    $"<span class=\"perseus-foreign-{lang}\">"

let private replaceHiWithSpan (m: Match) =
    let style = m.Groups[1].Value
    let text = m.Groups[2].Value
    $"<span class=\"perseus-{style}\">{text}</span>"

let private replaceHiWithDiv (m: Match) =
    let text = m.Groups[1].Value
    $"<div class=\"perseus-center\">{text}</div>"

let private removeNodesByName (name: string) (input: string) =
    // printfn "%s" input
    let xDoc = XDocument.Parse($"<div>{input}</div>", LoadOptions.PreserveWhitespace)
    xDoc.Descendants().Where(fun d -> d.Name.LocalName = name).Remove()
    let result = xDoc.ToString(SaveOptions.DisableFormatting)
    result.Substring("<div>".Length, result.Length - "<div></div>".Length)

let private failOnUnrecognizedElement (input: string) =
    let recognizedElements = [ "div"; "p"; "span"; "a"; "h1"; "h2"; "h3" ]
    use rdr = XmlReader.Create(new StringReader($"<div>{input}</div>"))

    while rdr.Read() do
        if rdr.NodeType = XmlNodeType.Element then
            if not (Seq.contains rdr.Name recognizedElements) then
                failwith $"Unrecognized element '{rdr.Name}' in\n{input}"

    input

let cleanHtml (raw: string) =
    raw
    |> regexReplace "<div3 " "<div class=\"perseus-div3\" "
    |> regexReplace "</div3>" "</div>"
    |> regexReplace "</div3>" "</div>"
    |> regexReplace "<div4 .*?type=\"Enunc\"" "<div class=\"perseus-enunc\""
    |> regexReplace "<div4 .*?type=\"Proof\"" "<div class=\"perseus-proof\""
    |> regexReplace "<div4 .*?type=\"QED\"" "<div class=\"perseus-qed\""
    |> regexReplace "<div4 .*?type=\"porism\"" "<div class=\"perseus-porism\""
    |> regexReplace "<div4 .*?type=\"lemma\"" "<div class=\"perseus-lemma\""
    |> regexReplace "</div4>" "</div>"
    // |> regexReplace "<head>" "<div class=\"perseus-head\">"
    // |> regexReplace "</head>" "</div>"
    |> removeNodesByName "head"
    |> regexReplace "<figure />" ""
    |> regexReplace "<emph>" "<span class=\"perseus-emph\">"
    |> regexReplace "</emph>" "</span>"
    |> regexReplace "<term>" "<span class=\"perseus-term\">"
    |> regexReplace "</term>" "</span>"
    |> regexReplace "<title>" "<span class=\"perseus-title\">"
    |> regexReplace "</title>" "</span>"
    |> regexReplace "<trailer>" "<span class=\"perseus-trailer\">"
    |> regexReplace "</trailer>" "</span>"
    |> regexReplace "<pb n=\"\d+\" />" ""
    |> regexReplace "<lb n=\"\d+\" />" ""
    |> regexReplaceWith "(?s)<hi rend=\"(bold|ital)\">(.+?)</hi>" replaceHiWithSpan
    |> regexReplaceWith "(?s)<hi rend=\"center\">(.+?)</hi>" replaceHiWithDiv
    |> regexReplace "<p>" "<p class=\"perseus-p\">"
    |> regexReplace "<quote>" "“"
    |> regexReplace "</quote>" "”"
    |> regexReplaceWith "<ref target=\"([\w\.]+?)\" targOrder=\"U\">([\w\.,\- ]+?)<\/ref>" replaceWithReference
    |> regexReplaceWith "<foreign lang=\"(\w+?)\">" replaceWithForeignSpan
    |> regexReplace "</foreign>" "</span>"
    |> removeNodesByName "note"
    |> failOnUnrecognizedElement

let generateHtml (e: Element) =
    let summary =
        e.SummaryRaw
        |> Option.map cleanHtml
        |> Option.defaultValue "<!-- no summary -->"

    // let proof =
    e.ProofRaw |> Option.map cleanHtml |> Option.defaultValue "<!-- no body -->"

    // let conclusion =
    //     e.ConclusionRaw
    //     |> Option.map cleanHtml
    //     |> Option.defaultValue "<!-- no conclusion -->"

    // let definition =
    //     e.DefinitionRaw
    //     |> Option.map cleanHtml
    //     |> Option.defaultValue "<!-- no definition -->"

    // let bookRomanNumeral = BookNumber.toRomanNumeral e.Book.Number

    // <h1 class="perseus-book">Book {bookRomanNumeral}.</h1>
    // <h2 class="perseus-section">{e.Section.SectionType} {e.Index}</h2>
    // <div class="perseus-definition">
    //     {definition}
    // </div>
    // <div class="perseus-summary">
    //     {summary}
    // </div>
    // <div class="perseus-conclusion">
    //     {conclusion}
    // </div>
    // <div class="perseus-proof">
    // </div>
//     $"""
// <div class="perseus-element">
//         {proof}
// </div>
//     """

let createOutputFile (e: Element) =
    try
        let b =
            match e.Book.Number with
            | BookNumber b -> b

        // let p = e.Index
        let p = if e.Index = 0 then e.Section.Index else e.Index

        let s =
            match e.Section.SectionType with
            | Definition -> "def"
            | Proposition -> "prop"
            | CommonNotion -> "cn"
            | Postulate -> "post"
            | Lemma -> "lemma"
            | Porism -> "porism"

        let html = generateHtml e

        let filename = $"html/book%02d{b}%s{s}%02d{p}.html"
        printfn "%s" filename

        Directory.CreateDirectory("html") |> ignore

        File.WriteAllText(filename, html)

    with ex ->
        printfn "%s" ex.Message
        printfn "----"
        printfn "%A" e.ProofRaw
        reraise ()

PerseusXmlParsing.streamPropositions "./Perseus_text_1999.01.0086.xml"
// |> Seq.skip 0
// |> Seq.take 100
|> Seq.iter createOutputFile
