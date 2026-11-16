# Review of the translations into it

Made by `review.py` from the filled key groups.

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | QUESTO SOFTWARE È FORNITO DAI DETENTORI DEL COPYRIGHT E DAI CONTRIBUTORI "COSÌ COM'È" E OGNI GARANZIA ESPRESSA O IMPLICITA, INCLUSE, MA NON SOLO, LE GARANZIE IMPLICITE DI COMMERCIABILITÀ E DI IDONEITÀ PER UNO SCOPO PARTICOLARE, È ESCLUSA.<br>(corrects: QUESTO SOFTWARE È FORNITO DAI DETENTORI DEL COPYRIGHT E DAI CONTRIBUTORI "COSÌ COM'È" E OGNI GARANZIA ESPRESSA O SOTTINTESA, INCLUDENDO, MA NON LIMITATA A, LE GARANZIE SOTTINTESE DI COMMERCIABILITÀ E DI BENESSERE PER UNO SCOPO PARTICOLARE SONO DICHIARATE.) | the translation said that the warranties "SONO DICHIARATE" (are declared), the opposite of disclaimed, and "BENESSERE" (well-being) for fitness; now they are excluded, as in the English | fr es |
| `IncrementButton` | Increment | Aumenta | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Diminuisci | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Contatore: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Sì | button of the message box (MessageBoxDialog); "sì" (yes) takes its accent; the term and two texts of the language write "Si", which is corrected | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | No | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Cambia lingua... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Chiudi | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `ReportText.EventAudit_Yes` | Yes | Sì<br>(corrects: Si) | "Si" is the pronoun; "sì" (yes) takes its accent; beside "No" in the event audit | YesButton |
| `ReportText_EventAudit_Yes` | Yes | Sì<br>(corrects: Si) | the copy of ReportText.EventAudit_Yes; "Si" is the pronoun; "sì" (yes) takes its accent | YesButton |
| `CreateImageFiles_comboBoxWorldFile_Items1` | Yes | Sì<br>(corrects: Si) | item "Yes" of the world file choice, beside "No"; "Si" is the pronoun; "sì" (yes) takes its accent | YesButton |
| `SwitchLanguageDialog_Title` | Switch Language | Cambia lingua | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Seleziona una lingua: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
