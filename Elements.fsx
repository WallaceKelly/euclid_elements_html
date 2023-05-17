#load "Books.fsx"
#load "Sections.fsx"

open Books
open Sections

type Element =
    { Index: int
      IdRaw: string
      Book: Book
      Section: Section
      ProofRaw: string option
      BodyRaw: string }
