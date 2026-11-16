# Review of the translations into ro

Made by `review.py` from the filled key groups.

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_copyrightLabel_Text` | Copyright © by Peter Golde | Copyright © Peter Golde | one line of the About dialog, under the version; kept short, as the languages that keep "Copyright ©" | cs es fi hu it |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | ACEST PROGRAM ESTE OFERIT DE CĂTRE DEȚINĂTORUL DREPTURILOR DE AUTOR ȘI CONTRIBUTORI "AȘA CUM ESTE" ȘI SUNT EXCLUSE ORICE GARANȚII IMPLICITE SAU EXPLICITE, SAU GARANȚII IMPLICITE DE COMERCIALIZARE SAU PENTRU UN ANUMIT SCOP, DAR FĂRĂ A SE LIMITA LA ACESTEA.<br>(corrects: ACEST PROGRAM ESTE OFERIT DE CĂTRE DEȚINĂTORUL DREPTURILOR DE AUTOR ȘI CONTRIBUTORI "AȘA CUM ESTE" ȘI SUNT EXCLUSE ORICE GARAȚII IMPLICITE SAU EXPLICITE, SAU GARAȚII IMPLICITE DE COMERCIALIZARE SAU PENTRU UN ANUMIT SCOP, DAR FARĂ A SE LIMITA LA ACESTEA.) | the typos "GARAȚII" (twice) and "FARĂ"; nothing else changes |  |
| `IncrementButton` | Increment | Creștere | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Descreștere | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Contor: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Da | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Nu | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Schimbare limbă... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Închide | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Schimbare limbă | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Selectați o limbă: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
