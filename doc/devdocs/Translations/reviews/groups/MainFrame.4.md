# Key group MainFrame.4

Nov 16, 2026. The fourth part of the main window: the Course, Item, Reports and Help menus of the menu bar, with the
submenu Leg Flagging of the Item menu, and the four items of the hidden Debug menu of the WinForms application
(Symbol Browser, Description Browser, Control Tester, Map Tester), which the Avalonia application does not show.
claude-opus-5-5 at effort max made every line below, and each entry says so in its comment.

- **Translated**: 115 lines, in 13 languages. ru lacked the whole group, and nl all of it but eight items of the Item
  menu. sl, nn-NO and ro lacked the newer items of the Course and Item menus (Duplicate Course, Course Order, Relay
  Team Variations, Delete Fork/Loop, Remove Gap, Change Line Appearance, Stretch) and Make a Donation; sl lacked About
  and the Debug items too. 11 languages lacked Stretch. Each new item of a menu has an access key that no other item
  of its menu uses. The items of the Reports menu are also the titles of their reports. nl calls the course "route"
  and the leg "been", as its terms do, and writes "estafetteteam" as one word. The Translated Help item of nl and ru
  names the language, as fi and nb-NO do, because it shows only where a help in the language exists.
- **Not needed**: none.
- **Corrected**: 122 lines, in 22 languages, most of them after terms.csv.
  - da, 29 lines. Its course was "kursus" (a course of study), its control "kontrol" (a check), its leg "ben" (a leg
    of the body), its taping "udflagning" (no Danish word), its fork the English "fork", its bend "buk" (a he-goat),
    its Item menu "Vare" (merchandise) and its event a "hændelse" (an incident). Its Competitor Load was "Konkurrent
    Læs" (a rival's cartload), and its Control and Leg Load "Kontrol og læs" (check and read). They are now "bane",
    "post", "stræk", "afmærket", "gafling", "knæk", "Element" and "løb".
  - The leg. tr's leg was "bacak", a leg of the body; it is now "ayak". cs's leg between two controls was "úsek", a leg
    of a relay; it is now "postup". The Italian legs were "frazioni", the legs of a relay; they are now "tratte". uk's
    nominative was the Russian "перегон".
  - Wrong meanings. The Italian Leg Flagging was the changeover zone of a relay. Its Web Site item said "Purple Pen
    Help (Translated)", and its Translated Help item was English. sl's Change Text was Delete Text. de's Change
    Displayed Courses named another thing, and its Leg Lengths were the lengths of courses. hu's and tr's Change Line
    Appearance changed a line of text. fr's Control Cross-reference was a cross-check. fr, pt and pt-BR named the
    Control and Leg Load with the verb that loads a file ("Charger", "Carregar"). tr's relay was an electrical relay
    ("Röle"), and its Competitor Load meant "load a rival". pt-BR's Course Order was "course and order", and its Event
    Audit audited a course. pt's flagged beginning and end were of a course. et's Entire Leg Flagged was a command.
    zh-CN's competitor was a rival (竞争对手), and its leg a course of study (课程). zh-TW's load was the loading of a
    file (載入).
  - English. cs's Stretch, fi's Make a Donation and Debug items, the bend of nb-NO and nn-NO, da's Symbol Browser and
    Support/Bug Reporting, and pt's Support were English.
  - Grammar and typos. sl and the Italian put the English word for word ("Proga Povzetek", "Tekmovalec Obremenitev",
    "Percorso Sommario"). tr's compounds lacked their suffix ("Parkur Özet", "Etkinlik Denetim"), and "Ekle_Kıvrım"
    and "Ekle_Açıklık" lacked a space. Typos: cs "zančení" and "Přida", et "järjekod" and "Purpe", uk "Довжити", ro
    "Fruzărire", "Adaugare" and "si", pl "Złóź", fr "DupliquerCircuit", a space in zh-CN and two in pt-BR. bg and ro
    did not agree in gender. The Italian "Duplicata" was a noun. nb-NO, nl, hu and sl split compounds or wrote
    capitals. nn-NO wrote bokmål ("merket", "hjemmeside", "utnyttelse").
- **Left empty**: none.
- **terms.csv**: five cells were corrected with the group, and terms-decisions.tsv records each: nl's Relay Team and
  Relay Team Variations, tr's Change Line Appearance and Control Cross-reference, and da's Delete Fork/Loop.

Doubts:

- ru's Course menu takes the access key "и" ("Д_истанция"), as "д" is its Add menu. Its Help menu, in MainFrame.5,
  keeps "с" for "Справка".
- nl's Item menu is "Voorwerp", the term of terms.csv, from the language's report column. "Element" or "Object" is
  the more usual name of such a menu.
- nl's Competitor Load is "Drukte per route", after its term "Drukte" and its Leg load "Drukte per been".
- The Italian keeps "Controllo" and "Controlli" in the items that are otherwise right (Control Tester, Change
  Control Codes), as MainFrame.2 and MainFrame.3 do. The items that the group corrected take the term "punti".
- The Italian Remove Gap "Rimuovi Tratta Vuota" stays. "Tratta" is also the language's leg, but in Italian it is any
  stretch of a line.
- de's Add Bend and Remove Bend keep "Eckpunkt", which terms.csv finds ambiguous with Add Corner. The menu shows the
  two items together.
- Some existing items keep a form against terms.csv, as they are not wrong: cs's Relay Team Variations "Úseky štafet",
  the Italian About "Riguardo A Purple Pen", uk's Change Displayed Courses "Відобразити на дистанціях", and hu's
  "Váltó csapat Variációk".
- Several menus had an access key twice before this group (the Item menus of bg, cs, de, es, et, fi, hu, pl and sv,
  the Reports menus of bg and sl, and several Help menus). The group leaves those of the lines that it does not
  change, and adds no new one.
- Some items have no access key: no item of the zh-CN and zh-TW menus has one, and the Control and Leg Load of nb-NO
  and nn-NO had none. check.py warns of them. Items that are longer than the English have room, as a menu widens.
#### bg

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_beginFlaggingMenu_Text` | Beginning of Leg Flagged | Началото на отсечката е маркирано<br>(corrects: Началото на отсечката е маркиран) | "началото" is neuter, and the participle agrees with it |  |

#### cs

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addBendMenu_Text` | Add _Bend | Přidat ohy_b<br>(corrects: Přida _ohyb) | the typo "Přida" (the term Add Bend); the access key "b" is a letter that no other item of the Item menu uses ("o" is the Rotate) |  |
| `MainFrame_stretchMenu_Text` | _Stretch | _Roztáhnout<br>(corrects: _Stretch) | "Stretch" is the English (the term Stretch) |  |
| `MainFrame_noFlaggingMenu_Text` | No Flagging | Bez značení<br>(corrects: Bez zančení) | the typo "zančení" (the term No Flagging) |  |
| `MainFrame_entireFlaggingMenu_Text` | Entire Leg Flagged | Celý postup je značený<br>(corrects: Celý úsek je značený) | the language's leg between two controls is "postup"; "úsek" is a leg of a relay (the term Entire Leg Flagged) |  |
| `MainFrame_beginFlaggingMenu_Text` | Beginning of Leg Flagged | Začátek postupu je značený<br>(corrects: Začátek úseku je značený) | the language's leg between two controls is "postup"; "úsek" is a leg of a relay (the term Leg) |  |
| `MainFrame_endFlaggingMenu_Text` | End of Leg Flagged | Konec postupu je značený<br>(corrects: Konec úseku je značený) | the language's leg between two controls is "postup"; "úsek" is a leg of a relay (the term Leg) |  |
| `MainFrame_legFlaggingMenu_Text` | _Leg Flagging | Značení _postupu<br>(corrects: _Značení úseku) | the language's leg between two controls is "postup"; "úsek" is a leg of a relay (the term Leg Flagging); the access key "p" is a letter that no other item of the Item menu uses |  |
| `MainFrame_legLengthsMenu_Text` | _Leg Lengths | Dé_lky postupů<br>(corrects: Dé_lky úseků) | the language's leg between two controls is "postup"; "úsek" is a leg of a relay (the term Leg Lengths) |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | _Vytížení kontrol a postupů<br>(corrects: užití kontrol a úseků) | the item began with a small letter, its leg was "úsek", a leg of a relay, and "užití" is the use, where the load is "vytížení" (the term Load) |  |

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_stretchMenu_Text` | _Stretch | St_ræk ud | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses: "r", as "s" is the Delete, "_Slet" |  |
| `MainFrame_addCourseMenu_Text` | _Add Course... | _Tilføj bane...<br>(corrects: _Tilføj kursus...) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term Add Course) |  |
| `MainFrame_deleteCourseMenu_Text` | _Delete Course | _Slet bane<br>(corrects: _Slet kursus) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term Delete Course) |  |
| `MainFrame_duplicateCourseMenu_Text` | D_uplicate Course... | Du_pliker bane...<br>(corrects: Du_pliker kursus...) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term Duplicate Course) |  |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | Banernes _rækkefølge...<br>(corrects: Kursus _Rækkefølge...) | Danish orienteering calls a course "bane" ("kursus" is a course of study), and "rækkefølge" takes no capital (the term Course Order) |  |
| `MainFrame_courseLoadMenu_Text` | Competitor _Load... | Antal _løbere...<br>(corrects: Konkurrent _Læs) | "Konkurrent" is a rival and "læs" a cartload; the item sets the number of runners (the term Competitor Load), and opens a dialog |  |
| `MainFrame_courseVariationReportMenu_Text` | Relay Team _Variations... | Stafethold_variationer...<br>(corrects: Relay Team _Variationer...) | "Relay Team" is the English (the term Relay Team Variations) |  |
| `MainFrame_courseMenu_Text` | _Course | _Bane<br>(corrects: _Kursus) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term Course); the access key "b" is a letter that no other menu of the menu bar uses |  |
| `MainFrame_deleteForkMenu_Text` | Delete For_k/Loop | Slet _gafling/løkke<br>(corrects: Slet For_k/Løkke) | "Fork" is the English; the fork of a course is a "gafling" (the term Fork), and "løkke" takes no capital |  |
| `MainFrame_addBendMenu_Text` | Add _Bend | Tilføj _knæk<br>(corrects: Tilføj _Buk) | "buk" is a he-goat or a belly; a bend of a line is a "knæk" (the term Add Bend) |  |
| `MainFrame_removeBendMenu_Text` | Remove B_end | Fjern k_næk<br>(corrects: Fjern B_uk) | "buk" is a he-goat or a belly; a bend of a line is a "knæk" (the term Remove Bend); the access key "n" is a letter that no other item of the Item menu uses ("k" is the Add Bend) |  |
| `MainFrame_removeGapMenu_Text` | Remove Ga_p | _Fjern hul<br>(corrects: Fjern H_ul) | "hul" takes no capital in Danish (the term Remove Gap); the access key "f" is a letter that no other item of the Item menu uses ("u" is the Change Line Appearance) |  |
| `MainFrame_changeTextMenu_Text` | Change _Text... | Ændr _tekst...<br>(corrects: Ændr _Tekst...) | "tekst" takes no capital in Danish (the term Change Text) |  |
| `MainFrame_noFlaggingMenu_Text` | No Flagging | Ingen afmærkning<br>(corrects: Ingen Udflagning) | "udflagning" is no Danish word; the language says "afmærket" of a taped route (the term No Flagging) |  |
| `MainFrame_entireFlaggingMenu_Text` | Entire Leg Flagged | Hele strækket afmærket<br>(corrects: Hele Ben Udflagning) | "ben" is a leg of the body, and "udflagning" no Danish word; Danish orienteering says "stræk" and "afmærket" (the term Entire Leg Flagged) |  |
| `MainFrame_beginFlaggingMenu_Text` | Beginning of Leg Flagged | Begyndelsen af strækket afmærket<br>(corrects: Begyndelse af ben Udflagning) | "ben" is a leg of the body, and "udflagning" no Danish word; as the language's Entire Leg Flagged, "Hele strækket afmærket" |  |
| `MainFrame_endFlaggingMenu_Text` | End of Leg Flagged | Slutningen af strækket afmærket<br>(corrects: Slut på ben Udflagning) | "ben" is a leg of the body, and "udflagning" no Danish word; as the language's Entire Leg Flagged, "Hele strækket afmærket" |  |
| `MainFrame_legFlaggingMenu_Text` | _Leg Flagging | _Afmærkning af stræk<br>(corrects: _Ben Udflagning) | "ben" is a leg of the body, and "udflagning" no Danish word (the term Leg Flagging); the access key "a" is a letter that no other item of the Item menu uses |  |
| `MainFrame_changeDisplayedCoursesMenu_Text` | Change Displa_yed Courses... | Ændr viste _baner...<br>(corrects: Ændr Vi_ste kurser...) | Danish orienteering calls courses "baner", and "viste" takes no capital (the term Change Displayed Courses); the access key "b" is a letter that no other item of the Item menu uses ("s" is the Delete) |  |
| `MainFrame_itemMenu_Text` | _Item | _Element<br>(corrects: _Vare) | "vare" is merchandise (the term Item); the access key "e" is a letter that no other menu of the menu bar uses |  |
| `MainFrame_courseSummaryMenu_Text` | Course _Summary | Bane_oversigt<br>(corrects: Kursus _Resumé) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term Course Summary); the access key "o" is a letter that no other item of the Reports menu uses |  |
| `MainFrame_eventAuditMenu_Text` | Event _Audit | _Gennemgang af løbet<br>(corrects: Hændelse _Resumé) | "Hændelse Resumé" is the summary of an incident; the report checks the event, "løbet" (the term Event Audit) |  |
| `MainFrame_legLengthsMenu_Text` | _Leg Lengths | _Stræklængder<br>(corrects: _Ben længder) | "ben" is a leg of the body, and the compound was split; Danish orienteering says "stræk" (the term Leg Lengths) |  |
| `MainFrame_controlCrossrefMenu_Text` | Control _Cross-reference | Post_krydsreference<br>(corrects: Kontrol _Krydsreference) | Danish orienteering calls a control "post" ("kontrol" is a check), and the compound was split (the term Control Cross-reference) |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | Post- og stræk_belastning<br>(corrects: Kontrol og læ_s) | "Kontrol og læs" is "check and read"; the report gives the load of the controls and legs, as the term Leg load ("Belastning af stræk") and sv "Kontroll- och sträckbelastning" |  |
| `MainFrame_supportWebSiteMenu_Text` | _Support/Bug Reporting | _Support/fejlrapportering<br>(corrects: _Support/Bug Rapportering) | "Bug Rapportering" is half English (the term Support/Bug Reporting) |  |
| `MainFrame_donateWebSiteMenu_Text` | Make a _Donation | Giv et _bidrag<br>(corrects: Giv en _Donation) | "Donation" took a capital (the term Make a donation, "Giv et bidrag"); the access key "b" is a letter that no other item of the Help menu uses |  |
| `MainFrame_symbolBrowserMenu_Text` | Symbol Browser | Symbolbrowser<br>(corrects: Symbol Browser) | "Symbol Browser" is the English (the term Symbol Browser) |  |
| `MainFrame_descriptionBrowserMenu_Text` | Description Browser | Beskrivelsesbrowser<br>(corrects: Beskrivelses Browser) | the compound was split; as the language's "Symbolbrowser" |  |
| `MainFrame_controlTesterMenu_Text` | Control Tester | Posttester<br>(corrects: Kontroltester) | Danish orienteering calls a control "post" ("kontrol" is a check; the term Control) |  |

#### de

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_legLengthsMenu_Text` | _Leg Lengths | Teilstrecken_längen<br>(corrects: Bahn_längen) | "Bahnlängen" are the lengths of courses; the report gives the lengths of the legs (the term Leg Lengths) |  |
| `MainFrame_changeLineAppearanceMenu_Text` | Change Line _Appearance... | Linienaus_sehen ändern...<br>(corrects: Linie _Aussehen ändern) | "Linie Aussehen ändern" is not grammatical (the term Change Line Appearance), and the item opens a dialog; the access key "s" is a letter that no other item of the Item menu uses ("a" is the Leg Flagging) |  |
| `MainFrame_changeDisplayedCoursesMenu_Text` | Change Displa_yed Courses... | Angezeigte Bahnen _wechseln...<br>(corrects: Anze_ige besonderer Objekte...) | "Anzeige besonderer Objekte" names another thing; the item chooses the courses that show the selected object (the term Change Displayed Courses); the access key "w" is a letter that no other item of the Item menu uses |  |

#### es

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_removeGapMenu_Text` | Remove Ga_p | Su_primir recorte | item of the Item menu of the main window, which removes a gap from the selected control circle or line; the term Remove Gap; its access key is a letter that no other item of its menu uses | UIText.CommandNameText_RemoveGap CommandNameText.RemoveGap |
| `MainFrame_stretchMenu_Text` | _Stretch | E_stirar | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_changeDisplayedCoursesMenu_Text` | Change Displa_yed Courses... | _Cambiar recorridos en que aparece...<br>(corrects: _Cambiar rutas en que aparece...) | the language's course is "recorrido"; a "ruta" is a route (the term Change Displayed Courses) |  |

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | Radade _järjekord...<br>(corrects: Radade _järjekod...) | the typo "järjekod" (the term Course Order) |  |
| `MainFrame_entireFlaggingMenu_Text` | Entire Leg Flagged | Kogu etapp tähistatud<br>(corrects: Lisa etapi märgistus) | "Lisa etapi märgistus" is the command Add leg marking; the item names a state, with the language's "tähistatud" (the term Entire Leg Flagged) |  |
| `MainFrame_changeDisplayedCoursesMenu_Text` | Change Displa_yed Courses... | Muuda _valitud radu...<br>(corrects: Muuda valitud radade...) | "radade" is a genitive with no noun after it (the term Change Displayed Courses); the access key "v" is a letter that no other item of the Item menu uses |  |
| `MainFrame_mainWebSiteToolMenu_Text` | Purple Pen _Web Site | Purple Pen _kodulehekülg<br>(corrects: Purpe Pen _Kodulehekülg) | the typo "Purpe", and "kodulehekülg" takes no capital |  |

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_donateWebSiteMenu_Text` | Make a _Donation | _Lahjoita<br>(corrects: Make a _Donation) | "Make a Donation" is the English (the term Make a donation); the access key "l" is a letter that no other item of the Help menu uses |  |
| `MainFrame_symbolBrowserMenu_Text` | Symbol Browser | Symboliselain<br>(corrects: Symbol Browser) | "Symbol Browser" is the English (the term Symbol Browser) |  |
| `MainFrame_descriptionBrowserMenu_Text` | Description Browser | Rastinmääriteselain<br>(corrects: Description Browser) | "Description Browser" is the English; as the language's "Symboliselain" and "rastinmääritteet" (Descriptions) |  |
| `MainFrame_controlTesterMenu_Text` | Control Tester | Rastitesteri<br>(corrects: Control Tester) | "Control Tester" is the English; the language's control is "rasti" |  |
| `MainFrame_mapTesterMenu_Text` | Map Tester | Karttatesteri<br>(corrects: Map Tester) | "Map Tester" is the English |  |

#### fr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_stretchMenu_Text` | _Stretch | _Étirer | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses | UIText.CommandNameText_Stretch CommandNameText.Stretch |
| `MainFrame_duplicateCourseMenu_Text` | D_uplicate Course... | D_upliquer le circuit...<br>(corrects: D_upliquerCircuit...) | the space between the words was missing ("DupliquerCircuit") (the term Duplicate Course) |  |
| `MainFrame_noFlaggingMenu_Text` | No Flagging | Sans jalonnage<br>(corrects: Sans Jalon) | a "jalon" is one marker; the language's flagging is "jalonnage" (the term No Flagging) |  |
| `MainFrame_controlCrossrefMenu_Text` | Control _Cross-reference | Références c_roisées des postes<br>(corrects: Contrôle C_roisé) | "Contrôle croisé" is a cross-check; the report lists the courses of each control (the term Control Cross-reference); the access key of the old text |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | _Charge des postes et interpostes<br>(corrects: _Charger les postes et interpostes) | "Charger" is the verb load, as of a file; the report gives the load of the controls and legs, "charge" (the term Load) |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_stretchMenu_Text` | _Stretch | _Nyújtás | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | Pálya_sorrend...<br>(corrects: Pálya sorrend...) | the compound was split (the term Course Order, "Pályasorrend"); the access key "s" is a letter that no other item of the Course menu uses |  |
| `MainFrame_changeLineAppearanceMenu_Text` | Change Line _Appearance... | _Vonal kinézetének módosítása...<br>(corrects: _Sor kinézete...) | "sor" is a line of text; a drawn line is "vonal" (the term Change Line Appearance); the access key "v" is a letter that no other item of the Item menu uses |  |

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_duplicateCourseMenu_Text` | D_uplicate Course... | _Duplica Percorso...<br>(corrects: _Duplicata Percorso...) | "Duplicata" is the noun, a copy; the command is the verb "Duplica" (the term Duplicate Course) |  |
| `MainFrame_legFlaggingMenu_Text` | _Leg Flagging | Tratta _Segnalata<br>(corrects: _Zona Cambio Frazionisti) | "Zona Cambio Frazionisti" is the changeover zone of a relay (the term Leg Flagging), in the capitals of the items of its submenu ("Intera Tratta Segnalata"); the access key "s" is a letter that no other item of the Item menu uses ("t" is the Change Text) |  |
| `MainFrame_courseSummaryMenu_Text` | Course _Summary | _Sommario Percorsi<br>(corrects: Percorso _Sommario) | "Percorso Sommario" is the English put word for word (the term Course Summary) |  |
| `MainFrame_legLengthsMenu_Text` | _Leg Lengths | _Lunghezza Tratte<br>(corrects: _Lunghezza Frazioni) | "frazione" is a leg of a relay; the report gives the legs between controls, "tratte" (the term Leg Lengths) |  |
| `MainFrame_controlCrossrefMenu_Text` | Control _Cross-reference | _Riferimenti Incrociati dei Punti<br>(corrects: Incroci Controlli) | "Incroci Controlli" are crossings of checks; the report lists the courses of each control (the term Control Cross-reference); the access key "r" is a letter that no other item of the Reports menu uses |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | _Passaggi per Punto e Tratta<br>(corrects: Carico Controlli e Frazioni) | "frazione" is a leg of a relay; the report counts the competitors at each control and on each leg, as the term Leg load ("Passaggi per tratta"); the access key "p" is a letter that no other item of the Reports menu uses |  |
| `MainFrame_helpTranslatedMenu_Text` | Purple Pen Help (Translated) | Aiuto Purple Pen (Tradotto)<br>(corrects: Purple Pen Help (Translated)) | the item was English (the term Purple Pen Help); the words that the Web Site item had by mistake |  |
| `MainFrame_mainWebSiteToolMenu_Text` | Purple Pen _Web Site | Sito _web di Purple Pen<br>(corrects: _Aiuto Purple Pen (Tradotto)) | the item said "Aiuto Purple Pen (Tradotto)", the help in translation; it opens the web site; the access key "w" is a letter that no other item of the Help menu uses |  |

#### ja

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_removeGapMenu_Text` | Remove Ga_p | 切れ目を除去(_P) | item of the Item menu of the main window, which removes a gap from the selected control circle or line; the term Remove Gap; its access key is a letter that no other item of its menu uses | UIText.CommandNameText_RemoveGap CommandNameText.RemoveGap |
| `MainFrame_stretchMenu_Text` | _Stretch | 伸縮(_S) | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses |  |

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | Løype_rekkefølge...<br>(corrects: Løype _Rekkefølge...) | the compound was split, with a capital (the term Course Order, "Løyperekkefølge") |  |
| `MainFrame_courseVariationReportMenu_Text` | Relay Team _Variations... | Stafett_varianter...<br>(corrects: Stafett lag _Varianter...) | "Stafett lag" was split, with a capital (the term Relay Team Variations, "Stafettvarianter") |  |
| `MainFrame_addBendMenu_Text` | Add _Bend | Legg til _knekkpunkt<br>(corrects: Legg til _bend) | "bend" is the English; the bend of a line is a "knekkpunkt" in Norwegian (the term Add Bend) |  |
| `MainFrame_removeBendMenu_Text` | Remove B_end | Slett k_nekkpunkt<br>(corrects: Slett bend) | "bend" is the English; the bend of a line is a "knekkpunkt" in Norwegian (the term Remove Bend); the access key "n" is a letter that no other item of the Item menu uses ("k" is the Add Bend) |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | Statistikk for post- og strekkutnyttelse<br>(corrects: Statistikk for post-og strekkutnyttelse) | the space after the hyphen was missing ("post-og") |  |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addCourseMenu_Text` | _Add Course... | Route _toevoegen... | item of the Course menu of the main window, which opens the dialog that adds a course; the term Add Course; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_deleteCourseMenu_Text` | _Delete Course | Route _verwijderen | item of the Course menu of the main window, which deletes the current course; the term Delete Course; its access key is a letter that no other item of its menu uses | UIText.CommandNameText_DeleteCourse CommandNameText.DeleteCourse |
| `MainFrame_duplicateCourseMenu_Text` | D_uplicate Course... | Route _dupliceren... | item of the Course menu of the main window, which opens the dialog that copies the current course; the term Duplicate Course; its access key is a letter that no other item of its menu uses: the infinitive of the items around it ("Route toevoegen", "Route verwijderen"), with the words of the language's command "Dupliceer route" | UIText.CommandNameText_DuplicateCourse UIText.MiscText_DuplicateCourseTitle |
| `MainFrame_propertiesMenu_Text` | _Properties... | _Eigenschappen... | item of the Course menu of the main window, which opens the properties of the current course; the term Properties; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | V_olgorde route... | item of the Course menu of the main window, which opens the dialog of the order of the courses; the term Course Order; its access key is a letter that no other item of its menu uses | UIText.ChangeCourseOrder_Text |
| `MainFrame_courseLoadMenu_Text` | Competitor _Load... | Drukte _per route... | item of the Course menu of the main window, which opens the dialog of the expected number of competitors on each course; the term Competitor Load; its access key is a letter that no other item of its menu uses, and the language's "Drukte per been" (Leg load) | UIText.CourseLoad_Load_HeaderText CourseLoad.resx,Load.HeaderText |
| `MainFrame_courseVariationReportMenu_Text` | Relay Team _Variations... | V_arianten van estafetteteams... | item of the Course menu of the main window, which opens the report of the variations of the relay teams; the term Relay Team Variations; its access key is a letter that no other item of its menu uses; "estafetteteam" is one word, which the language's "Estafette team varianten" splits; terms.csv is corrected with the group | UIText.CommandNameText_RelayTeamVariations UIText.TeamVariationsForm_Text |
| `MainFrame_courseMenu_Text` | _Course | _Route | menu Course of the menu bar of the main window; the term Course; its access key is a letter that no other menu of the menu bar uses | UIText.ReportText_ColumnHeader_Course ReportText.ColumnHeader_Course |
| `MainFrame_stretchMenu_Text` | _Stretch | _Uitrekken | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_noFlaggingMenu_Text` | No Flagging | Geen lint | check item of the submenu Leg Flagging of the Item menu, without an access key as the English: how much of the selected leg is taped: none; the term No Flagging |  |
| `MainFrame_entireFlaggingMenu_Text` | Entire Leg Flagged | Hele been gelint | check item of the submenu Leg Flagging of the Item menu, without an access key as the English: how much of the selected leg is taped: all of it; the term Entire Leg Flagged |  |
| `MainFrame_beginFlaggingMenu_Text` | Beginning of Leg Flagged | Begin van been gelint | check item of the submenu Leg Flagging of the Item menu, without an access key as the English: how much of the selected leg is taped: its beginning; the form of the language's Entire Leg Flagged |  |
| `MainFrame_endFlaggingMenu_Text` | End of Leg Flagged | Einde van been gelint | check item of the submenu Leg Flagging of the Item menu, without an access key as the English: how much of the selected leg is taped: its end; the form of the language's Entire Leg Flagged |  |
| `MainFrame_legFlaggingMenu_Text` | _Leg Flagging | Ge_lint been | item of the Item menu of the main window, a submenu of how much of the selected leg is taped; the term Leg Flagging; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_changeDisplayedCoursesMenu_Text` | Change Displa_yed Courses... | _Kies getoonde routes... | item of the Item menu of the main window, which opens the dialog of the courses that show the selected special object; the term Change Displayed Courses; its access key is a letter that no other item of its menu uses | UIText.ChangeSpecialCourses_Text UIText.CommandNameText_ChangeDisplayedCourses |
| `MainFrame_itemMenu_Text` | _Item | _Voorwerp | menu Item of the menu bar of the main window, which acts on the selected object; the term Item; its access key is a letter that no other menu of the menu bar uses | UIText.ReportText_ColumnHeader_Item ReportText.ColumnHeader_Item |
| `MainFrame_courseSummaryMenu_Text` | Course _Summary | _Routeoverzicht | item of the Reports menu of the main window, whose text is also the title of the report: the length, climb and controls of each course; the term Course Summary; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_eventAuditMenu_Text` | Event _Audit | Controle van _evenement | item of the Reports menu of the main window, whose text is also the title of the report: the problems that the program finds in the event; the term Event Audit; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_legLengthsMenu_Text` | _Leg Lengths | _Beenlengtes | item of the Reports menu of the main window, whose text is also the title of the report: the length of each leg of each course; the term Leg Lengths; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_controlCrossrefMenu_Text` | Control _Cross-reference | _Kruisverwijzing posten | item of the Reports menu of the main window, whose text is also the title of the report: the courses that use each control; the term Control Cross-reference; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | _Drukte per post en been | item of the Reports menu of the main window, whose text is also the title of the report: the number of competitors who visit each control and run each leg; the terms Control and Leg load; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_reportMenu_Text` | _Reports | Ra_pporten | menu Reports of the menu bar of the main window; the term Reports; its access key is a letter that no other menu of the menu bar uses: "p", as "r" is the Course menu, "_Route" |  |
| `MainFrame_helpContentsMenu_Text` | Purple Pen _Help | Purple Pen _Help | item of the Help menu of the main window, which opens the help of Purple Pen; the term Purple Pen Help; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_helpTranslatedMenu_Text` | Purple Pen Help (Translated) | Purple Pen Help (Nederlands) | item of the Help menu of the main window, which opens the help of Purple Pen in the language of the user, when one exists; without an access key as the English; the term Purple Pen Help; the item shows only where a help exists in the language, and names it, as fi "(suomi)" and nb-NO "(norsk)" |  |
| `MainFrame_mainWebSiteToolMenu_Text` | Purple Pen _Web Site | _Website van Purple Pen | item of the Help menu of the main window, which opens the web site of Purple Pen; the program name stays; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_supportWebSiteMenu_Text` | _Support/Bug Reporting | Ondersteuning/_Fouten melden | item of the Help menu of the main window, which opens the page of support and bug reports; the term Support/Bug Reporting; its access key is a letter that no other item of its menu uses: "f", as "o" is the About, "_Over Purple Pen" |  |
| `MainFrame_donateWebSiteMenu_Text` | Make a _Donation | _Doneren | item of the Help menu of the main window, which opens the page for donations; the term Make a donation; its access key is a letter that no other item of its menu uses | UIText.InitialScreen_donationLink_Text |
| `MainFrame_aboutMenu_Text` | _About Purple Pen | _Over Purple Pen | item of the Help menu of the main window, which opens the About dialog; the term About Purple Pen; its access key is a letter that no other item of its menu uses | UIText.AboutForm_Text |
| `MainFrame_symbolBrowserMenu_Text` | Symbol Browser | Symboolbrowser | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show; the term Symbol Browser |  |
| `MainFrame_descriptionBrowserMenu_Text` | Description Browser | Omschrijvingenbrowser | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a browser of the description symbols; as the language's Symbol Browser |  |
| `MainFrame_controlTesterMenu_Text` | Control Tester | Posttester | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a tester of the drawing of controls; the term Control |  |
| `MainFrame_mapTesterMenu_Text` | Map Tester | Kaarttester | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a tester of the drawing of maps |  |
| `MainFrame_changeLineAppearanceMenu_Text` | Change Line _Appearance... | Wijzig _weergave van lijn...<br>(corrects: Wijzig lijn _weergave...) | "lijn weergave" splits a compound; the language's command says "Wijzig weergave van lijn" (the term Change Line Appearance) | UIText.CommandNameText_ChangeLineAppearance UIText.MiscText_ChangeLineAppearanceTitle |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_duplicateCourseMenu_Text` | D_uplicate Course... | _Kopier løype... | item of the Course menu of the main window, which opens the dialog that copies the current course; the term Duplicate Course; its access key is a letter that no other item of its menu uses | nb-NO |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | _Rekkefølgje på løyper... | item of the Course menu of the main window, which opens the dialog of the order of the courses; the term Course Order; its access key is a letter that no other item of its menu uses | UIText.ChangeCourseOrder_Text |
| `MainFrame_courseVariationReportMenu_Text` | Relay Team _Variations... | Stafett_variantar... | item of the Course menu of the main window, which opens the report of the variations of the relay teams; the term Relay Team Variations; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_deleteForkMenu_Text` | Delete For_k/Loop | Slett _gafling/sommarfugl | item of the Item menu of the main window, which deletes the fork or loop at the selected control; the term Delete Fork/Loop; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_removeGapMenu_Text` | Remove Ga_p | Slett å_pning | item of the Item menu of the main window, which removes a gap from the selected control circle or line; the term Remove Gap; its access key is a letter that no other item of its menu uses | UIText.CommandNameText_RemoveGap CommandNameText.RemoveGap |
| `MainFrame_changeLineAppearanceMenu_Text` | Change Line _Appearance... | Endre _utsjånad på linje... | item of the Item menu of the main window, which opens the dialog of the colour, width and style of the selected line or shape; the term Change Line Appearance; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_stretchMenu_Text` | _Stretch | St_rekk ut | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses: "r", as "s" is the Delete, "_Slett", and "u" the Change Line Appearance |  |
| `MainFrame_donateWebSiteMenu_Text` | Make a _Donation | _Gje eit bidrag | item of the Help menu of the main window, which opens the page for donations; the term Make a donation; its access key is a letter that no other item of its menu uses | UIText.InitialScreen_donationLink_Text |
| `MainFrame_addBendMenu_Text` | Add _Bend | Legg til _knekkpunkt<br>(corrects: Legg til _bend) | "bend" is the English; the bend of a line is a "knekkpunkt" in Norwegian (the term Add Bend) |  |
| `MainFrame_removeBendMenu_Text` | Remove B_end | Slett k_nekkpunkt<br>(corrects: Slett bend) | "bend" is the English; the bend of a line is a "knekkpunkt" in Norwegian (the term Remove Bend); the access key "n" is a letter that no other item of the Item menu uses ("k" is the Add Bend) |  |
| `MainFrame_legFlaggingMenu_Text` | _Leg Flagging | _Merka strekk<br>(corrects: _Merket strekk) | "merket" is bokmål; the language says "merka" ("Heile strekket merka") (the term Leg Flagging) |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | Statistikk for post- og strekkutnytting<br>(corrects: Statistikk for post-og strekkutnyttelse) | the space after the hyphen was missing ("post-og"), and "utnyttelse" is bokmål; nynorsk says "utnytting" |  |
| `MainFrame_mainWebSiteToolMenu_Text` | Purple Pen _Web Site | Purple Pen h_eimeside<br>(corrects: Purple Pen _hjemmeside) | "hjemmeside" is bokmål; nynorsk says "heimeside"; the access key "e" is a letter that no other item of the Help menu uses ("h" is the Purple Pen Help) |  |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_courseVariationReportMenu_Text` | Relay Team _Variations... | Warianty drużyn _sztafetowych... | item of the Course menu of the main window, which opens the report of the variations of the relay teams; the term Relay Team Variations; its access key is a letter that no other item of its menu uses: "s", as "w" is the Properties, "_Właściwości" |  |
| `MainFrame_deleteForkMenu_Text` | Delete For_k/Loop | Usuń _rozwidlenie/pętlę | item of the Item menu of the main window, which deletes the fork or loop at the selected control; the term Delete Fork/Loop; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_removeGapMenu_Text` | Remove Ga_p | Usuń _przerwę | item of the Item menu of the main window, which removes a gap from the selected control circle or line; the term Remove Gap; its access key is a letter that no other item of its menu uses; "przerwę" in small letters, unlike the language's command "Usuń Przerwę" | UIText.CommandNameText_RemoveGap |
| `MainFrame_stretchMenu_Text` | _Stretch | Ro_zciągnij | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses: "z", as "r" is the Delete Fork/Loop |  |
| `MainFrame_donateWebSiteMenu_Text` | Make a _Donation | _Złóż darowiznę<br>(corrects: Złóź darowiznę) | the typo "Złóź" (the term Make a donation); the access key "z" is a letter that no other item of the Help menu uses |  |

#### pt

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_beginFlaggingMenu_Text` | Beginning of Leg Flagged | Início da Pernada Balizada<br>(corrects: Início de Percurso Balizado) | a "percurso" is a course; the item tapes the beginning of a leg, "pernada", as the language's "Toda a Pernada Balizada" |  |
| `MainFrame_endFlaggingMenu_Text` | End of Leg Flagged | Fim da Pernada Balizada<br>(corrects: Fim de Percurso Balizado) | a "percurso" is a course; the item tapes the end of a leg, "pernada", as the language's "Toda a Pernada Balizada" |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | Carga de Controlos e Pernadas (_D)<br>(corrects: Carregar Controlos e Pernadas (_D)) | "Carregar" is the verb load, as of a file; the report gives the load of the controls and legs, "carga" (the term Load) |  |
| `MainFrame_supportWebSiteMenu_Text` | _Support/Bug Reporting | _Suporte/Relatório de erros<br>(corrects: _Suporte/Report de Bugs) | "Report de Bugs" is English (the term Support/Bug Reporting) |  |

#### pt-BR

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | _Ordem do Percurso...<br>(corrects: Percurso e _Ordem...) | "Percurso e Ordem" is "course and order"; the dialog sets the order of the courses (the term Course Order) |  |
| `MainFrame_eventAuditMenu_Text` | Event _Audit | _Auditoria do Evento<br>(corrects: _Auditoria do Percurso) | the report checks the whole event, not one course ("Percurso") (the term Event) |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | Car_ga de Controles e Pernadas<br>(corrects: Carre_gar Pernada e Controle) | "Carregar" is the verb load, as of a file; the report gives the load of the controls and legs, "carga" (the term Load) |  |
| `MainFrame_aboutMenu_Text` | _About Purple Pen | S_obre o Purple Pen<br>(corrects: _Sobre o  Purple Pen) | two spaces stood after "o"; the access key "o" is a letter that no other item of the Help menu uses ("s" is the Support) |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_duplicateCourseMenu_Text` | D_uplicate Course... | _Duplicare traseu... | item of the Course menu of the main window, which opens the dialog that copies the current course; the term Duplicate Course; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | _Ordinea traseelor... | item of the Course menu of the main window, which opens the dialog of the order of the courses; the term Course Order; its access key is a letter that no other item of its menu uses, in the small letters of the language's new items | UIText.ChangeCourseOrder_Text |
| `MainFrame_courseVariationReportMenu_Text` | Relay Team _Variations... | _Variante pentru echipele de ștafetă... | item of the Course menu of the main window, which opens the report of the variations of the relay teams; the term Relay Team Variations; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_deleteForkMenu_Text` | Delete For_k/Loop | Ștergere _bifurcație/buclă | item of the Item menu of the main window, which deletes the fork or loop at the selected control; the term Delete Fork/Loop; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_removeGapMenu_Text` | Remove Ga_p | Ștergere s_pațiu gol | item of the Item menu of the main window, which removes a gap from the selected control circle or line; the term Remove Gap; its access key is a letter that no other item of its menu uses, in small letters | UIText.CommandNameText_RemoveGap CommandNameText.RemoveGap |
| `MainFrame_changeLineAppearanceMenu_Text` | Change Line _Appearance... | Schimbare _aspect linie... | item of the Item menu of the main window, which opens the dialog of the colour, width and style of the selected line or shape; the term Change Line Appearance; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_stretchMenu_Text` | _Stretch | Î_ntindere | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses: "n", a letter of every keyboard |  |
| `MainFrame_donateWebSiteMenu_Text` | Make a _Donation | _Fă o donație | item of the Help menu of the main window, which opens the page for donations; the term Make a donation; its access key is a letter that no other item of its menu uses | UIText.InitialScreen_donationLink_Text |
| `MainFrame_addCourseMenu_Text` | _Add Course... | _Adăugare traseu...<br>(corrects: _Adaugare Traseu...) | the breve of "Adăugare" was missing (the term Add Course), written in the small letters of the language's new items |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | _Încărcare Post de control și Secțiune<br>(corrects: _Încărcare Post de control si Secțiune) | the comma of "și" was missing |  |
| `MainFrame_helpTranslatedMenu_Text` | Purple Pen Help (Translated) | Asistență Purple Pen (Tradusă)<br>(corrects: Asistență Purple Pen (Tradus)) | "asistență" is feminine, and the participle agrees with it |  |
| `MainFrame_symbolBrowserMenu_Text` | Symbol Browser | Răsfoire Simboluri<br>(corrects: Fruzărire Simboluri) | the typo "Fruzărire" (the term Symbol Browser) |  |
| `MainFrame_descriptionBrowserMenu_Text` | Description Browser | Răsfoire Descrieri<br>(corrects: Fruzărire Descrieri) | the typo "Fruzărire", as the Symbol Browser |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addCourseMenu_Text` | _Add Course... | _Добавить дистанцию... | item of the Course menu of the main window, which opens the dialog that adds a course; the term Add Course; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_deleteCourseMenu_Text` | _Delete Course | _Удалить дистанцию | item of the Course menu of the main window, which deletes the current course; the term Delete Course; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_duplicateCourseMenu_Text` | D_uplicate Course... | Ду_блировать дистанцию... | item of the Course menu of the main window, which opens the dialog that copies the current course; the term Duplicate Course; its access key is a letter that no other item of its menu uses: "б", as "д" is the Add Course |  |
| `MainFrame_propertiesMenu_Text` | _Properties... | _Свойства... | item of the Course menu of the main window, which opens the properties of the current course; the term Properties; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | _Порядок дистанций... | item of the Course menu of the main window, which opens the dialog of the order of the courses; the term Course Order; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_courseLoadMenu_Text` | Competitor _Load... | _Число участников... | item of the Course menu of the main window, which opens the dialog of the expected number of competitors on each course; the term Competitor Load; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_courseVariationReportMenu_Text` | Relay Team _Variations... | _Варианты рассеивания эстафетных команд... | item of the Course menu of the main window, which opens the report of the variations of the relay teams; the term Relay Team Variations; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_courseMenu_Text` | _Course | Д_истанция | menu Course of the menu bar of the main window; the term Course; its access key is a letter that no other menu of the menu bar uses: "и", as "д" is the Add menu, "_Добавить" |  |
| `MainFrame_deleteForkMenu_Text` | Delete For_k/Loop | Удалить развил_ку/петлю | item of the Item menu of the main window, which deletes the fork or loop at the selected control; the term Delete Fork/Loop; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_addBendMenu_Text` | Add _Bend | Добавить _излом | item of the Item menu of the main window, which adds a bend to the selected leg or line; the term Add Bend; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_removeBendMenu_Text` | Remove B_end | Удалить из_лом | item of the Item menu of the main window, which removes a bend from the selected leg or line; the term Remove Bend; its access key is a letter that no other item of its menu uses: "л", as "и" is the Add Bend |  |
| `MainFrame_addGapMenu_Text` | Add _Gap | Добавить _разрыв | item of the Item menu of the main window, which adds a gap to the selected control circle or line; the term Add Gap; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_removeGapMenu_Text` | Remove Ga_p | Удалить ра_зрыв | item of the Item menu of the main window, which removes a gap from the selected control circle or line; the term Remove Gap; its access key is a letter that no other item of its menu uses: "з", as "р" is the Add Gap |  |
| `MainFrame_changeTextMenu_Text` | Change _Text... | Изменить _текст... | item of the Item menu of the main window, which opens the dialog that changes the selected text; the term Change Text; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_changeLineAppearanceMenu_Text` | Change Line _Appearance... | Изменить _вид линии... | item of the Item menu of the main window, which opens the dialog of the colour, width and style of the selected line or shape; the term Change Line Appearance; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_rotateMenu_Text` | R_otate | П_овернуть | item of the Item menu of the main window, which rotates the selected object; the term Rotate; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_stretchMenu_Text` | _Stretch | Р_астянуть | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses: "а", as "р" is the Add Gap |  |
| `MainFrame_noFlaggingMenu_Text` | No Flagging | Без разметки | check item of the submenu Leg Flagging of the Item menu, without an access key as the English: how much of the selected leg is taped: none; the term No Flagging |  |
| `MainFrame_entireFlaggingMenu_Text` | Entire Leg Flagged | Весь перегон размечен | check item of the submenu Leg Flagging of the Item menu, without an access key as the English: how much of the selected leg is taped: all of it; the term Entire Leg Flagged |  |
| `MainFrame_beginFlaggingMenu_Text` | Beginning of Leg Flagged | Начало перегона размечено | check item of the submenu Leg Flagging of the Item menu, without an access key as the English: how much of the selected leg is taped: its beginning; the form of the language's Entire Leg Flagged |  |
| `MainFrame_endFlaggingMenu_Text` | End of Leg Flagged | Конец перегона размечен | check item of the submenu Leg Flagging of the Item menu, without an access key as the English: how much of the selected leg is taped: its end; the form of the language's Entire Leg Flagged |  |
| `MainFrame_legFlaggingMenu_Text` | _Leg Flagging | Разметка _перегона | item of the Item menu of the main window, a submenu of how much of the selected leg is taped; the term Leg Flagging; its access key is a letter that no other item of its menu uses: "п", as "р" is the Add Gap |  |
| `MainFrame_changeDisplayedCoursesMenu_Text` | Change Displa_yed Courses... | Изменить показываемые _дистанции... | item of the Item menu of the main window, which opens the dialog of the courses that show the selected special object; the term Change Displayed Courses; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_itemMenu_Text` | _Item | _Элемент | menu Item of the menu bar of the main window, which acts on the selected object; the term Item; its access key is a letter that no other menu of the menu bar uses |  |
| `MainFrame_courseSummaryMenu_Text` | Course _Summary | _Сводка по дистанциям | item of the Reports menu of the main window, whose text is also the title of the report: the length, climb and controls of each course; the term Course Summary; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_eventAuditMenu_Text` | Event _Audit | _Проверка соревнования | item of the Reports menu of the main window, whose text is also the title of the report: the problems that the program finds in the event; the term Event Audit; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_legLengthsMenu_Text` | _Leg Lengths | _Длины перегонов | item of the Reports menu of the main window, whose text is also the title of the report: the length of each leg of each course; the term Leg Lengths; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_controlCrossrefMenu_Text` | Control _Cross-reference | Перекрёстная _таблица КП | item of the Reports menu of the main window, whose text is also the title of the report: the courses that use each control; the term Control Cross-reference; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | _Нагрузка на КП и перегоны | item of the Reports menu of the main window, whose text is also the title of the report: the number of competitors who visit each control and run each leg; the terms Control and Leg load; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_reportMenu_Text` | _Reports | _Отчёты | menu Reports of the menu bar of the main window; the term Reports; its access key is a letter that no other menu of the menu bar uses |  |
| `MainFrame_helpContentsMenu_Text` | Purple Pen _Help | _Справка Purple Pen | item of the Help menu of the main window, which opens the help of Purple Pen; the term Purple Pen Help; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_helpTranslatedMenu_Text` | Purple Pen Help (Translated) | Справка Purple Pen (на русском) | item of the Help menu of the main window, which opens the help of Purple Pen in the language of the user, when one exists; without an access key as the English; the term Purple Pen Help; the item shows only where a help exists in the language, and names it, as fi "(suomi)" and nb-NO "(norsk)" |  |
| `MainFrame_mainWebSiteToolMenu_Text` | Purple Pen _Web Site | _Веб-сайт Purple Pen | item of the Help menu of the main window, which opens the web site of Purple Pen; the program name stays; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_supportWebSiteMenu_Text` | _Support/Bug Reporting | _Поддержка/Сообщить об ошибке | item of the Help menu of the main window, which opens the page of support and bug reports; the term Support/Bug Reporting; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_donateWebSiteMenu_Text` | Make a _Donation | С_делать пожертвование | item of the Help menu of the main window, which opens the page for donations; the term Make a donation; its access key is a letter that no other item of its menu uses: "д", as "с" is the Purple Pen Help and "п" the Support | UIText.InitialScreen_donationLink_Text |
| `MainFrame_aboutMenu_Text` | _About Purple Pen | _О Purple Pen | item of the Help menu of the main window, which opens the About dialog; the term About Purple Pen; its access key is a letter that no other item of its menu uses | UIText.AboutForm_Text |
| `MainFrame_symbolBrowserMenu_Text` | Symbol Browser | Обозреватель символов | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show; the term Symbol Browser |  |
| `MainFrame_descriptionBrowserMenu_Text` | Description Browser | Обозреватель легенд | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a browser of the description symbols; as the language's Symbol Browser |  |
| `MainFrame_controlTesterMenu_Text` | Control Tester | Тестер КП | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a tester of the drawing of controls; the term Control |  |
| `MainFrame_mapTesterMenu_Text` | Map Tester | Тестер карты | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a tester of the drawing of maps |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_duplicateCourseMenu_Text` | D_uplicate Course... | _Podvoji progo... | item of the Course menu of the main window, which opens the dialog that copies the current course; the term Duplicate Course; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_courseOrderMenu_Text` | Course _Order... | _Vrstni red prog... | item of the Course menu of the main window, which opens the dialog of the order of the courses; the term Course Order; its access key is a letter that no other item of its menu uses | UIText.ChangeCourseOrder_Text |
| `MainFrame_courseVariationReportMenu_Text` | Relay Team _Variations... | V_ariante štafetnih ekip... | item of the Course menu of the main window, which opens the report of the variations of the relay teams; the term Relay Team Variations; its access key is a letter that no other item of its menu uses: "a", as "v" is the Course Order, "_Vrstni red prog" |  |
| `MainFrame_deleteForkMenu_Text` | Delete For_k/Loop | Izbriši razcep/zan_ko | item of the Item menu of the main window, which deletes the fork or loop at the selected control; the term Delete Fork/Loop; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_removeGapMenu_Text` | Remove Ga_p | Izbriši raz_mak | item of the Item menu of the main window, which removes a gap from the selected control circle or line; the term Remove Gap; its access key is a letter that no other item of its menu uses | UIText.CommandNameText_RemoveGap CommandNameText.RemoveGap |
| `MainFrame_changeLineAppearanceMenu_Text` | Change Line _Appearance... | Spremeni videz _črte... | item of the Item menu of the main window, which opens the dialog of the colour, width and style of the selected line or shape; the term Change Line Appearance; its access key is a letter that no other item of its menu uses: "č", as "v" is the Remove Bend, "Izbriši za_voj" |  |
| `MainFrame_stretchMenu_Text` | _Stretch | Raz_tegni | item of the Item menu of the main window, which stretches the selected object; the term Stretch; its access key is a letter that no other item of its menu uses: "t", as "r" is the Add Gap |  |
| `MainFrame_donateWebSiteMenu_Text` | Make a _Donation | _Doniraj | item of the Help menu of the main window, which opens the page for donations; the term Make a donation; its access key is a letter that no other item of its menu uses | UIText.InitialScreen_donationLink_Text |
| `MainFrame_aboutMenu_Text` | _About Purple Pen | _Vizitka | item of the Help menu of the main window, which opens the About dialog; the term About Purple Pen; its access key is a letter that no other item of its menu uses | UIText.AboutForm_Text |
| `MainFrame_symbolBrowserMenu_Text` | Symbol Browser | Brskalnik simbolov | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show; the term Symbol Browser |  |
| `MainFrame_descriptionBrowserMenu_Text` | Description Browser | Brskalnik opisov | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a browser of the description symbols; as the language's Symbol Browser |  |
| `MainFrame_controlTesterMenu_Text` | Control Tester | Preizkus kontrolnih točk | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a tester of the drawing of controls; the term Control |  |
| `MainFrame_mapTesterMenu_Text` | Map Tester | Preizkus karte | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a tester of the drawing of maps |  |
| `MainFrame_courseLoadMenu_Text` | Competitor _Load... | Število _tekmovalcev...<br>(corrects: Tekmovalec _Obremenitev...) | "Tekmovalec Obremenitev" is the English put word for word (the term Competitor Load); the access key "t" is a letter that no other item of the Course menu uses |  |
| `MainFrame_removeBendMenu_Text` | Remove B_end | Izbriši za_voj<br>(corrects: Izbriši Z_avoj) | "zavoj" takes no capital in Slovenian (the term Remove Bend); the access key "v" is a letter that no other item of the Item menu uses ("a" is the Rotate) |  |
| `MainFrame_addGapMenu_Text` | Add _Gap | Dodaj _razmak<br>(corrects: Dodaj _Razmak) | "razmak" takes no capital in Slovenian (the term Add Gap) |  |
| `MainFrame_changeTextMenu_Text` | Change _Text... | Spremeni _besedilo...<br>(corrects: Izbriši _Besedilo...) | "Izbriši Besedilo" is Delete Text; the item changes the text (the term Change Text) |  |
| `MainFrame_changeDisplayedCoursesMenu_Text` | Change Displa_yed Courses... | Spremeni prikazane _proge...<br>(corrects: Spremeni označ_eno progo...) | "označeno progo" is the marked course, in the singular; the item chooses the courses that show the selected object (the term Change Displayed Courses), as the dialog that it opens; the access key "p" is a letter that no other item of the Item menu uses | UIText.ChangeSpecialCourses_Text |
| `MainFrame_courseSummaryMenu_Text` | Course _Summary | _Povzetek prog<br>(corrects: Proga _Povzetek) | "Proga Povzetek" is the English put word for word (the term Course Summary) |  |
| `MainFrame_eventAuditMenu_Text` | Event _Audit | P_regled prireditve<br>(corrects: Prireditev _Pregled) | "Prireditev Pregled" is the English put word for word (the term Event Audit); the access key "r" is a letter that no other item of the Reports menu uses ("p" is the Course Summary) |  |
| `MainFrame_controlCrossrefMenu_Text` | Control _Cross-reference | _Navzkrižne reference kontrolnih točk<br>(corrects: Kontrolna točka _Navzkrižna referenca) | "Kontrolna točka Navzkrižna referenca" is the English put word for word (the term Control Cross-reference) |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | _Obremenitev kontrolnih točk in etap<br>(corrects: Kontrolna točka in obremenitev et_ap) | "Kontrolna točka in obremenitev etap" is not grammatical; the report gives the load of the controls and of the legs (the terms Load and Leg load); the access key "o" is a letter that no other item of the Reports menu uses |  |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_courseLoadMenu_Text` | Competitor _Load... | Yarışmacı _Yükü...<br>(corrects: Rakip _Yükle...) | "Rakip Yükle" means load a rival; the load of competitors is "Yarışmacı Yükü" (the term Competitor Load) |  |
| `MainFrame_courseVariationReportMenu_Text` | Relay Team _Variations... | Bayrak Takımı _Varyasyonları...<br>(corrects: Röle Takımı _Varyasyonlar...) | "Röle" is an electrical relay, and the compound needs its suffix; a relay race is a "bayrak yarışı" (the term Relay Team Variations) |  |
| `MainFrame_addBendMenu_Text` | Add _Bend | _Bükme Ekle<br>(corrects: Ekle_Kıvrım) | "Ekle_Kıvrım" had no space, and the words of the English order; as the language's Remove Bend "Bükme Sil" (the term Add Bend) |  |
| `MainFrame_addGapMenu_Text` | Add _Gap | B_oşluk Ekle<br>(corrects: Ekle_Açıklık) | "Ekle_Açıklık" had no space, and the words of the English order (the term Add Gap); the access key "o" is a letter that no other item of the Item menu uses ("b" is the Add Bend) |  |
| `MainFrame_changeTextMenu_Text` | Change _Text... | _Metni Değiştir...<br>(corrects: Değiştir _Metin...) | "Değiştir Metin" has the words of the English order, without the case of the object (the term Change Text) |  |
| `MainFrame_changeLineAppearanceMenu_Text` | Change Line _Appearance... | Çizgi _Görünümünü Değiştir...<br>(corrects: Satır _Görünümünü Değiştir...) | "satır" is a line of text; a drawn line is "çizgi" (the term Line (drawn)); terms.csv is corrected with the group |  |
| `MainFrame_entireFlaggingMenu_Text` | Entire Leg Flagged | Tüm Ayak İşaretlendi<br>(corrects: Tüm Bacak İşaretlendi) | "bacak" is a leg of the body; the language's leg is "ayak" (the term Entire Leg Flagged) |  |
| `MainFrame_beginFlaggingMenu_Text` | Beginning of Leg Flagged | Ayak Başlangıcı İşaretlendi<br>(corrects: Bacak Bayraklı Başlangıcı) | "bacak" is a leg of the body, and "Bayraklı Başlangıcı" a beginning with flags; as the language's "Tüm Ayak İşaretlendi" |  |
| `MainFrame_endFlaggingMenu_Text` | End of Leg Flagged | Ayak Sonu İşaretlendi<br>(corrects: Bacak Sonu İşaretlendi) | "bacak" is a leg of the body; the language's leg is "ayak", as its "Tüm Ayak İşaretlendi" |  |
| `MainFrame_legFlaggingMenu_Text` | _Leg Flagging | _Ayak İşaretleme<br>(corrects: _Bacak İşaretleme) | "bacak" is a leg of the body; the language's leg is "ayak" (the term Leg Flagging) |  |
| `MainFrame_courseSummaryMenu_Text` | Course _Summary | Parkur _Özeti<br>(corrects: Parkur _Özet) | the compound needs its suffix (the term Course Summary) |  |
| `MainFrame_eventAuditMenu_Text` | Event _Audit | Etkinlik _Denetimi<br>(corrects: Etkinlik _Denetim) | the compound needs its suffix (the term Event Audit) |  |
| `MainFrame_legLengthsMenu_Text` | _Leg Lengths | _Ayak Uzunlukları<br>(corrects: _Bacak Uzunlukları) | "bacak" is a leg of the body; the language's leg is "ayak" (the term Leg Lengths) |  |
| `MainFrame_controlCrossrefMenu_Text` | Control _Cross-reference | Kontrol _Çapraz Referansı<br>(corrects: Kontrol _Çapraz referans) | the compound needs its suffix, as "Parkur Özeti"; terms.csv is corrected with the group |  |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | Kontrol ve Ayak _Yükü<br>(corrects: Kontrol ve Bacak Yükleme) | "bacak" is a leg of the body, and "Yükleme" is the loading, as of a file; the load is "yük" (the terms Leg load and Load); the access key "y" is a letter that no other item of the Reports menu uses |  |
| `MainFrame_reportMenu_Text` | _Reports | _Raporlar<br>(corrects: _Rapor) | the menu holds several reports; "Rapor" is singular (the term Reports) |  |

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_controlTesterMenu_Text` | Control Tester | Тестер КП | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a tester of the drawing of controls; the term Control |  |
| `MainFrame_mapTesterMenu_Text` | Map Tester | Тестер карти | item of the hidden Debug menu of the WinForms application, which the Avalonia application does not show: a tester of the drawing of maps |  |
| `MainFrame_entireFlaggingMenu_Text` | Entire Leg Flagged | Весь перегін маркований<br>(corrects: Весь перегон маркований) | the Ukrainian nominative is "перегін"; "перегон" is Russian (the term Entire Leg Flagged) |  |
| `MainFrame_legLengthsMenu_Text` | _Leg Lengths | _Довжини перегонів<br>(corrects: _Довжити перегонів) | the typo "Довжити" (the term Leg Lengths) |  |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_legFlaggingMenu_Text` | _Leg Flagging | 路段标记 | item of the Item menu of the main window, a submenu of how much of the selected leg is taped; the term Leg Flagging; without an access key, as no item of the language's Item menu has one | loc/po/purplepen/zh_CN.po |
| `MainFrame_courseLoadMenu_Text` | Competitor _Load... | 参赛者通过量…<br>(corrects: 竞争对手和负载…) | 竞争对手 is a rival; the language's command says 参赛者通过量 (the term Competitor Load) |  |
| `MainFrame_deleteForkMenu_Text` | Delete For_k/Loop | 删除分支/循环<br>(corrects: 删除 分支/循环) | a space stood between the words (the term Delete Fork/Loop) |  |
| `MainFrame_legLengthsMenu_Text` | _Leg Lengths | 路段长度<br>(corrects: 课程的长度) | 课程 is a course of study; a leg is 路段 (the term Leg Lengths) |  |

#### zh-TW

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_controlAndLegLoadMenu_Text` | Control and Leg Loa_d | 檢查點與賽段負荷量<br>(corrects: 檢查點與賽段載入) | 載入 is the loading of a file; the load of competitors is 負荷量, as the language's "選手負荷量" (the term Load) |  |
