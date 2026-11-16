# Review of the translations into bg

Made by `review.py` from the filled key groups.

#### bg

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (без промяна) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Число<br>(corrects: Номер) | the column shows singular or plural, the grammatical number; "номер" is a number in a sequence (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_genderColumn_HeaderText` | Gender | Род<br>(corrects: Пол) | the grammatical gender of the description text; "пол" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_labelGenderChooser_Text` | Gender: | Род:<br>(corrects: Пол:) | the grammatical gender of the description text; "пол" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Падеж<br>(corrects: Главни/малки букви) | the grammatical case of the description text; "Главни/малки букви" is upper and lower case of letters (the term Case) |  |
| `EnterSymbolText_checkBoxGender_Text` | Show gender forms | Показвай формите по род<br>(corrects: Показвай формите по пол) | "пол" is the sex of a person; the grammatical gender is "род" (the term Show gender forms) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Показвай падежните форми<br>(corrects: Показвай формите по регистър) | "регистър" is the case of letters; the grammatical case is "падеж" (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Падеж на променения символ:<br>(corrects: Регистър на променения символ:) | the label names the grammatical case of the symbol that this symbol modifies; "регистър" is the case of letters (the term Case) |  |
| `MainFrame_openFileDialog_Filter` | Purple Pen files\|*.ppen\|All files\|*.* | Файлове на Purple Pen\|*.ppen\|Всички файлове\|*.*<br>(corrects: Файлове на Purple Pen\|.ppen\|Всички файлове\|.*) | the patterns ".ppen" and ".*" lacked their "*", so the dialog listed no event file; the names stay |  |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | Създай P_DF файлове<br>(corrects: Create P_DFs) | "Create PDFs" was English (the term Create PDFs) |  |
| `MainFrame_undoMenu_Text` | _Undo | _Отмени<br>(corrects: _Undo) | "Undo" was English (the term Undo) |  |
| `MainFrame_beginFlaggingMenu_Text` | Beginning of Leg Flagged | Началото на отсечката е маркирано<br>(corrects: Началото на отсечката е маркиран) | "началото" is neuter, and the participle agrees with it |  |
| `IncrementButton` | Increment | Увеличаване | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Намаляване | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Брояч: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Да | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Не | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Смяна на езика... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Затвори | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Смяна на езика | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Изберете език: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
