# Review of the translations into tr

Made by `review.py` from the filled key groups.

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | BU YAZILIM, TELİF HAKKI SAHİPLERİ VE KATKIDA BULUNANLAR TARAFINDAN "OLDUĞU GİBİ" SAĞLANMAKTADIR VE TİCARİ ELVERİŞLİLİK VE BELİRLİ BİR AMACA UYGUNLUK İÇİN ZIMNİ GARANTİLER DAHİL ANCAK BUNLARLA SINIRLI OLMAMAK ÜZERE, HER TÜRLÜ AÇIK VEYA ZIMNİ GARANTİ REDDEDİLMEKTEDİR.<br>(corrects: BU YAZILIM, TESCİL HAKKI SAHİPLERİ  VE DİĞER KATKI VERENLER TARAFINDAN "OLDUĞU GİBİ" SAĞLANMAKTA VE HER TÜRLÜ DİREKT/DOLAYLI (HIZLI VE AMACA ÖZEL) GARANTİYİ İÇERMEKTEDİR. <br><br>ANCAK BUNLARLA SINIRLI OLMAKSIZIN, YAZILIMIN BELİRLİ BİR AMACA UYGUN SATILABİLİRLİĞİ VE KONDİSYONUNA(YAZILIMIN DURUMUNA) BAĞLI İLGİLİ ÖZEL GARANTİLER REDDEDİLMEKTEDİR.) | the translation said that the software "İÇERMEKTEDİR" (includes) every warranty, the opposite of the English, and named the copyright "TESCİL HAKKI" (a registration right); now every warranty is disclaimed |  |
| `IncrementButton` | Increment | Artır | sample key of the localization (LocalizeExtension.cs), shown in no view; the button raises a counter; the term Increment |  |
| `DecrementButton` | Decrement | Azalt | sample key of the localization (LocalizeExtension.cs), shown in no view; the button lowers a counter; the term Decrement |  |
| `CounterLabel` | Counter: {0} | Sayaç: {0} | sample key of the localization (LocalizeExtension.cs), shown in no view; a label that shows the value of a counter, as fr "Compteur : {0}" |  |
| `YesButton` | Yes | Evet | button of the message box (MessageBoxDialog), beside No and Cancel; the term Yes | UIText.CreateImageFiles_comboBoxWorldFile_Items1 UIText.ReportText_EventAudit_Yes |
| `NoButton` | No | Hayır | button of the message box (MessageBoxDialog), beside Yes and Cancel; the term No | UIText.CreateImageFiles_comboBoxWorldFile_Items UIText.ReportText_EventAudit_No |
| `SwitchLanguageButton` | Switch Language... | Dili Değiştir... | button that opens the dialog Switch Language, shown in no view yet; the term Switch Language, with the ellipsis of a button that opens a dialog |  |
| `CloseButton` | Close | Kapat | button that closes the print preview (PrintPreviewDialog); the term Close | UIText.ReportForm_okButton_Text UIText.TeamVariationsForm_buttonClose_Text |
| `SwitchLanguageDialog_Title` | Switch Language | Dili Değiştir | title of the dialog that the menu Program Language opens; the term Switch Language, as the button SwitchLanguageButton | UIText.SwitchLanguageButton |
| `SwitchLanguageDialog_Prompt` | Select a language: | Bir dil seçin: | label above the list of languages of the dialog Switch Language; the term Select a language, with the colon of a label |  |
