# Key group EnterSymbolText

Nov 16, 2026. The Customized Symbol Text dialog, which edits the text of a symbol of the control descriptions in each
grammatical form of a description language: a grid with the columns Number (singular or plural), Gender, Case and
Text, three check boxes that add the rows of the plural, gender and case forms, and two combo boxes, the gender of the
symbol and the case into which the symbol puts the symbol that it modifies, whose first item is "(unchanged)".
claude-opus-5-5 at effort max made every line below, and each entry says so in its comment.

- **Translated**: 60 lines, in 26 languages. The new item "(unchanged)" was missing in every language. The case texts
  were missing in es, nl, nn-NO, pl, ro, ru, sl and uk, and most of the dialog in ru and uk.
- **Not needed**: 1 line. en-GB writes "(unchanged)" as the English does.
- **Corrected**: 69 lines, most of them after terms.csv. The translators did not see that the dialog is about grammar.
  - The column Number was a number in a sequence (bg, de, et, ja, nb-NO, nn-NO, pl, sl, uk, zh-CN, zh-TW) or a count
    (nl, sv): now the grammatical number, as "Число", "Numerus", "Liczba", 数.
  - Gender was the sex of a person (bg, cs, pl, sv, uk): now "Род", "Rod", "Rodzaj", "Genus", "Рід".
  - Case was the case of letters (bg, nb-NO, nl), a task (da), a choice (et), a box (fr), the English (sv) or a case
    study (zh-CN, zh-TW): now the grammatical case. The label of the case of the modified symbol lacked the case in et,
    hu, pt and zh-CN, and was an edition in nb-NO.
  - The check boxes showed forms to fill in or editions (hu, nb-NO, pt, pt-BR, sl, sv), and zh-CN's plural said
    "express".
  - pl's column Text was "Opis", a description. ro's instruction said "show" for "enter", uk's said "corrected" for
    "customized", and zh-CN's was garbled. Typos and colons: de "Symols", nb-NO "Teskt", sl "Prilagojeno opis", and
    the colon of the titles of da and de.
- **Left empty**: none.
- **terms.csv**: ten cells were changed with the group, and terms-decisions.tsv records each. The three check boxes
  stand side by side in about 360 px, as the English (319 px). The terms of ru, uk and hu were about twice as long, so
  they leave out the verb "show": ru "Формы мн. числа", "Формы рода", "Падежные формы"; uk "Форми множини", "Форми
  роду", "Відмінкові форми"; hu "Többes számú alakok", "Nemek szerinti alakok", "Esetek szerinti alakok". nl's Show
  case forms takes the form of its other two check boxes, "Toon naamvalsvormen".

Doubts:

- tr "Cinsiyet" and zh-CN 性别, zh-TW 性別 stay for the grammatical gender, against terms.csv ("Cins", 性), because
  the languages use these words for the grammatical gender too: Turkish linguistics writes "dil bilgisel cinsiyet",
  and Chinese 名词的性别 is the gender of a noun.
- The check boxes of bg, fr and pt-BR still take about 500 px of the 360, as before, and sl's about 410 now. The dialog
  can be made wider.
- et "Näita vorme" (show forms) leaves out the case, and nn-NO's column Text is "tekst", with a small letter. terms.csv
  gives both, so they stay.
- The column Number shows MiscText's Singular and Plural, which the groups of MiscText translate.

#### bg

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (без промяна) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Число<br>(corrects: Номер) | the column shows singular or plural, the grammatical number; "номер" is a number in a sequence (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_genderColumn_HeaderText` | Gender | Род<br>(corrects: Пол) | the grammatical gender of the description text; "пол" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_labelGenderChooser_Text` | Gender: | Род:<br>(corrects: Пол:) | the grammatical gender of the description text; "пол" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Падеж<br>(corrects: Главни/малки букви) | the grammatical case of the description text; "Главни/малки букви" is upper and lower case of letters (the term Case) |  |
| `EnterSymbolText_checkBoxGender_Text` | Show gender forms | Показвай формите по род<br>(corrects: Показвай формите по пол) | "пол" is the sex of a person; the grammatical gender is "род" (the term Show gender forms) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Показвай падежните форми<br>(corrects: Показвай формите по регистър) | "регистър" is the case of letters; the grammatical case is "падеж" (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Падеж на променения символ:<br>(corrects: Регистър на променения символ:) | the label names the grammatical case of the symbol that this symbol modifies; "регистър" is the case of letters (the term Case) |  |

#### cs

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (beze změny) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_genderColumn_HeaderText` | Gender | Rod<br>(corrects: Pohlaví) | the grammatical gender of the description text; "pohlaví" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_labelGenderChooser_Text` | Gender: | Rod:<br>(corrects: Pohlaví:) | the grammatical gender of the description text; "pohlaví" is the sex of a person (the term Gender) |  |

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (uændret) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Kasus<br>(corrects: Opgave) | the grammatical case of the description text; "Opgave" is a task (the term Case) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Vis kasusformer<br>(corrects: Vis tilfældeformer) | "tilfælde" is a case as in an instance; the grammatical case is "kasus" (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Kasus for det modificerede symbol:<br>(corrects: Opgave af modificeret symbol:) | the label names the grammatical case of the symbol that this symbol modifies; "Opgave" is a task (the term Case) |  |
| `EnterSymbolText_Text` | Customized Symbol Text | Tilpasset symboltekst<br>(corrects: Tilpasset symboltekst:) | the colon does not belong in the title of a dialog (the term Customized Symbol Text) |  |

#### de

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (unverändert) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Numerus<br>(corrects: Nummer) | the column shows singular or plural, the grammatical number; "Nummer" is a number in a sequence (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Fall des geänderten Symbols:<br>(corrects: Fall des geänderten Symols:) | the typo "Symols" |  |
| `EnterSymbolText_Text` | Customized Symbol Text | Benutzerdefinierter Symboltext<br>(corrects: Benutzerdefinierter Symboltext:) | the colon does not belong in the title of a dialog (the term Customized Symbol Text) |  |

#### en-GB

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (not needed) | en-GB writes it as the English does |  |

#### es

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Caso | header of the column of the grid that shows the grammatical case of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Case |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Caso del símbolo modificado: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Mostrar formas de caso | check box above the grid that adds the rows of the case forms; the term Show case forms |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (sin cambios) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (muutmata) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Arv<br>(corrects: Number) | the column shows singular or plural, the grammatical number; "number" is a number in a sequence; the grammatical number is "arv" (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Kääne<br>(corrects: Valik) | the grammatical case of the description text; "Valik" is a choice (the term Case) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Muudetud sümboli kääne:<br>(corrects: Muudetud sümbolid:) | the label names the grammatical case of the symbol that this symbol modifies; "Muudetud sümbolid" (modified symbols) lacked the case (the term Case) |  |

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (ennallaan) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |

#### fr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (inchangé) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Cas<br>(corrects: Case) | the grammatical case of the description text; "case" is a box in French; the grammatical case is "cas" (the term Case) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Afficher les formes de cas<br>(corrects: Afficher les formes de case) | "case" is a box in French; the grammatical case is "cas" (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Cas du symbole modifié :<br>(corrects: Case du symbole modifié :) | the label names the grammatical case of the symbol that this symbol modifies; "case" is a box in French; the grammatical case is "cas" (the term Case) |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (változatlan) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_checkBoxPlural_Text` | Show plural forms | Többes számú alakok<br>(corrects: Mutassa a többesszám űrlapját) | "űrlap" is a form to fill in; the grammatical forms are "alakok". The three check boxes stand side by side in about 360 px, as the English (319 px), so the verb is left out (terms.csv now gives this form) |  |
| `EnterSymbolText_checkBoxGender_Text` | Show gender forms | Nemek szerinti alakok<br>(corrects: Mutassa a nem űrlapját) | "űrlap" is a form to fill in; the grammatical forms are "alakok". The verb is left out for the room, as in the check box of the plural (terms.csv now gives this form) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Esetek szerinti alakok<br>(corrects: Esetek adatlapja) | "Esetek adatlapja" is the data sheet of the cases; the grammatical forms are "alakok", by case as by gender. The verb is left out for the room (terms.csv now gives this form) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | A módosított szimbólum esete:<br>(corrects: Módosított jelkulcs) | the label names the grammatical case of the symbol that this symbol modifies; "Módosított jelkulcs" (modified legend) lacked the case; the language's symbol is "szimbólum", as in the instruction of the dialog (the term Case) |  |

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (invariato) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |

#### ja

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (変更なし) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | 数<br>(corrects: 番号) | the column shows singular or plural, the grammatical number; 番号 is a number in a sequence (the term Number (grammatical: singular or plural)) |  |

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (uendret) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Tall<br>(corrects: Nummer) | the column shows singular or plural, the grammatical number; "nummer" is a number in a sequence; the grammatical number is "tall", as in "entall" and "flertall" (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Kasus<br>(corrects: Små/store bokstavar) | the grammatical case of the description text; "Små/store bokstavar" is the case of letters (the term Case) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Vis kasusformer<br>(corrects: Vis utgaver) | "Vis utgaver" is show editions (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Kasus for endret symbol:<br>(corrects: Utgave av endret symbol:) | the label names the grammatical case of the symbol that this symbol modifies; "utgave" is an edition (the term Case) |  |
| `EnterSymbolText_labelDescription_Text` | Enter the customized text for the symbol in the "Text" column. | Skriv inn tilpasset tekst for symbol i "Tekst" kolonnen.<br>(corrects: Skriv inn tilpasset tekst for symbol i "Teskt" kolonnen.) | the typo "Teskt": the column is "Tekst"; nothing else changes |  |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_labelDescription_Text` | Enter the customized text for the symbol in the "Text" column. | Voer in de kolom "Tekst" de aangepaste tekst voor het symbool in. | instruction above the grid of the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the terms Customized text and Column, and the header of the column "Text" |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Naamval van het gewijzigde symbool: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Toon naamvalsvormen | check box above the grid that adds the rows of the case forms; the term Show case forms, in the form of the dialog's other check boxes "Toon meervoudsvormen" and "Toon geslachts-vervoegingen" (terms.csv now gives it) |  |
| `EnterSymbolText_Text` | Customized Symbol Text | Aangepaste symbooltekst | title of the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Customized Symbol Text |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (ongewijzigd) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Getal<br>(corrects: Aantal) | the column shows singular or plural, the grammatical number; "aantal" is a count; the grammatical number is "getal" (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Naamval<br>(corrects: Hoofd/kleine letters) | the grammatical case of the description text; "Hoofd/kleine letters" is the case of letters (the term Case) |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Kasus | header of the column of the grid that shows the grammatical case of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Case |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Kasus for endra symbol: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Vis kasusformer | check box above the grid that adds the rows of the case forms; the term Show case forms |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (uendra) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Tal<br>(corrects: Nummer) | the column shows singular or plural, the grammatical number; "nummer" is a number in a sequence; the grammatical number is "tal", as in "eintal" and "fleirtal" (the term Number (grammatical: singular or plural)) |  |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Przypadek | header of the column of the grid that shows the grammatical case of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Case |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Przypadek modyfikowanego symbolu: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Pokaż formy przypadków | check box above the grid that adds the rows of the case forms; the term Show case forms |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (bez zmian) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Liczba<br>(corrects: Numer) | the column shows singular or plural, the grammatical number; "numer" is a number in a sequence; the grammatical number is "liczba" (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_genderColumn_HeaderText` | Gender | Rodzaj<br>(corrects: Płeć) | the grammatical gender of the description text; "płeć" is the sex of a person (the term Gender), as the language's "Pokaż formy rodzaju gramatycznego" |  |
| `EnterSymbolText_labelGenderChooser_Text` | Gender: | Rodzaj:<br>(corrects: Płeć:) | the grammatical gender of the description text; "płeć" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_textColumn_HeaderText` | Text | Tekst<br>(corrects: Opis) | header of the column in which the text is entered: "Opis" is a description (the term Text), and the instruction of the dialog names the column "Tekst" |  |

#### pt

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (inalterado) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Mostrar formas de caso<br>(corrects: Mostrar a caixa de formulário) | "caixa de formulário" is a form box (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Caso do símbolo modificado:<br>(corrects: No caso de modificar o símbolo) | the label names the grammatical case of the symbol that this symbol modifies; "No caso de modificar o símbolo" means "in case the symbol is modified" (the term Case), as pt-BR "Caso de símbolo modificado:" |  |

#### pt-BR

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (inalterado) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_checkBoxPlural_Text` | Show plural forms | Mostrar formas no plural<br>(corrects: Mostrar formulário de plural) | "formulário" is a form to fill in; the grammatical forms are "formas" (the term Show plural forms) |  |
| `EnterSymbolText_checkBoxGender_Text` | Show gender forms | Mostrar formas de gênero<br>(corrects: Mostrar formulário de gênero) | "formulário" is a form to fill in; the grammatical forms are "formas" (the term Show gender forms) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Mostrar formas de caso<br>(corrects: Mostrar formulário de casos) | "formulário" is a form to fill in; the grammatical forms are "formas" (the term Show case forms) |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Caz | header of the column of the grid that shows the grammatical case of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Case |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Cazul simbolului modificat: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Afișează formele de caz | check box above the grid that adds the rows of the case forms; the term Show case forms |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (neschimbat) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_labelDescription_Text` | Enter the customized text for the symbol in the "Text" column. | Introduceți textul personalizat pentru simbol în coloana "Text".<br>(corrects: Afișează textul personalizat pentru simbol în coloana "Text") | "Afișează" is "show"; the user enters the text (the language's "Selectați o limbă:"); nothing else changes |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Число | header of the column of the grid that shows singular or plural, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Number (grammatical: singular or plural) |  |
| `EnterSymbolText_genderColumn_HeaderText` | Gender | Род | header of the column of the grid that shows the grammatical gender of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Gender |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Падеж | header of the column of the grid that shows the grammatical case of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Case |  |
| `EnterSymbolText_checkBoxPlural_Text` | Show plural forms | Формы мн. числа | check box above the grid that adds the rows of the plural forms; the three check boxes stand side by side in about 360 px, as the English (319 px), so the verb "show" is left out (terms.csv now gives this form) |  |
| `EnterSymbolText_checkBoxGender_Text` | Show gender forms | Формы рода | check box above the grid that adds the rows of the gender forms; the three check boxes stand side by side in about 360 px, as the English (319 px), so the verb "show" is left out (terms.csv now gives this form); "Показать роды" would also read as "show the childbirth" |  |
| `EnterSymbolText_labelDescription_Text` | Enter the customized text for the symbol in the "Text" column. | Введите изменённый текст символа в столбце «Текст». | instruction above the grid of the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the terms Customized text and Column, and the header of the column "Text" |  |
| `EnterSymbolText_labelGenderChooser_Text` | Gender: | Род: | label of the combo box that chooses the grammatical gender of the symbol; the term Gender, with the colon of a label |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Падеж изменяемого символа: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Падежные формы | check box above the grid that adds the rows of the case forms; the three check boxes stand side by side in about 360 px, as the English (319 px), so the verb "show" is left out (terms.csv now gives this form) |  |
| `EnterSymbolText_Text` | Customized Symbol Text | Изменённый текст символа | title of the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Customized Symbol Text |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (без изменений) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Sklon | header of the column of the grid that shows the grammatical case of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Case |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Sklon spremenjenega simbola: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Prikaži sklonske oblike | check box above the grid that adds the rows of the case forms; the term Show case forms |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (nespremenjeno) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Število<br>(corrects: Številka) | the column shows singular or plural, the grammatical number; "številka" is a number in a sequence; the grammatical number is "število" (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_checkBoxPlural_Text` | Show plural forms | Prikaži množinske oblike<br>(corrects: Prikaži množinske obrazce) | "obrazec" is a form to fill in; a grammatical form is "oblika" (the term Show plural forms) |  |
| `EnterSymbolText_checkBoxGender_Text` | Show gender forms | Prikaži oblike za spol<br>(corrects: Prikaži obrazce za spol) | "obrazec" is a form to fill in; a grammatical form is "oblika" (the term Show gender forms) |  |
| `EnterSymbolText_Text` | Customized Symbol Text | Prilagojen opis simbola<br>(corrects: Prilagojeno opis simbola) | "opis" is masculine (the term Customized Symbol Text) |  |

#### sv

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (oförändrad) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Numerus<br>(corrects: Antal) | the column shows singular or plural, the grammatical number; "antal" is a count; the grammatical number is "numerus" (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_genderColumn_HeaderText` | Gender | Genus<br>(corrects: Kön) | the grammatical gender of the description text; "kön" is the sex of a person (the term Gender), as the language's "Visa ords genusändelse" |  |
| `EnterSymbolText_labelGenderChooser_Text` | Gender: | Genus:<br>(corrects: Kön:) | the grammatical gender of the description text; "kön" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Kasus<br>(corrects: Case) | the grammatical case of the description text; "Case" was the English (the term Case) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Visa kasusformer<br>(corrects: Visa case-formulär) | "case-formulär" is a form to fill in, with the English "case" (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Kasus för modifierad symbol:<br>(corrects: Case på modifierad symbol:) | the label names the grammatical case of the symbol that this symbol modifies; "Case" was the English (the term Case) |  |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (değişiklik yok) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Відмінок | header of the column of the grid that shows the grammatical case of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Case |  |
| `EnterSymbolText_checkBoxPlural_Text` | Show plural forms | Форми множини | check box above the grid that adds the rows of the plural forms; the three check boxes stand side by side in about 360 px, as the English (319 px), so the verb "show" is left out (terms.csv now gives this form) |  |
| `EnterSymbolText_checkBoxGender_Text` | Show gender forms | Форми роду | check box above the grid that adds the rows of the gender forms; the three check boxes stand side by side in about 360 px, as the English (319 px), so the verb "show" is left out (terms.csv now gives this form) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Відмінок змінюваного символу: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Відмінкові форми | check box above the grid that adds the rows of the case forms; the three check boxes stand side by side in about 360 px, as the English (319 px), so the verb "show" is left out (terms.csv now gives this form) |  |
| `EnterSymbolText_Text` | Customized Symbol Text | Змінений текст символу | title of the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Customized Symbol Text |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (без змін) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Число<br>(corrects: Номер) | the column shows singular or plural, the grammatical number; "номер" is a number in a sequence (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_genderColumn_HeaderText` | Gender | Рід<br>(corrects: Стать) | the grammatical gender of the description text; "стать" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_labelGenderChooser_Text` | Gender: | Рід:<br>(corrects: Стать:) | the grammatical gender of the description text; "стать" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_labelDescription_Text` | Enter the customized text for the symbol in the "Text" column. | Введіть змінений текст для умовного знаку у колонку "Текст".<br>(corrects: Введіть виправлений текст для умовного знаку у колонку "Текст".) | "виправлений" is corrected; the text is customized, "змінений" (the term Customized text); nothing else changes |  |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | （不变） | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | 数<br>(corrects: 编号) | the column shows singular or plural, the grammatical number; 编号 is a serial number; the grammatical number is 数, as ja (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | 格<br>(corrects: 案例) | the grammatical case of the description text; 案例 is a case study; the grammatical case is 格, as ja (the term Case) |  |
| `EnterSymbolText_checkBoxPlural_Text` | Show plural forms | 显示复数形式<br>(corrects: 表示复数形式) | 表示 is to express; the check box shows the plural forms, 显示, as the language's other two check boxes (the term Show plural forms) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | 显示格形式<br>(corrects: 展示形式) | 展示形式 is "display form" and lacks the case; the grammatical case is 格 (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | 被修饰符号的格：<br>(corrects: 改性符号的情况下：) | the label names the grammatical case of the symbol that this symbol modifies; 改性符号的情况下 means "in the case of a modified symbol", and 改性 is the modification of a material; the grammatical case is 格 (the term Case) |  |
| `EnterSymbolText_labelDescription_Text` | Enter the customized text for the symbol in the "Text" column. | 在“文本”栏中输入该符号的自定义文本。<br>(corrects: 输入自定义文本的符号在“文字”栏。) | the sentence was garbled ("input the symbol of the customized text is in the column"), and it named the column 文字, which the language calls 文本 (the terms Customized text and Text) |  |

#### zh-TW

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (不變) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | 數<br>(corrects: 編碼) | the column shows singular or plural, the grammatical number; 編碼 is a code; the grammatical number is 數, as ja 数 (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | 格<br>(corrects: 案例) | the grammatical case of the description text; 案例 is a case study; the grammatical case is 格, as ja (the term Case) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | 顯示格形式<br>(corrects: 顯示情況表格) | 情況表格 is a table of situations; the grammatical case is 格 (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | 被修飾符號的格：<br>(corrects: 修改符號的情況：) | the label names the grammatical case of the symbol that this symbol modifies; 修改符號的情況 is the situation of a changed symbol; the grammatical case is 格 (the term Case) |  |
