# Phase 2: the Include choice and the faults

Nov 16, 2026. Task 9's first branch, `fix/translation-faults` (1de84f81), from phases 1 and 2 of [README.md](../README.md). claude-opus-5-5 at effort max made every translation and correction below, in UIText and PurplePenCore's resources, and the same in the WinForms forms' satellites (CreatePdfCourses, CreateImageFiles, PrintDescriptions, PrintPunches). Each entry says so in its comment.

- **The Include choice** (`CreatePdfCourses_` and `CreateImageFiles_labelPrintBaseMap_Text`, `comboBoxPrintBaseMap_Items`, `_Items1`): 31 languages. en-GB needs none (`not-needed/en-GB.tsv`). The label is "content" in most languages, which reads naturally before the items "course and map" and "course only".
- **Doubts.** ku, vec, zh-HK and id had no earlier translation of "course": their words are guesses (ku "rê", vec after Italian, zh-HK after zh-TW, id "lintasan"). A speaker should look at them.
- **Corrections**: the errors that `check.py` found, the Japanese `{0}` of task 9, pt `CustomizedSymbolDesc`, zh-CN `CreateMapFiles`, and the PDF dialog's title "Print Courses" in eight languages. Each line gives the text that it replaced and why.

#### bg

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Съдържание: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Маршрут и карта | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Само маршрут | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Съдържание: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Маршрут и карта | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Само маршрут | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### ca

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Contingut: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Recorregut i mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Només el recorregut | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Contingut: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Recorregut i mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Només el recorregut | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### cs

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Obsah: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Trať a mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Pouze trať | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Obsah: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Trať a mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Pouze trať | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Indhold: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Kursus og kort | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Kun kursus | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Indhold: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Kursus og kort | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Kun kursus | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### de

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Inhalt: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Bahn und Karte | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Nur Bahn | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Inhalt: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Bahn und Karte | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Nur Bahn | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_Text` | Create PDF Files | PDF-Dateien erstellen<br>(corrects: Bahnen drucken) | "Bahnen drucken" names another dialog (Print Courses); mirrors CreateImageFiles_Text | CreateImageFiles_Text |

#### en-GB

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | (not needed) | en-GB writes it as the English does | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | (not needed) | en-GB writes it as the English does | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | (not needed) | en-GB writes it as the English does | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | (not needed) | en-GB writes it as the English does | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | (not needed) | en-GB writes it as the English does | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | (not needed) | en-GB writes it as the English does | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_Text` | Create PDF Files | Create PDF Files<br>(corrects: Print Courses) | "Print Courses" names another dialog (Print Courses); mirrors CreateImageFiles_Text | CreateImageFiles_Text |

#### es

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Contenido: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Recorrido y mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Solo el recorrido | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Contenido: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Recorrido y mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Solo el recorrido | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Sisu: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Rada ja kaart | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Ainult rada | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Sisu: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Rada ja kaart | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Ainult rada | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_Text` | Create PDF Files | Salvesta PDF-failid<br>(corrects: Trüki rajad) | "Trüki rajad" names another dialog (Print Courses); mirrors CreateImageFiles_Text | CreateImageFiles_Text |

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Sisältö: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Rata ja kartta | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Vain rata | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Sisältö: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Rata ja kartta | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Vain rata | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_Text` | Create PDF Files | Luo PDF-tiedostoja<br>(corrects: Tulosta radat) | "Tulosta radat" names another dialog (Print Courses); mirrors CreateImageFiles_Text | CreateImageFiles_Text |

#### fr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Contenu : | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Circuit et carte | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Circuit seul | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Contenu : | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Circuit et carte | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Circuit seul | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_Text` | Create PDF Files | Créer des fichiers PDF<br>(corrects: Imprimer les circuits) | "Imprimer les circuits" names another dialog (Print Courses); mirrors CreateImageFiles_Text | CreateImageFiles_Text |

#### he

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | תוכן: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | מסלול ומפה | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | מסלול בלבד | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | תוכן: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | מסלול ומפה | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | מסלול בלבד | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Tartalom: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Pálya és térkép | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Csak a pálya | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Tartalom: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Pálya és térkép | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Csak a pálya | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `MiscText.PrintingPage` | Printing page {0} (of {1}) | Nyomtatás {0} / {1}. oldal<br>(corrects: Nyomtatás (0} / {1}. oldal) | "(0}" is "{0}" mistyped; string.Format throws on it |  |
| `MiscText_PrintingPage` | Printing page {0} (of {1}) | Nyomtatás {0} / {1}. oldal<br>(corrects: Nyomtatás (0} / {1}. oldal) | "(0}" is "{0}" mistyped; string.Format throws on it |  |

#### id

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Isi: | label beside the combo box of what the file holds; "content" reads naturally before the items; doubt: Indonesian has no earlier translation of "course"; "lintasan" (track) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Lintasan dan peta | the file holds the course over the base map; doubt: Indonesian has no earlier translation of "course"; "lintasan" (track) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Lintasan saja | the file holds the course without the base map; doubt: Indonesian has no earlier translation of "course"; "lintasan" (track) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Isi: | label beside the combo box of what the file holds; "content" reads naturally before the items; doubt: Indonesian has no earlier translation of "course"; "lintasan" (track) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Lintasan dan peta | the file holds the course over the base map; doubt: Indonesian has no earlier translation of "course"; "lintasan" (track) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Lintasan saja | the file holds the course without the base map; doubt: Indonesian has no earlier translation of "course"; "lintasan" (track) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Contenuto: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Percorso e mappa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Solo il percorso | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Contenuto: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Percorso e mappa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Solo il percorso | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### ja

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | 内容: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | コースと地図 | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | コースのみ | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | 内容: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | コースと地図 | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | コースのみ | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `MiscText.CannotCreatePdfs` | Cannot create PDFs for the following reason: | PDF を作成できません。以下の理由:<br>(corrects: PDF '{0}' を作成できません。以下の理由:) | HandleExceptions formats it with no arguments, so "{0}" throws (task 9) |  |
| `MiscText_CannotCreatePdfs` | Cannot create PDFs for the following reason: | PDF を作成できません。以下の理由:<br>(corrects: PDF '{0}' を作成できません。以下の理由:) | HandleExceptions formats it with no arguments, so "{0}" throws (task 9) |  |
| `MiscText.CannotCreateOcadFiles` | Cannot create OCAD files for the following reason: | OCAD ファイルを作成できません。以下の理由:<br>(corrects: OCAD ファイル '{0}' を作成できません。以下の理由:) | HandleExceptions formats it with no arguments, so "{0}" throws (task 9) |  |
| `MiscText_CannotCreateOcadFiles` | Cannot create OCAD files for the following reason: | OCAD ファイルを作成できません。以下の理由:<br>(corrects: OCAD ファイル '{0}' を作成できません。以下の理由:) | HandleExceptions formats it with no arguments, so "{0}" throws (task 9) |  |
| `SelectionDescriptionText.TextLine_AboveAllCourses` | Appears above {0}, in all courses that contain {0} | {0} の上に表示、{0} を含む全てのコース<br>(corrects: {0} の上に表示、(0} を含む全てのコース) | "(0}" is "{0}" mistyped; string.Format throws on it |  |
| `SelectionDescriptionText_TextLine_AboveAllCourses` | Appears above {0}, in all courses that contain {0} | {0} の上に表示、{0} を含む全てのコース<br>(corrects: {0} の上に表示、(0} を含む全てのコース) | "(0}" is "{0}" mistyped; string.Format throws on it |  |
| `SelectionDescriptionText.TextLine_BelowAllCourses` | Appears below {0}, in all courses that contain {0} | {0} の下に表示、{0} を含む全てのコース<br>(corrects: {0} の下に表示、(0} を含む全てのコース) | "(0}" is "{0}" mistyped; string.Format throws on it |  |
| `SelectionDescriptionText_TextLine_BelowAllCourses` | Appears below {0}, in all courses that contain {0} | {0} の下に表示、{0} を含む全てのコース<br>(corrects: {0} の下に表示、(0} を含む全てのコース) | "(0}" is "{0}" mistyped; string.Format throws on it |  |

#### ku

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Naverok: | label beside the combo box of what the file holds; "content" reads naturally before the items; doubt: Kurmanji has no earlier translation of "course"; "rê" (way) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Rê û nexşe | the file holds the course over the base map; doubt: Kurmanji has no earlier translation of "course"; "rê" (way) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Tenê rê | the file holds the course without the base map; doubt: Kurmanji has no earlier translation of "course"; "rê" (way) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Naverok: | label beside the combo box of what the file holds; "content" reads naturally before the items; doubt: Kurmanji has no earlier translation of "course"; "rê" (way) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Rê û nexşe | the file holds the course over the base map; doubt: Kurmanji has no earlier translation of "course"; "rê" (way) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Tenê rê | the file holds the course without the base map; doubt: Kurmanji has no earlier translation of "course"; "rê" (way) is a guess | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Innhold: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Løype og kart | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Bare løype | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Innhold: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Løype og kart | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Bare løype | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `ReportText.LegLength_CourseInfoNoClimb` | {0} ({1} controls, {2}) | {0} ({1} poster, {2})<br>(corrects: 0} ({1} poster, {2})) | the "{" of "{0}" is missing; string.Format throws on it |  |
| `ReportText_LegLength_CourseInfoNoClimb` | {0} ({1} controls, {2}) | {0} ({1} poster, {2})<br>(corrects: 0} ({1} poster, {2})) | the "{" of "{0}" is missing; string.Format throws on it |  |
| `CreatePdfCourses_Text` | Create PDF Files | Lag PDF-filer<br>(corrects: Skriv ut løyper) | "Skriv ut løyper" names another dialog (Print Courses); mirrors CreateImageFiles_Text | CreateImageFiles_Text |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Inhoud: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Route en kaart | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Alleen route | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Inhoud: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Route en kaart | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Alleen route | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Innhald: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Løype og kart | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Berre løype | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Innhald: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Løype og kart | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Berre løype | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_Text` | Create PDF Files | Lag PDF-filer<br>(corrects: Skriv ut løyper) | "Skriv ut løyper" names another dialog (Print Courses); mirrors CreateImageFiles_Text | CreateImageFiles_Text |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Zawartość: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Trasa i mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Tylko trasa | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Zawartość: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Trasa i mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Tylko trasa | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### pt-BR

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Conteúdo: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Percurso e mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Somente o percurso | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Conteúdo: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Percurso e mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Somente o percurso | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### pt

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Conteúdo: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Percurso e mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Apenas o percurso | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Conteúdo: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Percurso e mapa | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Apenas o percurso | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `SelectionDescriptionText.CustomizedSymbolDesc` | Customized symbol description | Descrição do símbolo personalizada<br>(corrects: Percurso "{0}") | "Percurso "{0}"" means "Course "{0}"", another text; mirrors pt-BR |  |
| `SelectionDescriptionText_CustomizedSymbolDesc` | Customized symbol description | Descrição do símbolo personalizada<br>(corrects: Percurso "{0}") | "Percurso "{0}"" means "Course "{0}"", another text; mirrors pt-BR |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Conținut: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Traseu și hartă | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Doar traseul | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Conținut: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Traseu și hartă | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Doar traseul | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_Text` | Create PDF Files | Creare fișiere PDF<br>(corrects: Tipărire Trasee) | "Tipărire Trasee" names another dialog (Print Courses); mirrors CreateImageFiles_Text | CreateImageFiles_Text |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Содержимое: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Дистанция и карта | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Только дистанция | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Содержимое: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Дистанция и карта | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Только дистанция | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Vsebina: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Proga in karta | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Samo proga | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Vsebina: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Proga in karta | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Samo proga | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### sv

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Innehåll: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Bana och karta | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Endast bana | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Innehåll: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Bana och karta | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Endast bana | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | İçerik: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Parkur ve harita | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Yalnızca parkur | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | İçerik: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Parkur ve harita | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Yalnızca parkur | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `PrintDescriptions_marginsLabel_Text` | Margins: | Kenar Boşluğu:<br>(corrects: Kenar Boşluğu ({0})) | the text of "Margins ({0})" where the English is "Margins:", so the label shows "{0}" | PrinterMargins_groupBoxMargins_Text |
| `PrintPunches_marginsLabel_Text` | Margins: | Kenar Boşluğu:<br>(corrects: Kenar Boşluğu ({0})) | the text of "Margins ({0})" where the English is "Margins:", so the label shows "{0}" | PrinterMargins_groupBoxMargins_Text |

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Вміст: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Дистанція і карта | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Лише дистанція | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Вміст: | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Дистанція і карта | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Лише дистанція | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### vec

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | Contenuo: | label beside the combo box of what the file holds; "content" reads naturally before the items; doubt: Venetian has no earlier translations; the words follow Italian | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | Percorso e mapa | the file holds the course over the base map; doubt: Venetian has no earlier translations; the words follow Italian | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | Solo el percorso | the file holds the course without the base map; doubt: Venetian has no earlier translations; the words follow Italian | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | Contenuo: | label beside the combo box of what the file holds; "content" reads naturally before the items; doubt: Venetian has no earlier translations; the words follow Italian | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | Percorso e mapa | the file holds the course over the base map; doubt: Venetian has no earlier translations; the words follow Italian | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | Solo el percorso | the file holds the course without the base map; doubt: Venetian has no earlier translations; the words follow Italian | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | 内容： | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | 线路和地图 | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | 仅线路 | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | 内容： | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | 线路和地图 | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | 仅线路 | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `ReportText.LegLength_CourseInfoNoClimb` | {0} ({1} controls, {2}) | {0}（{1}检查点，{2}）<br>(corrects: {0}（{一} Controls，{二}）) | "{一}" and "{二}" are not placeholders; string.Format throws on them; mirrors LegLength_CourseInfo |  |
| `ReportText_LegLength_CourseInfoNoClimb` | {0} ({1} controls, {2}) | {0}（{1}检查点，{2}）<br>(corrects: {0}（{一} Controls，{二}）) | "{一}" and "{二}" are not placeholders; string.Format throws on them; mirrors LegLength_CourseInfo |  |
| `MiscText.Margins_LRTB` | Left: {0}, Right: {1}, Top: {2}, Bottom: {3} | 左：{0}，右：{1}，上：{2}，下：{3}<br>(corrects: 左：{0}：{1}右上：{2}，{三}：底) | "{三}" is not a placeholder, and the sides are out of order; mirrors zh-TW |  |
| `MiscText_Margins_LRTB` | Left: {0}, Right: {1}, Top: {2}, Bottom: {3} | 左：{0}，右：{1}，上：{2}，下：{3}<br>(corrects: 左：{0}：{1}右上：{2}，{三}：底) | "{三}" is not a placeholder, and the sides are out of order; mirrors zh-TW |  |
| `MiscText.PartXOfY` | Part {0} (of {1}) | 第 {0} 部分（共 {1} 部分）<br>(corrects: （除了{0} {一}）) | "（除了{0} {一}）" means "except {0} {one}"; "{一}" is not a placeholder |  |
| `MiscText_PartXOfY` | Part {0} (of {1}) | 第 {0} 部分（共 {1} 部分）<br>(corrects: （除了{0} {一}）) | "（除了{0} {一}）" means "except {0} {one}"; "{一}" is not a placeholder |  |
| `CommandNameText.CreateMapFiles` | Create {0} &Files | 创建 {0} 文件(&F)<br>(corrects: 创建{0}和文件) | 和 ("and") stands for the access-key mark "&" (task 9); mirrors zh-TW |  |
| `CommandNameText_CreateMapFiles` | Create {0} _Files | 创建 {0} 文件(_F)<br>(corrects: 创建{0}和文件) | 和 ("and") stands for the access-key mark "&" (task 9); mirrors zh-TW |  |

#### zh-HK

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | 內容： | label beside the combo box of what the file holds; "content" reads naturally before the items; doubt: zh-HK has no earlier translations; the terms follow zh-TW | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | 賽程及地圖 | the file holds the course over the base map; doubt: zh-HK has no earlier translations; the terms follow zh-TW | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | 只有賽程 | the file holds the course without the base map; doubt: zh-HK has no earlier translations; the terms follow zh-TW | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | 內容： | label beside the combo box of what the file holds; "content" reads naturally before the items; doubt: zh-HK has no earlier translations; the terms follow zh-TW | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | 賽程及地圖 | the file holds the course over the base map; doubt: zh-HK has no earlier translations; the terms follow zh-TW | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | 只有賽程 | the file holds the course without the base map; doubt: zh-HK has no earlier translations; the terms follow zh-TW | MainFrame_courseMenu_Text ChangeMapFile_Text |

#### zh-TW

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `CreatePdfCourses_labelPrintBaseMap_Text` | Include: | 內容： | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items` | Course and map | 賽程和地圖 | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreatePdfCourses_comboBoxPrintBaseMap_Items1` | Course only | 僅賽程 | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_labelPrintBaseMap_Text` | Include: | 內容： | label beside the combo box of what the file holds; "content" reads naturally before the items | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items` | Course and map | 賽程和地圖 | the file holds the course over the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
| `CreateImageFiles_comboBoxPrintBaseMap_Items1` | Course only | 僅賽程 | the file holds the course without the base map | MainFrame_courseMenu_Text ChangeMapFile_Text |
