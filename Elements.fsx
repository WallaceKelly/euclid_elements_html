#load "Books.fsx"
#load "Sections.fsx"

open Books
open Sections

type Element =
    { Index: int
      IdRaw: string
      Book: Book
      Section: Section
      SummaryRaw: string option
      ProofRaw: string option
      ConclusionRaw: string option
      DefinitionRaw: string option
      BodyRaw: string }
