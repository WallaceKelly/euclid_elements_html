type SectionType =
    | Definition
    | CommonNotion
    | Postulate
    | Proposition

module SectionType =
    let fromHeadTitle (str: string) =
        match str.ToLower() with
        | s when s.Contains("def") -> Definition
        | s when s.Contains("com") -> CommonNotion
        | s when s.Contains("pro") -> Proposition
        | s when s.Contains("pos") -> Postulate
        | s -> failwith $"Could not parse section type for '{str}'."

type Section =
    { Index: int
      TitleRaw: string
      SectionType: SectionType }
