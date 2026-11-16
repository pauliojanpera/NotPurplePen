# Key group MainFrame.3

Nov 16, 2026. The third part of the main window: the rest of the Add menu, from the dangerous area to the ellipse
(the special items of the ISOM and the drawn objects), with the same items in the toolbar's Special Item list, the Add
menu itself, and the Event menu of the menu bar with its submenu IOF Standards. claude-opus-5-5 at effort max made
every line below, and each entry says so in its comment.

- **Translated**: 205 lines, in 22 languages. ru and nl lacked almost the whole group. sl, nn-NO and ro lacked the
  drawn objects (image, line, rectangle, ellipse), the Add menu and the newer items of the Event menu. 21 languages
  lacked Temporary Construction, the special item of the ISSprOM for a temporary construction or a closed area. 16
  languages lacked Move All Controls, and 11 the ISSprOM 2019 map standard, which takes the form of the language's
  other map standards. Each new item of a menu has an access key that no other item of its menu uses. ru's Event menu
  is "Со_ревнование": "с" stays free for its Help menu, "Справка". nl names the items after the dialogs that they
  open ("Wijzig codes posten", "Automatische nummering", "Wijzig tekstomschrijving"). pt and pt-BR write the new items
  with the capitals of the items around them.
- **Not needed**: none.
- **Corrected**: 75 lines, in 17 languages, most of them after terms.csv.
  - The special items. zh-CN's dangerous area was a section of a road (路段), its forbidden route marking a signpost
    (路标), its registration mark the registration of a user (注册), its white-out area an outer area (外部), and its
    line a straight line (直线). zh-TW's line was a row of text (列). da's water and first aid locations were "Vand
    lokation" and "Førstehjælps lokation", and its forbidden route marking was a forbidden area. hu's water location
    was a wet place ("Vizes hely"), and the Italian one was the position of the water. fr's forbidden route was a road
    ("Route interdite"). pt-BR's and ro's forbidden route markings were a forbidden area and a forbidden marking, and
    ro's uncrossable boundary was an area. tr's white-out area was an outer area ("Dış"), and et's image was image
    files. Typos: en-GB "First Aid Posy", et "Esmaaabi", ro "Macaj".
  - The Event menu. de's Move All Controls moved the controls of a user interface ("Bedienelemente"). IOF Standards
    was English in cs, da, the Italian and pt. da's controls were "kontroller", and its event a "hændelse" (an
    incident). uk's event was an occurrence ("Подія"), and its punch patterns were in the singular. The description
    standards had typos in pt-BR (2014), tr ("Satandardı") and hu ("lerírás"). sl's Change Control Codes was not
    grammatical.
  - Capitals and compounds. da, nb-NO and sl wrote common nouns inside an item with a capital ("Tilpas _Udseende",
    "Flytt alle Poster", "Avtomatsko _Številčenje"). da split its compounds ("beskrivelses standard", "kort
    Standard"). sl's registration mark began with a small letter.
  - Marks. zh-CN's Add menu had an ellipsis, which a menu of the menu bar does not take. Its Punch Patterns had the
    access key on a Chinese character, which no key can type.
- **Left empty**: none.
- **terms.csv**: eight cells were corrected with the group, and terms-decisions.tsv records each: da's four standards,
  en-GB's First Aid Location ("First Aid Post"), nb-NO's Move All Controls, and ro's and sl's Registration Mark.

Doubts:

- The Italian keeps "Controllo" and "Controlli" for a control in Change Control Codes and Remove Unused Controls,
  against terms.csv ("Punto"). The language uses "controllo" about twice as often as "punto", and MainFrame.2 kept it
  too.
- de's IOF Standards stays "IOF Standards", a correct German plural. The language's label that names the menu says
  "Wettkampf/IOF Normen", the term of terms.csv.
- Some existing special items stay, as they are not certainly wrong: cs's forbidden route marking "Zakázané křížení"
  (a forbidden crossing), pl's registration mark "Znak wewnętrzny" (an inner mark), ja's 内トンボ (inner trim marks),
  zh-TW's ellipse 橢圓模式 (ellipse mode), the boundary line "Upasserbar/forbudt krysning" of nb-NO and nn-NO, and
  the Italian description standards in the order of the English ("2004 Descrizione Standard").
- The copies of the special items in SelectionDescriptionText (`SpecialName_*`) keep the old texts that this group
  corrected. The groups of SelectionDescriptionText correct them.
- zh-TW's Move All Controls has no access key, as no item of the language's Event menu has one. check.py warns of it.
- Several menus had an access key twice before this group (the Event menu of cs, es, et, hu, nb-NO, ro and uk, and
  several menu bars). The group leaves those of the lines that it does not change, and adds no new one.
#### cs

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Dočasná stavba | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Dočasná stavba | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | _Přesunout všechny kontroly... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | IOF _Standardy<br>(corrects: IOF _Standards) | "Standards" is the English; the language's own label that names this menu says "Závod/IOF Standardy" (the term IOF Standards) | UIText.NewEventStandards_labelTitle_Text UIText.NewEventStandards_changeLaterLabel_Text |

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Midlertidig konstruktion | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Midlertidig konstruktion | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | Ellipse | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | Ellipse | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | _Flyt alle poster... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | ISSprOM 2019 kortstandard | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items, as they are corrected with this group ("ISOM2017 kortstandard") |  |
| `MainFrame_addWaterMenu_Text` | Water Location | Væskepost<br>(corrects: Vand lokation) | "Vand lokation" is no Danish term; the refreshment point of a course is a "væskepost" (the term Water Location) |  |
| `MainFrame_waterLocationToolStripMenuItem_Text` | Water Location | Væskepost<br>(corrects: Vand lokation) | "Vand lokation" is no Danish term; the refreshment point of a course is a "væskepost" (the term Water Location) |  |
| `MainFrame_addFirstAidMenu_Text` | First Aid Location | Førstehjælpssted<br>(corrects: Førstehjælps lokation) | "lokation" is no Danish word for a place, and the compound was split (the term First Aid Location) |  |
| `MainFrame_firstAidLocationToolStripMenuItem_Text` | First Aid Location | Førstehjælpssted<br>(corrects: Førstehjælps lokation) | "lokation" is no Danish word for a place, and the compound was split (the term First Aid Location) |  |
| `MainFrame_addForbiddenMenu_Text` | Forbidden Route Marking | Markering af forbudt rute<br>(corrects: Forbudt område markering) | "Forbudt område" is a forbidden area; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_forbiddenRouteMarkingToolStripMenuItem_Text` | Forbidden Route Marking | Markering af forbudt rute<br>(corrects: Forbudt område markering) | "Forbudt område" is a forbidden area; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_addRegMarkMenu_Text` | Registration Mark | Registreringsmærke<br>(corrects: Registrerings mærke) | the compound is one word in Danish (the term Registration Mark) |  |
| `MainFrame_registrationMarkToolStripMenuItem_Text` | Registration Mark | Registreringsmærke<br>(corrects: Registrerings mærke) | the compound is one word in Danish (the term Registration Mark) |  |
| `MainFrame_whiteOutMenu_Text` | White Out Area | Hvidt område<br>(corrects: Hvidt ud område) | "Hvidt ud område" is not grammatical (the term White Out Area) |  |
| `MainFrame_whiteOutToolStripMenuItem_Text` | White Out Area | Hvidt område<br>(corrects: Hvidt ud område) | "Hvidt ud område" is not grammatical (the term White Out Area) |  |
| `MainFrame_changeCodesMenu_Text` | Change Control _Codes... | Ændr _postkoder...<br>(corrects: Ændr kontrol _koder...) | Danish orienteering calls a control "post" ("kontrol" is a check), and "kontrolkoder" is one word (the term Change Control Codes); the access key "p" is a letter that no other item of the Event menu uses ("k" is the Map File) |  |
| `MainFrame_removeUnusedControlsMenu_Text` | Remove _Unused Controls... | Fjern _ubrugte poster...<br>(corrects: Fjern _Ubrugte kontroller...) | Danish orienteering calls controls "poster" (the term Remove Unused Controls), and "ubrugte" takes no capital |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | IOF-_standarder<br>(corrects: IOF _Standards) | "Standards" is the English; the plural of the language is "standarder", joined to the abbreviation by a hyphen (the term IOF Standards) |  |
| `MainFrame_customizeDescriptionsMenu_Text` | Customize _Description Text... | Tilpas _beskrivelsestekst...<br>(corrects: Tilpas _Beskrivelses tekst...) | the compound was split, and "beskrivelsestekst" takes no capital (the term Customize Description Text) |  |
| `MainFrame_customizeCourseAppearanceMenu_Text` | Customize _Appearance... | Tilpas u_dseende...<br>(corrects: Tilpas _Udseende...) | "udseende" takes no capital in Danish (the term Customize Appearance), as the dialog that the item opens; the access key "d" is a letter that no other item of the Event menu uses ("u" is the Remove Unused Controls) | UIText.CourseAppearanceDialog_Text |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | 2004 beskrivelsesstandard<br>(corrects: 2004 beskrivelses standard) | the compound was split; "beskrivelsesstandard" is one word, as nb-NO; terms.csv is corrected with the group |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | 2018/2024 beskrivelsesstandard<br>(corrects: 2018/2024 beskrivelses standard) | the compound was split; "beskrivelsesstandard" is one word, as nb-NO; terms.csv is corrected with the group |  |
| `MainFrame_mapStd2000Menu_Text` | ISOM2000 Map Standard | ISOM2000 kortstandard<br>(corrects: ISOM2000 kort Standard) | the compound was split, with a capital; "kortstandard" is one word, as the nb-NO "kartstandard"; terms.csv is corrected with the group |  |
| `MainFrame_mapStd2017Menu_Text` | ISOM2017 Map Standard | ISOM2017 kortstandard<br>(corrects: ISOM2017 Kort Standard) | the compound was split, with a capital; "kortstandard" is one word, as the nb-NO "kartstandard"; terms.csv is corrected with the group |  |
| `MainFrame_eventMenu_Text` | Eve_nt | _Løb<br>(corrects: Hæ_ndelse) | "hændelse" is an incident; an orienteering event is a "løb" (the term Event), as the language's New Event, "Nyt løb"; the access key "l" is a letter that no other menu of the menu bar uses | UIText.MainFrame_newEventMenu_Text |

#### de

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Temporäre Konstruktion | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Temporäre Konstruktion | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | Alle Posten _verschieben...<br>(corrects: Alle Bedienelemente verschieben...) | "Bedienelemente" are the controls of a user interface; the item moves the controls of the courses, "Posten" (the term Move All Controls); the access key "v" is a letter that no other item of the Event menu uses |  |

#### en-GB

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addFirstAidMenu_Text` | First Aid Location | First Aid Post<br>(corrects: First Aid Posy) | the typo "Posy"; the language names the special items by the IOF ("Refreshment Point"), and the IOF name is "First aid post"; terms.csv is corrected with the group |  |
| `MainFrame_firstAidLocationToolStripMenuItem_Text` | First Aid Location | First Aid Post<br>(corrects: First Aid Posy) | the typo "Posy"; the language names the special items by the IOF ("Refreshment Point"), and the IOF name is "First aid post"; terms.csv is corrected with the group |  |

#### es

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Construcción temporal | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Construcción temporal | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | Elipse | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | Elipse | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | Mo_ver todos los controles... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | Estándar de mapa ISSprOM 2019 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items | UIText.MainFrame_mapStd2017Menu_Text |

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Ajutine rajatis | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Ajutine rajatis | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | _Liiguta kõiki KPsid... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_addFirstAidMenu_Text` | First Aid Location | Esmaabi koht<br>(corrects: Esmaaabi koht) | the typo "Esmaaabi" (the term First Aid Location) |  |
| `MainFrame_firstAidLocationToolStripMenuItem_Text` | First Aid Location | Esmaabi koht<br>(corrects: Esmaaabi koht) | the typo "Esmaaabi" (the term First Aid Location) |  |
| `MainFrame_addImageMenu_Text` | Image | Pilt<br>(corrects: Pildi failid) | "Pildi failid" are image files; the item places one image (the term Image) |  |
| `MainFrame_imageToolStripMenuItem_Text` | Image | Pilt<br>(corrects: Pildi failid) | "Pildi failid" are image files; the item places one image (the term Image) |  |

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Tilapäinen rakennelma | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Tilapäinen rakennelma | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |

#### fr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | Norme de définitions 2004 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2004 standard of the IOF control descriptions; the term 2004 Description Standard |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | Norme de définitions 2018/2024 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2018 standard of the IOF control descriptions, as revised in 2024; the term 2018/2024 Description Standard |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | Norme ISSprOM 2019 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items | UIText.MainFrame_mapStd2017Menu_Text |
| `MainFrame_addForbiddenMenu_Text` | Forbidden Route Marking | Itinéraire interdit<br>(corrects: Route interdite) | a "route" is a road; the forbidden route of the ISOM is an "itinéraire interdit" (the term Forbidden Route Marking) |  |
| `MainFrame_forbiddenRouteMarkingToolStripMenuItem_Text` | Forbidden Route Marking | Itinéraire interdit<br>(corrects: Route interdite) | a "route" is a road; the forbidden route of the ISOM is an "itinéraire interdit" (the term Forbidden Route Marking) |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Ideiglenes építmény | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Ideiglenes építmény | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | Ellipszis | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | Ellipszis | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | Összes ellenőrzőpont _mozgatása... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | ISSprOM 2019 térkép jelkulcs | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items | UIText.MainFrame_mapStd2017Menu_Text |
| `MainFrame_addWaterMenu_Text` | Water Location | Frissítő pont<br>(corrects: Vizes hely) | "Vizes hely" is a wet place; the refreshment point is "Frissítő pont", as the IOF text of symbols.xml (the term Water Location) |  |
| `MainFrame_waterLocationToolStripMenuItem_Text` | Water Location | Frissítő pont<br>(corrects: Vizes hely) | "Vizes hely" is a wet place; the refreshment point is "Frissítő pont", as the IOF text of symbols.xml (the term Water Location) |  |
| `MainFrame_removeUnusedControlsMenu_Text` | Remove _Unused Controls... | _Nem használt ellenőrzőpontok törlése...<br>(corrects: _Nem használt ellenőrzöpontok törlése) | the typo "ellenőrzöpontok": the term Control is "ellenőrzőpont" (the term Remove Unused Controls); the item opens a dialog |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | 2018-as/2024-es leírás szabvány<br>(corrects: 2018-as/2024-as lerírás szabvány) | the typo "lerírás", and 2024 takes the suffix "-es", as "2004-es" (the term 2018/2024 Description Standard) |  |

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addWaterMenu_Text` | Water Location | Ristoro<br>(corrects: Posizione Dell'Acqua) | "Posizione Dell'Acqua" is the position of the water; the refreshment point is "Ristoro", as the IOF text of symbols.xml (the term Water Location) |  |
| `MainFrame_waterLocationToolStripMenuItem_Text` | Water Location | Ristoro<br>(corrects: Posizione Dell'Acqua) | "Posizione Dell'Acqua" is the position of the water; the refreshment point is "Ristoro", as the IOF text of symbols.xml (the term Water Location) |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | _Standard IOF<br>(corrects: IOF _Standards) | "Standards" is the English; the Italian plural of "standard" is "standard" (the term IOF Standards) |  |

#### ja

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | 仮設構造物 | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | 仮設構造物 | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | 楕円 | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | 楕円 | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | すべてのコントロールを移動(_V)... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | 2004年版ディスクリプション規格 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2004 standard of the IOF control descriptions; the term 2004 Description Standard |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | 2018/2024年版ディスクリプション規格 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2018 standard of the IOF control descriptions, as revised in 2024; the term 2018/2024 Description Standard |  |
| `MainFrame_mapStd2000Menu_Text` | ISOM2000 Map Standard | ISOM2000 地図規程 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2000; the term ISOM2000 Map Standard |  |
| `MainFrame_mapStd2017Menu_Text` | ISOM2017 Map Standard | ISOM2017 地図規程 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2017; the term ISOM2017 Map Standard |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | ISSprOM 2019 地図規程 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | IOF規格(_S) | item of the Event menu of the main window, a submenu of the IOF standards of the descriptions and of the map; the term IOF Standards; its access key is a letter that no other item of its menu uses |  |

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Midlertidig konstruksjon | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Midlertidig konstruksjon | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | _Flytt alle poster...<br>(corrects: Flytt alle Poster) | "poster" takes no capital in Norwegian (the term Move All Controls), and the item opens a dialog; the access key "f" is a letter that no other item of the Event menu uses; terms.csv is corrected with the group |  |
| `MainFrame_customizeCourseAppearanceMenu_Text` | Customize _Appearance... | Tilpass u_tseende...<br>(corrects: Tilpass _Utseende) | "utseende" takes no capital in Norwegian (the term Customize Appearance), as the dialog that the item opens; the access key "t" is a letter that no other item of the Event menu uses ("u" is the Remove Unused Controls) | UIText.CourseAppearanceDialog_Text |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addDangerousMenu_Text` | Dangerous Area | Gevaarlijk gebied | item of the Add menu of the main window, without an access key as the English: an area that is dangerous to the competitors (ISOM 710); the term Dangerous Area |  |
| `MainFrame_dangerousToolStripMenuItem_Text` | Dangerous Area | Gevaarlijk gebied | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an area that is dangerous to the competitors (ISOM 710); the term Dangerous Area |  |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Tijdelijke constructie | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Tijdelijke constructie | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addWaterMenu_Text` | Water Location | Drankpost | item of the Add menu of the main window, without an access key as the English: the refreshment point (ISOM 713); the term Water Location |  |
| `MainFrame_waterLocationToolStripMenuItem_Text` | Water Location | Drankpost | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the refreshment point (ISOM 713); the term Water Location |  |
| `MainFrame_addFirstAidMenu_Text` | First Aid Location | EHBO-post | item of the Add menu of the main window, without an access key as the English: the first aid post (ISOM 712); the term First Aid Location |  |
| `MainFrame_firstAidLocationToolStripMenuItem_Text` | First Aid Location | EHBO-post | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the first aid post (ISOM 712); the term First Aid Location |  |
| `MainFrame_addForbiddenMenu_Text` | Forbidden Route Marking | Markering verboden route | item of the Add menu of the main window, without an access key as the English: the crosses that mark a route the competitors must not use (ISOM 711); the term Forbidden Route Marking |  |
| `MainFrame_forbiddenRouteMarkingToolStripMenuItem_Text` | Forbidden Route Marking | Markering verboden route | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the crosses that mark a route the competitors must not use (ISOM 711); the term Forbidden Route Marking |  |
| `MainFrame_addBoundaryMenu_Text` | Uncrossable Boundary | Onpasseerbare grens | item of the Add menu of the main window, without an access key as the English: a line that the competitors must not cross (ISOM 707); the term Uncrossable Boundary |  |
| `MainFrame_boundaryToolStripMenuItem_Text` | Uncrossable Boundary | Onpasseerbare grens | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a line that the competitors must not cross (ISOM 707); the term Uncrossable Boundary |  |
| `MainFrame_addRegMarkMenu_Text` | Registration Mark | Paskruis | item of the Add menu of the main window, without an access key as the English: the cross that aligns the printed courses with a map printed before; the term Registration Mark |  |
| `MainFrame_registrationMarkToolStripMenuItem_Text` | Registration Mark | Paskruis | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the cross that aligns the printed courses with a map printed before; the term Registration Mark |  |
| `MainFrame_whiteOutMenu_Text` | White Out Area | Wit gebied | item of the Add menu of the main window, without an access key as the English: a white area that hides the map under it; the term White Out Area |  |
| `MainFrame_whiteOutToolStripMenuItem_Text` | White Out Area | Wit gebied | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a white area that hides the map under it; the term White Out Area |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | Ellips | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | Ellips | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_addMenu_Text` | _Add | _Toevoegen | menu Add of the menu bar of the main window; the term Add; its access key is a letter that no other menu of the menu bar uses ("t") |  |
| `MainFrame_changeMapFileMenu_Text` | _Map File... | _Kaartbestand... | item of the Event menu of the main window, which opens the dialog that changes the map file of the event; the term Map File; its access key is a letter that no other item of its menu uses | UIText.ChangeMapFile_Text UIText.NewEventMapFile_labelTitle_Text |
| `MainFrame_changeCodesMenu_Text` | Change Control _Codes... | Wijzig _codes posten... | item of the Event menu of the main window, which opens the dialog that changes the codes of the controls; the term Change Control Codes; its access key is a letter that no other item of its menu uses | UIText.ChangeAllCodes_Text |
| `MainFrame_autoNumberingMenu_Text` | Automatic _Numbering... | Automatische _nummering... | item of the Event menu of the main window, which opens the dialog that numbers the controls automatically; the term Automatic Numbering; its access key is a letter that no other item of its menu uses | UIText.AutoNumbering_Text UIText.CommandNameText_AutoNumbering |
| `MainFrame_removeUnusedControlsMenu_Text` | Remove _Unused Controls... | Verwijder _ongebruikte posten... | item of the Event menu of the main window, which opens the dialog that removes the controls that no course uses; the term Remove Unused Controls; its access key is a letter that no other item of its menu uses | UIText.CommandNameText_RemoveUnusedControls CommandNameText.RemoveUnusedControls |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | Alle posten _verplaatsen... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_punchPatternsMenu_Text` | _Punch Patterns... | _Prikpatronen... | item of the Event menu of the main window, which opens the dialog of the patterns of the pin punches; the term Punch Patterns; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | Postomschrijvingsnorm 2004 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2004 standard of the IOF control descriptions; the term 2004 Description Standard |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | Postomschrijvingsnorm 2018/2024 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2018 standard of the IOF control descriptions, as revised in 2024; the term 2018/2024 Description Standard |  |
| `MainFrame_mapStd2000Menu_Text` | ISOM2000 Map Standard | ISOM2000-kaartnorm | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2000; the term ISOM2000 Map Standard |  |
| `MainFrame_mapStd2017Menu_Text` | ISOM2017 Map Standard | ISOM2017-kaartnorm | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2017; the term ISOM2017 Map Standard |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | ISSprOM 2019-kaartnorm | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items; the hyphen joins the name of several words to "kaartnorm" |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | _IOF-normen | item of the Event menu of the main window, a submenu of the IOF standards of the descriptions and of the map; the term IOF Standards; its access key is a letter that no other item of its menu uses: "i", as "n" is the Automatic Numbering |  |
| `MainFrame_customizeDescriptionsMenu_Text` | Customize _Description Text... | Wijzig _tekstomschrijving... | item of the Event menu of the main window, which opens the dialog that changes the texts of the description symbols; the term Customize Description Text; its access key is a letter that no other item of its menu uses | UIText.CustomSymbolText_Text UIText.CommandNameText_SetCustomSymbolText |
| `MainFrame_customizeCourseAppearanceMenu_Text` | Customize _Appearance... | _Weergave aanpassen... | item of the Event menu of the main window, which opens the dialog of the appearance of the courses; the term Customize Appearance; its access key is a letter that no other item of its menu uses | UIText.CourseAppearanceDialog_Text $this.Text |
| `MainFrame_eventMenu_Text` | Eve_nt | _Evenement | menu Event of the menu bar of the main window; the term Event; its access key is a letter that no other menu of the menu bar uses ("e") |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Mellombels konstruksjon | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Mellombels konstruksjon | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addImageMenu_Text` | Image | Bilde | item of the Add menu of the main window, without an access key as the English: an image file placed on the map; the term Image |  |
| `MainFrame_imageToolStripMenuItem_Text` | Image | Bilde | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an image file placed on the map; the term Image |  |
| `MainFrame_addLineMenu_Text` | Line | Linje | item of the Add menu of the main window, without an access key as the English: a line drawn on the map; the term Line (drawn) |  |
| `MainFrame_lineToolStripMenuItem_Text` | Line | Linje | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a line drawn on the map; the term Line (drawn) |  |
| `MainFrame_addRectangleMenu_Text` | Rectangle | Rektangel | item of the Add menu of the main window, without an access key as the English: a rectangle drawn on the map; the term Rectangle |  |
| `MainFrame_rectangleToolStripMenuItem_Text` | Rectangle | Rektangel | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a rectangle drawn on the map; the term Rectangle |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | Ellipse | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | Ellipse | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_addMenu_Text` | _Add | Legg _til | menu Add of the menu bar of the main window; the term Add; its access key is a letter that no other menu of the menu bar uses: "t", as "l" is the Course menu, "_Løype" |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | _Flytt alle poster... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | 2004 beskrivelsesstandard | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2004 standard of the IOF control descriptions; the term 2004 Description Standard | nb-NO |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | 2018/2024 beskrivelsesstandard | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2018 standard of the IOF control descriptions, as revised in 2024; the term 2018/2024 Description Standard | nb-NO |
| `MainFrame_mapStd2000Menu_Text` | ISOM2000 Map Standard | ISOM2000 kartstandard | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2000; the term ISOM2000 Map Standard | nb-NO |
| `MainFrame_mapStd2017Menu_Text` | ISOM2017 Map Standard | ISOM2017 kartstandard | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2017; the term ISOM2017 Map Standard | nb-NO |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | ISSprOM 2019 kartstandard | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items ("ISOM2017 kartstandard", as nb-NO) |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | _IOF-standardar | item of the Event menu of the main window, a submenu of the IOF standards of the descriptions and of the map; the term IOF Standards; its access key is a letter that no other item of its menu uses: "i", as "s" is the Punch Patterns, "_Stemplingsmønster" |  |
| `MainFrame_customizeCourseAppearanceMenu_Text` | Customize _Appearance... | Tilpass u_tsjånad... | item of the Event menu of the main window, which opens the dialog of the appearance of the courses; the term Customize Appearance; its access key is a letter that no other item of its menu uses: "t", as "u" is the Remove Unused Controls, "Slett _ubrukte poster" |  |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Tymczasowa konstrukcja | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Tymczasowa konstrukcja | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | Elipsa | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | Elipsa | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_addMenu_Text` | _Add | _Dodaj | menu Add of the menu bar of the main window; the term Add; its access key is a letter that no other menu of the menu bar uses ("d") |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | Prze_suń wszystkie PK... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | Standard opisów 2004 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2004 standard of the IOF control descriptions; the term 2004 Description Standard |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | Standard opisów 2018/2024 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2018 standard of the IOF control descriptions, as revised in 2024; the term 2018/2024 Description Standard |  |
| `MainFrame_mapStd2000Menu_Text` | ISOM2000 Map Standard | Standard map ISOM2000 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2000; the term ISOM2000 Map Standard |  |
| `MainFrame_mapStd2017Menu_Text` | ISOM2017 Map Standard | Standard map ISOM2017 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2017; the term ISOM2017 Map Standard |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | Standard map ISSprOM 2019 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | Standardy _IOF | item of the Event menu of the main window, a submenu of the IOF standards of the descriptions and of the map; the term IOF Standards; its access key is a letter that no other item of its menu uses: "i", as "s" is the Move All Controls, "Prze_suń wszystkie PK..." |  |

#### pt

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Construção Temporária | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction, in the capitals of the neighbouring items of the language's Add menu ("Área Perigosa", "Área Branca") |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Construção Temporária | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction, in the capitals of the neighbouring items of the language's Add menu ("Área Perigosa", "Área Branca") |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | Mo_ver Todos os Pontos de Controlo... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses, in the capitals of the neighbouring items ("Remover Controlos não Utilizados...") |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | Padrõe_s IOF<br>(corrects: IOF _Standards) | "Standards" is the English (the term IOF Standards), as the language's title of the IOF Standards page of the New Event wizard; the access key of pt-BR | UIText.NewEventStandards_labelTitle_Text |

#### pt-BR

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Construção Temporária | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction, in the capitals of the neighbouring items of the language's Add menu ("Área Perigosa", "Área Branca") |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Construção Temporária | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction, in the capitals of the neighbouring items of the language's Add menu ("Área Perigosa", "Área Branca") |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | Mo_ver Todos os Controles... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses, in the capitals of the neighbouring items ("Remover Controles não Utilizados...") |  |
| `MainFrame_addForbiddenMenu_Text` | Forbidden Route Marking | Marcação de Rota Proibida<br>(corrects: Balizamento de Área Proibida) | "Área Proibida" is a forbidden area; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_forbiddenRouteMarkingToolStripMenuItem_Text` | Forbidden Route Marking | Marcação de Rota Proibida<br>(corrects: Balizamento de Área Proibida) | "Área Proibida" is a forbidden area; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | Padrão de Descrição 2004<br>(corrects: Padrão de Descrição 2014) | the year was 2014, a typo (the term 2004 Description Standard) |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Construcție temporară | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Construcție temporară | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addImageMenu_Text` | Image | Imagine | item of the Add menu of the main window, without an access key as the English: an image file placed on the map; the term Image |  |
| `MainFrame_imageToolStripMenuItem_Text` | Image | Imagine | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an image file placed on the map; the term Image |  |
| `MainFrame_addLineMenu_Text` | Line | Linie | item of the Add menu of the main window, without an access key as the English: a line drawn on the map; the term Line (drawn) |  |
| `MainFrame_lineToolStripMenuItem_Text` | Line | Linie | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a line drawn on the map; the term Line (drawn) |  |
| `MainFrame_addRectangleMenu_Text` | Rectangle | Dreptunghi | item of the Add menu of the main window, without an access key as the English: a rectangle drawn on the map; the term Rectangle |  |
| `MainFrame_rectangleToolStripMenuItem_Text` | Rectangle | Dreptunghi | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a rectangle drawn on the map; the term Rectangle |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | Elipsă | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | Elipsă | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_addMenu_Text` | _Add | A_dăugare | menu Add of the menu bar of the main window; the term Add; its access key is a letter that no other menu of the menu bar uses: "d", as "a" is the Help menu, "_Asistență" |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | _Mută toate posturile... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | Standard de descriere 2004 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2004 standard of the IOF control descriptions; the term 2004 Description Standard |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | Standard de descriere 2018/2024 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2018 standard of the IOF control descriptions, as revised in 2024; the term 2018/2024 Description Standard |  |
| `MainFrame_mapStd2000Menu_Text` | ISOM2000 Map Standard | Standard de hartă ISOM2000 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2000; the term ISOM2000 Map Standard |  |
| `MainFrame_mapStd2017Menu_Text` | ISOM2017 Map Standard | Standard de hartă ISOM2017 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2017; the term ISOM2017 Map Standard |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | Standard de hartă ISSprOM 2019 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | _Standarde IOF | item of the Event menu of the main window, a submenu of the IOF standards of the descriptions and of the map; the term IOF Standards; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_customizeCourseAppearanceMenu_Text` | Customize _Appearance... | Personalizare _aspect... | item of the Event menu of the main window, which opens the dialog of the appearance of the courses; the term Customize Appearance; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_addForbiddenMenu_Text` | Forbidden Route Marking | Marcare rută interzisă<br>(corrects: Marcare Interzisă) | "Marcare Interzisă" is a forbidden marking; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_forbiddenRouteMarkingToolStripMenuItem_Text` | Forbidden Route Marking | Marcare rută interzisă<br>(corrects: Marcare Interzisă) | "Marcare Interzisă" is a forbidden marking; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_addBoundaryMenu_Text` | Uncrossable Boundary | Limită de netrecut<br>(corrects: Zonă Inaccesibilă) | "Zonă Inaccesibilă" is an area; the item is a boundary line (the term Uncrossable Boundary) |  |
| `MainFrame_boundaryToolStripMenuItem_Text` | Uncrossable Boundary | Limită de netrecut<br>(corrects: Zonă Inaccesibilă) | "Zonă Inaccesibilă" is an area; the item is a boundary line (the term Uncrossable Boundary) |  |
| `MainFrame_addRegMarkMenu_Text` | Registration Mark | Marcaj de înregistrare<br>(corrects: Macaj de Înregistrare) | the typo "Macaj"; the mark is a "marcaj de înregistrare", written in small letters as the language's corrected items of the Add menu ("Punct de trecere obligatoriu"); terms.csv is corrected with the group |  |
| `MainFrame_registrationMarkToolStripMenuItem_Text` | Registration Mark | Marcaj de înregistrare<br>(corrects: Macaj de Înregistrare) | the typo "Macaj"; the mark is a "marcaj de înregistrare", written in small letters as the language's corrected items of the Add menu ("Punct de trecere obligatoriu"); terms.csv is corrected with the group |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addDangerousMenu_Text` | Dangerous Area | Опасная зона | item of the Add menu of the main window, without an access key as the English: an area that is dangerous to the competitors (ISOM 710); the term Dangerous Area |  |
| `MainFrame_dangerousToolStripMenuItem_Text` | Dangerous Area | Опасная зона | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an area that is dangerous to the competitors (ISOM 710); the term Dangerous Area |  |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Временное сооружение | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Временное сооружение | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addWaterMenu_Text` | Water Location | Пункт питания | item of the Add menu of the main window, without an access key as the English: the refreshment point (ISOM 713); the term Water Location |  |
| `MainFrame_waterLocationToolStripMenuItem_Text` | Water Location | Пункт питания | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the refreshment point (ISOM 713); the term Water Location |  |
| `MainFrame_addFirstAidMenu_Text` | First Aid Location | Медпункт | item of the Add menu of the main window, without an access key as the English: the first aid post (ISOM 712); the term First Aid Location |  |
| `MainFrame_firstAidLocationToolStripMenuItem_Text` | First Aid Location | Медпункт | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the first aid post (ISOM 712); the term First Aid Location |  |
| `MainFrame_addForbiddenMenu_Text` | Forbidden Route Marking | Обозначение запрещённого маршрута | item of the Add menu of the main window, without an access key as the English: the crosses that mark a route the competitors must not use (ISOM 711); the term Forbidden Route Marking |  |
| `MainFrame_forbiddenRouteMarkingToolStripMenuItem_Text` | Forbidden Route Marking | Обозначение запрещённого маршрута | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the crosses that mark a route the competitors must not use (ISOM 711); the term Forbidden Route Marking |  |
| `MainFrame_addBoundaryMenu_Text` | Uncrossable Boundary | Непреодолимая граница | item of the Add menu of the main window, without an access key as the English: a line that the competitors must not cross (ISOM 707); the term Uncrossable Boundary |  |
| `MainFrame_boundaryToolStripMenuItem_Text` | Uncrossable Boundary | Непреодолимая граница | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a line that the competitors must not cross (ISOM 707); the term Uncrossable Boundary |  |
| `MainFrame_addRegMarkMenu_Text` | Registration Mark | Метка совмещения | item of the Add menu of the main window, without an access key as the English: the cross that aligns the printed courses with a map printed before; the term Registration Mark |  |
| `MainFrame_registrationMarkToolStripMenuItem_Text` | Registration Mark | Метка совмещения | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the cross that aligns the printed courses with a map printed before; the term Registration Mark |  |
| `MainFrame_whiteOutMenu_Text` | White Out Area | Белая область | item of the Add menu of the main window, without an access key as the English: a white area that hides the map under it; the term White Out Area |  |
| `MainFrame_whiteOutToolStripMenuItem_Text` | White Out Area | Белая область | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a white area that hides the map under it; the term White Out Area |  |
| `MainFrame_addImageMenu_Text` | Image | Изображение | item of the Add menu of the main window, without an access key as the English: an image file placed on the map; the term Image |  |
| `MainFrame_imageToolStripMenuItem_Text` | Image | Изображение | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an image file placed on the map; the term Image |  |
| `MainFrame_addLineMenu_Text` | Line | Линия | item of the Add menu of the main window, without an access key as the English: a line drawn on the map; the term Line (drawn) |  |
| `MainFrame_lineToolStripMenuItem_Text` | Line | Линия | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a line drawn on the map; the term Line (drawn) |  |
| `MainFrame_addRectangleMenu_Text` | Rectangle | Прямоугольник | item of the Add menu of the main window, without an access key as the English: a rectangle drawn on the map; the term Rectangle |  |
| `MainFrame_rectangleToolStripMenuItem_Text` | Rectangle | Прямоугольник | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a rectangle drawn on the map; the term Rectangle |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | Эллипс | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | Эллипс | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_addMenu_Text` | _Add | _Добавить | menu Add of the menu bar of the main window; the term Add; its access key is a letter that no other menu of the menu bar uses ("д") |  |
| `MainFrame_changeMapFileMenu_Text` | _Map File... | _Файл карты... | item of the Event menu of the main window, which opens the dialog that changes the map file of the event; the term Map File; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_changeCodesMenu_Text` | Change Control _Codes... | Изменить _коды КП... | item of the Event menu of the main window, which opens the dialog that changes the codes of the controls; the term Change Control Codes; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_autoNumberingMenu_Text` | Automatic _Numbering... | Автоматическая _нумерация... | item of the Event menu of the main window, which opens the dialog that numbers the controls automatically; the term Automatic Numbering; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_removeUnusedControlsMenu_Text` | Remove _Unused Controls... | _Удалить неиспользуемые КП... | item of the Event menu of the main window, which opens the dialog that removes the controls that no course uses; the term Remove Unused Controls; its access key is a letter that no other item of its menu uses: "у", as "н" is the Automatic Numbering above it |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | _Переместить все КП... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_punchPatternsMenu_Text` | _Punch Patterns... | _Рисунки компостеров... | item of the Event menu of the main window, which opens the dialog of the patterns of the pin punches; the term Punch Patterns; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | Стандарт легенд КП 2004 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2004 standard of the IOF control descriptions; the term 2004 Description Standard |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | Стандарт легенд КП 2018/2024 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2018 standard of the IOF control descriptions, as revised in 2024; the term 2018/2024 Description Standard |  |
| `MainFrame_mapStd2000Menu_Text` | ISOM2000 Map Standard | Картографический стандарт ISOM2000 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2000; the term ISOM2000 Map Standard |  |
| `MainFrame_mapStd2017Menu_Text` | ISOM2017 Map Standard | Картографический стандарт ISOM2017 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2017; the term ISOM2017 Map Standard |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | Картографический стандарт ISSprOM 2019 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | _Стандарты IOF | item of the Event menu of the main window, a submenu of the IOF standards of the descriptions and of the map; the term IOF Standards; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_customizeDescriptionsMenu_Text` | Customize _Description Text... | Настроить текст _легенд... | item of the Event menu of the main window, which opens the dialog that changes the texts of the description symbols; the term Customize Description Text; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_customizeCourseAppearanceMenu_Text` | Customize _Appearance... | Настроить _оформление... | item of the Event menu of the main window, which opens the dialog of the appearance of the courses; the term Customize Appearance; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_eventMenu_Text` | Eve_nt | Со_ревнование | menu Event of the menu bar of the main window; the term Event; its access key is a letter that no other menu of the menu bar uses: "р", as "с" is kept for the Help menu, "Справка", by the custom of the language |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Začasni objekt | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Začasni objekt | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_whiteOutMenu_Text` | White Out Area | Belo območje | item of the Add menu of the main window, without an access key as the English: a white area that hides the map under it; the term White Out Area |  |
| `MainFrame_whiteOutToolStripMenuItem_Text` | White Out Area | Belo območje | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a white area that hides the map under it; the term White Out Area |  |
| `MainFrame_addImageMenu_Text` | Image | Slika | item of the Add menu of the main window, without an access key as the English: an image file placed on the map; the term Image |  |
| `MainFrame_imageToolStripMenuItem_Text` | Image | Slika | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an image file placed on the map; the term Image |  |
| `MainFrame_addLineMenu_Text` | Line | Črta | item of the Add menu of the main window, without an access key as the English: a line drawn on the map; the term Line (drawn) |  |
| `MainFrame_lineToolStripMenuItem_Text` | Line | Črta | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a line drawn on the map; the term Line (drawn) |  |
| `MainFrame_addRectangleMenu_Text` | Rectangle | Pravokotnik | item of the Add menu of the main window, without an access key as the English: a rectangle drawn on the map; the term Rectangle |  |
| `MainFrame_rectangleToolStripMenuItem_Text` | Rectangle | Pravokotnik | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: a rectangle drawn on the map; the term Rectangle |  |
| `MainFrame_addEllipseMenu_Text` | Ellipse | Elipsa | item of the Add menu of the main window, without an access key as the English: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_ellipseToolStripMenuItem_Text` | Ellipse | Elipsa | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: an ellipse drawn on the map; the term Ellipse |  |
| `MainFrame_addMenu_Text` | _Add | Dod_aj | menu Add of the menu bar of the main window; the term Add; its access key is a letter that no other menu of the menu bar uses: "a", as "d" is the File menu, "_Datoteka" |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | _Premakni vse kontrolne točke... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | Standard opisov 2004 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2004 standard of the IOF control descriptions; the term 2004 Description Standard |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | Standard opisov 2018/2024 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the 2018 standard of the IOF control descriptions, as revised in 2024; the term 2018/2024 Description Standard |  |
| `MainFrame_mapStd2000Menu_Text` | ISOM2000 Map Standard | Kartografski standard ISOM2000 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2000; the term ISOM2000 Map Standard |  |
| `MainFrame_mapStd2017Menu_Text` | ISOM2017 Map Standard | Kartografski standard ISOM2017 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF map standard ISOM2017; the term ISOM2017 Map Standard |  |
| `MainFrame_mapStdSpr2019Menu_Text` | ISSprOM 2019 Map Standard | Kartografski standard ISSprOM 2019 | check item of the submenu IOF Standards of the Event menu, without an access key as the English: the IOF sprint map standard ISSprOM 2019; the form of the language's ISOM2000 and ISOM2017 Map Standard items |  |
| `MainFrame_iOFStandardsToolStripMenuItem_Text` | IOF _Standards | _Standardi IOF | item of the Event menu of the main window, a submenu of the IOF standards of the descriptions and of the map; the term IOF Standards; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_customizeCourseAppearanceMenu_Text` | Customize _Appearance... | Prilagodi v_idez... | item of the Event menu of the main window, which opens the dialog of the appearance of the courses; the term Customize Appearance; its access key is a letter that no other item of its menu uses: "i", as "v" is the Punch Patterns, "_Vzorci perforacij" |  |
| `MainFrame_addRegMarkMenu_Text` | Registration Mark | Oznaka registra<br>(corrects: oznaka registra) | the item began with a small letter, unlike every other item of the menu; terms.csv is corrected with the group |  |
| `MainFrame_registrationMarkToolStripMenuItem_Text` | Registration Mark | Oznaka registra<br>(corrects: oznaka registra) | the item began with a small letter, unlike every other item of the menu; terms.csv is corrected with the group |  |
| `MainFrame_changeCodesMenu_Text` | Change Control _Codes... | Spremeni _kodne številke...<br>(corrects: Spremeni kontrolno točko _kodne številke...) | "Spremeni kontrolno točko kodne številke" is not grammatical; the dialog that the item opens is "Spremeni kodne številke" (the term Change Control Codes) | UIText.ChangeAllCodes_Text |
| `MainFrame_autoNumberingMenu_Text` | Automatic _Numbering... | Avtomatsko _številčenje...<br>(corrects: Avtomatsko _Številčenje) | "številčenje" takes no capital in Slovenian, as the dialog that the item opens (the term Automatic Numbering); the item opens a dialog | UIText.AutoNumbering_Text |
| `MainFrame_removeUnusedControlsMenu_Text` | Remove _Unused Controls... | Odstrani _neuporabljene kontrolne točke...<br>(corrects: Odstrani _Neuporabljene kontrolne točke...) | "neuporabljene" takes no capital in Slovenian (the term Remove Unused Controls) |  |
| `MainFrame_customizeDescriptionsMenu_Text` | Customize _Description Text... | Prilagodi _besedilni opis...<br>(corrects: Prilagodi _Besedilni opis) | "besedilni" takes no capital in Slovenian, as the dialog that the item opens (the term Customize Description Text); the item opens a dialog | UIText.CustomSymbolText_Text |

#### sv

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Tillfällig konstruktion | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Tillfällig konstruktion | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_whiteOutMenu_Text` | White Out Area | Beyaz Alan<br>(corrects: Beyaz Dış Alan) | "Dış" means outer; the item is a white area (the term White Out Area) |  |
| `MainFrame_whiteOutToolStripMenuItem_Text` | White Out Area | Beyaz Alan<br>(corrects: Beyaz Dış Alan) | "Dış" means outer; the item is a white area (the term White Out Area) |  |
| `MainFrame_descriptionStd2004Menu_Text` | 2004 Description Standard | 2004 Açıklama Standardı<br>(corrects: 2004 Tanım Satandardı) | the typo "Satandardı", and "tanım" is a definition; the language's descriptions are "açıklamalar" (the term 2004 Description Standard) |  |
| `MainFrame_descriptionStd2018Menu_Text` | 2018/2024 Description Standard | 2018/2024 Açıklama Standardı<br>(corrects: 2018/2024 Tanım Satandardı) | the typo "Satandardı", and "tanım" is a definition; the language's descriptions are "açıklamalar" (the term 2018/2024 Description Standard) |  |

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | Тимчасова споруда | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | Тимчасова споруда | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | _Перемістити всі КП... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; its access key is a letter that no other item of its menu uses |  |
| `MainFrame_punchPatternsMenu_Text` | _Punch Patterns... | Фор_ми компостерів...<br>(corrects: _Форма компостерів...) | "Форма" is singular, and the dialog holds the patterns of all punches (the term Punch Patterns); the access key "м" is a letter that no other item of the Event menu uses ("ф" is the Map File) |  |
| `MainFrame_eventMenu_Text` | Eve_nt | _Змагання<br>(corrects: Подія) | "подія" is an occurrence; the language's event is "змагання" (the term Event), as its New Event, "Нові змагання", and its label that names this menu, "меню Змагання/Стандарти IOF"; the access key "з" is a letter that no other menu of the menu bar uses | UIText.MainFrame_newEventMenu_Text UIText.NewEventStandards_changeLaterLabel_Text |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | 临时建筑 | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | 临时建筑 | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_addDangerousMenu_Text` | Dangerous Area | 危险区域<br>(corrects: 危险路段) | 路段 is a section of a road; the item is an area (the term Dangerous Area), as zh-TW 危險區域 |  |
| `MainFrame_dangerousToolStripMenuItem_Text` | Dangerous Area | 危险区域<br>(corrects: 危险路段) | 路段 is a section of a road; the item is an area (the term Dangerous Area), as zh-TW 危險區域 |  |
| `MainFrame_addForbiddenMenu_Text` | Forbidden Route Marking | 禁止路线标记<br>(corrects: 禁止的路标) | 路标 is a signpost; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_forbiddenRouteMarkingToolStripMenuItem_Text` | Forbidden Route Marking | 禁止路线标记<br>(corrects: 禁止的路标) | 路标 is a signpost; the item marks a forbidden route (the term Forbidden Route Marking) |  |
| `MainFrame_addRegMarkMenu_Text` | Registration Mark | 套印标记<br>(corrects: 注册标记) | 注册 is a registration, as of a user; the mark that aligns a print is 套印标记 (the term Registration Mark), as zh-TW |  |
| `MainFrame_registrationMarkToolStripMenuItem_Text` | Registration Mark | 套印标记<br>(corrects: 注册标记) | 注册 is a registration, as of a user; the mark that aligns a print is 套印标记 (the term Registration Mark), as zh-TW |  |
| `MainFrame_whiteOutMenu_Text` | White Out Area | 白色区域<br>(corrects: 白色外部区域) | 外部 means outer; the item is a white area (the term White Out Area) |  |
| `MainFrame_whiteOutToolStripMenuItem_Text` | White Out Area | 白色区域<br>(corrects: 白色外部区域) | 外部 means outer; the item is a white area (the term White Out Area) |  |
| `MainFrame_addLineMenu_Text` | Line | 线条<br>(corrects: 直线) | 直线 is a straight line, and the item draws a line of several segments (the term Line (drawn)), as the language's "添加线条" |  |
| `MainFrame_lineToolStripMenuItem_Text` | Line | 线条<br>(corrects: 直线) | 直线 is a straight line, and the item draws a line of several segments (the term Line (drawn)), as the language's "添加线条" |  |
| `MainFrame_addMenu_Text` | _Add | 添加(_A)<br>(corrects: 添加 (_A)...) | the ellipsis marks an item that opens a dialog, and a menu of the menu bar opens none; the access key is written as in the language's other menus ("文件(_F)", "帮助(_H)") |  |
| `MainFrame_punchPatternsMenu_Text` | _Punch Patterns... | 机械打卡器针脚排列(_P)...<br>(corrects: _机械打卡器针脚排列...) | the access key stood on the Chinese character 机, which no key can type; the language writes it after the text, "(_P)", as its other items |  |

#### zh-TW

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addConstructionMenu_Text` | Temporary Construction | 臨時建築 | item of the Add menu of the main window, without an access key as the English: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_constructionToolStripMenuItem_Text` | Temporary Construction | 臨時建築 | item of the drop-down Special Item of the toolbar, without an access key as the English; the same text as the item of the Add menu: the special item of the ISSprOM, a temporary construction or closed area; the term Temporary Construction |  |
| `MainFrame_moveAllControlsMenu_Text` | Mo_ve All Controls... | 移動所有檢查點... | item of the Event menu of the main window, which moves all controls together, as after a change of the map; the term Move All Controls; without an access key, as the other items of the language's Event menu |  |
| `MainFrame_addLineMenu_Text` | Line | 線段<br>(corrects: 列) | 列 is a row of text; the item draws a line (the term Line (drawn)), as the language's "新增線段" |  |
| `MainFrame_lineToolStripMenuItem_Text` | Line | 線段<br>(corrects: 列) | 列 is a row of text; the item draws a line (the term Line (drawn)), as the language's "新增線段" |  |
