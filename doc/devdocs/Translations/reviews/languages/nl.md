# Review of the translations into nl

Made by `review.py` from the filled key groups.

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_licenseButton_Text` | View Full License | Bekijk de volledige licentie<br>(corrects: Bekijke de volledige licentie) | the typo "Bekijke"; the term View Full License |  |
| `InitialScreen_openLastRadioButton_Text` | Open last viewed event | Open laatst bekeken evenement | third choice of the window that opens at start (InitialScreenWindow, 335 px wide, one line); the terms Open and Event | InitialScreen_createNewRadioButton_Text |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Open een voorbeeldevenement | fourth choice of the start window; the terms Open, Sample and Event |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Open een bestaand evenement... | second choice of the start window; the terms Open and Event, with the ellipsis of a choice that opens a file dialog |  |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen is gratis software en mag worden gekopieerd en gedeeld. De ontwikkeling van Purple Pen wordt uitsluitend gesteund door uw donaties. | the note above the donation link of the start window; its first sentence as the language's About dialog (AboutForm_freeLabel_Text) | AboutForm_freeLabel_Text |
| `InitialScreen_donationLink_Text` | Make a donation | Doneren | the link under the note of the start window, which opens the donation page; the term Make a donation |  |
| `InitialScreen_Text` | Welcome to Purple Pen | Welkom bij Purple Pen | title of the start window |  |
| `LicenseForm_bsdLicenseLinkLabel_Text` | Purple Pen is provided under a "BSD" style license. | Purple Pen wordt aangeboden onder een "BSD"-achtige licentie. | the line above the licence text of the License dialog, which the About dialog opens | de da |
| `LicenseForm_Text` | Purple Pen License | Licentie van Purple Pen | title of the License dialog, which the About dialog's button View Full License opens | AboutForm_licenseButton_Text |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Niet meer waarschuwen voor ontbrekende lettertypen in dit evenement | check box beside the OK button of the Missing Fonts dialog (resizable, 480 px wide); the setting is kept for the event; the term Event |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Kaartbestand "{0}" gebruikt lettertypen die niet op uw computer zijn geïnstalleerd. Tekst in deze lettertypen wordt met een standaardlettertype getekend en afgedrukt, waardoor de tekst er anders kan uitzien dan de maker van de kaart bedoelde. <br><br>Zoek de hieronder vermelde lettertypen op en installeer ze via het Configuratiescherm van Windows om dit probleem op te lossen. Start Purple Pen daarna opnieuw.<br> | the text above the list of the missing fonts, a format string with the map file's name as {0}; two paragraphs and the closing line break of the English; the terms Map File and Font | de |
| `MissingFonts_Text` | Missing Fonts | Ontbrekende lettertypen | title of the Missing Fonts dialog; the term Missing Fonts |  |
| `IncrementButton` | Increment | Verhogen | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Verlagen | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Teller: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Ja | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Nee | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Taal wijzigen... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CancelButton` | Cancel | Annuleren | button of the message box and of many dialogs; the term Cancel; the language has no Cancel button yet |  |
| `CloseButton` | Close | Sluit | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Taal wijzigen | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Kies een taal: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
