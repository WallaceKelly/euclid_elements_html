#load "Books.fsx"
#load "Sections.fsx"
#load "Elements.fsx"

open System.Xml
open Books
open Sections
open Elements

let readElement book section elementIndex (reader: XmlReader) =
    reader.MoveToContent() |> ignore
    let elementId = reader.GetAttribute("id")
    // printfn "%s" elementId
    let doc = new XmlDocument(PreserveWhitespace = true)
    doc.Load(reader)

    let summaryRaw =
        doc.SelectSingleNode("/div3/div4[@type='Enunc']")
        |> Option.ofObj
        |> Option.map (fun s -> s.InnerXml)

    let proofRaw =
        doc.SelectSingleNode("/div3/div4[@type='Proof']")
        |> Option.ofObj
        |> Option.map (fun b -> b.InnerXml)

    let conclusionRaw =
        doc.SelectSingleNode("/div3/div4[@type='QED']")
        |> Option.ofObj
        |> Option.map (fun c -> c.InnerXml)

    let definitionRaw =
        if Option.isSome proofRaw then
            None
        else
            doc.SelectSingleNode("/div3/p")
            |> Option.ofObj
            |> Option.map (fun c -> c.InnerXml)

    { Index = elementIndex
      IdRaw = elementId
      Book = book
      Section = section
      SummaryRaw = summaryRaw
      ProofRaw = proofRaw
      ConclusionRaw = conclusionRaw
      DefinitionRaw = definitionRaw
      BodyRaw = doc.OuterXml }

let readSection book sectionIndex (reader: XmlReader) =
    seq {
        let mutable elementIndex = 0
        reader.ReadToDescendant("head") |> ignore
        let sectionTitle = reader.ReadElementContentAsString()
        let sectionType = SectionType.fromHeadTitle sectionTitle

        let section =
            { Index = sectionIndex
              TitleRaw = sectionTitle
              SectionType = sectionType }
        // printfn "\t%s" sectionTitle

        while reader.ReadToFollowing("div3") do
            elementIndex <- elementIndex + 1
            use element = reader.ReadSubtree()
            yield readElement book section elementIndex element
    }

let readBook bookIndex (reader: XmlReader) =
    seq {
        let mutable sectionIndex = 0
        reader.ReadToDescendant("head") |> ignore
        let bookTitle = reader.ReadElementContentAsString()
        //printfn "%s" bookTitle
        let book =
            { TitleRaw = bookTitle
              Number = BookNumber bookIndex }

        while reader.ReadToFollowing("div2") do
            sectionIndex <- sectionIndex + 1
            use section = reader.ReadSubtree()

            yield! readSection book sectionIndex section
    }

let readBody (reader: XmlReader) =
    seq {
        let mutable bookIndex = 0

        while reader.ReadToFollowing("div1") do
            bookIndex <- bookIndex + 1
            use book = reader.ReadSubtree()
            yield! readBook bookIndex book
    }

let streamPropositions uri =
    seq {
        use reader = XmlReader.Create(uri: string)
        reader.MoveToContent() |> ignore
        reader.ReadToFollowing("body") |> ignore
        use body = reader.ReadSubtree()
        yield! readBody body
    }
