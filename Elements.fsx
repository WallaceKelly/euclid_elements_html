#load "Books.fsx"
#load "Sections.fsx"

open Books
open Sections

type Element =
    { index: int
      id: string
      book: Book
      section: Section
      text: string }
