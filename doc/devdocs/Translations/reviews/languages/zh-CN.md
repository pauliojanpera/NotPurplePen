# Review of the translations into zh-CN

Made by `review.py` from the filled key groups.

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | 本软件由版权持有人和贡献者“按原样”提供，不作任何明示或暗示的保证，包括但不限于对适销性和特定用途适用性的暗示保证。<br>(corrects: 该软件由版权持有人和贡献者提供，包括但不限于，商业性和其他用途默认放弃，提供任何明示或暗示的保证。) | the translation ended by saying that the software provides (提供) any express or implied warranty, the opposite of the English; now it gives none, in the usual Chinese wording of the licence | zh-TW |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | 不包含在报表中 | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_scoreColumnLabel_Text` | Show points in: | 分值显示在：<br>(corrects: 显示点：) | 点 is a point of geometry; the score is 分值 (the term Show points in); "shown in", as zh-TW 積分值顯示在 | zh-TW |
| `AddCourse_groupBox1_Text` | Appearance | 外观<br>(corrects: 界面) | 界面 is the user interface; the appearance is 外观, as zh-TW 外觀 (the term Appearance) |  |
| `AllControlsProperties_appearanceGroupBox_Text` | Appearance | 外观<br>(corrects: 界面) | 界面 is the user interface; the appearance is 外观, as zh-TW 外觀 (the term Appearance) |  |
| `IncrementButton` | Increment | 增加 | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | 减少 | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | 计数器：{0} | sample key of the localization (LocalizeExtension.cs), shown in no view; the full-width colon of the language's labels |  |
| `YesButton` | Yes | 是 | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | 否 | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | 切换语言… | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the single-character ellipsis that most of the language's texts use |  |
| `CloseButton` | Close | 关闭 | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | 切换语言 | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | 选择语言： | label above the list of languages of the dialog Switch Language; the term Select a language, with the full-width colon of the language's labels |  |
