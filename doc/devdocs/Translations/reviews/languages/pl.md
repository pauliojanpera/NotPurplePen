# Review of the translations into pl

Made by `review.py` from the filled key groups.

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_copyrightLabel_Text` | Copyright © by Peter Golde | Copyright © Peter Golde | one line of the About dialog, under the version; kept short, as the languages that keep "Copyright ©" | cs es fi hu it |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | TO OPROGRAMOWANIE JEST DOSTARCZANE PRZEZ POSIADACZY PRAW AUTORSKICH I WSPÓŁTWÓRCÓW „TAKIM, JAKIE JEST”. WSZELKIE GWARANCJE WYRAŹNE LUB DOROZUMIANE, W TYM, ALE NIE WYŁĄCZNIE, DOROZUMIANE GWARANCJE PRZYDATNOŚCI HANDLOWEJ I PRZYDATNOŚCI DO OKREŚLONEGO CELU, SĄ WYŁĄCZONE. | the disclaimer of the BSD licence in the About dialog, in the wording of Polish translations of the licence, in capitals as the English | cs ru |
| `AboutForm_freeLabel_Text` | Purple Pen is free software and may be copied and shared. | Purple Pen jest darmową aplikacją i jako taka może być kopiowana i udostępniana.<br>(corrects: Purple Pen jest darmową aplikacją i jako taka może być kopiowany i udostępniany.) | "jako taka" is the application, so the participles are feminine; the translation had "kopiowany i udostępniany" |  |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Punkty (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Nie uwzględniaj w raportach | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_descKindCombo_Items1` | Text | Tekst<br>(corrects: Opis) | "Opis" is a description; the item is the text of the descriptions (the term Text, the language's "Dodaj tekst") |  |
| `AddCourse_descKindCombo_Items2` | Symbols and text | Piktografiki i tekst<br>(corrects: Piktografiki i opis) | "opis" is a description; the term Text, as the item before |  |
| `AddCourse_labelKindCombo_Items1` | Control code (145) | Kod PK (145)<br>(corrects: Kod punktu (145)) | the language's control is "PK" (the term Control code (145)); "punkty" are also the score points |  |
| `AddCourse_Text` | New Course | Nowa trasa<br>(corrects: Nowy Kurs) | "kurs" is a course of study; the language's course is "trasa" (the term New Course) |  |
| `AllControlsProperties_descKindCombo_Items1` | Text | Tekst<br>(corrects: Opis) | "Opis" is a description; the item is the text of the descriptions (the term Text) |  |
| `AllControlsProperties_descKindCombo_Items2` | Symbols and text | Piktografiki i tekst<br>(corrects: Piktografiki i opis) | "opis" is a description; the term Text, as the item before |  |
| `AllControlsProperties_Text` | All Controls Properties | Ustawienia wszystkich PK<br>(corrects: Ustawienia Wszystkich Punktów) | the language's control is "PK", as its view "Wszystkie PK" (the term All Controls Properties) |  |
| `AllControlsProperties_allControlsPropertiesLabel_Text` | These settings control how the map and descriptions for All Controls are printed. | Te ustawienia mają wpływ na to, w jaki sposób drukowane są mapa oraz opisy dla Wszystkich PK.<br>(corrects: Te ustawienia mają wpływ na to w jaki sposób mapa oraz opisy Wszystkich Punktów są drukowane.) | the language's control is "PK", as its view "Wszystkie PK"; "punkty" are also the score points |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Plik mapy "{0}" używa czcionek, które nie są zainstalowane na tym komputerze. Tekst w tych czcionkach zostanie narysowany i wydrukowany czcionką domyślną, przez co może wyglądać inaczej, niż zamierzył autor mapy. <br><br>Aby rozwiązać ten problem, znajdź i zainstaluj wymienione poniżej czcionki za pomocą Panelu sterowania systemu Windows, a następnie ponownie uruchom program Purple Pen.<br> | the text above the list of the missing fonts, a format string with the map file's name as {0}; two paragraphs and the closing line break of the English; the terms Map File and Font | cs |
| `IncrementButton` | Increment | Zwiększ | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Zmniejsz | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Licznik: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Tak | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Nie | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Zmień język... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Zamknij | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Zmień język | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Wybierz język: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
