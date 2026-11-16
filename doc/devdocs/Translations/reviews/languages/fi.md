# Review of the translations into fi

Made by `review.py` from the filled key groups.

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Älä sisällytä raportteihin | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (ennallaan) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen on vapaa ohjelma, jota saa kopioida ja jakaa. Purple Penin kehitys on ainoastaan lahjoitustenne varassa.<br>(corrects: Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations.) | the satellite held the English; the first sentence as the language's About dialog (AboutForm_freeLabel_Text), "lahjoitus" as the term Make a donation | AboutForm_freeLabel_Text |
| `InitialScreen_donationLink_Text` | Make a donation | Lahjoita<br>(corrects: Make a donation) | the satellite held the English; the term Make a donation |  |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | _Leimauskortit...<br>(corrects: _Leimauskuviot...) | "Leimauskuviot" are the punch patterns; the item makes the punch cards (the term Punch Cards) |  |
| `MainFrame_printPunchCardsMenu_Text` | Print Pu_nch Cards... | Tulosta _leimauskortit...<br>(corrects: Tulosta _leimauskuviot...) | "Leimauskuviot" are the punch patterns; the item prints the punch cards (the term Print Punch Cards) |  |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Tilapäinen rakennelma | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Tilapäinen rakennelma | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_donateWebSiteMenu_Text` | Make a _Donation | _Lahjoita<br>(corrects: Make a _Donation) | "Make a Donation" is the English (the term Make a donation); the access key "l" is a letter that no other item of the Help menu uses |  |
| `MainFrame_symbolBrowserMenu_Text` | Symbol Browser | Symboliselain<br>(corrects: Symbol Browser) | "Symbol Browser" is the English (the term Symbol Browser) |  |
| `MainFrame_descriptionBrowserMenu_Text` | Description Browser | Rastinmääriteselain<br>(corrects: Description Browser) | "Description Browser" is the English; as the language's "Symboliselain" and "rastinmääritteet" (Descriptions) |  |
| `MainFrame_controlTesterMenu_Text` | Control Tester | Rastitesteri<br>(corrects: Control Tester) | "Control Tester" is the English; the language's control is "rasti" |  |
| `MainFrame_mapTesterMenu_Text` | Map Tester | Karttatesteri<br>(corrects: Map Tester) | "Map Tester" is the English |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Karttatiedostossa "{0}" on käytössä fontteja, joita ei ole asennettu tietokoneellesi. Tekstit, joissa näitä fontteja on käytössä, piirretään ja tulostetaan oletusfontilla, mikä saattaa aiheuttaa, että teksti näkyy kartalla eri tavalla kuin kartan tekijä on tarkoittanut. <br><br>Korjataksesi ongelman, etsi ja asenna alla listatut fontit ja käynnistä Purple Pen sen jälkeen uudelleen.<br><br>(corrects: Karttatiedostossa "{0}" on käytössä fontteja, joita ei ole asennettu tietokoneellesi. Tekstit, joissa näitä fontteja on käytössä, piirretään ja tulostetaan oletusfontilla, mikä saattaa aiheuttaa, että teksti näkyy kartalla ei tavalla kuin kartan tekijä on tarkoittanut. <br><br>Korjataksesi ongelman, etsi ja asenna alla listatut fontit ja käynnistä Purple Pen sen jälkeen uudelleen.<br>) | the typo "ei tavalla" (no way) for "eri tavalla" (differently); nothing else changes |  |
| `PrintDescriptions_copiesCombo_Items` | one copy of each course | yksi kopio jokaisesta radasta<br>(corrects: yksi kopio jokaista rataa) | "jokaista rataa" needs "kohden"; "of each course" is "jokaisesta radasta" |  |
| `PrintDescriptions_copiesCombo_Items1` | one page of each course | yksi sivu jokaisesta radasta<br>(corrects: yksi sivu jokaista rataa) | "jokaista rataa" needs "kohden"; "of each course" is "jokaisesta radasta" |  |
| `PrintDescriptions_copiesCombo_Items2` | multiple pages of each course | useita sivuja jokaisesta radasta<br>(corrects: useita sivuja jokaista rataa) | "jokaista rataa" needs "kohden"; "of each course" is "jokaisesta radasta" |  |
| `IncrementButton` | Increment | Kasvata | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Vähennä | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Laskuri: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Kyllä | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Ei | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Vaihda kieli... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Sulje | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Vaihda kieli | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Valitse kieli: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
