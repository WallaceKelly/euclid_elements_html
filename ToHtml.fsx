#load "Books.fsx"
#load "Sections.fsx"
#load "Elements.fsx"
#load "PerseusXmlParsing.fsx"
#load "PerseusIds.fsx"

open System.IO
open System.Text.RegularExpressions
open System.Xml
open Books
open Sections
open Elements
open System.Text

let private regexReplace (pattern: string) (replacement: string) (input: string) =
    Regex.Replace(input, pattern, replacement)

let private regexReplaceWith (pattern: string) (replacement: MatchEvaluator) (input: string) =
    Regex.Replace(input, pattern, replacement)

let replaceWithReference (m: Match) =
    let target = m.Groups[1].Value
    let ref = m.Groups[2].Value
    let htmlRef = PerseusIds.toHtmlRef target
    $"<a class=\"perseus-ref\" href=\"{htmlRef}\">{ref}</a>"

// problems
// 1. this needs a root element, so that is being added,
// but then it needs to be removed afterwards.
// 2. it is generating an error, but not just due to UTF-16
// 3. WriteNode writes the entire node and all children!
//
// Need a completely different approach?!
//
// Maybe this:
// https://stackoverflow.com/a/58217454/167920
//
let private removeNoteElements (input: string) =
    use rdr = XmlReader.Create(new StringReader($"<div>{input}</div>"))
    // use swt = new StringWriter()
    let sb = StringBuilder()
    // use ms = new MemoryStream()
    // use wrt = XmlWriter.Create(ms)
    // use wrt = XmlWriter.Create(swt)
    use wrt = XmlWriter.Create(sb, XmlWriterSettings(Encoding = Encoding.UTF8))
    // wrt.Settings.Encoding <- Encoding.UTF8

    while rdr.Read() do
        printfn $"NodeType = {rdr.NodeType}, Name = {rdr.Name}"

        if rdr.NodeType = XmlNodeType.Element && rdr.Name = "note" then
            printfn "Skipping a note"
            rdr.Skip()
        else
            wrt.WriteNode(rdr, false)

    wrt.Flush()
    //swt.ToString()
    //let result = Encoding.UTF8.GetString(ms.ToArray())
    let result =
        sb.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "")

    // printfn "'%s'" result
    result

let private failOnUnrecognizedElement (input: string) =
    let recognizedElements = [ "div"; "p"; "span"; "a"; "h1"; "h2"; "h3" ]
    use rdr = XmlReader.Create(new StringReader($"<div>{input}</div>"))


    while rdr.Read() do
        if rdr.NodeType = XmlNodeType.Element then
            if not (Seq.contains rdr.Name recognizedElements) then
                failwith $"Unrecognized element '{rdr.Name}'"

    input

let cleanHtml (raw: string) =
    raw
    |> regexReplace "<figure />" ""
    |> regexReplace "<emph>" "<span class=\"perseus-emph\">"
    |> regexReplace "</emph>" "</span>"
    |> regexReplace "<term>" "<span class=\"perseus-term\">"
    |> regexReplace "</term>" "</span>"
    |> regexReplace "<title>" "<span class=\"perseus-title\">"
    |> regexReplace "</title>" "</span>"
    |> regexReplace "<pb n=\"\d+\" />" ""
    |> regexReplace "<lb n=\"\d+\" />" ""
    |> regexReplace "<hi rend=\"center\">" "<div class=\"perseus-center\">"
    |> regexReplace "<hi rend=\"bold\">" "<div class=\"perseus-bold\">"
    |> regexReplace "<hi rend=\"ital\">" "<div class=\"perseus-ital\">"
    |> regexReplace "</hi>" "</div>"
    |> regexReplace "<p>" "<p class=\"perseus-p\">"
    |> regexReplaceWith "<ref target=\"([\w\.]+)\" targOrder=\"U\">([\w\. ]+)<\/ref>" replaceWithReference
    // |> removeNoteElements
    |> failOnUnrecognizedElement

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
|> Seq.skip 50
|> Seq.take 50
|> Seq.iter createOutputFile
