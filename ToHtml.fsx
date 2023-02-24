#load "Books.fsx"
#load "Sections.fsx"
#load "Elements.fsx"
#load "PerseusXmlParsing.fsx"
#load "PerseusIds.fsx"

open System.IO
open System.Text.RegularExpressions
open Books
open Sections
open Elements

let private regexReplace (pattern: string) (replacement: string) (input: string) =
    Regex.Replace(input, pattern, replacement)

let private regexReplaceWith (pattern: string) (replacement: MatchEvaluator) (input: string) =
    Regex.Replace(input, pattern, replacement)

let replaceWithReference (m: Match) =
    let target = m.Groups[1].Value
    let ref = m.Groups[2].Value
    let htmlRef = PerseusIds.toHtmlRef target
    $"<a class=\"perseus-ref\" href=\"{htmlRef}\">{ref}</a>"

let cleanHtml (raw: string) =
    raw
    |> regexReplace "<figure />" ""
    |> regexReplace "<emph>" "<span class=\"perseus-emph\">"
    |> regexReplace "</emph>" "</span>"
    |> regexReplace "<term>" "<span class=\"perseus-term\">"
    |> regexReplace "</term>" "</span>"
    |> regexReplace "<pb n=\"\d+\" />" ""
    |> regexReplace "<lb n=\"\d+\" />" ""
    |> regexReplace "<hi rend=\"center\">" "<div class=\"perseus-center\">"
    |> regexReplace "</hi>" "</div>"
    |> regexReplace "<p>" "<p class=\"perseus-p\">"
    |> regexReplaceWith "<ref target=\"([\w\.]+)\" targOrder=\"U\">([\w\. ]+)<\/ref>" replaceWithReference

let generateHtml (e: Element) =
    let summary =
        e.SummaryRaw
        |> Option.map cleanHtml
        |> Option.defaultValue "<!-- no summary -->"

    let proof =
        e.ProofRaw |> Option.map cleanHtml |> Option.defaultValue "<!-- no body -->"

    let conclusion =
        e.ConclusionRaw
        |> Option.map cleanHtml
        |> Option.defaultValue "<!-- no conclusion -->"

    let definition =
        e.DefinitionRaw
        |> Option.map cleanHtml
        |> Option.defaultValue "<!-- no definition -->"

    let bookRomanNumeral = BookNumber.toRomanNumeral e.Book.Number

    $"""
<div class="perseus-element">
    <h1 class="perseus-book">Book {bookRomanNumeral}.</h1>
    <h2 class="perseus-section">{e.Section.SectionType} {e.Index}</h2>
    <div class="perseus-definition">
        {definition}
    </div>
    <div class="perseus-summary">
        {summary}
    </div>
    <div class="perseus-proof">
        {proof}
    </div>
    <div class="perseus-conclusion">
        {conclusion}
    </div>
</div>
    """

let createOutputFile (e: Element) =
    let b =
        match e.Book.Number with
        | BookNumber b -> b

    let p = e.Index

    let s =
        match e.Section.SectionType with
        | Definition -> "def"
        | Proposition -> "prop"
        | CommonNotion -> "cn"
        | Postulate -> "post"

    let html = generateHtml e

    let filename = $"html/book%02d{b}%s{s}%02d{p}.html"
    printfn "%s" filename

    Directory.CreateDirectory("html") |> ignore

    File.WriteAllText(filename, html)

PerseusXmlParsing.streamPropositions "./Perseus_text_1999.01.0086.xml"
|> Seq.take 50
|> Seq.iter createOutputFile
