# Review of the translations into nn-NO

Made by `review.py` from the filled key groups.

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_copyrightLabel_Text` | Copyright © by Peter Golde | Opphavsrett © Peter Golde | one line of the About dialog, under the version; "opphavsrett" is Nynorsk as Bokmål, as nb-NO | nb-NO |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen er fri programvare og kan kopierast og delast. Utviklinga av Purple Pen blir berre støtta av bidraga dykkar. | the note above the donation link of the start window; Nynorsk forms (kopierast, delast, utviklinga, berre), "bidrag" as the term Make a donation |  |
| `InitialScreen_donationLink_Text` | Make a donation | Gje eit bidrag | the link under the note of the start window, which opens the donation page; the term Make a donation |  |
| `InitialScreen_openLastRadioButton_Text` | Open last viewed event | Opne sist opna arrangement<br>(corrects: Åpne sist opna arrangement) | "Åpne" is Bokmål; the Nynorsk verb is "opne" (the term Open), as the text's own "opna" |  |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Opne eksempelarrangementet<br>(corrects: Åpne eksempelarrangementet) | "Åpne" is Bokmål; the Nynorsk verb is "opne" (the term Open) |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Opne eit eksisterande arrangement...<br>(corrects: Opne eit eksisterende arrangenment) | the typo "arrangenment", and the Bokmål participle "eksisterende" (Nynorsk "eksisterande"); the ellipsis of the English, as the choice opens a file dialog |  |
| `InitialScreen_Text` | Welcome to Purple Pen | Velkomen til Purple Pen<br>(corrects: Velkommen til Purple Pen) | "Velkommen" is Bokmål; Nynorsk writes "velkomen" |  |
| `LicenseForm_bsdLicenseLinkLabel_Text` | Purple Pen is provided under a "BSD" style license. | Purple Pen er utgitt under ein "BSD"-liknande lisens.<br>(corrects: Purpel Pen er utgitt under "BSD" liknande lisens.) | the typo "Purpel" in the program's name, and the compound "BSD"-liknande is joined with a hyphen |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Kartfila "{0}" brukar fontar som ikkje er installerte på datamaskina di. Tekst med desse fontane blir teikna og skriven ut med ein standardfont, og då kan teksten sjå annleis ut enn kartteiknaren meinte. <br><br>For å rette problemet må du finne og installere fontane som er lista opp nedanfor, med kontrollpanelet i Windows, og så starte Purple Pen på nytt.<br><br>(corrects: Kartfilen "{0}"benytter ei fontfil som ikkje er installert på datamskinen. Tekst med denne fonten vil vises med standard font, dette kan medføre at tekst ikke ser ut som ønsket. <br><br>For å rette på problemet må fonten installeres eller en annen font velges. Etter installasjon av fonter må Purple Pen startes på nytt for å kunne bruke nye fonter.<br>) | the text was Bokmål ("benytter", "ikke", "installeres", "en annen", "startes") with the typo "datamskinen"; now Nynorsk, after the English |  |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Ikkje varsle om manglande fontar igjen<br>(corrects: Ikkje varsle om manglende fonter igjen) | "manglende" and "fonter" are Bokmål; Nynorsk "manglande" and "fontar" |  |
| `MissingFonts_Text` | Missing Fonts | Manglande fontar<br>(corrects: Manglande fonter) | "fonter" is the Bokmål plural; the Nynorsk plural of "font" is "fontar" (terms.csv now gives it) |  |
| `IncrementButton` | Increment | Auk | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Reduser | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Teljar: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Ja | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Nei | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Byt språk... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Lat att | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Byt språk | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Vel eit språk: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
