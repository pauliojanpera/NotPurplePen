# Review of the translations into tr

Made by `review.py` from the filled key groups.

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | BU YAZILIM, TELİF HAKKI SAHİPLERİ VE KATKIDA BULUNANLAR TARAFINDAN "OLDUĞU GİBİ" SAĞLANMAKTADIR VE TİCARİ ELVERİŞLİLİK VE BELİRLİ BİR AMACA UYGUNLUK İÇİN ZIMNİ GARANTİLER DAHİL ANCAK BUNLARLA SINIRLI OLMAMAK ÜZERE, HER TÜRLÜ AÇIK VEYA ZIMNİ GARANTİ REDDEDİLMEKTEDİR.<br>(corrects: BU YAZILIM, TESCİL HAKKI SAHİPLERİ  VE DİĞER KATKI VERENLER TARAFINDAN "OLDUĞU GİBİ" SAĞLANMAKTA VE HER TÜRLÜ DİREKT/DOLAYLI (HIZLI VE AMACA ÖZEL) GARANTİYİ İÇERMEKTEDİR. <br><br>ANCAK BUNLARLA SINIRLI OLMAKSIZIN, YAZILIMIN BELİRLİ BİR AMACA UYGUN SATILABİLİRLİĞİ VE KONDİSYONUNA(YAZILIMIN DURUMUNA) BAĞLI İLGİLİ ÖZEL GARANTİLER REDDEDİLMEKTEDİR.) | the translation said that the software "İÇERMEKTEDİR" (includes) every warranty, the opposite of the English, and named the copyright "TESCİL HAKKI" (a registration right); now every warranty is disclaimed |  |
| `AddCourse_descriptionAppearanceLabel_Text` | Description appearance: | Açıklama görünümü:<br>(corrects: Görüntü açıklaması:) | "Görüntü açıklaması" means the description of an image; the term Description appearance |  |
| `AddCourse_labelKindCombo_Items3` | Sequence and score (3(30)) | Sıra No ve Puan (3(30))<br>(corrects: Sıra No ve Nokta(3(30))) | "Nokta" is a geometric point; the score points are "puan" (the term Show points in) |  |
| `AddCourse_labelKindCombo_Items4` | Code and score (145(30)) | Kod ve Puan (145(30))<br>(corrects: Kod ve Nokta) | "Nokta" is a geometric point; the score points are "puan"; the example of the English, as the other items |  |
| `AddCourse_scoreColumnLabel_Text` | Show points in: | Puanları göster:<br>(corrects: Noktaları Göster:) | "Noktalar" are geometric points; the score points are "puan" (the term Show points in) |  |
| `AddCourse_courseKindCombo_Items1` | Score Course | Puanlı Parkur<br>(corrects: Parkur Puanı) | "Parkur Puanı" is the score of a course (the term Score Course) |  |
| `AllControlsProperties_descriptionAppearanceLabel_Text` | Description appearance: | Açıklama görünümü:<br>(corrects: Görüntü açıklaması:) | "Görüntü açıklaması" means the description of an image; the term Description appearance |  |
| `AllControlsProperties_Text` | All Controls Properties | Tüm Kontrol Noktalarının Özellikleri<br>(corrects: Tüm Kontrol Noktaların  Özellikleri) | the genitive of "kontrol noktaları" is "noktalarının"; a double space (terms.csv now gives it) |  |
| `ChangeText_variationMenuItem_Text` | Course Variation | Parkur Varyasyonu<br>(corrects: Parkur Varyansonu) | the typo "Varyansonu" (the term Course Variation) |  |
| `ChangeText_checkBoxAutoFontSize_Text` | Automatic | Otomatik<br>(corrects: Otomarik) | the typo "Otomarik" (the term Automatic) |  |
| `EnterSymbolText_defaultCaseText` | (unchanged) | (değişiklik yok) | first item of the combo box "Case of modified symbol": the symbol leaves the case of the symbol that it modifies as it is; the term (unchanged) |  |
| `MainFrame_addControlToolStripButton_Text` | Add Control | Hedef Ekle<br>(corrects: Hedef  Ekle) | a double space; nothing else changes |  |
| `MainFrame_saveGpxFileDialog_Title` | Create GPX File | GPX Dosyası Oluştur<br>(corrects: GPX dosysı oluştur) | the typo "dosysı" (the term Create GPX File) |  |
| `MainFrame_radioButtonTopology_Text` | Ordering | Sıralama<br>(corrects: Sipariş) | "Sipariş" is an order of goods; the button shows the order of the controls (the term Ordering) |  |
| `MainFrame_createOcadFilesMenu_Text` | Create OCAD _Files... | OCAD _Dosyası Oluştur...<br>(corrects: OCAD ve Dosyalar Oluştur ...) | "OCAD ve Dosyalar" is "OCAD and files" (the term Create OCAD Files), with the access key of the English |  |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | _Görüntü Dosyası Oluştur...<br>(corrects: Ima_ge Dosyası Oluştur...) | "Image" was English (the term Create Image Files), as the language's dialog "Görüntü Dosyası Oluştur"; the access key stays "g" |  |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | _Delme Kartları...<br>(corrects: Delme Kartları Yazdır...) | "Yazdır" (print) does not belong to the item of the submenu Create PDFs (the term Punch Cards); the access key "d" is a letter that no other item of the submenu uses |  |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | PD_F'ler oluştur<br>(corrects: P_DF ler oluştur) | the suffix of an abbreviation takes an apostrophe, "PDF'ler" (the term Create PDFs); the access key "f" is a letter that no other item of the File menu uses ("d" is also Program Dili) |  |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | _Veri Değişim Dosyası Oluştur (IOF XML)...<br>(corrects: Veri ve Değişim Dosyası Oluştur (IOF XML)...) | "Veri ve Değişim" is "data and interchange"; the access key "v" is a letter that no other item of the File menu uses |  |
| `MainFrame_printCoursesMenu_Text` | Print _Courses... | Pa_rkurları Yazdır...<br>(corrects: Yazdır _Parkurları...) | the verb comes last, as the language's "Açıklamaları Yazdır" (the term Print Courses); the access key "r" is a letter that no other item of the File menu uses |  |
| `MainFrame_exitMenu_Text` | E_xit | _Çıkış<br>(corrects: E_xit) | "Exit" was English (the term Exit); the access key "ç" is a letter that no other item of the File menu uses |  |
| `MissingFonts_Text` | Missing Fonts | Eksik Fontlar<br>(corrects: Eksik Fontları Yoksay) | "Eksik Fontları Yoksay" is Ignore Missing Fonts; the dialog only shows them (the term Missing Fonts) |  |
| `PrintDescriptions_orientation_Text` | Landscape | Yatay<br>(corrects: Düzenlemek) | "Düzenlemek" is to arrange; the page orientation is "yatay" (the term Landscape) |  |
| `PrintDescriptions_paperSize_Text` | Tabloid (11"x17") | Tabloid (11"x17")<br>(corrects: Tablet (11"x17")) | "Tablet" is a tablet; the paper size is Tabloid (the term Tabloid (11"x17")) |  |
| `PrintDescriptions_lineSizeLabel_Text` | Line size: | Satır boyutu:<br>(corrects: Çizgi Boyutu:) | "Çizgi" is a drawn line; the box sets the size of a row of the description sheet, in mm (the term Line size) |  |
| `IncrementButton` | Increment | Artır | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Azalt | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Sayaç: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Evet | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Hayır | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Dili Değiştir... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Kapat | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Dili Değiştir | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Bir dil seçin: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
