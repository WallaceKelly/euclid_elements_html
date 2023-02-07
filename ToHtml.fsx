#load "Books.fsx"
#load "Sections.fsx"
#load "Elements.fsx"
#load "PerseusXmlParsing.fsx"

open Books
open Sections
open Elements
open System.IO
open System.Xml

let generateHtml (e: Element) =
    let summary = e.SummaryRaw |> Option.defaultValue "<!-- no summary -->"
    let proof = e.ProofRaw |> Option.defaultValue "<!-- no body -->"
    let conclusion = e.ConclusionRaw |> Option.defaultValue "<!-- no conclusion -->"
    let definition = e.DefinitionRaw |> Option.defaultValue "<!-- no definition -->"

    let bookRomanNumeral = BookNumber.toRomanNumeral e.Book.Number

    $"""
<div>
    <h1>Book {bookRomanNumeral}.</h1>
    <h2>{e.Section.SectionType} {e.Index}</h2>
    <div>
        {definition}
    <div>
        {summary}
    </div>
    <div>
        {proof}
    </div>
    <div>
        {conclusion}
    </div>
</div>
<!--
{e.BodyRaw}
-->
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
