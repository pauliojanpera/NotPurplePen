# Review of the translations into ru

Made by `review.py` from the filled key groups.

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_openLastRadioButton_Text` | Open last viewed event | Открыть последнее соревнование | third choice of the start window, one line of about 275 px; "последнее просмотренное соревнование" would not fit, and uk also says the last event; the terms Open and Event | uk |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Открыть образец соревнования | fourth choice of the start window; the terms Open, Sample and Event | uk |
| `InitialScreen_createNewRadioButton_Text` | Create a new event... | Создать новое соревнование... | first choice of the start window; the term Create Event, with the ellipsis of a choice that opens the wizard |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Открыть существующее соревнование... | second choice of the start window; the terms Open and Event; about 267 px of the 275 that the line gives | uk |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen — бесплатная программа, её можно копировать и распространять. Разработка Purple Pen поддерживается только вашими пожертвованиями. | the note above the donation link of the start window; "пожертвование" as the term Make a donation | uk |
| `InitialScreen_donationLink_Text` | Make a donation | Сделать пожертвование | the link under the note of the start window, which opens the donation page; the term Make a donation |  |
| `InitialScreen_Text` | Welcome to Purple Pen | Добро пожаловать в Purple Pen | title of the start window |  |
| `LicenseForm_bsdLicenseLinkLabel_Text` | Purple Pen is provided under a "BSD" style license. | Purple Pen распространяется по лицензии типа «BSD». | the line above the licence text of the License dialog; the quotation marks of Russian | uk bg |
| `LicenseForm_Text` | Purple Pen License | Лицензия Purple Pen | title of the License dialog, which the About dialog's button View Full License opens | uk |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Не предупреждать об отсутствии шрифтов в этом соревновании | check box beside the OK button of the Missing Fonts dialog; "не предупреждать" without "больше" keeps it shorter (about 404 px; the English 302) | uk |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Файл карты «{0}» использует шрифты, которые не установлены на этом компьютере. Текст, набранный этими шрифтами, будет отображаться и печататься шрифтом по умолчанию, поэтому он может выглядеть не так, как задумал автор карты. <br><br>Чтобы устранить проблему, найдите и установите перечисленные ниже шрифты через панель управления Windows, а затем перезапустите Purple Pen.<br> | the text above the list of the missing fonts, a format string with the map file's name as {0}, in the quotation marks of Russian; the terms Map File and Font | uk |
| `MissingFonts_Text` | Missing Fonts | Отсутствующие шрифты | title of the Missing Fonts dialog; the term Missing Fonts |  |
| `IncrementButton` | Increment | Увеличить | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Уменьшить | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Счетчик: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Да | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes |  |
| `NoButton` | No | Нет | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No |  |
| `SwitchLanguageButton` | Switch Language... | Сменить язык... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CancelButton` | Cancel | Отмена | button of the message box and of many dialogs; the term Cancel, the usual Cancel button of Russian dialogs |  |
| `CloseButton` | Close | Закрыть | button that closes the print preview (PrintPreviewDialog); the term Close |  |
| `SwitchLanguageDialog_Title` | Switch Language | Сменить язык | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Выберите язык: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
