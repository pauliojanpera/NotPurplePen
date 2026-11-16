# Review of the translations into uk

Made by `review.py` from the filled key groups.

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
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
