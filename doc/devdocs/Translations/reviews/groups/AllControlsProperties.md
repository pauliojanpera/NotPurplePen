# Key group AllControlsProperties

Nov 16, 2026. The dialog that sets how the view All Controls is printed: its title, its text, the printing scale and
the appearance of the descriptions. claude-opus-5-5 at effort max made every line below, and each entry says so in
its comment.

- **Translated**: 3 lines: the title and the text in ru, and the item "Symbols and text" in da.
- **Not needed**: none.
- **Corrected**: 22 lines.
  - The same labels and items as in the New Course dialog (group AddCourse): the description appearance in cs, sl,
    sv and tr, the printing scale in da and ru, pl's text "Opis", it's full stop after an item, zh-CN's appearance
    界面.
  - The title and the text name the view All Controls after terms.csv: da "Alle poster", et "Kõigi KPde", it "di
    Tutti i punti", pl "wszystkich PK", sl "vseh KT". tr's title had a wrong genitive, "Noktaların".
  - et's text said that the descriptions are printed onto the map; sl's had the typo "kart" and lacked the
    descriptions.
  - nn-NO's title and text were Bokmål ("Innstillinger", "avgjør", "skrives ut").
  - terms.csv now gives the corrected tr and nn-NO titles.
- **Left empty**: none.

Doubts: es calls the view All Controls "Mapa maestro", and pt and pt-BR "Matriz de Pontos de Controlo/Controle". These
are their terms in terms.csv, so they stay.

#### cs

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_descriptionAppearanceLabel_Text` | Description appearance: | Zobrazení popisu:<br>(corrects: Popis zobrazení:) | "Popis zobrazení" means the description of a display; the term Description appearance, as in AddCourse |  |

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_descKindCombo_Items2` | Symbols and text | Symboler og tekst | third item of the combo box Description appearance, as in the New Course dialog; the term Symbols and text | UIText.AddCourse_descKindCombo_Items2 |
| `AllControlsProperties_printingScaleLabel_Text` | Map printing scale: | Kortets udskriftsskala:<br>(corrects: Kort udskrivnings scala:) | the compounds were split and "scala" has a typo; the term Map printing scale |  |
| `AllControlsProperties_allControlsPropertiesLabel_Text` | These settings control how the map and descriptions for All Controls are printed. | Disse indstillinger bestemmer, hvordan kortet og beskrivelserne for Alle poster udskrives.<br>(corrects: Disse indstillinger kontrollerer hvordan kortet og beskrivelserne for alle kontroller er udskrevet.) | the view is "Alle poster": Danish orienteering calls a control "post" (the term All Controls); "kontrollerer" (checks) and "er udskrevet" become "bestemmer" and "udskrives" |  |
| `AllControlsProperties_Text` | All Controls Properties | Egenskaber for Alle poster<br>(corrects: Alle kontrollers egenskaber) | Danish orienteering calls controls "poster"; the view is "Alle poster" (the term All Controls Properties) |  |

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_Text` | All Controls Properties | Kõigi KPde seaded<br>(corrects: Kõik punktid seaded) | "Kõik punktid seaded" is not grammatical, and "punkt" is a point (the term All Controls Properties) |  |
| `AllControlsProperties_allControlsPropertiesLabel_Text` | These settings control how the map and descriptions for All Controls are printed. | Need seaded määravad, kuidas trükitakse kõigi KPde kaart ja legendid.<br>(corrects: Allolevad seaded määravad kuidas trükitakse kaardile legendid kõikide kontrollpunktide korral.) | the translation said that the descriptions are printed onto the map; the settings are for printing the map and the descriptions (the term All Controls Properties) |  |

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_descKindCombo_Items2` | Symbols and text | Simboli e testo<br>(corrects: Simboli e testo.) | the full stop does not belong to an item of a combo box |  |
| `AllControlsProperties_Text` | All Controls Properties | Proprietà di Tutti i punti<br>(corrects: Proprietà della tutti i punti) | "della tutti" is not grammatical (the term All Controls Properties) |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_Text` | All Controls Properties | Innstillingar for alle poster<br>(corrects: Innstillinger alle poster) | "Innstillinger" is the Bokmål plural; Nynorsk "innstillingar" (terms.csv now gives it) |  |
| `AllControlsProperties_allControlsPropertiesLabel_Text` | These settings control how the map and descriptions for All Controls are printed. | Desse innstillingane avgjer korleis kartet og postbeskrivelsen for alle poster blir skrivne ut.<br>(corrects: Desse innstillingene avgjør korleis kartet og postbeskrivelsen for alle poster skrives ut.) | the Bokmål forms "innstillingene", "avgjør" and "skrives ut"; Nynorsk "innstillingane", "avgjer", "blir skrivne ut" |  |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_descKindCombo_Items1` | Text | Tekst<br>(corrects: Opis) | "Opis" is a description; the item is the text of the descriptions (the term Text) |  |
| `AllControlsProperties_descKindCombo_Items2` | Symbols and text | Piktografiki i tekst<br>(corrects: Piktografiki i opis) | "opis" is a description; the term Text, as the item before |  |
| `AllControlsProperties_Text` | All Controls Properties | Ustawienia wszystkich PK<br>(corrects: Ustawienia Wszystkich Punktów) | the language's control is "PK", as its view "Wszystkie PK" (the term All Controls Properties) |  |
| `AllControlsProperties_allControlsPropertiesLabel_Text` | These settings control how the map and descriptions for All Controls are printed. | Te ustawienia mają wpływ na to, w jaki sposób drukowane są mapa oraz opisy dla Wszystkich PK.<br>(corrects: Te ustawienia mają wpływ na to w jaki sposób mapa oraz opisy Wszystkich Punktów są drukowane.) | the language's control is "PK", as its view "Wszystkie PK"; "punkty" are also the score points |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_allControlsPropertiesLabel_Text` | These settings control how the map and descriptions for All Controls are printed. | Эти настройки определяют, как печатаются карта и легенды для «Все КП». | the text at the top of the dialog that sets the printing of the view All Controls; the terms All Controls (the view) and Descriptions | uk |
| `AllControlsProperties_Text` | All Controls Properties | Свойства «Все КП» | title of the dialog that sets the printing of the view All Controls; the term All Controls Properties | uk |
| `AllControlsProperties_printingScaleLabel_Text` | Map printing scale: | Масштаб печати карты:<br>(corrects: Маштаб карты для печати) | the typo "Маштаб"; the term Map printing scale, with the colon of a label |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_descriptionAppearanceLabel_Text` | Description appearance: | Videz opisa:<br>(corrects: predogled opisa) | "predogled" is a preview; the term Description appearance, as in AddCourse |  |
| `AllControlsProperties_Text` | All Controls Properties | Lastnosti vseh KT<br>(corrects: Lastnosti Vse KT) | "Vse" is the nominative; the properties are those "of all controls" (the term All Controls Properties) |  |
| `AllControlsProperties_allControlsPropertiesLabel_Text` | These settings control how the map and descriptions for All Controls are printed. | Te nastavitve določajo, kako bosta natisnjena karta in opis za vse KT.<br>(corrects: Te nastavitve določajo kako bo natisnjena kart z vsemi KT.) | the typo "kart", and the descriptions were missing |  |

#### sv

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_descriptionAppearanceLabel_Text` | Description appearance: | Kontrollbeskrivningens utseende:<br>(corrects: Definitionens utseende:) | "definition" is the language's rarer word; its descriptions are "kontrollbeskrivningar" (the term Description appearance) |  |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_descriptionAppearanceLabel_Text` | Description appearance: | Açıklama görünümü:<br>(corrects: Görüntü açıklaması:) | "Görüntü açıklaması" means the description of an image; the term Description appearance |  |
| `AllControlsProperties_Text` | All Controls Properties | Tüm Kontrol Noktalarının Özellikleri<br>(corrects: Tüm Kontrol Noktaların  Özellikleri) | the genitive of "kontrol noktaları" is "noktalarının"; a double space (terms.csv now gives it) |  |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `AllControlsProperties_appearanceGroupBox_Text` | Appearance | 外观<br>(corrects: 界面) | 界面 is the user interface; the appearance is 外观, as zh-TW 外觀 (the term Appearance) |  |
