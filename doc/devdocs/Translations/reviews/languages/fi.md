# Review of the translations into fi

Made by `review.py` from the filled key groups.

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Älä sisällytä raportteihin | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen on vapaa ohjelma, jota saa kopioida ja jakaa. Purple Penin kehitys on ainoastaan lahjoitustenne varassa.<br>(corrects: Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations.) | the satellite held the English; the first sentence as the language's About dialog (AboutForm_freeLabel_Text), "lahjoitus" as the term Make a donation | AboutForm_freeLabel_Text |
| `InitialScreen_donationLink_Text` | Make a donation | Lahjoita<br>(corrects: Make a donation) | the satellite held the English; the term Make a donation |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Karttatiedostossa "{0}" on käytössä fontteja, joita ei ole asennettu tietokoneellesi. Tekstit, joissa näitä fontteja on käytössä, piirretään ja tulostetaan oletusfontilla, mikä saattaa aiheuttaa, että teksti näkyy kartalla eri tavalla kuin kartan tekijä on tarkoittanut. <br><br>Korjataksesi ongelman, etsi ja asenna alla listatut fontit ja käynnistä Purple Pen sen jälkeen uudelleen.<br><br>(corrects: Karttatiedostossa "{0}" on käytössä fontteja, joita ei ole asennettu tietokoneellesi. Tekstit, joissa näitä fontteja on käytössä, piirretään ja tulostetaan oletusfontilla, mikä saattaa aiheuttaa, että teksti näkyy kartalla ei tavalla kuin kartan tekijä on tarkoittanut. <br><br>Korjataksesi ongelman, etsi ja asenna alla listatut fontit ja käynnistä Purple Pen sen jälkeen uudelleen.<br>) | the typo "ei tavalla" (no way) for "eri tavalla" (differently); nothing else changes |  |
| `IncrementButton` | Increment | Kasvata | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Vähennä | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Laskuri: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Kyllä | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Ei | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Vaihda kieli... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Sulje | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Vaihda kieli | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Valitse kieli: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
