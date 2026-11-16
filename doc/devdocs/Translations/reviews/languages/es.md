# Review of the translations into es

Made by `review.py` from the filled key groups.

#### es

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | ESTE SOFTWARE SE PROPORCIONA POR LOS PROPIETARIOS DEL COPYRIGHT Y COLABORADORES «TAL CUAL» Y SE DESESTIMA CUALQUIER GARANTÍA EXPRESA O IMPLÍCITA, INCLUYENDO, PERO NO LIMITADA A, LAS GARANTÍAS IMPLÍCITAS DE COMERCIALIZACIÓN Y APROVECHAMIENTO PARA UN PROPÓSITO PARTICULAR.<br>(corrects: ESTE SOFTWARE SE PROPORCIONA POR LOS PROPIETARIOS DEL COPYRIGHT Y COLABORADORES «TAL CUAL» Y SE DESESTIMA CUALQUIER GARANTÍA EXPRESA O IMPLÍCITA, INCLUYENDO, PERO NO LIMITADA A, LAS GARANTÍAS IMPLICITAS DE COMERCIALIZACIÓN Y APROVECHAIMIENTO PARA UN PROPÓSITO PARTICULAR.) | the typos "APROVECHAIMIENTO" and "IMPLICITAS" (the text writes "IMPLÍCITA" before); nothing else changes |  |
| `AddCourse_labelKindCombo_Items5` | Score (30) | Puntuación (30) | last item of the combo box Control circle labels of a score course: the points alone; the term Score (30) |  |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | No incluir en los informes | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `EnterSymbolText_caseColumn_HeaderText` | Case | Caso | header of the column of the grid that shows the grammatical case of each row, in the Customized Symbol Text dialog, which edits the text of a description symbol in each grammatical form of the description language (number, gender, case); the term Case |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Caso del símbolo modificado: | label of the combo box that chooses the grammatical case into which the symbol puts the symbol that it modifies (SymbolDB: "the case of what this symbol modifies"); the term Case |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Mostrar formas de caso | check box above the grid that adds the rows of the case forms; the term Show case forms |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (sin cambios) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | Crear archivos de RouteGadget... | item of the submenu Create Route Review Files: makes the files of RouteGadget, a program that shows the routes that the competitors ran; the name RouteGadget stays; the term Create RouteGadget Files | UIText.CreateRouteGadget_Text $this.Text |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Crear archivos para el análisis de rutas | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Crear archivo _KML... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_programLanguageMenu_Text` | Program _Language... | Idio_ma del programa...<br>(corrects: _Lenguaje del programa...) | "lenguaje" is a language as in a programming language; the language of a program is its "idioma" (the term Program Language); the access key "m" is a letter that no other item of the File menu uses |  |
| `MainFrame_addMapFlipMenuItem_Text` | Map _Flip at Control Point | _Volteo de mapa en punto de control | item of the submenu Map Flip/Exchange of the Add menu: the competitors turn over their double-sided map at a control; the term Add Map Flip, and the form of the language's Map Exchange at Control Point; its access key is a letter that no other item of its menu uses |  |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | No repetir más el aviso de que faltan fuentes para este evento.<br>(corrects: No repetir más el aviso de que faltan fuente para este evento.) | "faltan fuente": the verb is plural, the noun singular; nothing else changes |  |
| `IncrementButton` | Increment | Aumentar | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Disminuir | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Contador: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Sí | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | No | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Cambiar idioma... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Cerrar | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Cambiar idioma | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Seleccione un idioma: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
