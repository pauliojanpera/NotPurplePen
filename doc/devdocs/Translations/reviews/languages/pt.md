# Review of the translations into pt

Made by `review.py` from the filled key groups.

#### pt

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AddCourse_hideFromReportsCheckBox_Text` | Do not include in reports | Não incluir nos relatórios | check box under the length in the New Course dialog: the course is left out of the reports; the term Reports |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (inalterado) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `EnterSymbolText_checkBoxCases_Text` | Show case forms | Mostrar formas de caso<br>(corrects: Mostrar a caixa de formulário) | "caixa de formulário" is a form box (the term Show case forms) |  |
| `EnterSymbolText_labelCaseChooser_Text` | Case of modified symbol: | Caso do símbolo modificado:<br>(corrects: No caso de modificar o símbolo) | the label names the grammatical case of the symbol that this symbol modifies; "No caso de modificar o símbolo" means "in case the symbol is modified" (the term Case), as pt-BR "Caso de símbolo modificado:" |  |
| `InitialScreen_Text` | Welcome to Purple Pen | Bem-vindo ao Purple Pen<br>(corrects: Benvindo ao Purple Pen) | "Benvindo" is a misspelling of "Bem-vindo", as pt-BR |  |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Criar Ficheiro de _Intercâmbio de Dados (IOF XML)...<br>(corrects: Criar Data _Interchange File (IOF XML)...) | "Data Interchange File" was English, as pt-BR "Criar Arquivo de Intercâmbio de Dados" (the terms Create, File and IOF XML) |  |
| `MainFrame_exitMenu_Text` | E_xit | _Sair<br>(corrects: _Saír) | "Saír" has a wrong accent (the term Exit) |  |
| `MainFrame_addMapFlipMenuItem_Text` | Map _Flip at Control Point | _Viragem de mapa no Ponto de Controlo | item of the submenu Map Flip/Exchange of the Add menu: the competitors turn over their double-sided map at a control; the term Add Map Flip, and the form of the language's Map Exchange at Control Point; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_allControlsMenu_Text` | _All Controls | Ma_triz de Pontos de Controlo<br>(corrects: Matriz de Pontos de Controlo (_All)) | the item ended in "(All)", English, to carry the access key; the access key "t" is a letter that no other item of the View menu uses |  |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Construção Temporária | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction, in the capitals of the neighbouring items of the language's Add menu ("Área Perigosa", "Área Branca") |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Construção Temporária | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction, in the capitals of the neighbouring items of the language's Add menu ("Área Perigosa", "Área Branca") |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | Mo_ver Todos os Pontos de Controlo... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses, in the capitals of the neighbouring items ("Remover Controlos não Utilizados...") |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | Padrõe_s IOF<br>(corrects: IOF _Standards) | "Standards" is the English (the term IOF Standards), as the language's title of the IOF Standards page of the New Event wizard; the access key of pt-BR | UIText.NewEventStandards_labelTitle_Text |
| `MainFrame_beginFlaggingMenu_Text` | Beginning of Leg Flagged | Início da Pernada Balizada<br>(corrects: Início de Percurso Balizado) | a "percurso" is a course; the item tapes the beginning of a leg, "pernada", as the language's "Toda a Pernada Balizada" |  |
| `MainFrame_endFlaggingMenu_Text` | End of Leg Flagged | Fim da Pernada Balizada<br>(corrects: Fim de Percurso Balizado) | a "percurso" is a course; the item tapes the end of a leg, "pernada", as the language's "Toda a Pernada Balizada" |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | Carga de Controlos e Pernadas (_D)<br>(corrects: Carregar Controlos e Pernadas (_D)) | "Carregar" is the verb load, as of a file; the report gives the load of the controls and legs, "carga" (the term Load) |  |
| `MainFrame_supportWebSiteMenu_Text` | _Support/Bug Reporting | _Suporte/Relatório de erros<br>(corrects: _Suporte/Report de Bugs) | "Report de Bugs" is English (the term Support/Bug Reporting) |  |
| `PrintDescriptions_descriptionTypeLabel_Text` | Description type: | Tipo de sinalética:<br>(corrects: Tipo de descrição:) | the language's descriptions are "sinalética", as its title "Imprimir Sinalética" (the term Description type) |  |
| `PrintDescriptions_printerGroup_Text` | Output | Saída<br>(corrects: Saída:) | the colon does not belong in the header of a group, as pt-BR "Saída" (the term Output) |  |
| `IncrementButton` | Increment | Aumentar | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Diminuir | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Contador: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Sim | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Não | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Mudar de idioma... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Fechar | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Mudar de idioma | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Selecione um idioma: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
