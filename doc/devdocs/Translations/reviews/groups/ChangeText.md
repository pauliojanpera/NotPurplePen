# Key group ChangeText

Nov 16, 2026. The Text Properties dialog (the WinForms ChangeText), which sets a text on the map: the text, with the
button Insert Special Text and its menu of the texts that Purple Pen fills in (the event title, the course's name,
part, variation, length and climb, the class list, the print scale, the relay team and leg, and the names of the
event file and the map file), and the appearance (font, bold, italic, color, size in mm or automatic), with a sample.
claude-opus-5-5 at effort max made every line below, and each entry says so in its comment.

- **Translated**: 68 lines, in 7 languages. ru lacked the whole dialog. nn-NO, ro and sl lacked half of the menu of
  special texts and the appearance, pl four items of the menu, nl three texts and ja one.
- **Not needed**: none.
- **Corrected**: 21 lines, most of them after terms.csv.
  - da called the course "kursus", a course of study, in five items of the menu, and the event "hændelse", an
    incident: now "bane" and "løb". It kept the English "Relay Team" and "Relay ben" ("ben" is a leg of the body):
    now "Stafethold" and "Stafettur". "Udskrift skala" is one word in Danish, "Kortdilnavn" was a typo, and the label
    "størrelse:" began with a small letter.
  - The class list was a school class in hu ("Osztálylista") and uk ("Список класів").
  - pl's group of the text was "Opis", a description. zh-CN's appearance was the user interface, 界面.
  - sl's Insert Special Text was "Vnesi dodaten tekst", enter additional text.
  - Typos: tr "Otomarik" and "Parkur Varyansonu", uk "Маштаб". uk's genitive "файла" is now "файлу", as terms.csv
    gives it.
- **Left empty**: none.

Doubts:

- terms.csv gives other words for the relay leg than four languages use here. hu "Váltó ág", pt "Pernada da
  Estafeta" and pt-BR "Pernada de Revezamento" stay, because these languages use the same word for a relay leg in
  their other texts ("Minden váltó ág külön", "Cada pernada de Estafeta Separadamente"). nl "Estafette been" stays,
  because nl uses "been" for every leg and has no other text of a relay leg. bg "Щафетен пост" stays, because I am
  not sure that "пост" is wrong for a relay leg in Bulgarian. The groups of the relay dialogs and reports can settle
  the word for all their texts.
- de calls the course part "Teilstrecke" here and in three other keys, which terms.csv gives as de's leg; the term is
  "Bahnabschnitt". It stays until the groups of the course part.
- nn-NO writes "Text" as "tekst", with a small letter, also as a header and a menu item, and terms.csv gives it so. It
  stays.
- The button Change Color... is a key of the WinForms dialog: the Avalonia dialog shows a color chooser in its place.
  It is translated, as the other languages translate it.
- uk still writes "файла" in about 15 other texts. Their groups correct them against terms.csv.

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_eventTitleMenuItem_Text` | Event Title | Løbets titel<br>(corrects: Hændelsestitel) | "hændelse" is an incident; an orienteering event is a "løb" (the term Event Title) |  |
| `ChangeText_courseNameMenuItem_Text` | Course Name | Banenavn<br>(corrects: Kursusnavn) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term Course Name) |  |
| `ChangeText_coursePartMenuItem_Text` | Course Part | Banedel<br>(corrects: Kursusdel) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term Course Part) |  |
| `ChangeText_variationMenuItem_Text` | Course Variation | Banevariation<br>(corrects: Kursus variation) | Danish orienteering calls a course "bane" ("kursus" is a course of study), and the compound is one word (the term Course Variation) |  |
| `ChangeText_courseLengthMenuItem_Text` | Course Length | Banelængde<br>(corrects: Kursuslængde) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term Course Length) |  |
| `ChangeText_courseClimbMenuItem_Text` | Course Climb | Banens stigning<br>(corrects: Kursusstigning) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term Course Climb) |  |
| `ChangeText_printScaleMenuItem_Text` | Print Scale | Udskriftsskala<br>(corrects: Udskrift skala) | the compound is one word in Danish (the term Print Scale) |  |
| `ChangeText_relayTeamMenuItem_Text` | Relay Team | Stafethold<br>(corrects: Relay Team) | "Relay Team" was the English; a relay team is a "stafethold" (the term Relay Team) |  |
| `ChangeText_relayLegMenuItem_Text` | Relay Leg | Stafettur<br>(corrects: Relay ben) | "Relay" was English and "ben" is a leg of the body; Danish relays count their legs as "ture" (the term Relay Leg) |  |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | Kortfilnavn<br>(corrects: Kortdilnavn) | the typo "Kortdilnavn" (the term Map File Name) |  |
| `ChangeText_label3_Text` | Size: | Størrelse:<br>(corrects: størrelse:) | the label began with a small letter, unlike the other labels of the dialog (the term Size) |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_classListMenuItem_Text` | Class List | Kategórialista<br>(corrects: Osztálylista) | Hungarian orienteering calls a class "kategória"; "osztály" is a school class (the term Class List) |  |

#### ja

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | 地図ファイル名 | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the name of the map file; the term Map File Name |  |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | Kaartbestandsnaam | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the name of the map file; the term Map File Name |  |
| `ChangeText_label3_Text` | Size: | Grootte: | label of the font size in mm; the term Size, with the colon of a label |  |
| `ChangeText_checkBoxAutoFontSize_Text` | Automatic | Automatisch | check box after the font size: the size is set automatically, and the size box is disabled; the term Automatic |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_coursePartMenuItem_Text` | Course Part | Løypedel | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the course part; the term Course Part |  |
| `ChangeText_variationMenuItem_Text` | Course Variation | Løypevariasjon | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the course's variation code; the term Course Variation |  |
| `ChangeText_printScaleMenuItem_Text` | Print Scale | Utskriftsskala | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the scale that the course is printed at; the term Print Scale |  |
| `ChangeText_relayTeamMenuItem_Text` | Relay Team | Stafettlag | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay team; the term Relay Team |  |
| `ChangeText_relayLegMenuItem_Text` | Relay Leg | Stafettetappe | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay leg; the term Relay Leg |  |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | Kartfilnamn | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the name of the map file; the term Map File Name |  |
| `ChangeText_buttonChangeColor_Text` | Change Color... | Endre farge... | button of the WinForms dialog that opens the color chooser (the Avalonia dialog shows a color chooser in its place); the term Change Color, with the ellipsis of a button that opens a dialog |  |
| `ChangeText_label2_Text` | Color: | Farge: | label of the color of the text; the term Color, with the colon of a label |  |
| `ChangeText_checkBoxItalic_Text` | Italic | Kursiv | check box beside the list of fonts: the text is italic; the term Italic |  |
| `ChangeText_label1_Text` | Font: | Skrift: | label of the list of fonts; the term Font, with the colon of a label |  |
| `ChangeText_label3_Text` | Size: | Storleik: | label of the font size in mm; the term Size, with the colon of a label | UIText.PunchcardLayoutDialog_sizeGroupBox_Text sizeGroupBox.Text |
| `ChangeText_checkBoxAutoFontSize_Text` | Automatic | Automatisk | check box after the font size: the size is set automatically, and the size box is disabled; the term Automatic |  |
| `ChangeText_groupBox2_Text` | Sample | Eksempel | header of the group that shows a sample of the text as it will be drawn; the term Sample |  |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_variationMenuItem_Text` | Course Variation | Wariant trasy | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the course's variation code; the term Course Variation |  |
| `ChangeText_relayTeamMenuItem_Text` | Relay Team | Drużyna sztafetowa | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay team; the term Relay Team |  |
| `ChangeText_relayLegMenuItem_Text` | Relay Leg | Zmiana sztafety | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay leg; the term Relay Leg |  |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | Nazwa pliku mapy | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the name of the map file; the term Map File Name |  |
| `ChangeText_groupBox3_Text` | Text | Tekst<br>(corrects: Opis) | header of the group of the text box: "Opis" is a description; the text is "Tekst" (the term Text), as the earlier groups corrected it |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_coursePartMenuItem_Text` | Course Part | Parte de traseu | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the course part; the term Course Part |  |
| `ChangeText_variationMenuItem_Text` | Course Variation | Variantă de traseu | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the course's variation code; the term Course Variation |  |
| `ChangeText_printScaleMenuItem_Text` | Print Scale | Scara de tipărire | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the scale that the course is printed at; the term Print Scale |  |
| `ChangeText_relayTeamMenuItem_Text` | Relay Team | Echipă de ștafetă | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay team; the term Relay Team |  |
| `ChangeText_relayLegMenuItem_Text` | Relay Leg | Etapă de ștafetă | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay leg; the term Relay Leg |  |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | Nume fișier hartă | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the name of the map file; the term Map File Name |  |
| `ChangeText_buttonChangeColor_Text` | Change Color... | Schimbă culoarea... | button of the WinForms dialog that opens the color chooser (the Avalonia dialog shows a color chooser in its place); the term Change Color, with the ellipsis of a button that opens a dialog |  |
| `ChangeText_label2_Text` | Color: | Culoare: | label of the color of the text; the term Color, with the colon of a label |  |
| `ChangeText_checkBoxItalic_Text` | Italic | Cursiv | check box beside the list of fonts: the text is italic; the term Italic |  |
| `ChangeText_label1_Text` | Font: | Font: | label of the list of fonts; the term Font, with the colon of a label |  |
| `ChangeText_label3_Text` | Size: | Dimensiune: | label of the font size in mm; the term Size, with the colon of a label | UIText.PunchcardLayoutDialog_sizeGroupBox_Text sizeGroupBox.Text |
| `ChangeText_checkBoxAutoFontSize_Text` | Automatic | Automat | check box after the font size: the size is set automatically, and the size box is disabled; the term Automatic |  |
| `ChangeText_groupBox2_Text` | Sample | Exemplu | header of the group that shows a sample of the text as it will be drawn; the term Sample |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_eventTitleMenuItem_Text` | Event Title | Название соревнования | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the event's title; the term Event Title |  |
| `ChangeText_courseNameMenuItem_Text` | Course Name | Название дистанции | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the course's name; the term Course Name | UIText.AddCourse_courseNameLabel_Text courseNameLabel.Text |
| `ChangeText_coursePartMenuItem_Text` | Course Part | Часть дистанции | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the course part; the term Course Part |  |
| `ChangeText_variationMenuItem_Text` | Course Variation | Вариант дистанции | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the course's variation code; the term Course Variation |  |
| `ChangeText_courseLengthMenuItem_Text` | Course Length | Длина дистанции | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the course's length; the term Course Length |  |
| `ChangeText_courseClimbMenuItem_Text` | Course Climb | Набор высоты дистанции | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the course's climb; the term Course Climb |  |
| `ChangeText_classListMenuItem_Text` | Class List | Список групп | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the course's class list; the term Class List |  |
| `ChangeText_printScaleMenuItem_Text` | Print Scale | Масштаб печати | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the scale that the course is printed at; the term Print Scale |  |
| `ChangeText_relayTeamMenuItem_Text` | Relay Team | Эстафетная команда | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay team; the term Relay Team |  |
| `ChangeText_relayLegMenuItem_Text` | Relay Leg | Этап эстафеты | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay leg; the term Relay Leg |  |
| `ChangeText_fileNameMenuItem_Text` | File Name | Имя файла | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the name of the event file; the term File Name |  |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | Имя файла карты | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the name of the map file; the term Map File Name |  |
| `ChangeText_buttonChangeColor_Text` | Change Color... | Изменить цвет... | button of the WinForms dialog that opens the color chooser (the Avalonia dialog shows a color chooser in its place); the term Change Color, with the ellipsis of a button that opens a dialog |  |
| `ChangeText_label2_Text` | Color: | Цвет: | label of the color of the text; the term Color, with the colon of a label |  |
| `ChangeText_checkBoxItalic_Text` | Italic | Курсив | check box beside the list of fonts: the text is italic; the term Italic |  |
| `ChangeText_checkBoxBold_Text` | Bold | Полужирный | check box beside the list of fonts: the text is bold; the term Bold |  |
| `ChangeText_label1_Text` | Font: | Шрифт: | label of the list of fonts; the term Font, with the colon of a label |  |
| `ChangeText_label3_Text` | Size: | Размер: | label of the font size in mm; the term Size, with the colon of a label |  |
| `ChangeText_labelFontSizeMm_Text` | mm | мм | the unit after the box of the font size, in the Cyrillic letters of the language | UIText.PrintDescriptions_mmSuffixLabel_Text |
| `ChangeText_checkBoxAutoFontSize_Text` | Automatic | Автоматически | check box after the font size: the size is set automatically, and the size box is disabled; the term Automatic |  |
| `ChangeText_groupBox2_Text` | Sample | Образец | header of the group that shows a sample of the text as it will be drawn; the term Sample |  |
| `ChangeText_insertSpecialButton_Text` | Insert Special Text | Вставить специальный текст | button under the text box that opens the menu of the special texts (macros such as the event title); the term Insert Special Text |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_coursePartMenuItem_Text` | Course Part | Del proge | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the course part; the term Course Part |  |
| `ChangeText_variationMenuItem_Text` | Course Variation | Varianta proge | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the course's variation code; the term Course Variation |  |
| `ChangeText_relayTeamMenuItem_Text` | Relay Team | Štafetna ekipa | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay team; the term Relay Team |  |
| `ChangeText_relayLegMenuItem_Text` | Relay Leg | Etapa štafete | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the number of the relay leg; the term Relay Leg |  |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | Ime datoteke s karto | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the name of the map file; the term Map File Name |  |
| `ChangeText_buttonChangeColor_Text` | Change Color... | Spremeni barvo... | button of the WinForms dialog that opens the color chooser (the Avalonia dialog shows a color chooser in its place); the term Change Color, with the ellipsis of a button that opens a dialog |  |
| `ChangeText_label2_Text` | Color: | Barva: | label of the color of the text; the term Color, with the colon of a label |  |
| `ChangeText_checkBoxItalic_Text` | Italic | Ležeče | check box beside the list of fonts: the text is italic; the term Italic |  |
| `ChangeText_label1_Text` | Font: | Pisava: | label of the list of fonts; the term Font, with the colon of a label |  |
| `ChangeText_label3_Text` | Size: | Velikost: | label of the font size in mm; the term Size, with the colon of a label |  |
| `ChangeText_checkBoxAutoFontSize_Text` | Automatic | Samodejno | check box after the font size: the size is set automatically, and the size box is disabled; the term Automatic |  |
| `ChangeText_groupBox2_Text` | Sample | Vzorec | header of the group that shows a sample of the text as it will be drawn; the term Sample |  |
| `ChangeText_insertSpecialButton_Text` | Insert Special Text | Vstavi posebno besedilo<br>(corrects: Vnesi dodaten tekst) | "Vnesi dodaten tekst" is "enter additional text"; the button inserts the special texts (the term Insert Special Text) |  |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_variationMenuItem_Text` | Course Variation | Parkur Varyasyonu<br>(corrects: Parkur Varyansonu) | the typo "Varyansonu" (the term Course Variation) |  |
| `ChangeText_checkBoxAutoFontSize_Text` | Automatic | Otomatik<br>(corrects: Otomarik) | the typo "Otomarik" (the term Automatic) |  |

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_classListMenuItem_Text` | Class List | Список груп<br>(corrects: Список класів) | Ukrainian orienteering calls a class "група"; "клас" is a school class (the term Class List) |  |
| `ChangeText_printScaleMenuItem_Text` | Print Scale | Масштаб для друку<br>(corrects: Маштаб для друку) | the typo "Маштаб" (the term Print Scale) |  |
| `ChangeText_fileNameMenuItem_Text` | File Name | Назва файлу<br>(corrects: Назва файла) | the genitive of "файл" is "файлу" (the term File Name) |  |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | Назва файлу карти<br>(corrects: Назва файла карти) | the genitive of "файл" is "файлу" (the term Map File Name) |  |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `ChangeText_groupBox1_Text` | Appearance | 外观<br>(corrects: 界面) | 界面 is the user interface; the appearance is 外观, as zh-TW 外觀 (the term Appearance) |  |
