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

    let toInt =
        function
        | BookNumber n -> n

    let toRomanNumeral =
        function
        | BookNumber n -> romanNumerals[n]

type Book =
    { Number: BookNumber; TitleRaw: string }

module Book =

    let fromInt n =
        let bookNumber = BookNumber.fromInt n
        let bookTitle = $"Book {BookNumber.toRomanNumeral bookNumber}"

        { Number = bookNumber
          TitleRaw = bookTitle }
