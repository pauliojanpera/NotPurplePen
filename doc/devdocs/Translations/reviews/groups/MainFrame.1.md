# Key group MainFrame.1

Nov 16, 2026. The first part of the main window: the File menu with its submenus Create PDFs, Create Route Review
Files and Set Print Area, the labels of four buttons of the toolbar, the file dialogs of events and GPX files, the
buttons Descriptions and Ordering above the side panel, and two items of the WinForms Edit menu. claude-opus-5-5 at
effort max made every line below, and each entry says so in its comment.

- **Translated**: 125 lines, in 11 languages. ru lacked the whole group. nl lacked the File menu of the menu bar and a
  third of its items. ja, nn-NO, pl, ro and sl lacked the newer items of the File menu (image files, PDF files, route
  review files, KML file), and da, es and hu two of them. Each new item of a menu has an access key that no other item
  of its menu uses. The items of the Create Route Review Files submenu are for the review of the routes that the
  competitors ran, as in RouteGadget, so they name the route choice ("vejvalg", "vegval") or the route ("путь", "pot"),
  not the course.
- **Not needed**: 6 lines. mapViewerTopology is the name of a WinForms control, whose text shows nowhere.
- **Corrected**: 72 lines, most of them after terms.csv.
  - da's course, control, finish and event were "kursus", "kontrol", "slut" and "hændelse": now "bane", "post", "mål"
    and "løb". uk's New Event was "подія", an occurrence: now "змагання".
  - English was left in the text: bg Create PDFs and Undo, nn-NO and tr Exit, tr Image, pt-BR and zh-TW "All files", da
    "file", cs and pt "Data Interchange File", de and fr "File", it "finish". pt-BR's File menu was "Ficheiro/Arquivo",
    and its OCAD item "Ficheiros", the European words.
  - bg's open dialog had the patterns ".ppen" and ".*" without their "*", so it listed no event file.
  - The punch cards were the punch patterns in fi and the pin punches in fr. The Courses item of nl's PDF submenu said
    "Create PDFs". de and sv named the route review files after the check of the courses before the race
    ("Bahnprüfung", "bangranskning"). tr's Ordering was an order of goods, and zh-TW's a sorting in progress. cs's image
    files were "JPEG maps", but the dialog makes PNG, JPEG or GIF files. tr's OCAD and XML items said "OCAD and files"
    and "data and interchange".
  - Typos, split compounds, word order and marks in cs, da, et, it, nb-NO, nl, pt, ro, sl, tr, uk and zh-CN: for
    example it "Stampa_Percorsi" (no space) and "%parte" (the mark shown), sl "Izbran Samo delno", zh-TW's two
    access-key marks in one item, zh-CN 这部分只, tr "Yazdır Parkurları".
- **Left empty**: none.
- **terms.csv**: nn-NO's Create RouteGadget Files is "Lag RouteGadget-filer", with the hyphen of nb-NO;
  terms-decisions.tsv records it.

Doubts:

- Many menus of the existing translations give one access key to two or three items, as pl "_Otwórz", "I_OF XML" and
  "_obszar". The new and corrected items avoid the letters of their menu, and the existing pairs stay. pt-BR's File
  menu takes "q" ("Ar_quivo"), because "a" is Adicionar and Ajuda.
- The Create OCAD Files item of the Avalonia File menu takes its text from CommandNameText.CreateMapFiles, which the
  group CommandNameText translates. ru's File menu leaves "ф" free for it.
- de's Undo item "_Rückgängig:" keeps its colon: MiscText.UndoWithShortcut writes "Rückgängig:" before the name of the
  command.
- nl's "_Ongedaan maken" (Undo) is three times as long as the English, as Dutch writes it; it stands in a menu.
- tr calls a control "Hedef" (target) in the toolbar and in many other texts, and terms.csv gives "Kontrol". Only the
  double space was corrected.
- sv's toolbar button "Lägg till spridningsmetod" and nl's button "Beschrijvingen" stay, against terms.csv ("gaffling",
  "Postomschrijvingen"), as they are not wrong.

#### bg

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_openFileDialog_Filter` | Purple Pen files\|*.ppen\|All files\|*.* | Файлове на Purple Pen\|*.ppen\|Всички файлове\|*.*<br>(corrects: Файлове на Purple Pen\|.ppen\|Всички файлове\|.*) | the patterns ".ppen" and ".*" lacked their "*", so the dialog listed no event file; the names stay |  |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | Създай P_DF файлове<br>(corrects: Create P_DFs) | "Create PDFs" was English (the term Create PDFs) |  |
| `MainFrame_undoMenu_Text` | _Undo | _Отмени<br>(corrects: _Undo) | "Undo" was English (the term Undo) |  |

#### cs

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | Vytvořit ob_rázky...<br>(corrects: Vytvořit  _JPEG mapy...) | "JPEG mapy" (JPEG maps) names one format, and the dialog makes PNG, JPEG or GIF files; the language's dialog "Vytvořit obrázek" (the term Create Image Files); a double space; the access key "r" is a letter that no other item of the File menu uses |  |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Vytvořit soubor pro výměnu dat (_IOF XML)...<br>(corrects: Vytvořit soubory Data _Interchange File (IOF XML)...) | "Data Interchange File" was English (the terms Create, File and IOF XML); the access key stays "i" |  |
| `MainFrame_printAreaThisPartMenu_Text` | This _Part Only | _Pouze tuto část<br>(corrects: _Pouze tutu část) | the typo "tutu" (the term This Part Only) |  |

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Opret filer til vejvalgsanalyse | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English; "vejvalg" is the route choice of Danish orienteering |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Opret _KML-fil... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_addControlToolStripButton_Text` | Add Control | Tilføj post<br>(corrects: Tilføj kontrol) | Danish orienteering calls a control "post" ("kontrol" is a check; the term Add Control) |  |
| `MainFrame_addFinishToolStripButton_Text` | Add Finish | Tilføj mål<br>(corrects: Tilføj slut) | Danish orienteering calls the finish "mål" ("slut" is the end; the term Add Finish) |  |
| `MainFrame_tabPage1_Text` | All Controls | Alle poster<br>(corrects: Alle kontroller) | Danish orienteering calls controls "poster" (the term All Controls) |  |
| `MainFrame_saveFileDialog_Filter` | Purple Pen file\|*.ppen | Purple Pen-fil\|*.ppen<br>(corrects: Purple Pen file\|*.ppen) | "file" was English (the term File), as the language's "Purple Pen filer" of the open dialog |  |
| `MainFrame_newEventMenu_Text` | _New Event... | _Nyt løb...<br>(corrects: _Ny hændelse...) | "hændelse" is an incident; an orienteering event is a "løb" (the term New Event) |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | B_aner...<br>(corrects: _Kurser...) | Danish orienteering calls courses "baner" ("kurser" are courses of study; the term Courses); the access key "a" is a letter that no other item of the submenu uses ("b" is Beskrivelser) |  |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Opret dataudvekslingsfil (_IOF XML)...<br>(corrects: Opret Data _interchangefil (IOF XML)...) | "Data interchangefil" was English; the access key "i" stays, on IOF |  |
| `MainFrame_printCoursesMenu_Text` | Print _Courses... | Udskriv _baner...<br>(corrects: Udskriv _Kurser...) | Danish orienteering calls courses "baner" ("kurser" are courses of study; the term Print Courses); the access key "b" is a letter that no other item of the File menu uses |  |
| `MainFrame_printAreaThisCourseMenu_Text` | _This Course | Denne _bane<br>(corrects: _Dette kursus) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term This Course); the access key "b" is a letter that no other item of the submenu uses |  |
| `MainFrame_printAreaAllCoursesMenu_Text` | _All Courses | _Alle baner<br>(corrects: _Alle kurser) | Danish orienteering calls a course "bane" ("kursus" is a course of study) (the term All courses) |  |
| `MainFrame_setPrintAreaMenu_Text` | Set Print A_rea | Sæt udskriftsom_råde<br>(corrects: Sæt udskrift Om_råde) | the compound "udskriftsområde" is one word in Danish (the term Set Print Area); the access key stays "r" |  |
| `MainFrame_programLanguageMenu_Text` | Program _Language... | Program_sprog...<br>(corrects: Program _Sprog...) | the compound "programsprog" is one word in Danish (the term Program Language); the access key stays "s" |  |

#### de

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_saveGpxFileDialog_Title` | Create GPX File | GPX-Datei erstellen<br>(corrects: GPX-File erstellen ...) | "File" is English, and the title of a dialog has no ellipsis (the term Create GPX File) |  |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Dateien für die Routenwahlanalyse erstellen<br>(corrects: Bahnprüfungs-Dateien erstellen) | "Bahnprüfung" is the check of the courses before the race; the submenu makes the files of the programs that review the routes that the competitors ran, such as RouteGadget |  |
| `MainFrame_createGPXFileMenu_Text` | Create G_PX File... | G_PX-Datei erstellen...<br>(corrects: G_PX File erstellen...) | "File" is English (the term Create GPX File) |  |

#### es

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | Crear archivos de RouteGadget... | item of the submenu Create Route Review Files: makes the files of RouteGadget, a program that shows the routes that the competitors ran; the name RouteGadget stays; the term Create RouteGadget Files | UIText.CreateRouteGadget_Text $this.Text |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Crear archivos para el análisis de rutas | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Crear archivo _KML... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_programLanguageMenu_Text` | Program _Language... | Idio_ma del programa...<br>(corrects: _Lenguaje del programa...) | "lenguaje" is a language as in a programming language; the language of a program is its "idioma" (the term Program Language); the access key "m" is a letter that no other item of the File menu uses |  |

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | Salvesta pil_difailid...<br>(corrects: Salvesta pil_difaild ...) | the typo "pildifaild", and a space before the ellipsis (the term Create Image Files) |  |
| `MainFrame_programLanguageMenu_Text` | Program _Language... | Programmi _keel...<br>(corrects: Programm _Keel) | "Programm" takes the genitive, "programmi keel", and the item opens a dialog (the term Program Language) |  |

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | _Leimauskortit...<br>(corrects: _Leimauskuviot...) | "Leimauskuviot" are the punch patterns; the item makes the punch cards (the term Punch Cards) |  |
| `MainFrame_printPunchCardsMenu_Text` | Print Pu_nch Cards... | Tulosta _leimauskortit...<br>(corrects: Tulosta _leimauskuviot...) | "Leimauskuviot" are the punch patterns; the item prints the punch cards (the term Print Punch Cards) |  |

#### fr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | Cartons de co_ntrôle...<br>(corrects: Poinçons...) | "Poinçons" are the pin punches; the item makes the punch cards (the term Punch Cards), with the access key of the English |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | _Circuits...<br>(corrects: _Courses...) | "courses" are races in French; the language's courses are "circuits" (the term Courses) |  |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Créer un fichier d'échange de données (IOF _XML)...<br>(corrects: Créer Fichier _Interchange XML...) | "Interchange" is English (a French "interchange" is a road junction), and IOF was missing (the term IOF XML); the access key "x" is a letter that no other item of the File menu uses |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Créer un fichier _KML...<br>(corrects: Créer _KML File...) | "File" is English (the term Create KML File) |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Útvonalelemzési fájlok létrehozása | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | KML fájl létre_hozása... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses | UIText.CreateKmlFiles_Text $this.Text |
| `MainFrame_programLanguageMenu_Text` | Program _Language... | A program nyel_ve...<br>(corrects: A program nyel_ve:) | a menu item does not end with a colon; the item opens a dialog |  |

#### it

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addFinishToolStripButton_Text` | Add Finish | Aggiungi arrivo<br>(corrects: Aggiungi finish) | "finish" is English; the language's finish is "arrivo" (the term Add Finish) |  |
| `MainFrame_printCoursesMenu_Text` | Print _Courses... | Stampa perc_orsi...<br>(corrects: Stampa_Percorsi) | the space between the words was missing ("StampaPercorsi"), and the ellipsis of an item that opens a dialog (the term Print Courses); the access key "o" is a letter that no other item of the File menu uses |  |
| `MainFrame_printAreaThisPartMenu_Text` | This _Part Only | Solo questa _parte<br>(corrects: Solo questa %parte) | the access key was written "%", which shows; the mark is "_" (the term This Part Only) |  |
| `MainFrame_setPrintAreaMenu_Text` | Set Print A_rea | Imposta a_rea di stampa<br>(corrects: Imposta Stampa _Area) | "Stampa Area" keeps the order of the English; the area of printing is "area di stampa" (the term Set Print Area), with the access key of the English |  |

#### ja

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | 画像ファイルを作成(_G)... | item of the File menu of the main window; the term Create Image Files; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createDescriptionPdfMenu_Text` | _Descriptions... | ディスクリプション(_D)... | item of the submenu Create PDFs of the File menu: makes PDF files of the control descriptions; the term Descriptions | UIText.MainFrame_radioButtonDescriptions_Text UIText.MainFrame_addDescriptionsMenu_Text |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | コントロールカード(_N)... | item of the submenu Create PDFs of the File menu: makes PDF files of the punch cards; the term Punch Cards |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | コース(_C)... | item of the submenu Create PDFs of the File menu: makes PDF files of the course maps; the term Courses | UIText.AddTextLine_coursesLabel_Text UIText.CreateGpx_coursesGroupBox_Text |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | PDFを作成(_D) | item of the File menu of the main window, a submenu of the PDF files of the descriptions, punch cards and courses; the term Create PDFs; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | RouteGadgetファイルを作成... | item of the submenu Create Route Review Files: makes the files of RouteGadget, a program that shows the routes that the competitors ran; the name RouteGadget stays; the term Create RouteGadget Files | UIText.CreateRouteGadget_Text $this.Text |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | ルート分析用ファイルを作成 | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | KMLファイルを作成(_K)... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Lag data_utvekslingsfil (IOF XML)...<br>(corrects: Lag data _Utvekslingsfil (IOF XML)...) | the compound "datautvekslingsfil" is one word; the access key stays "u" |  |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_openFileDialog_Filter` | Purple Pen files\|*.ppen\|All files\|*.* | Purple Pen-bestanden\|*.ppen\|Alle bestanden\|*.* | filter of the file dialog that opens an event: a name and a pattern for each choice, separated by "\|"; the patterns stay as they are; the terms Files and All |  |
| `MainFrame_saveFileDialog_Filter` | Purple Pen file\|*.ppen | Purple Pen-bestand\|*.ppen | filter of the file dialog that saves an event: the name and the pattern, separated by "\|"; the pattern stays as it is; the term File |  |
| `MainFrame_newEventMenu_Text` | _New Event... | _Nieuw evenement... | item of the File menu of the main window, which starts the wizard of a new event; the term New Event; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | Creëer afbeeld_ingsbestanden... | item of the File menu of the main window; the term Create Image Files; its access key is a letter that no other item of the File menu uses | UIText.CreateImageFiles_Text $this.Text |
| `MainFrame_createDescriptionPdfMenu_Text` | _Descriptions... | _Postomschrijvingen... | item of the submenu Create PDFs of the File menu: makes PDF files of the control descriptions; the term Descriptions | UIText.MainFrame_radioButtonDescriptions_Text radioButtonDescriptions.Text |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | Pri_kkaarten... | item of the submenu Create PDFs of the File menu: makes PDF files of the punch cards; the term Punch Cards |  |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | Creëer P_DF's | item of the File menu of the main window, a submenu of the PDF files of the descriptions, punch cards and courses; the term Create PDFs; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Creëer bestanden voor routeanalyse | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Creëer K_ML-bestand... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses | UIText.CreateKmlFiles_Text $this.Text |
| `MainFrame_printPunchCardsMenu_Text` | Print Pu_nch Cards... | Pri_kkaarten afdrukken... | item of the File menu of the main window; the term Print Punch Cards; its access key is a letter that no other item of the File menu uses | UIText.PrintPunches_Text |
| `MainFrame_printAreaThisPartMenu_Text` | This _Part Only | Alleen dit _deel | item of the submenu Set Print Area of the File menu: the print area of the course part shown; the term This Part Only |  |
| `MainFrame_printAreaThisCourseMenu_Text` | _This Course | Deze _route | item of the submenu Set Print Area of the File menu: the print area of the course shown; the term This Course |  |
| `MainFrame_printAreaAllCoursesMenu_Text` | _All Courses | _Alle routes | item of the submenu Set Print Area of the File menu: one print area for all courses; the term All courses | UIText.SelectionDescriptionText_CourseList_AllCourses SelectionDescriptionText.CourseList_AllCourses |
| `MainFrame_programLanguageMenu_Text` | Program _Language... | Programma_taal... | item of the File menu of the main window, which opens the dialog of the language of the program; the term Program Language; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_exitMenu_Text` | E_xit | A_fsluiten | last item of the File menu, which closes the program; the term Exit; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_fileMenu_Text` | _File | _Bestand | menu File of the menu bar of the main window; the term File; its access key is a letter that no other menu of the menu bar uses |  |
| `MainFrame_cancelMenu_Text` | _Clear Selection | _Selectie opheffen | item of the Edit menu of the WinForms application (the Avalonia menu takes MiscText.ClearSelectionWithShortcut); the term Clear Selection |  |
| `MainFrame_undoMenu_Text` | _Undo | _Ongedaan maken | item of the Edit menu of the WinForms application (the menu shows MiscText.UndoWithShortcut with the name of the command); the term Undo |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | _Routes...<br>(corrects: Creëer PDF's...) | the item said "Creëer PDF's...", the name of its submenu; it makes the PDF files of the courses (the term Courses) |  |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Creëer data-_uitwisselingsbestand (IOF XML)...<br>(corrects: Creëer Data _Uitwisselings bestand (IOF XML)...) | the compound "uitwisselingsbestand" is one word; the access key stays "u" |  |
| `MainFrame_printDescriptionsMenu_Text` | _Print Descriptions... | _Postomschrijvingen afdrukken...<br>(corrects: _Postomschrijvingen afrdukken...) | the typo "afrdukken" (the term Print Descriptions) |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addVariationToolStripButton_Text` | Add Variation | Legg til variasjon | label of a button of the toolbar of the main window, which adds a variation (a fork or a loop) to the course; the term Add Variation |  |
| `MainFrame_saveGpxFileDialog_Title` | Create GPX File | Lag GPX-fil | title of the file dialog that saves the GPX file; the term Create GPX File |  |
| `MainFrame_radioButtonDescriptions_Text` | Descriptions | Postbeskrivelser | button above the side panel of the main window that shows the control descriptions, beside Ordering; the term Descriptions |  |
| `MainFrame_radioButtonTopology_Text` | Ordering | Rekkjefølgje | button above the side panel of the main window that shows the order of the controls of the course (its topology), beside Descriptions; the term Ordering |  |
| `MainFrame_mapViewerTopology_Text` | mapViewerTopology | (not needed) | the Text of the WinForms control mapViewerTopology, which shows nowhere (the Avalonia view uses only its name); the languages that translate it keep the name or translate the name of the control |  |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | Lag _bildefiler... | item of the File menu of the main window; the term Create Image Files; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createDescriptionPdfMenu_Text` | _Descriptions... | _Postbeskrivelser... | item of the submenu Create PDFs of the File menu: makes PDF files of the control descriptions; the term Descriptions |  |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | _Klippekort... | item of the submenu Create PDFs of the File menu: makes PDF files of the punch cards; the term Punch Cards |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | _Løyper... | item of the submenu Create PDFs of the File menu: makes PDF files of the course maps; the term Courses | UIText.AddTextLine_coursesLabel_Text UIText.CreateGpx_coursesGroupBox_Text |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | Lag P_DF-filer | item of the File menu of the main window, a submenu of the PDF files of the descriptions, punch cards and courses; the term Create PDFs; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | Lag RouteGadget-filer... | item of the submenu Create Route Review Files: makes the files of RouteGadget, a program that shows the routes that the competitors ran; the name RouteGadget stays; the term Create RouteGadget Files | nb-NO |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Lag filer for vegvalsanalyse | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English; "vegval" is the route choice of Norwegian orienteering |  |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Lag data_utvekslingsfil (IOF XML)... | item of the File menu of the main window: makes the IOF XML file of the courses, which other programs read; the term IOF XML; its access key is a letter that no other item of the File menu uses | nb-NO |
| `MainFrame_createGPXFileMenu_Text` | Create G_PX File... | Lag G_PX-fil... | item of the File menu of the main window; the term Create GPX File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Lag _KML-fil... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_printAreaThisPartMenu_Text` | This _Part Only | _Berre denne delen | item of the submenu Set Print Area of the File menu: the print area of the course part shown; the term This Part Only |  |
| `MainFrame_exitMenu_Text` | E_xit | _Avslutt<br>(corrects: E_xit) | "Exit" was English (the term Exit); the access key "a" is a letter that no other item of the File menu uses |  |

#### pl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addVariationToolStripButton_Text` | Add Variation | Dodaj wariant | label of a button of the toolbar of the main window, which adds a variation (a fork or a loop) to the course; the term Add Variation |  |
| `MainFrame_radioButtonDescriptions_Text` | Descriptions | Opisy | button above the side panel of the main window that shows the control descriptions, beside Ordering; the term Descriptions |  |
| `MainFrame_radioButtonTopology_Text` | Ordering | Kolejność | button above the side panel of the main window that shows the order of the controls of the course (its topology), beside Descriptions; the term Ordering |  |
| `MainFrame_mapViewerTopology_Text` | mapViewerTopology | (not needed) | the Text of the WinForms control mapViewerTopology, which shows nowhere (the Avalonia view uses only its name); the languages that translate it keep the name or translate the name of the control |  |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | Utwórz pliki o_brazów... | item of the File menu of the main window; the term Create Image Files; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createDescriptionPdfMenu_Text` | _Descriptions... | _Opisy... | item of the submenu Create PDFs of the File menu: makes PDF files of the control descriptions; the term Descriptions |  |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | _Karty do perforacji... | item of the submenu Create PDFs of the File menu: makes PDF files of the punch cards; the term Punch Cards |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | _Trasy... | item of the submenu Create PDFs of the File menu: makes PDF files of the course maps; the term Courses | UIText.AddTextLine_coursesLabel_Text UIText.CreateGpx_coursesGroupBox_Text |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | Utwórz pliki PD_F | item of the File menu of the main window, a submenu of the PDF files of the descriptions, punch cards and courses; the term Create PDFs; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | Utwórz pliki RouteGadget... | item of the submenu Create Route Review Files: makes the files of RouteGadget, a program that shows the routes that the competitors ran; the name RouteGadget stays; the term Create RouteGadget Files | UIText.CreateRouteGadget_Text $this.Text |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Utwórz pliki do analizy biegu | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English; "the analysis of the run", as fr "l'analyse de course": "trasa" is the course and "wariant" the variation |  |
| `MainFrame_createGPXFileMenu_Text` | Create G_PX File... | Utwórz plik GP_X... | item of the File menu of the main window; the term Create GPX File; its access key is a letter that no other item of the File menu uses | UIText.CreateGpx_Text UIText.MainFrame_saveGpxFileDialog_Title |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Utwórz plik _KML... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_addTextMenu_Text` | Text | Tekst<br>(corrects: Opis) | "Opis" is a description; the item adds a text (the term Text), as the earlier groups corrected it |  |
| `MainFrame_textToolStripMenuItem_Text` | Text | Tekst<br>(corrects: Opis) | "Opis" is a description; the item adds a text (the term Text), as the earlier groups corrected it |  |
| `MainFrame_addControlToolStripButton_Text` | Add Control | Dodaj PK<br>(corrects: Dodaj Punkt) | the language's control is "PK", as "Wszystkie PK"; "punkty" are also the score points (the term Add Control) |  |

#### pt

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Criar Ficheiro de _Intercâmbio de Dados (IOF XML)...<br>(corrects: Criar Data _Interchange File (IOF XML)...) | "Data Interchange File" was English, as pt-BR "Criar Arquivo de Intercâmbio de Dados" (the terms Create, File and IOF XML) |  |
| `MainFrame_exitMenu_Text` | E_xit | _Sair<br>(corrects: _Saír) | "Saír" has a wrong accent (the term Exit) |  |

#### pt-BR

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_radioButtonTopology_Text` | Ordering | Ordenação<br>(corrects: Ordenando) | "Ordenando" is a gerund; the button names the ordering, as pt (the term Ordering) |  |
| `MainFrame_openFileDialog_Filter` | Purple Pen files\|*.ppen\|All files\|*.* | Arquivos Purple Pen\|*.ppen\|Todos os arquivos\|*.*<br>(corrects: Arquivos Purple Pen \|*.ppen\|All files\|*.*) | "All files" was English, and a space ended the first name (the terms Files and All) |  |
| `MainFrame_saveFileDialog_Filter` | Purple Pen file\|*.ppen | Arquivo Purple Pen\|*.ppen<br>(corrects: Arquivo Purple Pen \|*.ppen) | a space ended the name; nothing else changes |  |
| `MainFrame_createOcadFilesMenu_Text` | Create OCAD _Files... | Criar Arquivos _OCAD...<br>(corrects: Criar _Ficheiros OCAD...) | "Ficheiros" is European Portuguese; Brazil says "Arquivos" (the term Create OCAD Files); the access key "o" is a letter that no other item of the File menu uses |  |
| `MainFrame_fileMenu_Text` | _File | Ar_quivo<br>(corrects: _Ficheiro/Arquivo) | "Ficheiro/Arquivo" held two words, and "Ficheiro" is European Portuguese (the term File); the access key "q" is a letter that no other menu of the menu bar uses ("a" is Adicionar and Ajuda) |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addVariationToolStripButton_Text` | Add Variation | Adăugare variantă | label of a button of the toolbar of the main window, which adds a variation (a fork or a loop) to the course; the term Add Variation |  |
| `MainFrame_saveGpxFileDialog_Title` | Create GPX File | Creare fișier GPX | title of the file dialog that saves the GPX file; the term Create GPX File |  |
| `MainFrame_radioButtonDescriptions_Text` | Descriptions | Descrieri | button above the side panel of the main window that shows the control descriptions, beside Ordering; the term Descriptions |  |
| `MainFrame_radioButtonTopology_Text` | Ordering | Ordine | button above the side panel of the main window that shows the order of the controls of the course (its topology), beside Descriptions; the term Ordering |  |
| `MainFrame_mapViewerTopology_Text` | mapViewerTopology | (not needed) | the Text of the WinForms control mapViewerTopology, which shows nowhere (the Avalonia view uses only its name); the languages that translate it keep the name or translate the name of the control |  |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | Creare fișiere ima_gine... | item of the File menu of the main window; the term Create Image Files; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createDescriptionPdfMenu_Text` | _Descriptions... | _Descrieri... | item of the submenu Create PDFs of the File menu: makes PDF files of the control descriptions; the term Descriptions |  |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | _Fișe de compostare... | item of the submenu Create PDFs of the File menu: makes PDF files of the punch cards; the term Punch Cards |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | _Trasee... | item of the submenu Create PDFs of the File menu: makes PDF files of the course maps; the term Courses | UIText.AddTextLine_coursesLabel_Text UIText.CreateGpx_coursesGroupBox_Text |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | Creare fișiere PD_F | item of the File menu of the main window, a submenu of the PDF files of the descriptions, punch cards and courses; the term Create PDFs; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | Creare fișiere RouteGadget... | item of the submenu Create Route Review Files: makes the files of RouteGadget, a program that shows the routes that the competitors ran; the name RouteGadget stays; the term Create RouteGadget Files | UIText.CreateRouteGadget_Text $this.Text |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Creare fișiere pentru analiza rutelor | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English |  |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Creare fișier de schimb de date (IOF _XML)... | item of the File menu of the main window: makes the IOF XML file of the courses, which other programs read; the term IOF XML; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createGPXFileMenu_Text` | Create G_PX File... | Creare fișier G_PX... | item of the File menu of the main window; the term Create GPX File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Creare fișier _KML... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_printAreaThisPartMenu_Text` | This _Part Only | Doar această _parte | item of the submenu Set Print Area of the File menu: the print area of the course part shown; the term This Part Only |  |
| `MainFrame_newEventMenu_Text` | _New Event... | Eveniment _Nou...<br>(corrects: Evniment _Nou...) | the typo "Evniment" (the term New Event) |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addControlToolStripButton_Text` | Add Control | Добавить КП | label of a button of the toolbar of the main window; the term Add Control |  |
| `MainFrame_addStartToolStripButton_Text` | Add Start | Добавить старт | label of a button of the toolbar of the main window; the term Add Start |  |
| `MainFrame_addFinishToolStripButton_Text` | Add Finish | Добавить финиш | label of a button of the toolbar of the main window; the term Add Finish |  |
| `MainFrame_saveGpxFileDialog_Title` | Create GPX File | Создать файл GPX | title of the file dialog that saves the GPX file; the term Create GPX File |  |
| `MainFrame_tabPage1_Text` | All Controls | Все КП | name of the view of all controls (the WinForms tab); the term All Controls (the view that holds every control) |  |
| `MainFrame_radioButtonDescriptions_Text` | Descriptions | Легенды | button above the side panel of the main window that shows the control descriptions, beside Ordering; the term Descriptions |  |
| `MainFrame_radioButtonTopology_Text` | Ordering | Порядок | button above the side panel of the main window that shows the order of the controls of the course (its topology), beside Descriptions; the term Ordering |  |
| `MainFrame_mapViewerTopology_Text` | mapViewerTopology | (not needed) | the Text of the WinForms control mapViewerTopology, which shows nowhere (the Avalonia view uses only its name); the languages that translate it keep the name or translate the name of the control |  |
| `MainFrame_openFileDialog_Filter` | Purple Pen files\|*.ppen\|All files\|*.* | Файлы Purple Pen\|*.ppen\|Все файлы\|*.* | filter of the file dialog that opens an event: a name and a pattern for each choice, separated by "\|"; the patterns stay as they are; the terms Files and All |  |
| `MainFrame_saveFileDialog_Filter` | Purple Pen file\|*.ppen | Файл Purple Pen\|*.ppen | filter of the file dialog that saves an event: the name and the pattern, separated by "\|"; the pattern stays as it is; the term File |  |
| `MainFrame_newEventMenu_Text` | _New Event... | _Новое соревнование... | item of the File menu of the main window, which starts the wizard of a new event; the term New Event; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_openMenu_Text` | _Open... | _Открыть... | item of the File menu of the main window; the term Open; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_saveMenu_Text` | _Save | _Сохранить | item of the File menu of the main window; the term Save; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_saveAsMenu_Text` | Save _As... | Сохранить _как... | item of the File menu of the main window; the term Save As; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createOcadFilesMenu_Text` | Create OCAD _Files... | Создать _файлы OCAD... | item of the File menu of the main window of the WinForms application (the Avalonia menu takes CommandNameText.CreateMapFiles instead); the term Create OCAD Files |  |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | Создать файлы _изображений... | item of the File menu of the main window; the term Create Image Files; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createDescriptionPdfMenu_Text` | _Descriptions... | _Легенды... | item of the submenu Create PDFs of the File menu: makes PDF files of the control descriptions; the term Descriptions |  |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | _Контрольные карточки... | item of the submenu Create PDFs of the File menu: makes PDF files of the punch cards; the term Punch Cards |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | _Дистанции... | item of the submenu Create PDFs of the File menu: makes PDF files of the course maps; the term Courses | UIText.PrintDescriptions_coursesGroupBox_Text |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | Создать P_DF | item of the File menu of the main window, a submenu of the PDF files of the descriptions, punch cards and courses; the term Create PDFs; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | Создать файлы RouteGadget... | item of the submenu Create Route Review Files: makes the files of RouteGadget, a program that shows the routes that the competitors ran; the name RouteGadget stays; the term Create RouteGadget Files |  |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Создать файлы для анализа путей | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English; "путь" is the route that a competitor ran, as "выбор пути"; "дистанция" is the course |  |
| `MainFrame_createXmlMenu_Text` | Create Data _Interchange File (IOF XML)... | Создать файл о_бмена данными (IOF XML)... | item of the File menu of the main window: makes the IOF XML file of the courses, which other programs read; the term IOF XML; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createGPXFileMenu_Text` | Create G_PX File... | Создать файл G_PX... | item of the File menu of the main window; the term Create GPX File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Создать файл _KML... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_printDescriptionsMenu_Text` | _Print Descriptions... | _Печать легенд... | item of the File menu of the main window; the term Print Descriptions; its access key is a letter that no other item of the File menu uses | UIText.PrintDescriptions_Text |
| `MainFrame_printPunchCardsMenu_Text` | Print Pu_nch Cards... | Печать контрольных к_арточек... | item of the File menu of the main window; the term Print Punch Cards; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_printCoursesMenu_Text` | Print _Courses... | Печать _дистанций... | item of the File menu of the main window; the term Print Courses; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_printAreaThisPartMenu_Text` | This _Part Only | Только эта _часть | item of the submenu Set Print Area of the File menu: the print area of the course part shown; the term This Part Only |  |
| `MainFrame_printAreaThisCourseMenu_Text` | _This Course | _Эта дистанция | item of the submenu Set Print Area of the File menu: the print area of the course shown; the term This Course |  |
| `MainFrame_printAreaAllCoursesMenu_Text` | _All Courses | _Все дистанции | item of the submenu Set Print Area of the File menu: one print area for all courses; the term All courses |  |
| `MainFrame_setPrintAreaMenu_Text` | Set Print A_rea | _Задать область печати | item of the File menu of the main window, a submenu that sets the print area; the term Set Print Area; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_programLanguageMenu_Text` | Program _Language... | _Язык программы... | item of the File menu of the main window, which opens the dialog of the language of the program; the term Program Language; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_exitMenu_Text` | E_xit | _Выход | last item of the File menu, which closes the program; the term Exit; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_fileMenu_Text` | _File | _Файл | menu File of the menu bar of the main window; the term File; its access key is a letter that no other menu of the menu bar uses |  |
| `MainFrame_cancelMenu_Text` | _Clear Selection | _Снять выделение | item of the Edit menu of the WinForms application (the Avalonia menu takes MiscText.ClearSelectionWithShortcut); the term Clear Selection |  |
| `MainFrame_undoMenu_Text` | _Undo | _Отменить | item of the Edit menu of the WinForms application (the menu shows MiscText.UndoWithShortcut with the name of the command); the term Undo |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_addVariationToolStripButton_Text` | Add Variation | Dodaj varianto | label of a button of the toolbar of the main window, which adds a variation (a fork or a loop) to the course; the term Add Variation |  |
| `MainFrame_saveGpxFileDialog_Title` | Create GPX File | Ustvari datoteko GPX | title of the file dialog that saves the GPX file; the term Create GPX File |  |
| `MainFrame_radioButtonDescriptions_Text` | Descriptions | Opisi | button above the side panel of the main window that shows the control descriptions, beside Ordering; the term Descriptions |  |
| `MainFrame_radioButtonTopology_Text` | Ordering | Vrstni red | button above the side panel of the main window that shows the order of the controls of the course (its topology), beside Descriptions; the term Ordering |  |
| `MainFrame_mapViewerTopology_Text` | mapViewerTopology | (not needed) | the Text of the WinForms control mapViewerTopology, which shows nowhere (the Avalonia view uses only its name); the languages that translate it keep the name or translate the name of the control |  |
| `MainFrame_createImageFilesMenu_Text` | Create Ima_ge Files... | Ustvari s_likovne datoteke... | item of the File menu of the main window; the term Create Image Files; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createDescriptionPdfMenu_Text` | _Descriptions... | _Opisi... | item of the submenu Create PDFs of the File menu: makes PDF files of the control descriptions; the term Descriptions |  |
| `MainFrame_createPunchcardPdfMenu_Text` | Pu_nch Cards... | _Kontrolni kartoni... | item of the submenu Create PDFs of the File menu: makes PDF files of the punch cards; the term Punch Cards |  |
| `MainFrame_createCoursePdfMenu_Text` | _Courses... | _Proge... | item of the submenu Create PDFs of the File menu: makes PDF files of the course maps; the term Courses | UIText.AddTextLine_coursesLabel_Text UIText.CreateGpx_coursesGroupBox_Text |
| `MainFrame_createPDFsMenu_Text` | Create P_DFs | Ustvari PD_F-je | item of the File menu of the main window, a submenu of the PDF files of the descriptions, punch cards and courses; the term Create PDFs; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | Ustvari datoteke RouteGadget... | item of the submenu Create Route Review Files: makes the files of RouteGadget, a program that shows the routes that the competitors ran; the name RouteGadget stays; the term Create RouteGadget Files | UIText.CreateRouteGadget_Text $this.Text |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Ustvari datoteke za analizo poti | item of the File menu of the main window: a submenu of the files of the programs that review the routes that the competitors ran after the race, such as RouteGadget; no access key, as the English; "pot" is the route that a competitor ran; "proga" is the course |  |
| `MainFrame_createGPXFileMenu_Text` | Create G_PX File... | Ustvari datoteko GP_X... | item of the File menu of the main window; the term Create GPX File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Ustvari datoteko K_ML... | item of the File menu of the main window; the term Create KML File; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_printPunchCardsMenu_Text` | Print Pu_nch Cards... | Tiskaj kontrolne k_artone... | item of the File menu of the main window; the term Print Punch Cards; its access key is a letter that no other item of the File menu uses |  |
| `MainFrame_printDescriptionsMenu_Text` | _Print Descriptions... | _Tiskaj opise kontrolnih točk...<br>(corrects: _Tiskaj Opise kontrolnih točk...) | "Opise" takes the small letter of the language, as the title of the dialog (the term Print Descriptions) |  |
| `MainFrame_printAreaThisPartMenu_Text` | This _Part Only | _Samo ta del<br>(corrects: Izbran _Samo delno) | "Izbran Samo delno" means "selected only partly" (the term This Part Only); the access key stays "s" |  |
| `MainFrame_programLanguageMenu_Text` | Program _Language... | _Jezik programa...<br>(corrects: Program _Jezik...) | "Program Jezik" keeps the order of the English; the language of the program is "jezik programa" (the term Program Language) |  |

#### sv

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createRouteReviewFilesToolStripMenuItem_Text` | Create Route Review Files | Skapa filer för vägvalsanalys<br>(corrects: Skapa bangranskningsfiler) | "bangranskning" is the check of the courses before the race; the submenu makes the files of the programs that review the routes ("vägval") that the competitors ran, such as RouteGadget |  |

#### tr

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
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

#### uk

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_mapViewerTopology_Text` | mapViewerTopology | (not needed) | the Text of the WinForms control mapViewerTopology, which shows nowhere (the Avalonia view uses only its name); the languages that translate it keep the name or translate the name of the control |  |
| `MainFrame_newEventMenu_Text` | _New Event... | _Нові змагання...<br>(corrects: _Нова подія...) | "подія" is an occurrence; the language's event is "змагання" (the term New Event) |  |
| `MainFrame_createKMLFileMenu_Text` | Create _KML File... | Створити файл _KML...<br>(corrects: Сворити файл _KML...) | the typo "Сворити" (the term Create KML File) |  |

#### zh-CN

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_createRouteGadgetFilesMenu_Text` | Create RouteGadget Files... | 创建RouteGadget文件…<br>(corrects: 创建路径和文件的小工具…) | 路径和文件的小工具 ("a gadget of routes and files") translated the name RouteGadget (the term Create RouteGadget Files) |  |
| `MainFrame_printAreaThisPartMenu_Text` | This _Part Only | 仅此部分(_P)<br>(corrects: 这部分只) | 这部分只 puts "only" at the end (the term This Part Only), with the access key of the English |  |

#### zh-TW

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `MainFrame_radioButtonTopology_Text` | Ordering | 排序<br>(corrects: 排序中) | 排序中 means "sorting in progress" (the term Ordering) |  |
| `MainFrame_openFileDialog_Filter` | Purple Pen files\|*.ppen\|All files\|*.* | Purple Pen 檔案\|*.ppen\|所有檔案\|*.*<br>(corrects: Purple Pen 檔案\|*.ppen\|All files\|*.*) | "All files" was English (the terms Files and All) |  |
| `MainFrame_createOcadFilesMenu_Text` | Create OCAD _Files... | 建立OCAD檔案(_F)...<br>(corrects: 建立OCAD _檔案(_F)...) | the item had two access-key marks, "_檔" and "(_F)"; the second stays, as the language's other items |  |
