# Review of the translations into nb-NO

Made by `review.py` from the filled key groups.

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Åpne eksempelarrangement<br>(corrects: Åpne eksempel arrangement) | the compound is one word in Norwegian ("eksempel arrangement") |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Åpne et eksisterende arrangement...<br>(corrects: Åpne et eksisterende arrangenment) | the typo "arrangenment"; the ellipsis of the English, as the choice opens a file dialog |  |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen er fri programvare og kan kopieres og deles. Utvikling er kun støttet av frivillige donasjoner.<br>(corrects: Purple Pen er fri programvare og kan kopieres og deles. Utvilking er kun støttet av frivillige donasjoner.) | the typo "Utvilking"; nothing else changes |  |
| `LicenseForm_bsdLicenseLinkLabel_Text` | Purple Pen is provided under a "BSD" style license. | Purple Pen er utgitt under en "BSD"-liknende lisens.<br>(corrects: Purpel Pen er utgitt under "BSD" liknende lisens.) | the typo "Purpel" in the program's name, and the compound "BSD"-liknende is joined with a hyphen |  |
| `IncrementButton` | Increment | Øk | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Reduser | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Teller: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Ja | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Nei | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Bytt språk... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Lukk | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Bytt språk | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Velg et språk: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
