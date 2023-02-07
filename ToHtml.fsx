#load "Books.fsx"
#load "Sections.fsx"
#load "Elements.fsx"
#load "PerseusXmlParsing.fsx"

open Books
open Sections
open Elements
open System.IO
open System.Xml

let parseSummary (doc: XmlDocument) =
    doc.SelectSingleNode("/div3/div4[@type='Enunc']")
    |> Option.ofObj
    |> Option.map (fun s -> s.InnerXml)

let parseBody (doc: XmlDocument) =
    doc.SelectSingleNode("/div3/div4[@type='Proof']")
    |> Option.ofObj
    |> Option.map (fun b -> b.InnerXml)

let parseConclusion (doc: XmlDocument) =
    doc.SelectSingleNode("/div3/div4[@type='QED']")
    |> Option.ofObj
    |> Option.map (fun c -> c.InnerXml)

let generateHtml (e: Element) =
    let doc = new XmlDocument()
    doc.LoadXml(e.TextRaw)
    let summary = parseSummary doc |> Option.defaultValue "<!-- missing summary -->"
    let body = parseBody doc |> Option.defaultValue "<!-- missing body -->"

    let conclusion =
        parseConclusion doc |> Option.defaultValue "<!-- missing conclusion -->"

    let bookRomanNumeral = BookNumber.toRomanNumeral e.Book.Number

    $"""
        <div>
            <h1>Book {bookRomanNumeral}.</h1>
            <h2>{e.Section.SectionType} {e.Index}</h2>
            <div>
                {summary}
            </div>
            <div>
                {body}
            </div>
            <div>
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
        | Postulate -> "pos"

    let html = generateHtml e

    let filename = $"html/book%02d{b}%s{s}%02d{p}.html"
    printfn "%s" filename

    Directory.CreateDirectory("html") |> ignore

    File.WriteAllText(filename, html)
// File.AppendAllText(filename, $"\n\n{e.text}")

PerseusXmlParsing.streamPropositions "./Perseus_text_1999.01.0086.xml"
|> Seq.take 50
|> Seq.iter createOutputFile
