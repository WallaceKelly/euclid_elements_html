#load "Books.fsx"
#load "Sections.fsx"
#load "Elements.fsx"
#load "PerseusIds.fsx"

open System.Xml
open Books
open Elements
open Sections

let private readElement (reader: XmlReader) =
    reader.MoveToContent() |> ignore
    let doc = new XmlDocument(PreserveWhitespace = true)
    doc.Load(reader)
    let div3 = doc.SelectSingleNode("/div3")
    let elementId = div3.Attributes.GetNamedItem("id").Value
    let (bookNum, elemNum, sectId, sectNum) = PerseusIds.parseElementId elementId

    { Index = elemNum
      IdRaw = elementId
      Book = Book.fromInt bookNum
      Section = Section.create sectId sectNum
      ProofRaw = Some(doc.InnerXml)
      BodyRaw = doc.OuterXml }

let private readElements (reader: XmlReader) =
    seq {
        while reader.ReadToFollowing("div3") do
            let idString = reader.GetAttribute("id")

            if PerseusIds.isElementId idString then
                yield (readElement <| reader.ReadSubtree())
    }

let streamPropositions uri =
    seq {
        use reader = XmlReader.Create(uri: string)
        reader.MoveToContent() |> ignore
        reader.ReadToFollowing("body") |> ignore
        use body = reader.ReadSubtree()
        yield! readElements body
    }
