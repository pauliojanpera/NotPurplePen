# Review of the translations into zh-TW

Made by `review.py` from the filled key groups.

#### zh-TW

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | 不包含在報告中 | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (不變) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | 數<br>(corrects: 編碼) | the column shows singular or plural, the grammatical number; 編碼 is a code; the grammatical number is 數, as ja 数 (the term Number (grammatical: singular or plural)) |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | 格<br>(corrects: 案例) | the grammatical case of the description text; 案例 is a case study; the grammatical case is 格, as ja (the term Case) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | 顯示格形式<br>(corrects: 顯示情況表格) | 情況表格 is a table of situations; the grammatical case is 格 (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | 被修飾符號的格：<br>(corrects: 修改符號的情況：) | the label names the grammatical case of the symbol that this symbol modifies; 修改符號的情況 is the situation of a changed symbol; the grammatical case is 格 (the term Case) |  |
| `MainFrame_radioButtonTopology_Text` | Ordering | 排序<br>(corrects: 排序中) | 排序中 means "sorting in progress" (the term Ordering) |  |
| `MainFrame_openFileDialog_Filter` | Purple Pen files\|*.ppen\|All files\|*.* | Purple Pen 檔案\|*.ppen\|所有檔案\|*.*<br>(corrects: Purple Pen 檔案\|*.ppen\|All files\|*.*) | "All files" was English (the terms Files and All) |  |
| `MainFrame_createOcadFilesMenu_Text` | Create OCAD _Files... | 建立OCAD檔案(_F)...<br>(corrects: 建立OCAD _檔案(_F)...) | the item had two access-key marks, "_檔" and "(_F)"; the second stays, as the language's other items |  |
| `MainFrame_addMapFlipMenuItem_Text` | Map _Flip at Control Point | 在檢查點翻轉地圖(_F) | item of the submenu Map Flip/Exchange of the Add menu: the competitors turn over their double-sided map at a control; the term Add Map Flip, and the form of the language's Map Exchange at Control Point; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | 臨時建築 | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | 臨時建築 | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | 移動所有檢查點... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; without an access key, as the other items of the language's Event menu |  |
| `MainFrame_addLineMenu_Text` | Line | 線段<br>(corrects: 列) | 列 is a row of text; the item draws a line (the term Line (drawn)), as the language's "新增線段" |  |
| `MainFrame_lineToolStripMenuItem_Text` | Line | 線段<br>(corrects: 列) | 列 is a row of text; the item draws a line (the term Line (drawn)), as the language's "新增線段" |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | 列的大小:<br>(corrects: 線條尺寸:) | 線條 is a drawn line; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |
| `PrintDescriptions_marginChange_Text` | Change Margins... | 變更邊距...<br>(corrects: 變更邊距) | the button opens the Page Setup dialog, and the ellipsis that says so was missing |  |
| `IncrementButton` | Increment | 增加 | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | 減少 | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | 計數器：{0} | sample key of the localization (LocalizeExtension.cs), shown in no view; the full-width colon of most of the language's labels |  |
| `YesButton` | Yes | 是 | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | 否 | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | 切換語言... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | 關閉 | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | 切換語言 | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | 選擇語言： | label above the list of languages of the dialog Switch Language; the term Select a language, with the full-width colon of most of the language's labels |  |
