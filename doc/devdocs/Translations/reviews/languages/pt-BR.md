# Review of the translations into pt-BR

Made by `review.py` from the filled key groups.

#### pt-BR

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Não incluir nos relatórios | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `AddCourse_courseKindCombo_Items1` | Score Course | Percurso de pontuação<br>(corrects: Percurso de pontuação (fazenda)) | "(fazenda)" is a farm and does not belong to the kind of course (the term Score Course) |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (inalterado) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_checkBoxPlural_Text` | Show plural forms | Mostrar formas no plural<br>(corrects: Mostrar formulário de plural) | "formulário" is a form to fill in; the grammatical forms are "formas" (the term Show plural forms) |  |
| `EnterSymbolText_checkBoxGender_Text` | Show gender forms | Mostrar formas de gênero<br>(corrects: Mostrar formulário de gênero) | "formulário" is a form to fill in; the grammatical forms are "formas" (the term Show gender forms) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Mostrar formas de caso<br>(corrects: Mostrar formulário de casos) | "formulário" is a form to fill in; the grammatical forms are "formas" (the term Show case forms) |  |
| `MainFrame_radioButtonTopology_Text` | Ordering | Ordenação<br>(corrects: Ordenando) | "Ordenando" is a gerund; the button names the ordering, as pt (the term Ordering) |  |
| `MainFrame_openFileDialog_Filter` | Purple Pen files\|*.ppen\|All files\|*.* | Arquivos Purple Pen\|*.ppen\|Todos os arquivos\|*.*<br>(corrects: Arquivos Purple Pen \|*.ppen\|All files\|*.*) | "All files" was English, and a space ended the first name (the terms Files and All) |  |
| `MainFrame_saveFileDialog_Filter` | Purple Pen file\|*.ppen | Arquivo Purple Pen\|*.ppen<br>(corrects: Arquivo Purple Pen \|*.ppen) | a space ended the name; nothing else changes |  |
| `MainFrame_createOcadFilesMenu_Text` | Create OCAD _Files... | Criar Arquivos _OCAD...<br>(corrects: Criar _Ficheiros OCAD...) | "Ficheiros" is European Portuguese; Brazil says "Arquivos" (the term Create OCAD Files); the access key "o" is a letter that no other item of the File menu uses |  |
| `MainFrame_fileMenu_Text` | _File | Ar_quivo<br>(corrects: _Ficheiro/Arquivo) | "Ficheiro/Arquivo" held two words, and "Ficheiro" is European Portuguese (the term File); the access key "q" is a letter that no other menu of the menu bar uses ("a" is Adicionar and Ajuda) |  |
| `MainFrame_addMapFlipMenuItem_Text` | Map _Flip at Control Point | _Virada de Mapa no Ponto de Controle | item of the submenu Map Flip/Exchange of the Add menu: the competitors turn over their double-sided map at a control; the term Add Map Flip, and the form of the language's Map Exchange at Control Point; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_addStartMenu_Text` | _Start | _Partida<br>(corrects: _Iniciar) | the item adds the start triangle; "Iniciar" is "to begin" (the term Start (of a course)) |  |
| `MainFrame_addFinishMenu_Text` | _Finish | C_hegada<br>(corrects: _Finalizar) | the item adds the finish circle of the course; "Finalizar" is "to complete" (the term Finish (of a course)); the access key "h" is a letter that no other item of the Add menu uses ("c" is Controle) |  |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Construção Temporária | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction, in the capitals of the neighbouring items of the language's Add menu ("Área Perigosa", "Área Branca") |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Construção Temporária | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction, in the capitals of the neighbouring items of the language's Add menu ("Área Perigosa", "Área Branca") |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | Mo_ver Todos os Controles... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses, in the capitals of the neighbouring items ("Remover Controles não Utilizados...") |  |
| `MainFrame_addForbiddenMenu_Text` | Forbidden Route Marking | Marcação de Rota Proibida<br>(corrects: Balizamento de Área Proibida) | "Área Proibida" is a forbidden area; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_forbiddenRouteMarkingToolStripMenuItem_Text` | Forbidden Route Marking | Marcação de Rota Proibida<br>(corrects: Balizamento de Área Proibida) | "Área Proibida" is a forbidden area; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | Padrão de Descrição 2004<br>(corrects: Padrão de Descrição 2014) | the year was 2014, a typo (the term 2004 Description Standard) |  |
| `IncrementButton` | Increment | Aumentar | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Diminuir | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Contador: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Sim | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Não | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Mudar idioma... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Fechar | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Mudar idioma | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Selecione um idioma: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
