# Review of the translations into hu

Made by `review.py` from the filled key groups.

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_creditsButton_Text` | Credits | Közreműködők<br>(corrects: Névjegy) | "Névjegy" is the About of a program; the button shows the credits, the term Credits | nb-NO da nn-NO uk ru |
| `AboutForm_licenseButton_Text` | View Full License | A teljes licenc<br>(corrects: A teljes licensz) | the Hungarian spelling is "licenc"; the term "A teljes licenc megtekintése" would leave no room for the Credits button beside it (about 167 px of text against 102 of the English), so the button keeps its form without the verb |  |
| `InitialScreen_Text` | Welcome to Purple Pen | Üdvözöljük a Purple Pen-ben<br>(corrects: Üdözöljük a Purple Pen-ben) | the typo "Üdözöljük" |  |
| `IncrementButton` | Increment | Növelés | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Csökkentés | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Számláló: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Igen | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Nem | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Nyelv váltása... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Bezárás | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Nyelv váltása | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Válasszon nyelvet: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
