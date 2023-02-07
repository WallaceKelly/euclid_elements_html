type BookNumber = BookNumber of int

module BookNumber =
    let private romanNumerals =
        [| ""
           "I"
           "II"
           "III"
           "IV"
           "V"
           "VI"
           "VII"
           "VIII"
           "IX"
           "X"
           "XI"
           "XII"
           "XIII" |]

    let fromInt n =
        if n < 1 || n > 13 then
            failwith $"Book number {n} is out of range"
        else
            BookNumber n

    let toRomanNumeral =
        function
        | BookNumber n -> romanNumerals[n]

    let fromString (str: string) =
        [ 13; 12; 11; 9; 10; 8; 7; 6; 4; 5; 3; 2; 1 ]
        |> Seq.choose (fun n ->
            if str.Contains(n.ToString()) then
                Some(BookNumber n)
            else
                None)
        |> Seq.tryHead
        |> Option.defaultWith (fun _ -> failwith $"Cannot parse roman numeral from '${str}'.")

type Book =
    { Number: BookNumber; TitleRaw: string }
