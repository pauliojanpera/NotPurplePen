# Review of the translations into sv

Made by `review.py` from the filled key groups.

#### sv

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_firstControlLabel_Text` | First control number: | Första kontrollnummer:<br>(corrects: Första kodsiffra:) | "kodsiffra" is the code; the label asks for the number of the first control (the term First control number) |  |
| `AddCourse_descriptionAppearanceLabel_Text` | Description appearance: | Kontrollbeskrivningens utseende:<br>(corrects: Definitionens utseende:) | "definition" is the language's rarer word; its descriptions are "kontrollbeskrivningar" (the term Description appearance) |  |
| `AllControlsProperties_descriptionAppearanceLabel_Text` | Description appearance: | Kontrollbeskrivningens utseende:<br>(corrects: Definitionens utseende:) | "definition" is the language's rarer word; its descriptions are "kontrollbeskrivningar" (the term Description appearance) |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (oförändrad) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Numerus<br>(corrects: Antal) | the column shows singular or plural, the grammatical number; "antal" is a count; the grammatical number is "numerus" (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_genderColumn_HeaderText` | Gender | Genus<br>(corrects: Kön) | the grammatical gender of the description text; "kön" is the sex of a person (the term Gender), as the language's "Visa ords genusändelse" |  |
| `EnterSymbolText_labelGenderChooser_Text` | Gender: | Genus:<br>(corrects: Kön:) | the grammatical gender of the description text; "kön" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Kasus<br>(corrects: Case) | the grammatical case of the description text; "Case" was the English (the term Case) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Visa kasusformer<br>(corrects: Visa case-formulär) | "case-formulär" is a form to fill in, with the English "case" (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Kasus för modifierad symbol:<br>(corrects: Case på modifierad symbol:) | the label names the grammatical case of the symbol that this symbol modifies; "Case" was the English (the term Case) |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Radstorlek:<br>(corrects: Linjetjocklek:) | "Linjetjocklek" is the thickness of a drawn line; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |
| `PrintDescriptions_printerGroup_Text` | Output | Utdata<br>(corrects: Skrivare) | "Skrivare" is the printer; the group also holds the paper and the margins, and the dialog that makes a PDF shows it without a printer (terms.csv now gives "Utdata", as nb-NO and nn-NO) |  |
| `IncrementButton` | Increment | Öka | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Minska | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Räknare: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Ja | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Nej | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Byt språk... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Stäng | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Byt språk | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Välj ett språk: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
