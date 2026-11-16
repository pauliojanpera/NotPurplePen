# Review of the translations into et

Made by `review.py` from the filled key groups.

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Ära lisa aruannetesse | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_labelKindCombo_Items1` | Control code (145) | Tunnusnumber (145)<br>(corrects: Tunnusnumber (154)) | the example is the code 145, as the language's "Järjekorra ja tunnusnumber (3-145)"; "154" is a typo |  |
| `AddCourse_labelKindCombo_Items4` | Code and score (145(30)) | Tunnus ja väärtus (145(30))<br>(corrects: Tunnus ja väärtus 145(30))) | the opening parenthesis of the example was missing |  |
| `AllControlsProperties_Text` | All Controls Properties | Kõigi KPde seaded<br>(corrects: Kõik punktid seaded) | "Kõik punktid seaded" is not grammatical, and "punkt" is a point (the term All Controls Properties) |  |
| `AllControlsProperties_allControlsPropertiesLabel_Text` | These settings control how the map and descriptions for All Controls are printed. | Need seaded määravad, kuidas trükitakse kõigi KPde kaart ja legendid.<br>(corrects: Allolevad seaded määravad kuidas trükitakse kaardile legendid kõikide kontrollpunktide korral.) | the translation said that the descriptions are printed onto the map; the settings are for printing the map and the descriptions (the term All Controls Properties) |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (muutmata) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Arv<br>(corrects: Number) | the column shows singular or plural, the grammatical number; "number" is a number in a sequence; the grammatical number is "arv" (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Kääne<br>(corrects: Valik) | the grammatical case of the description text; "Valik" is a choice (the term Case) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Muudetud sümboli kääne:<br>(corrects: Muudetud sümbolid:) | the label names the grammatical case of the symbol that this symbol modifies; "Muudetud sümbolid" (modified symbols) lacked the case (the term Case) |  |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Ava näidisvõistlus<br>(corrects: Ava näidis võistlus) | the compound is one word in Estonian ("näidis võistlus") |  |
| `PrintDescriptions_orientation_Text` | Landscape | Rõhtpaigutus<br>(corrects: Maastik) | "Maastik" is terrain; the page orientation is "rõhtpaigutus" (the term Landscape) |  |
| `PrintDescriptions_Text` | Print Descriptions | Trüki legendid<br>(corrects: Trüki legendid:) | the colon does not belong in the title of a window (the term Print Descriptions) |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | Muuda ääriseid...<br>(corrects: Muuda ääriseid) | the button opens the Page Setup dialog, and the ellipsis that says so was missing |  |
| `PrintDescriptions_descriptionKindCombo_Items` | Course default | Vaikimisi rada<br>(corrects: Vaikimis rada) | "vaikimis" is a form of "vaikimine", keeping silent; "by default" is "vaikimisi", as the language's "vaikimisi kõigile uutele võistlustele" (terms.csv now gives it) |  |
| `IncrementButton` | Increment | Suurenda | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Vähenda | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Loendur: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Jah | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Ei | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Vaheta keelt... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Sulge | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Vaheta keelt | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Vali keel: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
