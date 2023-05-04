type SectionType =
    | Definition
    | CommonNotion
    | Postulate
    | Proposition
    | Lemma
    | Porism

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

module Section =

    let create sectId sectNum =
        let sectionType =
            match sectId with
            | s when System.String.IsNullOrWhiteSpace(s) -> Proposition
            | s when s = "prop" -> Proposition
            | s when s = "post" -> Postulate
            | s when s = "def" -> Definition
            | s when s = "c.n." -> CommonNotion
            | s when s = "l" -> Lemma
            | s when s = "p" -> Porism
            | _ -> failwith $"The section id \'{sectId}\' is not recognized."

        { Index = sectNum
          TitleRaw = ""
          SectionType = sectionType }        
