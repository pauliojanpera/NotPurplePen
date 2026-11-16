# Key group MissingFonts

Nov 16, 2026. The dialog that warns when the map uses fonts that the computer lacks: its title, the warning with the
map file's name, and the check box "Don't warn about missing fonts for this event again". claude-opus-5-5 at effort
max made every line below, and each entry says so in its comment.

- **Translated**: 10 lines, in 4 languages: nl, ru and sl lacked the dialog, pl the warning.
- **Not needed**: none.
- **Corrected**: 16 lines.
  - The event of the check box: da "hændelse" and de "Ereignis" are incidents, and cs wrote the English "event". Now
    "løb", "Wettkampf" and "závod", as the term Event.
  - de named the course setter, "Bahnleger", where the English names the map designer; now "Kartenzeichner".
  - da lacked the second paragraph of the warning, which tells how to fix the problem.
  - nn-NO's warning was Bokmål with typos; it is Nynorsk now. The Nynorsk plural of font is "fontar", so the title and
    the check box say "fontar", and terms.csv gives "Manglande fontar".
  - tr's title read "Eksik Fontları Yoksay", Ignore Missing Fonts; now "Eksik Fontlar".
  - Typos and grammar: nb-NO "datamskinen", fr "redémarrer" and "installez les", fi "ei tavalla", uk a doubled "до
    порушення", ro "vof", hu "betútípussal", es "faltan fuente".
- **Left empty**: none.

Doubts:

- The check box is one line beside the OK button, about 305 px in a dialog that the user can widen. The English takes
  302 px, and most languages more: bg 467, sv 426, pl 404. nl and ru are about 425 and 404 px; ru leaves out
  "больше" (any more) for that.

#### cs

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Znovu neupozorňovat na chybějící fonty pro tento závod<br>(corrects: Znovu NEupozorňovat na chybějící fonty pro tento event) | "event" is English; the language's event is "závod" (the term Event); "NEupozorňovat" in capitals |  |

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Advar ikke om manglende fonte for dette løb igen<br>(corrects: Advar ikke om manglende fonte for denne hændelse igen) | "hændelse" is an incident; an orienteering event is a "løb" (the term Event) |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Kortfil "{0}" bruger fonte der ikke er installeret på din computer. Tekst der bruger disse fonte vil blive tegnet og udskrevet med en standard font, hvilket kan få teksten til at fremtræde forskelligt fra kortdesignerens intentioner. <br><br>For at løse problemet skal du finde og installere de fonte, der er nævnt nedenfor, via Kontrolpanel i Windows og derefter starte Purple Pen igen.<br><br>(corrects: Kortfil "{0}" bruger fonte der ikke er installeret på din computer. Tekst der bruger disse fonte vil blive tegnet og udskrevet med en standard font, hvilket kan få teksten til at fremtræde forskelligt fra kortdesignerens intentioner.<br>) | the translation lacked the second paragraph, which tells how to fix the problem; the first stays as it was |  |

#### de

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Die Kartendatei "{0}" benutzt Schriftarten (Fonts), die auf diesem Computer nicht installiert sind. Texte, die diese Fonts verwenden, werden mit dem Standardfont gezeichnet und gedruckt, was den Text unter Umständen anders aussehen lässt, als der Kartenzeichner beabsichtigt hatte. <br><br>Um das Problem zu lösen, suchen und installieren Sie die unten gelisteten Fonts über die Windows Systemsteuerung. Starten Sie dann Purple Pen neu.<br><br>(corrects: Die Kartendatei "{0}" benutzt Schriftarten (Fonts), die auf diesem Computer nicht installiert sind. Texte, die diese Fonts verwenden, werden mit dem Standardfont gezeichnet und gedruckt, was den Text unter Umständen anders aussehen lässt, als der Bahnleger beabsichtigt hatte. <br><br>Um das Problem zu lösen, suchen und installieren Sie die unten gelisteten Fonts über die Windows Systemsteuerung. Starten Sie dann Purple Pen neu.<br>) | the fonts are those of the map, which its map designer chose: "Kartenzeichner", not "Bahnleger" (the course setter); nothing else changes |  |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Für diesen Wettkampf nie mehr wegen fehlender Fonts warnen<br>(corrects: Für dieses Ereignis nie mehr wegen fehlender Fonts warnen) | "Ereignis" is an occurrence; the language's event is "Wettkampf" (the term Event) |  |

#### es

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | No repetir más el aviso de que faltan fuentes para este evento.<br>(corrects: No repetir más el aviso de que faltan fuente para este evento.) | "faltan fuente": the verb is plural, the noun singular; nothing else changes |  |

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Karttatiedostossa "{0}" on käytössä fontteja, joita ei ole asennettu tietokoneellesi. Tekstit, joissa näitä fontteja on käytössä, piirretään ja tulostetaan oletusfontilla, mikä saattaa aiheuttaa, että teksti näkyy kartalla eri tavalla kuin kartan tekijä on tarkoittanut. <br><br>Korjataksesi ongelman, etsi ja asenna alla listatut fontit ja käynnistä Purple Pen sen jälkeen uudelleen.<br><br>(corrects: Karttatiedostossa "{0}" on käytössä fontteja, joita ei ole asennettu tietokoneellesi. Tekstit, joissa näitä fontteja on käytössä, piirretään ja tulostetaan oletusfontilla, mikä saattaa aiheuttaa, että teksti näkyy kartalla ei tavalla kuin kartan tekijä on tarkoittanut. <br><br>Korjataksesi ongelman, etsi ja asenna alla listatut fontit ja käynnistä Purple Pen sen jälkeen uudelleen.<br>) | the typo "ei tavalla" (no way) for "eri tavalla" (differently); nothing else changes |  |

#### fr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | La carte "{0}" utilise des polices qui ne sont pas installées sur votre ordinateur. Le texte qui utilise ces polices sera affiché et imprimé avec une police par défaut, ce qui pourra le faire apparaitre différemment de ce qu'avait prévu le créateur de la carte. <br><br>Pour résoudre ce problème, recherchez les polices listées ci-dessous et installez-les avec le tableau de bord Windows, puis redémarrez Purple Pen.<br><br>(corrects: La carte "{0}" utilise des polices qui ne sont pas installées sur votre ordinateur. Le texte qui utilise ces polices sera affiché et imprimé avec une police par défaut, ce qui pourra le faire apparaitre différemment de ce qu'avait prévu le créateur de la carte. <br>Pour résoudre ce problème, recherchez les polices listées ci-dessous et installez les avec le tableau de bord Windows, puis redémarrer Purple Pen.<br>) | "redémarrer" is the infinitive beside the imperatives "recherchez" and "installez"; "installez-les" takes its hyphen; the blank line between the paragraphs, as the English |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | A térkép fájl "{0}" olyan betűtípusokat használ, amik nincsenek telepítve a számítógépén. A szövegek amik ezeket használják az alapértelmezett betűtípussal jelennek meg és kerülnek nyomtatásra, ami jelentősen eltérhet attól amit a térkép tervezője megadott. <br><br>A probléma megoldására szerezze be és telepítse a Windows vezérlőpultban az alábbi betűtípusokat, majd indítsa újra a Purple Pen programot.<br><br>(corrects: A térkép fájl "{0}" olyan betűtípusokat használ, amik nincsenek telepítve a számítógépén. A szövegek amik ezeket használják az alapértelmezett betútípussal jelennek meg és kerülnek nyomtatásre, ami jelentősen eltérhet attól amit a térkép tervezője megadott. <br><br>A probléma megoldására szerezze be és telepítse a Windows vezérlőpultban az alábbi betűtípusokat, majd indítsa újra a Purple Pen programot.<br>) | the typos "betútípussal" and "nyomtatásre"; nothing else changes |  |

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Kartfilen "{0}" benytter en fontfil som ikke er installert på datamaskinen. Tekst med denne fonten vil vises med standardfont, dette kan medføre at tekst ikke ser ut som ønsket. <br><br>For å rette på problemet må fonten installeres eller en annen font velges. Etter installasjon av fonter må Purple Pen startes på nytt for å kunne bruke nye fonter.<br><br>(corrects: Kartfilen "{0}"benytter en fontfil som ikke er installert på datamskinen. Tekst med denne fonten vil vises med standard font,dette kan medføre at tekst ikke ser ut som ønsket. <br><br>For å rette på problemet må fonten installeres eller en annen font velges. Etter installasjon av fonter må Purple Pen startes på nytt for å kunne bruke nye fonter.<br>) | the typo "datamskinen", the missing spaces after "{0}" and after a comma, and the compound "standard font"; nothing else changes |  |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Niet meer waarschuwen voor ontbrekende lettertypen in dit evenement | check box beside the OK button of the Missing Fonts dialog (resizable, 480 px wide); the setting is kept for the event; the term Event |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Kaartbestand "{0}" gebruikt lettertypen die niet op uw computer zijn geïnstalleerd. Tekst in deze lettertypen wordt met een standaardlettertype getekend en afgedrukt, waardoor de tekst er anders kan uitzien dan de maker van de kaart bedoelde. <br><br>Zoek de hieronder vermelde lettertypen op en installeer ze via het Configuratiescherm van Windows om dit probleem op te lossen. Start Purple Pen daarna opnieuw.<br> | the text above the list of the missing fonts, a format string with the map file's name as {0}; two paragraphs and the closing line break of the English; the terms Map File and Font | de |
| `MissingFonts_Text` | Missing Fonts | Ontbrekende lettertypen | title of the Missing Fonts dialog; the term Missing Fonts |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Kartfila "{0}" brukar fontar som ikkje er installerte på datamaskina di. Tekst med desse fontane blir teikna og skriven ut med ein standardfont, og då kan teksten sjå annleis ut enn kartteiknaren meinte. <br><br>For å rette problemet må du finne og installere fontane som er lista opp nedanfor, med kontrollpanelet i Windows, og så starte Purple Pen på nytt.<br><br>(corrects: Kartfilen "{0}"benytter ei fontfil som ikkje er installert på datamskinen. Tekst med denne fonten vil vises med standard font, dette kan medføre at tekst ikke ser ut som ønsket. <br><br>For å rette på problemet må fonten installeres eller en annen font velges. Etter installasjon av fonter må Purple Pen startes på nytt for å kunne bruke nye fonter.<br>) | the text was Bokmål ("benytter", "ikke", "installeres", "en annen", "startes") with the typo "datamskinen"; now Nynorsk, after the English |  |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Ikkje varsle om manglande fontar igjen<br>(corrects: Ikkje varsle om manglende fonter igjen) | "manglende" and "fonter" are Bokmål; Nynorsk "manglande" and "fontar" |  |
| `MissingFonts_Text` | Missing Fonts | Manglande fontar<br>(corrects: Manglande fonter) | "fonter" is the Bokmål plural; the Nynorsk plural of "font" is "fontar" (terms.csv now gives it) |  |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Plik mapy "{0}" używa czcionek, które nie są zainstalowane na tym komputerze. Tekst w tych czcionkach zostanie narysowany i wydrukowany czcionką domyślną, przez co może wyglądać inaczej, niż zamierzył autor mapy. <br><br>Aby rozwiązać ten problem, znajdź i zainstaluj wymienione poniżej czcionki za pomocą Panelu sterowania systemu Windows, a następnie ponownie uruchom program Purple Pen.<br> | the text above the list of the missing fonts, a format string with the map file's name as {0}; two paragraphs and the closing line break of the English; the terms Map File and Font | cs |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Fișierul hartă "{0}" utilizează fonturi care nu sunt instalate pe acest calculator. Textele care folosesc aceste fonturi vor fi afișate și tipărite cu fontul implicit, care poate genera un aspect diferit față de designul original. <br><br>Pentru a remedia această problemă, căutați și instalați fonturile listate dedesubt în Windows Control Panel, apoi restartați Purple Pen.<br><br>(corrects: Fișierul hartă "{0}" utilizează fonturi care nu sunt instalate pe acest calculator. Textele care folosesc aceste fonturi vof fi afișate si tipărite cu fontul implicit, care poate genera un aspect diferit față de designul original. <br><br>Pentru a remedia acceastă problemă, căutați și instalați fonturile listate dedesubt în Windows Control Panel, apoi restartați Purple Pen.<br>) | the typos "vof", "si" and "acceastă"; nothing else changes |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Не предупреждать об отсутствии шрифтов в этом соревновании | check box beside the OK button of the Missing Fonts dialog; "не предупреждать" without "больше" keeps it shorter (about 404 px; the English 302) | uk |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Файл карты «{0}» использует шрифты, которые не установлены на этом компьютере. Текст, набранный этими шрифтами, будет отображаться и печататься шрифтом по умолчанию, поэтому он может выглядеть не так, как задумал автор карты. <br><br>Чтобы устранить проблему, найдите и установите перечисленные ниже шрифты через панель управления Windows, а затем перезапустите Purple Pen.<br> | the text above the list of the missing fonts, a format string with the map file's name as {0}, in the quotation marks of Russian; the terms Map File and Font | uk |
| `MissingFonts_Text` | Missing Fonts | Отсутствующие шрифты | title of the Missing Fonts dialog; the term Missing Fonts |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_checkBoxDontWarnAgain_Text` | Don't warn about missing fonts for this event again | Ne opozarjaj več na manjkajoče pisave za to prireditev | check box beside the OK button of the Missing Fonts dialog (resizable, 480 px wide); the setting is kept for the event; the term Event |  |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Datoteka s karto "{0}" uporablja pisave, ki niso nameščene v vašem računalniku. Besedilo v teh pisavah bo prikazano in natisnjeno s privzeto pisavo, zato je lahko videti drugače, kot je nameraval avtor karte. <br><br>Težavo odpravite tako, da poiščete spodaj navedene pisave in jih namestite z nadzorno ploščo sistema Windows, nato pa znova zaženete Purple Pen.<br> | the text above the list of the missing fonts, a format string with the map file's name as {0}; two paragraphs and the closing line break of the English; the terms Map File and Font | cs |
| `MissingFonts_Text` | Missing Fonts | Manjkajoče pisave | title of the Missing Fonts dialog; the term Missing Fonts |  |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_Text` | Missing Fonts | Eksik Fontlar<br>(corrects: Eksik Fontları Yoksay) | "Eksik Fontları Yoksay" is Ignore Missing Fonts; the dialog only shows them (the term Missing Fonts) |  |

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MissingFonts_labelWarning_Text` | Map file "{0}" uses fonts that are not installed on your computer. Text using these fonts will be drawn and printed with a default font, which may cause text to appear differently than the map designer intended. <br><br>To fix this problem, find and install the fonts listed below with the Windows control panel, then start Purple Pen again.<br> | Файл карти "{0}" використовує шрифти, що відсутні на комп'ютері. Текст, написаний такими шрифтами, буде відображено та роздруковано із використанням шрифта за замовчуванням, що може призвести до порушення дизайну. <br><br>Щоб виправити цю проблему, знайдіть та встановіть шрифти перелічені нижче за допомогою контрольної панелі Windows, тоді запустіть Purple Pen знову.<br><br>(corrects: Файл карти "{0}" використовує шрифти, що відсутні на комп'ютері. Текст, написаний такими шрифтами, буде відображено та роздруковано із використанням шрифта за замовченням, що може призвести до порушення до порушення дизайну. <br><br>Щоб виправити цю проблему, знайдіть та встановіть шрифти перелічені нижче за допомогою контрольної панелі Windows, тоді запустіть Purple Pen знову.<br>) | "до порушення" was written twice; "за замовчуванням" is the usual form of "by default"; nothing else changes |  |
