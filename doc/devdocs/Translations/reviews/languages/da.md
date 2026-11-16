# Review of the translations into da

Made by `review.py` from the filled key groups.

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_openLastRadioButton_Text` | Open last viewed event | Åbn sidst sete løb<br>(corrects: Åben sidst sete hændelse:) | "hændelse" is an incident, an orienteering event is a "løb" (the term Event); "Åben" is the adjective, the verb is "Åbn" (the term Open); the colon of a label does not belong to a choice |  |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Åbn et eksempelløb<br>(corrects: Åben en eksempel hændelse) | "hændelse" is an incident, an orienteering event is a "løb" (the term Event); "Åben" is the adjective, the verb is "Åbn"; the compound is one word |  |
| `InitialScreen_createNewRadioButton_Text` | Create a new event... | Opret et nyt løb...<br>(corrects: Opret en ny hændelse...) | "hændelse" is an incident, an orienteering event is a "løb" (the terms Event and New Event) |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Åbn et eksisterende løb...<br>(corrects: Åben en eksisterende hændelse...) | "hændelse" is an incident, an orienteering event is a "løb" (the term Event); "Åben" is the adjective, the verb is "Åbn" |  |
| `IncrementButton` | Increment | Forøg | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Formindsk | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Tæller: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Ja | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Nej | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Skift sprog... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Luk | button that closes the print preview (PrintPreviewDialog); the term Close |  |
| `SwitchLanguageDialog_Title` | Switch Language | Skift sprog | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Vælg et sprog: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
