# Key group PrintDescriptions

Nov 16, 2026. The Print Descriptions dialog, which prints the control descriptions of the chosen courses: the group
Output (printer, paper size, orientation, margins), the courses, the copies (one copy, one page or a number of
descriptions of each course), and the appearance (description type and line size). claude-opus-5-5 at effort max
made every line below, and each entry says so in its comment.

- **Translated**: 82 lines, in 4 languages. da, ru and sl lacked most of the dialog, and nl 17 of its texts.
- **Not needed**: 4 lines. da, nl and sl write the paper size "Tabloid (11"x17")" as the English does, and da writes
  its word "Printer" as the English does.
- **Corrected**: 40 lines, most of them after terms.csv.
  - The line size is the size of the boxes of the description sheet, in millimetres. de, nb-NO, nn-NO, sv, tr, zh-CN
    and zh-TW named the thickness of a drawn line, and zh-CN the diameter of a wire.
  - The landscape orientation was scenery (da "Landskab"), terrain (et "Maastik") or "to arrange" (tr
    "Düzenlemek").
  - zh-CN: the margins were a profit margin (利润率) and their button a rate of change (变化率). The appearance was the
    user interface (界面), the orientation a positioning (定位), and one item's course a course of study (课程). The
    placeholder of the margins was garbled.
  - sv's group Output was "Skrivare", the printer, also in the dialog that makes a PDF, which has no printer. ro's
    orientation was "Direcție", a direction. da called the courses "Kurser", courses of study.
  - Typos and grammar: nl's title "Routebeschrijvignen afdrukken" (route descriptions) and its "Afdrukkken:"; fr
    "cicuit"; it "pe"; fi's three items of the copies lacked their case ("jokaista rataa" without "kohden"); de's
    margins; et "Vaikimis" (keeping silent) for "Vaikimisi" (by default); nb-NO's split "Løype standard"; cs said
    "according to the course"; hu took the plural after a number.
  - Colons and ellipses: et's title and pt's group header had a colon; ja's and ro's labels lacked one; et's and
    zh-TW's Change Margins lacked the ellipsis of a button that opens a dialog.
  - pl's item Text was "Opis", a description; nn-NO's Change Printer was Bokmål; pt's description type now uses the
    language's term "sinalética".
- **Left empty**: none.
- **terms.csv**: five cells were corrected with the group, and terms-decisions.tsv records each: Course default in et
  ("Vaikimisi rada"), nb-NO ("Løypestandard") and nl ("Standaard van de route"), Orientation in ro ("Orientare"), and
  Output in sv ("Utdata"). The groups PrintCourses, PrintPunches and the Create dialogs hold the old words of sv and
  ro too, and their sessions find them against these terms.

Doubts:

- Four keys of the group are placeholders of the WinForms dialog (the printer's name, the paper size, the orientation
  and the margins), which both dialogs replace at runtime. They are translated, as the other languages translate
  them.
- sl's title is "Tiskaj opise kontrolnih točk", with the small letter of the language. Its menu item keeps "Tiskaj
  Opise kontrolnih točk" until the group MainFrame.
- Some languages' "Course default" says something else: es "Recorrido actual" (the current course), uk "Як і на
  картах" (as on the maps), zh-CN 默认线路 (the default course), et "Vaikimisi rada" (the default course). terms.csv
  gives them, so they stay.
- After the number box, et "legend raja kohta" takes the nominative, where a number above one takes the partitive
  "legendi", and tr "parkur başına açıklamalar" can read as "per N courses". They stay, as a better text is not
  certain.

#### cs

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_descriptionsLabel_Text` | descriptions per course | popisů na trať<br>(corrects: Popisky podle trati) | "podle trati" is "according to the course"; the text follows the number of descriptions of each course (the terms Descriptions and Course) |  |

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_printButton_Text` | Print | Udskriv | button that prints; the term Print |  |
| `PrintDescriptions_printerChange_Text` | Change Printer... | Skift printer... | button that chooses another printer; the term Change Printer |  |
| `PrintDescriptions_printerName_Text` | Printer Name | Printernavn | the WinForms dialog's placeholder, which both dialogs replace at runtime with the printer's name; the term Printer Name |  |
| `PrintDescriptions_printerLabel_Text` | Printer: | (not needed) | label of the printer's name; the language writes it as the English does: "printer" is the Danish word (the term Printer) |  |
| `PrintDescriptions_copiesGroupBox_Text` | Copies | Kopier | header of the group that sets how many descriptions are printed; the term Copies |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | Ændr margener... | button that opens the Page Setup dialog to change the margins; the term Change Margins |  |
| `PrintDescriptions_paperSize_Text` | Tabloid (11"x17") | (not needed) | the WinForms dialog's placeholder, which both dialogs replace at runtime; the language writes it as the English does, with the term Tabloid (11"x17") |  |
| `PrintDescriptions_paperSizeLabel_Text` | Paper Size: | Papirstørrelse: | label of the paper size; the term Paper Size |  |
| `PrintDescriptions_margins_Text` | Top: 0.55", Bottom: 0.55", Left: 0.55", Right: 0.55" | Top: 0.55", Bund: 0.55", Venstre: 0.55", Højre: 0.55" | the WinForms dialog's placeholder, which both dialogs replace at runtime (MiscText.Margins_LRTB); the terms Top, Bottom, Left and Right |  |
| `PrintDescriptions_marginsLabel_Text` | Margins: | Margener: | label of the margins of the page; the term Margins |  |
| `PrintDescriptions_orientationLabel_Text` | Orientation: | Retning: | label of the orientation of the paper; the term Orientation |  |
| `PrintDescriptions_descriptionKindCombo_Items` | Course default | Banens standard | first item of the combo box Description type: the kind that each course's properties set; the term Course default |  |
| `PrintDescriptions_descriptionKindCombo_Items3` | Symbols & Text | Symboler og tekst | fourth item of the combo box Description type, after "Symbols" and "Text"; the term Symbols & Text | UIText.AddCourse_descKindCombo_Items2 |
| `PrintDescriptions_descriptionTypeLabel_Text` | Description type: | Beskrivelsestype: | label of the combo box that chooses symbols, text or both; the term Description type |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Linjestørrelse: | label of the box size of the printed description in mm (1 to 50, default 6), the height of a row of the description sheet; the term Line size |  |
| `PrintDescriptions_descriptionsLabel_Text` | descriptions per course | beskrivelser pr. bane | text after the number box: how many descriptions of each course the pages hold at least; the terms Descriptions and Course |  |
| `PrintDescriptions_copiesCombo_Items` | one copy of each course | én kopi af hver bane | first item of the combo box after "Print:": one description of each course, on as few pages as possible; the term Course |  |
| `PrintDescriptions_copiesCombo_Items1` | one page of each course | én side af hver bane | second item of the combo box after "Print:": one page of each course, filled with as many descriptions as fit; the term Course |  |
| `PrintDescriptions_copiesCombo_Items2` | multiple pages of each course | flere sider af hver bane | third item of the combo box after "Print:": pages of each course until the number beside it is reached; the term Course |  |
| `PrintDescriptions_whatToPrintLabel_Text` | Print: | Udskriv: | label of the combo box that chooses how many descriptions of each course to print; the term Print |  |
| `PrintDescriptions_Text` | Print Descriptions | Udskriv beskrivelser | title of the dialog that prints the control descriptions; the term Print Descriptions | UIText.MainFrame_printDescriptionsMenu_Text printDescriptionsMenu.Text |
| `PrintDescriptions_coursesGroupBox_Text` | Courses | Baner<br>(corrects: Kurser) | Danish orienteering calls courses "baner"; "kurser" are courses of study (the term Courses) |  |
| `PrintDescriptions_orientation_Text` | Landscape | Liggende<br>(corrects: Landskab) | "Landskab" is scenery; the page orientation is "liggende" (the term Landscape) |  |

#### de

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Zeilengröße:<br>(corrects: Linienstärke:) | "Linienstärke" is the thickness of a drawn line; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |
| `PrintDescriptions_margins_Text` | Top: 0.55", Bottom: 0.55", Left: 0.55", Right: 0.55" | Oben: 0.55", Unten: 0.55", Links: 0.55", Rechts: 0.55"<br>(corrects: Oben 0.55", Unten: 0.55", Link: 0.55", Rechts: 0.55") | the colon after "Oben" was missing, and the left side is "Links", not "Link"; a placeholder that the dialog replaces at runtime |  |

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_orientation_Text` | Landscape | Rõhtpaigutus<br>(corrects: Maastik) | "Maastik" is terrain; the page orientation is "rõhtpaigutus" (the term Landscape) |  |
| `PrintDescriptions_Text` | Print Descriptions | Trüki legendid<br>(corrects: Trüki legendid:) | the colon does not belong in the title of a window (the term Print Descriptions) |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | Muuda ääriseid...<br>(corrects: Muuda ääriseid) | the button opens the Page Setup dialog, and the ellipsis that says so was missing |  |
| `PrintDescriptions_descriptionKindCombo_Items` | Course default | Vaikimisi rada<br>(corrects: Vaikimis rada) | "vaikimis" is a form of "vaikimine", keeping silent; "by default" is "vaikimisi", as the language's "vaikimisi kõigile uutele võistlustele" (terms.csv now gives it) |  |

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_copiesCombo_Items` | one copy of each course | yksi kopio jokaisesta radasta<br>(corrects: yksi kopio jokaista rataa) | "jokaista rataa" needs "kohden"; "of each course" is "jokaisesta radasta" |  |
| `PrintDescriptions_copiesCombo_Items1` | one page of each course | yksi sivu jokaisesta radasta<br>(corrects: yksi sivu jokaista rataa) | "jokaista rataa" needs "kohden"; "of each course" is "jokaisesta radasta" |  |
| `PrintDescriptions_copiesCombo_Items2` | multiple pages of each course | useita sivuja jokaisesta radasta<br>(corrects: useita sivuja jokaista rataa) | "jokaista rataa" needs "kohden"; "of each course" is "jokaisesta radasta" |  |

#### fr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_copiesCombo_Items` | one copy of each course | une copie de chaque circuit<br>(corrects: une copie de chaque cicuit) | the typo "cicuit" |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_descriptionsLabel_Text` | descriptions per course | pontleírás pályánként<br>(corrects: pontleírások pályánként) | the text follows the number box, and Hungarian takes the singular after a number |  |

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_copiesCombo_Items1` | one page of each course | una pagina per ogni percorso<br>(corrects: una pagina pe ogni percorso) | the typo "pe" for "per", as the other two items |  |

#### ja

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_orientationLabel_Text` | Orientation: | 印刷の向き:<br>(corrects: 印刷の向き) | the colon of a label was missing, as the language's other labels of the dialog ("用紙サイズ:", "余白:") |  |

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Linjehøyde:<br>(corrects: Linje tykkelse:) | "tykkelse" is the thickness of a drawn line, and "Linje tykkelse" is split; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |
| `PrintDescriptions_descriptionKindCombo_Items` | Course default | Løypestandard<br>(corrects: Løype standard) | the compound is one word in Norwegian, as nn-NO "Løypestandard" (terms.csv now gives it) |  |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_coursesGroupBox_Text` | Courses | Routes | header of the group that lists the courses to print; the term Courses | UIText.AddTextLine_coursesLabel_Text UIText.MainFrame_createCoursePdfMenu_Text |
| `PrintDescriptions_cancelButton_Text` | Cancel | Annuleren | button that closes the dialog without printing; the term Cancel | UIText.CancelButton |
| `PrintDescriptions_copiesGroupBox_Text` | Copies | Kopieën | header of the group that sets how many descriptions are printed; the term Copies |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | Marges wijzigen... | button that opens the Page Setup dialog to change the margins; the term Change Margins |  |
| `PrintDescriptions_paperSize_Text` | Tabloid (11"x17") | (not needed) | the WinForms dialog's placeholder, which both dialogs replace at runtime; the language writes it as the English does, with the term Tabloid (11"x17") |  |
| `PrintDescriptions_paperSizeLabel_Text` | Paper Size: | Papierformaat: | label of the paper size; the term Paper Size | UIText.SetPrintAreaDialog_groupBoxPaperSize_Text groupBoxPaperSize.Text |
| `PrintDescriptions_margins_Text` | Top: 0.55", Bottom: 0.55", Left: 0.55", Right: 0.55" | Boven: 0.55", Onder: 0.55", Links: 0.55", Rechts: 0.55" | the WinForms dialog's placeholder, which both dialogs replace at runtime (MiscText.Margins_LRTB); the terms Top, Bottom, Left and Right |  |
| `PrintDescriptions_marginsLabel_Text` | Margins: | Marges: | label of the margins of the page; the term Margins |  |
| `PrintDescriptions_orientationLabel_Text` | Orientation: | Afdrukstand: | label of the orientation of the paper; the term Orientation |  |
| `PrintDescriptions_descriptionKindCombo_Items` | Course default | Standaard van de route | first item of the combo box Description type: the kind that each course's properties set; the term Course default, with the article that the language needs (terms.csv now gives it) |  |
| `PrintDescriptions_descriptionKindCombo_Items3` | Symbols & Text | Symbolen en tekst | fourth item of the combo box Description type, after "Symbols" and "Text"; the term Symbols & Text | UIText.AddCourse_descKindCombo_Items2 |
| `PrintDescriptions_descriptionTypeLabel_Text` | Description type: | Type postomschrijving: | label of the combo box that chooses symbols, text or both; the term Description type |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Regelgrootte: | label of the box size of the printed description in mm (1 to 50, default 6), the height of a row of the description sheet; the term Line size |  |
| `PrintDescriptions_descriptionsLabel_Text` | descriptions per course | postomschrijvingen per route | text after the number box: how many descriptions of each course the pages hold at least; the terms Descriptions and Course |  |
| `PrintDescriptions_copiesCombo_Items` | one copy of each course | één kopie van elke route | first item of the combo box after "Print:": one description of each course, on as few pages as possible; the term Course |  |
| `PrintDescriptions_copiesCombo_Items1` | one page of each course | één pagina van elke route | second item of the combo box after "Print:": one page of each course, filled with as many descriptions as fit; the term Course |  |
| `PrintDescriptions_copiesCombo_Items2` | multiple pages of each course | meerdere pagina's van elke route | third item of the combo box after "Print:": pages of each course until the number beside it is reached; the term Course |  |
| `PrintDescriptions_Text` | Print Descriptions | Postomschrijvingen afdrukken<br>(corrects: Routebeschrijvignen afdrukken) | the typo "Routebeschrijvignen", and "routebeschrijvingen" are route descriptions; the language's descriptions are "postomschrijvingen" (the term Print Descriptions) |  |
| `PrintDescriptions_whatToPrintLabel_Text` | Print: | Afdrukken:<br>(corrects: Afdrukkken:) | the typo "Afdrukkken" (the term Print) |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_printerChange_Text` | Change Printer... | Endre skrivar...<br>(corrects: Endre skriver...) | "skriver" is Bokmål; Nynorsk "skrivar", as the language's "Skrivar:" (the term Change Printer) |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Linjehøgd:<br>(corrects: Linjetjukkelse:) | "tjukkelse" is the thickness of a drawn line; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_descriptionKindCombo_Items2` | Text | Tekst<br>(corrects: Opis) | "Opis" is a description; the item is the text of the descriptions, after "Piktografiki" (the term Text), as in AllControlsProperties |  |

#### pt

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_descriptionTypeLabel_Text` | Description type: | Tipo de sinalética:<br>(corrects: Tipo de descrição:) | the language's descriptions are "sinalética", as its title "Imprimir Sinalética" (the term Description type) |  |
| `PrintDescriptions_printerGroup_Text` | Output | Saída<br>(corrects: Saída:) | the colon does not belong in the header of a group, as pt-BR "Saída" (the term Output) |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_orientationLabel_Text` | Orientation: | Orientare:<br>(corrects: Direcție) | "Direcție" is a direction of travel; the orientation of the paper is "orientare", and the colon of a label was missing (terms.csv now gives it) |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_mmSuffixLabel_Text` | mm | мм | the unit after the box of the line size; Russian writes the millimetre in Cyrillic, as bg and uk | uk |
| `PrintDescriptions_coursesGroupBox_Text` | Courses | Дистанции | header of the group that lists the courses to print; the term Courses |  |
| `PrintDescriptions_cancelButton_Text` | Cancel | Отмена | button that closes the dialog without printing; the term Cancel | UIText.CancelButton |
| `PrintDescriptions_printerGroup_Text` | Output | Вывод | header of the group of the printer, the paper and the margins, which the dialog that makes a PDF shows without the printer; the term Output |  |
| `PrintDescriptions_orientation_Text` | Landscape | Альбомная | the WinForms dialog's placeholder, which both dialogs replace at runtime (MiscText.Landscape); the term Landscape |  |
| `PrintDescriptions_printButton_Text` | Print | Печать | button that prints; the term Print |  |
| `PrintDescriptions_printerChange_Text` | Change Printer... | Изменить принтер... | button that chooses another printer; the term Change Printer |  |
| `PrintDescriptions_printerName_Text` | Printer Name | Имя принтера | the WinForms dialog's placeholder, which both dialogs replace at runtime with the printer's name; the term Printer Name |  |
| `PrintDescriptions_printerLabel_Text` | Printer: | Принтер: | label of the printer's name; the term Printer |  |
| `PrintDescriptions_copiesGroupBox_Text` | Copies | Копии | header of the group that sets how many descriptions are printed; the term Copies |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | Изменить поля... | button that opens the Page Setup dialog to change the margins; the term Change Margins |  |
| `PrintDescriptions_paperSize_Text` | Tabloid (11"x17") | Таблоид (11"x17") | the WinForms dialog's placeholder, which both dialogs replace at runtime with the name of the paper size; the term Tabloid (11"x17") |  |
| `PrintDescriptions_paperSizeLabel_Text` | Paper Size: | Размер бумаги: | label of the paper size; the term Paper Size |  |
| `PrintDescriptions_margins_Text` | Top: 0.55", Bottom: 0.55", Left: 0.55", Right: 0.55" | Сверху: 0.55", Снизу: 0.55", Слева: 0.55", Справа: 0.55" | the WinForms dialog's placeholder, which both dialogs replace at runtime (MiscText.Margins_LRTB); the terms Top, Bottom, Left and Right |  |
| `PrintDescriptions_marginsLabel_Text` | Margins: | Поля: | label of the margins of the page; the term Margins |  |
| `PrintDescriptions_orientationLabel_Text` | Orientation: | Ориентация: | label of the orientation of the paper; the term Orientation |  |
| `PrintDescriptions_descriptionKindCombo_Items` | Course default | По умолчанию для дистанции | first item of the combo box Description type: the kind that each course's properties set; the term Course default |  |
| `PrintDescriptions_descriptionKindCombo_Items3` | Symbols & Text | Символы и текст | fourth item of the combo box Description type, after "Symbols" and "Text"; the term Symbols & Text |  |
| `PrintDescriptions_descriptionTypeLabel_Text` | Description type: | Тип легенд: | label of the combo box that chooses symbols, text or both; the term Description type |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Размер строки: | label of the box size of the printed description in mm (1 to 50, default 6), the height of a row of the description sheet; the term Line size |  |
| `PrintDescriptions_descriptionsLabel_Text` | descriptions per course | легенд на дистанцию | text after the number box: how many descriptions of each course the pages hold at least; the terms Descriptions and Course | uk |
| `PrintDescriptions_copiesCombo_Items` | one copy of each course | одна копия каждой дистанции | first item of the combo box after "Print:": one description of each course, on as few pages as possible; the term Course |  |
| `PrintDescriptions_copiesCombo_Items1` | one page of each course | одна страница на дистанцию | second item of the combo box after "Print:": one page of each course, filled with as many descriptions as fit; the term Course |  |
| `PrintDescriptions_copiesCombo_Items2` | multiple pages of each course | несколько страниц на дистанцию | third item of the combo box after "Print:": pages of each course until the number beside it is reached; the term Course |  |
| `PrintDescriptions_whatToPrintLabel_Text` | Print: | Печать: | label of the combo box that chooses how many descriptions of each course to print; the term Print |  |
| `PrintDescriptions_Text` | Print Descriptions | Печать легенд | title of the dialog that prints the control descriptions; the term Print Descriptions |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_orientation_Text` | Landscape | Ležeče | the WinForms dialog's placeholder, which both dialogs replace at runtime (MiscText.Landscape); the term Landscape |  |
| `PrintDescriptions_printButton_Text` | Print | Tiskaj | button that prints; the term Print |  |
| `PrintDescriptions_printerChange_Text` | Change Printer... | Spremeni tiskalnik... | button that chooses another printer; the term Change Printer |  |
| `PrintDescriptions_printerName_Text` | Printer Name | Ime tiskalnika | the WinForms dialog's placeholder, which both dialogs replace at runtime with the printer's name; the term Printer Name |  |
| `PrintDescriptions_printerLabel_Text` | Printer: | Tiskalnik: | label of the printer's name; the term Printer |  |
| `PrintDescriptions_copiesGroupBox_Text` | Copies | Kopije | header of the group that sets how many descriptions are printed; the term Copies |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | Spremeni robove... | button that opens the Page Setup dialog to change the margins; the term Change Margins |  |
| `PrintDescriptions_paperSize_Text` | Tabloid (11"x17") | (not needed) | the WinForms dialog's placeholder, which both dialogs replace at runtime; the language writes it as the English does, with the term Tabloid (11"x17") |  |
| `PrintDescriptions_paperSizeLabel_Text` | Paper Size: | Velikost papirja: | label of the paper size; the term Paper Size |  |
| `PrintDescriptions_margins_Text` | Top: 0.55", Bottom: 0.55", Left: 0.55", Right: 0.55" | Zgoraj: 0.55", Spodaj: 0.55", Levo: 0.55", Desno: 0.55" | the WinForms dialog's placeholder, which both dialogs replace at runtime (MiscText.Margins_LRTB); the terms Top, Bottom, Left and Right |  |
| `PrintDescriptions_marginsLabel_Text` | Margins: | Robovi: | label of the margins of the page; the term Margins |  |
| `PrintDescriptions_orientationLabel_Text` | Orientation: | Usmerjenost: | label of the orientation of the paper; the term Orientation |  |
| `PrintDescriptions_descriptionKindCombo_Items` | Course default | Privzeto za progo | first item of the combo box Description type: the kind that each course's properties set; the term Course default |  |
| `PrintDescriptions_descriptionKindCombo_Items3` | Symbols & Text | Simboli in besedilo | fourth item of the combo box Description type, after "Symbols" and "Text"; the term Symbols & Text | UIText.AddCourse_descKindCombo_Items2 |
| `PrintDescriptions_descriptionTypeLabel_Text` | Description type: | Vrsta opisa: | label of the combo box that chooses symbols, text or both; the term Description type |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Velikost vrstice: | label of the box size of the printed description in mm (1 to 50, default 6), the height of a row of the description sheet; the term Line size |  |
| `PrintDescriptions_descriptionsLabel_Text` | descriptions per course | opisov na progo | text after the number box: how many descriptions of each course the pages hold at least; the terms Descriptions and Course |  |
| `PrintDescriptions_copiesCombo_Items` | one copy of each course | ena kopija vsake proge | first item of the combo box after "Print:": one description of each course, on as few pages as possible; the term Course |  |
| `PrintDescriptions_copiesCombo_Items1` | one page of each course | ena stran vsake proge | second item of the combo box after "Print:": one page of each course, filled with as many descriptions as fit; the term Course |  |
| `PrintDescriptions_copiesCombo_Items2` | multiple pages of each course | več strani vsake proge | third item of the combo box after "Print:": pages of each course until the number beside it is reached; the term Course |  |
| `PrintDescriptions_whatToPrintLabel_Text` | Print: | Tiskaj: | label of the combo box that chooses how many descriptions of each course to print; the term Print |  |
| `PrintDescriptions_Text` | Print Descriptions | Tiskaj opise kontrolnih točk | title of the dialog that prints the control descriptions; the term Print Descriptions, in the small letter of the language | UIText.MainFrame_printDescriptionsMenu_Text printDescriptionsMenu.Text |

#### sv

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Radstorlek:<br>(corrects: Linjetjocklek:) | "Linjetjocklek" is the thickness of a drawn line; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |
| `PrintDescriptions_printerGroup_Text` | Output | Utdata<br>(corrects: Skrivare) | "Skrivare" is the printer; the group also holds the paper and the margins, and the dialog that makes a PDF shows it without a printer (terms.csv now gives "Utdata", as nb-NO and nn-NO) |  |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_orientation_Text` | Landscape | Yatay<br>(corrects: Düzenlemek) | "Düzenlemek" is to arrange; the page orientation is "yatay" (the term Landscape) |  |
| `PrintDescriptions_paperSize_Text` | Tabloid (11"x17") | Tabloid (11"x17")<br>(corrects: Tablet (11"x17")) | "Tablet" is a tablet; the paper size is Tabloid (the term Tabloid (11"x17")) |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Satır boyutu:<br>(corrects: Çizgi Boyutu:) | "Çizgi" is a drawn line; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_layoutGroup_Text` | Appearance | 外观<br>(corrects: 界面) | 界面 is the user interface; the appearance is 外观, as zh-TW 外觀 (the term Appearance) |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | 更改页边距…<br>(corrects: 变化率…) | 变化率 is a rate of change; the button changes the page margins (the term Change Margins) |  |
| `PrintDescriptions_marginsLabel_Text` | Margins: | 页边距：<br>(corrects: 利润率：) | 利润率 is a profit margin; the margins of the page are 页边距 (the term Margins) |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | 行尺寸：<br>(corrects: 线径：) | 线径 is the diameter of a wire; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |
| `PrintDescriptions_orientationLabel_Text` | Orientation: | 方向：<br>(corrects: 定位：) | 定位 is positioning; the orientation of the paper is 方向, as zh-TW (the term Orientation) |  |
| `PrintDescriptions_copiesCombo_Items2` | multiple pages of each course | 每条线路多页<br>(corrects: 每条课程多页) | 课程 is a course of study; the language's course is 线路, as in the other two items (the term Course) |  |
| `PrintDescriptions_margins_Text` | Top: 0.55", Bottom: 0.55", Left: 0.55", Right: 0.55" | 上：0.55"，下：0.55"，左：0.55"，右：0.55"<br>(corrects: 上：“：0.55 0.55，自底向上”，“左，右：0.55 0.55。”) | the text was garbled: 自底向上 is "bottom-up", and the values stood apart from their sides; a placeholder that the dialog replaces at runtime, now with the sides as MiscText.Margins_LRTB writes them |  |

#### zh-TW

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | 列的大小:<br>(corrects: 線條尺寸:) | 線條 is a drawn line; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | 變更邊距...<br>(corrects: 變更邊距) | the button opens the Page Setup dialog, and the ellipsis that says so was missing |  |
