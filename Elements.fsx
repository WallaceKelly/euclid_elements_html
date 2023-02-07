#load "Books.fsx"
#load "Sections.fsx"

open Books
open Sections

type Element =
    { index: int
      idRaw: string
      book: Book
      section: Section
      textRaw: string }
