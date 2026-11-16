# Review of the translations into ja

Made by `review.py` from the filled key groups.

#### ja

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_labelKindCombo_Items5` | Score (30) | 得点 (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | レポートに含めない | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `ChangeText_mapFileNameMenuItem_Text` | Map File Name | 地図ファイル名 | item of the menu of the button Insert Special Text in the Text Properties dialog: inserts the macro of the name of the map file; the term Map File Name |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (変更なし) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_numberColumn_HeaderText` | Number | 数<br>(corrects: 番号) | the column shows singular or plural, the grammatical number; 番号 is a number in a sequence (the term Number (grammatical: singular or plural)) |  |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | 画像ファイルを作成(_G)... | item of the File menu of the main window; the term Create Image Files; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createDescriptionPdfMenu_Text` | _Descriptions... | ディスクリプション(_D)... | item of the submenu Create PDFs of the File menu: makes PDF files of the control descriptions; the term Descriptions | UIText.MainFrame_radioButtonDescriptions_Text UIText.MainFrame_addDescriptionsMenu_Text |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | コントロールカード(_N)... | item of the submenu Create PDFs of the File menu: makes PDF files of the punch cards; the term Punch Cards |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | コース(_C)... | item of the submenu Create PDFs of the File menu: makes PDF files of the course maps; the term Courses | UIText.AddTextLine_coursesLabel_Text UIText.CreateGpx_coursesGroupBox_Text |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | PDFを作成(_D) | item of the File menu of the main window, a submenu of the PDF files of the descriptions, punch cards and courses; the term Create PDFs; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | RouteGadgetファイルを作成... | item of the submenu Create Route Review Files: makes the files of RouteGadget, a program that shows the routes that the competitors ran; the name RouteGadget stays; the term Create RouteGadget Files | UIText.CreateRouteGadget_Text $this.Text |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | ルート分析用ファイルを作成 | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | KMLファイルを作成(_K)... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_otherCoursesMenu_Text` | Other Courses... | その他のコース... | item of the View menu of the main window, which opens the dialog that chooses other courses to show beside the current one; the term Other Courses; no access key, as the English |  |
| `MainFrame_clearOtherCoursesMenu_Text` | Clear Other Courses | 他のコースをクリア | item of the View menu of the main window, which stops showing the other courses; the term Clear Other Courses; no access key, as the English |  |
| `MainFrame_addMapFlipMenuItem_Text` | Map _Flip at Control Point | コントロールでの地図の裏返し(_L) | item of the submenu Map Flip/Exchange of the Add menu: the competitors turn over their double-sided map at a control; the term Add Map Flip; the access key "L" (fLip), as "F" is the taped route of the submenu |  |
| `MainFrame_addMapIssueMenu_Text` | Timed Start | 計時スタート | item of the Add menu of the main window, which adds the timed start (the map issue point); the term Timed Start; no access key, as the English |  |
| `MainFrame_mapIssuePointToolStripMenuItem_Text` | Timed Start | 計時スタート | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu; the term Timed Start |  |
| `PrintDescriptions_orientationLabel_Text` | Orientation: | 印刷の向き:<br>(corrects: 印刷の向き) | the colon of a label was missing, as the language's other labels of the dialog ("用紙サイズ:", "余白:") |  |
| `IncrementButton` | Increment | 増やす | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | 減らす | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | カウンター: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | はい | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | いいえ | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | 言語を切り替え... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | 閉じる | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | 言語を切り替え | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | 言語を選択: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
