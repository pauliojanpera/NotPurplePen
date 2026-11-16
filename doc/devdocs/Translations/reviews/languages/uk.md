# Review of the translations into uk

Made by `review.py` from the filled key groups.

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Не включати у звіти | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_scoreColumnLabel_Text` | Show points in: | Відображати очки у:<br>(corrects: Відображати точки у:) | "точки" are geometric points; the score points are "очки" (the term Show points in) |  |
| `AddCourse_secondaryTitleGroup_Text` | Class list / Secondary title | Список груп / Додаткова назва<br>(corrects: Клас / Додаткова назва) | Ukrainian orienteering calls a class "група"; "клас" is a school class, and the list was missing (the term Class List) |  |
| `ChangeText_classListMenuItem_Text` | Class List | Список груп<br>(corrects: Список класів) | Ukrainian orienteering calls a class "група"; "клас" is a school class (the term Class List) |  |
| `ChangeText_printScaleMenuItem_Text` | Print Scale | Масштаб для друку<br>(corrects: Маштаб для друку) | the typo "Маштаб" (the term Print Scale) |  |
| `ChangeText_fileNameMenuItem_Text` | File Name | Назва файлу<br>(corrects: Назва файла) | the genitive of "файл" is "файлу" (the term File Name) |  |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | Назва файлу карти<br>(corrects: Назва файла карти) | the genitive of "файл" is "файлу" (the term Map File Name) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Відмінок | header of the column of the grid that shows the grammatical case of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Case |  |
| `EnterSymbolText_checkBoxPlural_Text` | Show plural forms | Форми множини | check box above the grid that adds the rows of the plural forms; the three check boxes stand side by side in about 360 px, as the English (319 px), so the verb "show" is left out (terms.csv now gives this form) |  |
| `EnterSymbolText_checkBoxGender_Text` | Show gender forms | Форми роду | check box above the grid that adds the rows of the gender forms; the three check boxes stand side by side in about 360 px, as the English (319 px), so the verb "show" is left out (terms.csv now gives this form) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Відмінок змінюваного символу: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Відмінкові форми | check box above the grid that adds the rows of the case forms; the three check boxes stand side by side in about 360 px, as the English (319 px), so the verb "show" is left out (terms.csv now gives this form) |  |
| `EnterSymbolText_Text` | Customized Symbol Text | Змінений текст символу | title of the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Customized Symbol Text |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (без змін) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | Число<br>(corrects: Номер) | the column shows singular or plural, the grammatical number; "номер" is a number in a sequence (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_genderColumn_HeaderText` | Gender | Рід<br>(corrects: Стать) | the grammatical gender of the description text; "стать" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_labelGenderChooser_Text` | Gender: | Рід:<br>(corrects: Стать:) | the grammatical gender of the description text; "стать" is the sex of a person (the term Gender) |  |
| `EnterSymbolText_labelDescription_Text` | Enter the customized text for the symbol in the "Text" column. | Введіть змінений текст для умовного знаку у колонку "Текст".<br>(corrects: Введіть виправлений текст для умовного знаку у колонку "Текст".) | "виправлений" is corrected; the text is customized, "змінений" (the term Customized text); nothing else changes |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Файл карти "{0}" використовує шрифти, що відсутні на комп'ютері. Текст, написаний такими шрифтами, буде відображено та роздруковано із використанням шрифта за замовчуванням, що може призвести до порушення дизайну. <br><br>Щоб виправити цю проблему, знайдіть та встановіть шрифти перелічені нижче за допомогою контрольної панелі Windows, тоді запустіть Purple Pen знову.<br><br>(corrects: Файл карти "{0}" використовує шрифти, що відсутні на комп'ютері. Текст, написаний такими шрифтами, буде відображено та роздруковано із використанням шрифта за замовченням, що може призвести до порушення до порушення дизайну. <br><br>Щоб виправити цю проблему, знайдіть та встановіть шрифти перелічені нижче за допомогою контрольної панелі Windows, тоді запустіть Purple Pen знову.<br>) | "до порушення" was written twice; "за замовчуванням" is the usual form of "by default"; nothing else changes |  |
| `IncrementButton` | Increment | Збільшити | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Зменшити | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Лічильник: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Так | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Ні | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Змінити мову... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Закрити | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Змінити мову | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Виберіть мову: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
