# Review of the translations into de

Made by `review.py` from the filled key groups.

#### de

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Die Kartendatei "{0}" benutzt Schriftarten (Fonts), die auf diesem Computer nicht installiert sind. Texte, die diese Fonts verwenden, werden mit dem Standardfont gezeichnet und gedruckt, was den Text unter Umständen anders aussehen lässt, als der Kartenzeichner beabsichtigt hatte. <br><br>Um das Problem zu lösen, suchen und installieren Sie die unten gelisteten Fonts über die Windows Systemsteuerung. Starten Sie dann Purple Pen neu.<br><br>(corrects: Die Kartendatei "{0}" benutzt Schriftarten (Fonts), die auf diesem Computer nicht installiert sind. Texte, die diese Fonts verwenden, werden mit dem Standardfont gezeichnet und gedruckt, was den Text unter Umständen anders aussehen lässt, als der Bahnleger beabsichtigt hatte. <br><br>Um das Problem zu lösen, suchen und installieren Sie die unten gelisteten Fonts über die Windows Systemsteuerung. Starten Sie dann Purple Pen neu.<br>) | the fonts are those of the map, which its map designer chose: "Kartenzeichner", not "Bahnleger" (the course setter); nothing else changes |  |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Für diesen Wettkampf nie mehr wegen fehlender Fonts warnen<br>(corrects: Für dieses Ereignis nie mehr wegen fehlender Fonts warnen) | "Ereignis" is an occurrence; the language's event is "Wettkampf" (the term Event) |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Zeilengröße:<br>(corrects: Linienstärke:) | "Linienstärke" is the thickness of a drawn line; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |
| `PrintDescriptions_margins_Text` | Top: 0.55", Bottom: 0.55", Left: 0.55", Right: 0.55" | Oben: 0.55", Unten: 0.55", Links: 0.55", Rechts: 0.55"<br>(corrects: Oben 0.55", Unten: 0.55", Link: 0.55", Rechts: 0.55") | the colon after "Oben" was missing, and the left side is "Links", not "Link"; a placeholder that the dialog replaces at runtime |  |
| `IncrementButton` | Increment | Erhöhen | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Verringern | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Zähler: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Ja | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Nein | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Sprache wechseln... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Schließen | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Sprache wechseln | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Sprache auswählen: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
