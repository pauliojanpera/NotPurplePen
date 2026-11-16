# Key group AboutForm

Nov 16, 2026. The About dialog: its title, its buttons License, Credits and OK, the copyright line, the line that says
Purple Pen is free software, and the disclaimer of its licence. claude-opus-5-5 at effort max made every line below,
and each entry says so in its comment.

- **Translated**: 5 lines, in 4 languages. The copyright line in nn-NO, pl, ro and sl, which keep it short as the
  other languages do, and the disclaimer in pl.
- **Not needed**: none.
- **Corrected**: 9 lines.
  - The disclaimer said the opposite of the English in three languages: it "SONO DICHIARATE" (the warranties are
    declared), tr "İÇERMEKTEDİR" (the software includes every warranty), and zh-CN 提供 (it provides any warranty).
    Each now disclaims the warranties, as the English does.
  - The disclaimer had typos in es ("APROVECHAIMIENTO", "IMPLICITAS") and ro ("GARAȚII", "FARĂ").
  - hu named the Credits button "Névjegy", which is the About of a program, and spelt licence "licensz".
  - nl had the typo "Bekijke" on the License button.
  - pl's free-software line mixed the genders of its participles.
- **Left empty**: none.

Doubts:

- The Hungarian License button keeps its form without a verb, "A teljes licenc". The term of terms.csv was "A teljes
  licenc megtekintése", but measured in the application's font, Source Sans 3, it would leave no room for the Credits
  button beside it. terms.csv now gives "A teljes licenc", and terms-decisions.tsv records why.
- The disclaimers of da, et, nb-NO and nn-NO are clumsy or shortened but keep the meaning, so they stay. nn-NO's is
  the Bokmål text of nb-NO, and sl's is the English.

#### es

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | ESTE SOFTWARE SE PROPORCIONA POR LOS PROPIETARIOS DEL COPYRIGHT Y COLABORADORES «TAL CUAL» Y SE DESESTIMA CUALQUIER GARANTÍA EXPRESA O IMPLÍCITA, INCLUYENDO, PERO NO LIMITADA A, LAS GARANTÍAS IMPLÍCITAS DE COMERCIALIZACIÓN Y APROVECHAMIENTO PARA UN PROPÓSITO PARTICULAR.<br>(corrects: ESTE SOFTWARE SE PROPORCIONA POR LOS PROPIETARIOS DEL COPYRIGHT Y COLABORADORES «TAL CUAL» Y SE DESESTIMA CUALQUIER GARANTÍA EXPRESA O IMPLÍCITA, INCLUYENDO, PERO NO LIMITADA A, LAS GARANTÍAS IMPLICITAS DE COMERCIALIZACIÓN Y APROVECHAIMIENTO PARA UN PROPÓSITO PARTICULAR.) | the typos "APROVECHAIMIENTO" and "IMPLICITAS" (the text writes "IMPLÍCITA" before); nothing else changes |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_creditsButton_Text` | Credits | Közreműködők<br>(corrects: Névjegy) | "Névjegy" is the About of a program; the button shows the credits, the term Credits | nb-NO da nn-NO uk ru |
| `AboutForm_licenseButton_Text` | View Full License | A teljes licenc<br>(corrects: A teljes licensz) | the Hungarian spelling is "licenc"; the term "A teljes licenc megtekintése" would leave no room for the Credits button beside it (about 167 px of text against 102 of the English), so the button keeps its form without the verb |  |

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | QUESTO SOFTWARE È FORNITO DAI DETENTORI DEL COPYRIGHT E DAI CONTRIBUTORI "COSÌ COM'È" E OGNI GARANZIA ESPRESSA O IMPLICITA, INCLUSE, MA NON SOLO, LE GARANZIE IMPLICITE DI COMMERCIABILITÀ E DI IDONEITÀ PER UNO SCOPO PARTICOLARE, È ESCLUSA.<br>(corrects: QUESTO SOFTWARE È FORNITO DAI DETENTORI DEL COPYRIGHT E DAI CONTRIBUTORI "COSÌ COM'È" E OGNI GARANZIA ESPRESSA O SOTTINTESA, INCLUDENDO, MA NON LIMITATA A, LE GARANZIE SOTTINTESE DI COMMERCIABILITÀ E DI BENESSERE PER UNO SCOPO PARTICOLARE SONO DICHIARATE.) | the translation said that the warranties "SONO DICHIARATE" (are declared), the opposite of disclaimed, and "BENESSERE" (well-being) for fitness; now they are excluded, as in the English | fr es |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_licenseButton_Text` | View Full License | Bekijk de volledige licentie<br>(corrects: Bekijke de volledige licentie) | the typo "Bekijke"; the term View Full License |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_copyrightLabel_Text` | Copyright © by Peter Golde | Opphavsrett © Peter Golde | one line of the About dialog, under the version; "opphavsrett" is Nynorsk as Bokmål, as nb-NO | nb-NO |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_copyrightLabel_Text` | Copyright © by Peter Golde | Copyright © Peter Golde | one line of the About dialog, under the version; kept short, as the languages that keep "Copyright ©" | cs es fi hu it |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | TO OPROGRAMOWANIE JEST DOSTARCZANE PRZEZ POSIADACZY PRAW AUTORSKICH I WSPÓŁTWÓRCÓW „TAKIM, JAKIE JEST”. WSZELKIE GWARANCJE WYRAŹNE LUB DOROZUMIANE, W TYM, ALE NIE WYŁĄCZNIE, DOROZUMIANE GWARANCJE PRZYDATNOŚCI HANDLOWEJ I PRZYDATNOŚCI DO OKREŚLONEGO CELU, SĄ WYŁĄCZONE. | the disclaimer of the BSD licence in the About dialog, in the wording of Polish translations of the licence, in capitals as the English | cs ru |
| `AboutForm_freeLabel_Text` | Purple Pen is free software and may be copied and shared. | Purple Pen jest darmową aplikacją i jako taka może być kopiowana i udostępniana.<br>(corrects: Purple Pen jest darmową aplikacją i jako taka może być kopiowany i udostępniany.) | "jako taka" is the application, so the participles are feminine; the translation had "kopiowany i udostępniany" |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_copyrightLabel_Text` | Copyright © by Peter Golde | Copyright © Peter Golde | one line of the About dialog, under the version; kept short, as the languages that keep "Copyright ©" | cs es fi hu it |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | ACEST PROGRAM ESTE OFERIT DE CĂTRE DEȚINĂTORUL DREPTURILOR DE AUTOR ȘI CONTRIBUTORI "AȘA CUM ESTE" ȘI SUNT EXCLUSE ORICE GARANȚII IMPLICITE SAU EXPLICITE, SAU GARANȚII IMPLICITE DE COMERCIALIZARE SAU PENTRU UN ANUMIT SCOP, DAR FĂRĂ A SE LIMITA LA ACESTEA.<br>(corrects: ACEST PROGRAM ESTE OFERIT DE CĂTRE DEȚINĂTORUL DREPTURILOR DE AUTOR ȘI CONTRIBUTORI "AȘA CUM ESTE" ȘI SUNT EXCLUSE ORICE GARAȚII IMPLICITE SAU EXPLICITE, SAU GARAȚII IMPLICITE DE COMERCIALIZARE SAU PENTRU UN ANUMIT SCOP, DAR FARĂ A SE LIMITA LA ACESTEA.) | the typos "GARAȚII" (twice) and "FARĂ"; nothing else changes |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_copyrightLabel_Text` | Copyright © by Peter Golde | Copyright © Peter Golde | one line of the About dialog, under the version; kept short, as the languages that keep "Copyright ©" | cs es fi hu it |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | BU YAZILIM, TELİF HAKKI SAHİPLERİ VE KATKIDA BULUNANLAR TARAFINDAN "OLDUĞU GİBİ" SAĞLANMAKTADIR VE TİCARİ ELVERİŞLİLİK VE BELİRLİ BİR AMACA UYGUNLUK İÇİN ZIMNİ GARANTİLER DAHİL ANCAK BUNLARLA SINIRLI OLMAMAK ÜZERE, HER TÜRLÜ AÇIK VEYA ZIMNİ GARANTİ REDDEDİLMEKTEDİR.<br>(corrects: BU YAZILIM, TESCİL HAKKI SAHİPLERİ  VE DİĞER KATKI VERENLER TARAFINDAN "OLDUĞU GİBİ" SAĞLANMAKTA VE HER TÜRLÜ DİREKT/DOLAYLI (HIZLI VE AMACA ÖZEL) GARANTİYİ İÇERMEKTEDİR. <br><br>ANCAK BUNLARLA SINIRLI OLMAKSIZIN, YAZILIMIN BELİRLİ BİR AMACA UYGUN SATILABİLİRLİĞİ VE KONDİSYONUNA(YAZILIMIN DURUMUNA) BAĞLI İLGİLİ ÖZEL GARANTİLER REDDEDİLMEKTEDİR.) | the translation said that the software "İÇERMEKTEDİR" (includes) every warranty, the opposite of the English, and named the copyright "TESCİL HAKKI" (a registration right); now every warranty is disclaimed |  |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AboutForm_disclaimerLabel_Text` | THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. | 本软件由版权持有人和贡献者“按原样”提供，不作任何明示或暗示的保证，包括但不限于对适销性和特定用途适用性的暗示保证。<br>(corrects: 该软件由版权持有人和贡献者提供，包括但不限于，商业性和其他用途默认放弃，提供任何明示或暗示的保证。) | the translation ended by saying that the software provides (提供) any express or implied warranty, the opposite of the English; now it gives none, in the usual Chinese wording of the licence | zh-TW |
