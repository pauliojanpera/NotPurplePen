# Review of the translations into sl

Made by `review.py` from the filled key groups.

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_copyrightLabel_Text` | Copyright © by Peter Golde | Copyright © Peter Golde | one line of the About dialog, under the version; kept short, as the languages that keep "Copyright ©" | cs es fi hu it |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Točke (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_lengthLabel_Text` | Length: | Dolžina: | label of the length of a normal course, beside the box and "km"; the term Length |  |
| `AddCourse_kmSuffix_Text` | km | (not needed) | the unit beside the length box; the language writes the symbol km as the English does |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Ne vključi v poročila | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_descriptionAppearanceLabel_Text` | Description appearance: | Videz opisa:<br>(corrects: predogled opisa) | "predogled" is a preview; the term Description appearance, with the capital and colon of a label |  |
| `AllControlsProperties_descriptionAppearanceLabel_Text` | Description appearance: | Videz opisa:<br>(corrects: predogled opisa) | "predogled" is a preview; the term Description appearance, as in AddCourse |  |
| `AllControlsProperties_Text` | All Controls Properties | Lastnosti vseh KT<br>(corrects: Lastnosti Vse KT) | "Vse" is the nominative; the properties are those "of all controls" (the term All Controls Properties) |  |
| `AllControlsProperties_allControlsPropertiesLabel_Text` | These settings control how the map and descriptions for All Controls are printed. | Te nastavitve določajo, kako bosta natisnjena karta in opis za vse KT.<br>(corrects: Te nastavitve določajo kako bo natisnjena kart z vsemi KT.) | the typo "kart", and the descriptions were missing |  |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen je brezplačen program, ki se lahko kopira in posreduje. Razvoj programa Purple Pen podpirajo le vaše donacije. | the note above the donation link of the start window; its first sentence as the language's About dialog (AboutForm_freeLabel_Text) | AboutForm_freeLabel_Text InitialScreen_donationLink_Text |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Ne opozarjaj več na manjkajoče pisave za to prireditev | check box beside the OK button of the Missing Fonts dialog (resizable, 480 px wide); the setting is kept for the event; the term Event |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Datoteka s karto "{0}" uporablja pisave, ki niso nameščene v vašem računalniku. Besedilo v teh pisavah bo prikazano in natisnjeno s privzeto pisavo, zato je lahko videti drugače, kot je nameraval avtor karte. <br><br>Težavo odpravite tako, da poiščete spodaj navedene pisave in jih namestite z nadzorno ploščo sistema Windows, nato pa znova zaženete Purple Pen.<br> | the text above the list of the missing fonts, a format string with the map file's name as {0}; two paragraphs and the closing line break of the English; the terms Map File and Font | cs |
| `MissingFonts_Text` | Missing Fonts | Manjkajoče pisave | title of the Missing Fonts dialog; the term Missing Fonts |  |
| `PrintDescriptions_orientation_Text` | Landscape | Ležeče | the WinForms dialog's placeholder, which both dialogs replace at runtime (MiscText.Landscape); the term Landscape |  |
| `PrintDescriptions_printButton_Text` | Print | Tiskaj | button that prints; the term Print |  |
| `PrintDescriptions_printerChange_Text` | Change Printer... | Spremeni tiskalnik... | button that chooses another printer; the term Change Printer |  |
| `PrintDescriptions_printerName_Text` | Printer Name | Ime tiskalnika | the WinForms dialog's placeholder, which both dialogs replace at runtime with the printer's name; the term Printer Name |  |
| `PrintDescriptions_printerLabel_Text` | Printer: | Tiskalnik: | label of the printer's name; the term Printer |  |
| `PrintDescriptions_copiesGroupBox_Text` | Copies | Kopije | header of the group that sets how many descriptions are printed; the term Copies |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | Spremeni robove... | button that opens the Page Setup dialog to change the margins; the term Change Margins |  |
| `PrintDescriptions_paperSize_Text` | Tabloid (11"x17") | (not needed) | the WinForms dialog's placeholder, which both dialogs replace at runtime; the language writes it as the English does, with the term Tabloid (11"x17") |  |
| `PrintDescriptions_paperSizeLabel_Text` | Paper Size: | Velikost papirja: | label of the paper size; the term Paper Size |  |
| `PrintDescriptions_margins_Text` | Top: 0.55", Bottom: 0.55", Left: 0.55", Right: 0.55" | Zgoraj: 0.55", Spodaj: 0.55", Levo: 0.55", Desno: 0.55" | the WinForms dialog's placeholder, which both dialogs replace at runtime (MiscText.Margins_LRTB); the terms Top, Bottom, Left and Right |  |
| `PrintDescriptions_marginsLabel_Text` | Margins: | Robovi: | label of the margins of the page; the term Margins |  |
| `PrintDescriptions_orientationLabel_Text` | Orientation: | Usmerjenost: | label of the orientation of the paper; the term Orientation |  |
| `PrintDescriptions_descriptionKindCombo_Items` | Course default | Privzeto za progo | first item of the combo box Description type: the kind that each course's properties set; the term Course default |  |
| `PrintDescriptions_descriptionKindCombo_Items3` | Symbols & Text | Simboli in besedilo | fourth item of the combo box Description type, after "Symbols" and "Text"; the term Symbols & Text | UIText.AddCourse_descKindCombo_Items2 |
| `PrintDescriptions_descriptionTypeLabel_Text` | Description type: | Vrsta opisa: | label of the combo box that chooses symbols, text or both; the term Description type |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Velikost vrstice: | label of the box size of the printed description in mm (1 to 50, default 6), the height of a row of the description sheet; the term Line size |  |
| `PrintDescriptions_descriptionsLabel_Text` | descriptions per course | opisov na progo | text after the number box: how many descriptions of each course the pages hold at least; the terms Descriptions and Course |  |
| `PrintDescriptions_copiesCombo_Items` | one copy of each course | ena kopija vsake proge | first item of the combo box after "Print:": one description of each course, on as few pages as possible; the term Course |  |
| `PrintDescriptions_copiesCombo_Items1` | one page of each course | ena stran vsake proge | second item of the combo box after "Print:": one page of each course, filled with as many descriptions as fit; the term Course |  |
| `PrintDescriptions_copiesCombo_Items2` | multiple pages of each course | več strani vsake proge | third item of the combo box after "Print:": pages of each course until the number beside it is reached; the term Course |  |
| `PrintDescriptions_whatToPrintLabel_Text` | Print: | Tiskaj: | label of the combo box that chooses how many descriptions of each course to print; the term Print |  |
| `PrintDescriptions_Text` | Print Descriptions | Tiskaj opise kontrolnih točk | title of the dialog that prints the control descriptions; the term Print Descriptions, in the small letter of the language | UIText.MainFrame_printDescriptionsMenu_Text printDescriptionsMenu.Text |
| `IncrementButton` | Increment | Povečaj | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Zmanjšaj | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Števec: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Da | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes |  |
| `NoButton` | No | Ne | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No |  |
| `SwitchLanguageButton` | Switch Language... | Zamenjaj jezik... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Zapri | button that closes the print preview (PrintPreviewDialog); the term Close |  |
| `SwitchLanguageDialog_Title` | Switch Language | Zamenjaj jezik | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Izberite jezik: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
