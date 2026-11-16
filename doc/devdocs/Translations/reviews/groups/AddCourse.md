# Key group AddCourse

Nov 16, 2026. The New Course dialog: the course's name, kind, climb and length, the check box that keeps the course
out of the reports, the second line of the description sheet, and the appearance (scale, descriptions, control circle
labels, first control number, column of the points). claude-opus-5-5 at effort max made every line below, and each
entry says so in its comment.

- **Translated**: 40 lines, in 21 languages. The check box "Do not include in reports" was missing in 20 languages,
  the score items of the control circle labels in up to 10, and some labels in ru and sl.
- **Not needed**: 4 lines. nn-NO, ro and sl write the unit "km" as the English does, and nl writes "Score (30)", its
  term, as the English does.
- **Corrected**: 44 lines, most of them after terms.csv.
  - da called the course "kursus", a course of study, and the control "kontrol", a check, throughout the dialog: now
    "bane" and "post". Its climb was the verb "Klatre", now "Stigning".
  - The points of a score course were geometric points or a grade: it "Punti", ru "оценка", tr "Nokta", uk "точки",
    zh-CN 点. Now the terms' "Punteggio", "Очки", "Puan", "очки" and 分值.
  - Mistranslated labels: the first control number was a check figure (ru) or the code (sv); the description
    appearance was the description of a display (cs), a preview (sl), the definitions (sv) or an image (tr); pl's
    text was "Opis", a description; fr's score course was a race, "Course"; tr's was the score of a course; pl's New
    Course was a course of study, "Kurs"; zh-CN's appearance was the user interface, 界面.
  - The class list was a school class in hu ("Osztály") and uk ("Клас").
  - Typos and split compounds in da, et, it, ru and pt-BR, where the score course read "(fazenda)", a farm.
  - nn-NO's Reports menu takes the Nynorsk plural "_Rapportar", as the new check box; terms.csv gives "Rapportar".
- **Left empty**: none.

Doubts:

- ru and ro leave out the examples of the other items of the control circle labels ("Код КП", "Codul postului"); the
  new items keep them, as the English and the terms do.
- The check of the terms also points at "Nummer", "Номер" and the like in "First control number" in nine languages.
  terms.csv corrects them for the grammatical number of the case and gender settings, not for a number in a sequence,
  so they stay.

#### cs

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Nezahrnovat do výkazů | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_descriptionAppearanceLabel_Text` | Description appearance: | Zobrazení popisu:<br>(corrects: Popis zobrazení:) | "Popis zobrazení" means the description of a display; the term Description appearance |  |

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_descKindCombo_Items2` | Symbols and text | Symboler og tekst | third item of the combo box Description appearance, beside "Symbols" and "Text"; the term Symbols and text | AddCourse_descKindCombo_Items AddCourse_descKindCombo_Items1 |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Point (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Medtag ikke i rapporter | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_firstControlLabel_Text` | First control number: | Første postnummer:<br>(corrects: Første kontrolnummer:) | Danish orienteering calls a control "post" ("kontrol" is a check); the term First control number |  |
| `AddCourse_mapScaleLabel_Text` | Map printing scale: | Kortets udskriftsskala:<br>(corrects: Kort udskrivnings scala:) | the compounds were split and "scala" has a typo; the term Map printing scale |  |
| `AddCourse_labelKindLabel_Text` | Control circle labels: | Postcirkeletiketter:<br>(corrects: Kontrolcirkelmærkater:) | Danish orienteering calls a control "post"; "mærkat" is a sticker (the term Control circle labels) |  |
| `AddCourse_labelKindCombo_Items` | Sequence number (3) | Sekvensnummer (3)<br>(corrects: Sekvens nummer (3)) | the compound is one word in Danish ("Sekvens nummer") |  |
| `AddCourse_labelKindCombo_Items1` | Control code (145) | Postkode (145)<br>(corrects: Kontrolkode (145)) | Danish orienteering calls a control "post" (the term Control code (145)) |  |
| `AddCourse_secondaryTitleDescription_Text` | The following (optional) text will appear on the second line of the control description sheet: | Den følgende (valgfrie) tekst vil fremtræde på den anden linje af postbeskrivelsessiden:<br>(corrects: Den følgende (valgfrie) tekst vil fremtræde på den anden linje af kontrolbeskrivelses siden:) | Danish orienteering calls a control "post"; the compound "postbeskrivelsessiden" is one word |  |
| `AddCourse_secondaryTitleGroup_Text` | Class list / Secondary title | Klasseliste / Sekundær titel<br>(corrects: Klasse liste / Sekundær titel) | the compound is one word in Danish (the term Class List) |  |
| `AddCourse_climbLabel_Text` | Climb: | Stigning:<br>(corrects: Klatre:) | "Klatre" is the verb to climb; the climb of a course is "stigning" (the term Climb) |  |
| `AddCourse_courseTypeLabel_Text` | Course type: | Banetype:<br>(corrects: Kursustype:) | Danish orienteering calls a course "bane" ("kursus" is a course of study; the term Course type) |  |
| `AddCourse_courseNameLabel_Text` | Course name: | Banenavn:<br>(corrects: Kursusnavn:) | Danish orienteering calls a course "bane" (the term Course Name) |  |
| `AddCourse_courseKindCombo_Items` | Normal Course | Normal bane<br>(corrects: Normal kursus) | Danish orienteering calls a course "bane" (the term Normal Course) |  |
| `AddCourse_courseKindCombo_Items1` | Score Course | Pointbane<br>(corrects: Score kursus) | Danish orienteering calls a course "bane" (the term Score Course) |  |
| `AddCourse_Text` | New Course | Ny bane<br>(corrects: Nyt kursus) | Danish orienteering calls a course "bane" (the term New Course) |  |

#### es

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Puntuación (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | No incluir en los informes | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Ära lisa aruannetesse | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_labelKindCombo_Items1` | Control code (145) | Tunnusnumber (145)<br>(corrects: Tunnusnumber (154)) | the example is the code 145, as the language's "Järjekorra ja tunnusnumber (3-145)"; "154" is a typo |  |
| `AddCourse_labelKindCombo_Items4` | Code and score (145(30)) | Tunnus ja väärtus (145(30))<br>(corrects: Tunnus ja väärtus 145(30))) | the opening parenthesis of the example was missing |  |

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Älä sisällytä raportteihin | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |

#### fr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Points (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Ne pas inclure dans les rapports | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_courseKindCombo_Items1` | Score Course | Circuit au score<br>(corrects: Course au score) | the kind of course, beside "Circuit normal"; a "course" is the race (the term Score Course) |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Pontérték (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Ne szerepeljen a jelentésekben | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_secondaryTitleGroup_Text` | Class list / Secondary title | Kategórialista / Másodlagos cím<br>(corrects: Osztály lista / Másodlagos cím) | Hungarian orienteering calls a class "kategória"; "osztály" is a school class (the term Class List) |  |

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_descKindCombo_Items2` | Symbols and text | Simboli e testo<br>(corrects: Simboli e testo.) | the full stop does not belong to an item of a combo box |  |
| `AddCourse_labelKindCombo_Items1` | Control code (145) | Codice punto (145)<br>(corrects: Codice di controllo (145)) | the language's control is "punto" (the term Control code (145)); "codice di controllo" is a check code |  |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Punteggio (30)<br>(corrects: Punti (30)) | the language's score is "punteggio" ("Totale punteggio"); "punti" are the controls (the term Score (30)) |  |
| `AddCourse_scoreColumnLabel_Text` | Show points in: | Mostra punteggio in:<br>(corrects: Mostra punti in) | the language's score is "punteggio"; "punti" are the controls (the term Show points in), with the colon of a label |  |
| `AddCourse_courseKindCombo_Items1` | Score Course | Percorso a sequenza libera<br>(corrects: percorso a sequenza libera) | the capital, as the other item "Percorso normale" (the term Score Course) |  |

#### ja

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items5` | Score (30) | 得点 (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | レポートに含めない | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Ikke ta med i rapporter | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items5` | Score (30) | (not needed) | last item of the combo box Control circle labels of a score course; the term Score (30) is the English, as the language's "Volgorde en score" |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Niet opnemen in rapporten | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items3` | Sequence and score (3(30)) | Sekvens og poeng (3(30)) | item of the combo box Control circle labels of a score course: the sequence number and the points, with the example of the English, as the other items | AddCourse_labelKindCombo_Items2 |
| `AddCourse_labelKindCombo_Items4` | Code and score (145(30)) | Kode og poeng (145(30)) | item of the combo box Control circle labels of a score course: the control code and the points, with the example of the English | AddCourse_labelKindCombo_Items2 |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Poeng (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_kmSuffix_Text` | km | (not needed) | the unit beside the length box; the language writes the symbol km as the English does |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Ikkje ta med i rapportar | check box under the length in the New Course dialog; "rapportar" is the Nynorsk plural (terms.csv gave the Bokmål "Rapporter") |  |
| `MainFrame_reportMenu_Text` | _Reports | _Rapportar<br>(corrects: _Rapporter) | "rapporter" is the Bokmål plural; the Nynorsk plural of "rapport" is "rapportar", as the check box Do not include in reports (AddCourse) | AddCourse_hideFromReportsCheckBox_Text |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Punkty (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Nie uwzględniaj w raportach | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_descKindCombo_Items1` | Text | Tekst<br>(corrects: Opis) | "Opis" is a description; the item is the text of the descriptions (the term Text, the language's "Dodaj tekst") |  |
| `AddCourse_descKindCombo_Items2` | Symbols and text | Piktografiki i tekst<br>(corrects: Piktografiki i opis) | "opis" is a description; the term Text, as the item before |  |
| `AddCourse_labelKindCombo_Items1` | Control code (145) | Kod PK (145)<br>(corrects: Kod punktu (145)) | the language's control is "PK" (the term Control code (145)); "punkty" are also the score points |  |
| `AddCourse_Text` | New Course | Nowa trasa<br>(corrects: Nowy Kurs) | "kurs" is a course of study; the language's course is "trasa" (the term New Course) |  |

#### pt

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Não incluir nos relatórios | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |

#### pt-BR

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Não incluir nos relatórios | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_courseKindCombo_Items1` | Score Course | Percurso de pontuação<br>(corrects: Percurso de pontuação (fazenda)) | "(fazenda)" is a farm and does not belong to the kind of course (the term Score Course) |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items3` | Sequence and score (3(30)) | Număr și punctaj (3(30)) | item of the combo box Control circle labels of a score course: the number and the points, as the language's "Număr și cod"; with the example of the English | AddCourse_labelKindCombo_Items2 |
| `AddCourse_labelKindCombo_Items4` | Code and score (145(30)) | Cod și punctaj (145(30)) | item of the combo box Control circle labels of a score course: the control code and the points, with the example of the English | AddCourse_labelKindCombo_Items2 |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Punctaj (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_kmSuffix_Text` | km | (not needed) | the unit beside the length box; the language writes the symbol km as the English does |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Nu include în rapoarte | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items3` | Sequence and score (3(30)) | Порядковый номер и очки (3(30)) | item of the combo box Control circle labels of a score course: the sequence number and the points, with the example of the English, as the other items | AddCourse_labelKindCombo_Items2 |
| `AddCourse_labelKindCombo_Items4` | Code and score (145(30)) | Код и очки (145(30)) | item of the combo box Control circle labels of a score course: the control code and the points, with the example of the English | AddCourse_labelKindCombo_Items2 |
| `AddCourse_scoreColumnLabel_Text` | Show points in: | Показывать очки в: | label of the combo box that chooses the column of the points (Column A, B, H) of a score course; the term Show points in | uk |
| `AddCourse_secondaryTitleDescription_Text` | The following (optional) text will appear on the second line of the control description sheet: | Следующий (необязательный) текст будет показан во второй строке легенды КП: | the text above the box of the second line of the description sheet; the description sheet is the "легенда КП" (the term Descriptions) | uk |
| `AddCourse_secondaryTitleGroup_Text` | Class list / Secondary title | Список групп / Подзаголовок | header of the group box of the second line of the description sheet; the term Class List | uk |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Не включать в отчёты | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_firstControlLabel_Text` | First control number: | Номер первого КП:<br>(corrects: Первое контрольное число) | "контрольное число" is a check figure; the label asks for the number of the first control (the term First control number), with the colon of a label |  |
| `AddCourse_mapScaleLabel_Text` | Map printing scale: | Масштаб печати карты:<br>(corrects: Маштаб карты для печати) | the typo "Маштаб"; the term Map printing scale, with the colon of a label |  |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Очки (30)<br>(corrects: оценка) | "оценка" is a grade; the points of a control are "очки" (the term Score (30)) |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Točke (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_lengthLabel_Text` | Length: | Dolžina: | label of the length of a normal course, beside the box and "km"; the term Length |  |
| `AddCourse_kmSuffix_Text` | km | (not needed) | the unit beside the length box; the language writes the symbol km as the English does |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Ne vključi v poročila | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_descriptionAppearanceLabel_Text` | Description appearance: | Videz opisa:<br>(corrects: predogled opisa) | "predogled" is a preview; the term Description appearance, with the capital and colon of a label |  |

#### sv

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_firstControlLabel_Text` | First control number: | Första kontrollnummer:<br>(corrects: Första kodsiffra:) | "kodsiffra" is the code; the label asks for the number of the first control (the term First control number) |  |
| `AddCourse_descriptionAppearanceLabel_Text` | Description appearance: | Kontrollbeskrivningens utseende:<br>(corrects: Definitionens utseende:) | "definition" is the language's rarer word; its descriptions are "kontrollbeskrivningar" (the term Description appearance) |  |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_descriptionAppearanceLabel_Text` | Description appearance: | Açıklama görünümü:<br>(corrects: Görüntü açıklaması:) | "Görüntü açıklaması" means the description of an image; the term Description appearance |  |
| `AddCourse_labelKindCombo_Items3` | Sequence and score (3(30)) | Sıra No ve Puan (3(30))<br>(corrects: Sıra No ve Nokta(3(30))) | "Nokta" is a geometric point; the score points are "puan" (the term Show points in) |  |
| `AddCourse_labelKindCombo_Items4` | Code and score (145(30)) | Kod ve Puan (145(30))<br>(corrects: Kod ve Nokta) | "Nokta" is a geometric point; the score points are "puan"; the example of the English, as the other items |  |
| `AddCourse_scoreColumnLabel_Text` | Show points in: | Puanları göster:<br>(corrects: Noktaları Göster:) | "Noktalar" are geometric points; the score points are "puan" (the term Show points in) |  |
| `AddCourse_courseKindCombo_Items1` | Score Course | Puanlı Parkur<br>(corrects: Parkur Puanı) | "Parkur Puanı" is the score of a course (the term Score Course) |  |

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Не включати у звіти | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_scoreColumnLabel_Text` | Show points in: | Відображати очки у:<br>(corrects: Відображати точки у:) | "точки" are geometric points; the score points are "очки" (the term Show points in) |  |
| `AddCourse_secondaryTitleGroup_Text` | Class list / Secondary title | Список груп / Додаткова назва<br>(corrects: Клас / Додаткова назва) | Ukrainian orienteering calls a class "група"; "клас" is a school class, and the list was missing (the term Class List) |  |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | 不包含在报表中 | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_scoreColumnLabel_Text` | Show points in: | 分值显示在：<br>(corrects: 显示点：) | 点 is a point of geometry; the score is 分值 (the term Show points in); "shown in", as zh-TW 積分值顯示在 | zh-TW |
| `AddCourse_groupBox1_Text` | Appearance | 外观<br>(corrects: 界面) | 界面 is the user interface; the appearance is 外观, as zh-TW 外觀 (the term Appearance) |  |

#### zh-TW

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | 不包含在報告中 | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
