# The SVG export's strings in every language

Task 4 of [the handover](SvgTargetProgress.md), Nov 13: the report of the agent that translated the SVG export's
strings, as it gave it, kept so that a review of the translations, by @Pauli or by Purple Pen's translators before
they are offered upstream, can see what decided each one. @Pauli's condition was that each and every translation
weigh its context. What @Pauli decided of the choices below is under "Decisions" in the handover.

I've translated the SVG export's strings into 30 of the 32 languages and merged them into `feature/svg-target` and `dev`. Every full run passed, and the packages download. Kurmanji (ku) and Venetian (vec) are held back as drafts, because I can't vouch for them; that and three other choices are @Pauli's (below).

## Branch, commits and runs
- **`feature/svg-translations`**, rooted at `feature/svg-target` 5bf82d8e. Nothing changes `UIText.resx` itself, an upstream test, a baseline or the handover.
  - cf77aee0: the ten dialog and menu keys in the 25 satellites whose PDF dialog is translated: bg cs da de en-GB es et fi fr hu it ja nb-NO nl nn-NO pl pt pt-BR ro sl sv tr uk zh-CN zh-TW.
  - 60e28d5a: the same keys in five files that translate almost nothing: ca, he, id, ru, zh-HK.
  - 1da0e9e2: the export's error message `CannotCreateSvgs` in the 22 `PurplePenCore/MiscText.<lang>.resx` files that exist (none for it, sl or the near-empty languages).
- **`wip/linux/svg-translations`** 705a9ba2: `base` 68228505 with the branch merged in, no conflict, and ce9f4e3's two baselines. Tests 43400766255 and Build 43400766275 both passed.
  - Release `trial-svg-translations` (4.0.263.220) has a deb, rpm and AppImage.
  - Inside the deb, `NotPurplePen.resources.dll` for de, ja and fi and `PurplePenCore.resources.dll` for de contain the new strings.
- **`feature/svg-target`**: fast-forwarded 5bf82d8e → 1da0e9e2.
- **`wip/linux/svg-target-into-dev`** 8042a0da: made anew on `dev` 2125b829, as a fast-forward of the existing branch.
  - The Finnish UIText and MiscText files conflicted only because `dev`'s own features also append Finnish strings at the end. I kept `dev`'s entries and put ours after them.
  - Tests 43402827580 and Build 43402827686 passed.
- **`dev`**: fast-forwarded 2125b829 → 8042a0da. Its Build 43404417608 published `channel-beta` 4.0.265.220 for Linux, Windows and macOS, and the Windows .exe downloads.
- Five review images, the dialog rendered in every language and the English clipping, were sent to @Pauli in the session.

## Which strings, and how room was checked
Comparing `UIText.resx` with `upstream/master` gives ten keys. Everything else the dialog, its code-behind and the command show reuses existing keys. One more English-only string turned up in `PurplePenCore/MiscText.resx`: the error heading "Cannot create SVG files for the following reason:". It is shown when the export fails, so I translated it too (K11).

| Key | | English | Where it appears |
|---|---|---|---|
| K1 | `MainFrame_createSvgFilesMenu_Text` | Create S_VG Files... | File menu item between Image Files and PDFs; opens the dialog |
| K2 | `CreateSvgCourses_Text` | Create SVG Files | window title |
| K3 | `CreateSvgCourses_bitmapsGroupBox_Text` | Bitmaps | group header, beside Courses/Appearance/Files/Folder |
| K4 | `CreateSvgCourses_bitmapsLabel_Text` | Bitmaps in the map: | label beside the bitmaps combo |
| K5–K7 | `CreateSvgCourses_comboBoxBitmaps_Items`, `_Items1`, `_Items2` | Embed in each file / Save beside each file / Leave out | combo items |
| K8 | `CreateSvgCourses_jpegQualityLabel_Text` | JPEG quality: | label beside a 1–100 box |
| K9 | `CreateSvgCourses_checkBoxCompress_Text` | Compress the files (.svgz) | check box in the Bitmaps group |
| K10 | `CreateSvgCourses_checkBoxDrawAreaPatternsAsSeparateShapes_Text` | Draw area patterns as separate shapes | check box under "Print Map Exchanges on Same Map" |
| K11 | `MiscText.CannotCreateSvgs` | Cannot create SVG files for the following reason: | error heading; goes through `string.Format`, so no braces |

To judge room, I built the app locally and rendered the real dialog headlessly in every language, measuring each text against its space.
- **The bitmaps combo is narrow.** Its text gets 232 px minus the wider of the K4 and K8 labels, about 90 to 150 px. Even the English "Save beside each file" is cut to "Save beside each fi".
- **What I did about it.** Where a full translation didn't fit, "each file" became "the file", or "save beside" became "save separately", and the table says so for each language.
- **Labels shortened.** For room, the K4 label drops part of its wording in bg, et, sl, uk, ru and es; es also avoids the clumsy "mapas de bits del mapa".
- **Wrapped check boxes.** In bg, es, fr, ja and pt the dialog's existing strings already wrap. A wrapped K10 then pushed the "Other folder" radio button out of its group, so K10 is kept to one line there.
- **Final state.** Every new string fits except the English and en-GB K6, which clips by 10 px. K10 wraps to two lines in ca, hu, ro, ru and uk, with nothing cut off.
- **Access keys.** Every menu access key is unique in its File menu. Finnish uses O and Hungarian S, because V is already taken there.

## For @Pauli to decide
1. **ku and vec are held back, not committed.** Drafts are at the end. Options: (a) ship the drafts, (b) wait for a speaker's review, (c) leave them in English. I recommend (b), or (c) if no reviewer turns up. vec is the one that matters: it ships on Linux and macOS.
2. **The five near-empty files (ca, he, id, ru, zh-HK) are translated and merged in their own commit, 60e28d5a**, because the brief says every language. In them the PDF dialog and the File menu are English, so the SVG strings are the only translated ones nearby. None of these files ships anyway: the installers leave out ca, en-GB, he, id, ku, ru, sl and zh-HK (`LinuxInstaller/publish-exclude.txt`, `MacInstaller/publish-exclude.txt`, `Innosetup/PurplePen.iss`). I recommend keeping them; reverting that one commit removes them.
3. **The bitmaps combo's layout.** Giving the combo a row of its own, as the multi-page combo has, would give it about 228 px. The English would then fit, and the translations could say "each file" again. That is a layout change I didn't make.
4. **A string with no key of its own.** The SVG dialog's folder picker is titled "Select a folder for the PDF files" (`CreatePdfCourses_folderBrowserDialog_Description`). It needs an SVG key, which I didn't add.

## Problems found in upstream's translations (not changed)
- **"Include:" is English in every language.** The row's label and its items "Course and map"/"Course only" have no translation in either the PDF or the SVG dialog. The satellites hold the same strings under the `CreateImageFiles_*` keys.
- **The PDF dialog is titled "Print Courses".** In de, en-GB, et, fi, fr, nb-NO, nn-NO and ro, `CreatePdfCourses_Text` reads "Print Courses" (or its translation). The SVG titles follow the OCAD and image export titles instead.
- **Japanese error messages crash.** ja's MiscText `CannotCreatePdfs` and `CannotCreateOcadFiles` contain a `{0}` that `HandleExceptions` formats with no arguments. A failed export would throw `FormatException` instead of showing its message. This is worth a `fix/` branch.
- **zh-CN OCAD menu item.** `CommandNameText.CreateMapFiles` turns the access-key `&` into 和 ("and"), so the item reads "创建OCAD和文件...".
- **"Select folder..." is cut off.** With "Other folder" chosen, the button is cut by 13 px in the SVG dialog and 23 px in the PDF dialog, even in English.

## Doubts, by language
- **bg:** K4 drops "in the map". "поотделно" means "one by one" rather than "as separate shapes". "шарка" is a general word for pattern.
- **cs:** "vzory ploch" for area patterns.
- **da:** "flademønstre"; orienteers may say "fladesignaturer".
- **de:** "Separat speichern" doesn't say where the files go.
- **es:** "tramas" for area patterns, rather than "patrones" or "relleno".
- **et:** "Tihenda" for compress, "Manusta" for embed, "alade mustrid" for area patterns.
- **fi:** "alueiden kuviot" for area patterns; the access key is O.
- **fr:** K10 drops "surfaciques"; "Intégrer" chosen over "Incorporer".
- **hu:** "Területminták" is my own compound.
- **it:** "motivi areali"; the file itself mixes "carta" and "mappa".
- **ja:** 面 for area symbols.
- **nb-NO:** "Bilder" chosen, although the file also uses "Bitmap".
- **nn-NO:** the Nynorsk forms.
- **ro:** "modelele de suprafață"; the file has no word for bitmap, so "Imagini".
- **sl:** "Bitne slike" (no term in the file); "Kvaliteta" follows the file's non-standard word.
- **tr:** "Ayrı kaydet" ("save separately").
- **uk:** "Зберегти поруч" has no object.
- **zh-CN:** I added the "(_V)" access key, although the file's other create items have none.
- **he:** "מפות סיביות במפה" repeats "map", since the Hebrew for bitmap is literally "map of bits".
- **ku and vec:** see the held drafts.
- **pt and pt-BR:** "forma a forma" is idiomatic but a little informal.

## Review tables
Each table gives, for every key, the English, the translation, the context that decided it, and the existing key it mirrors.

#### bg
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Създай S_VG файлове... | imperative, key on the format name as the GPX item | MainFrame_createGPXFileMenu_Text "Създай G_PX файл..." |
| K2 | Create SVG Files | Създай SVG файлове | window title | CreatePdfCourses_Text "Създай PDF файлове" |
| K3 | Bitmaps | Растерни изображения | the file's word for bitmap, растерн- | NewEventBitmapScale_labelTitle_Text "Резолюция на растерна карта", MainFrame_addImageMenu_Text "Изображение" |
| K4 | Bitmaps in the map: | Изображения: | room: "Изображения в картата:" (150 px) left the combo 82 px; the header says растерни | MainFrame_addImageMenu_Text "Изображение" |
| K5 | Embed in each file | Вгради | imperative as the combo items; "във всеки файл" doesn't fit | CreatePdfCourses_comboBoxMultiPage_Items "Ограничи до една страница" |
| K6 | Save beside each file | Запиши отделно | "save separately"; "до всеки файл" doesn't fit | MainFrame_saveMenu_Text "_Запиши" |
| K7 | Leave out | Пропусни | imperative | CreatePdfCourses_comboBoxMultiPage_Items "Ограничи..." |
| K8 | JPEG quality: | Качество на JPEG: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Качество на картата" |
| K9 | Compress the files (.svgz) | Компресирай файловете (.svgz) | imperative as the check boxes | CreatePdfCourses_checkBoxMergeParts_Text "Печатай смените на карта...", CourseAppearanceDialog_checkBoxStandardSizes_Text "Използвай..." |
| K10 | Draw area patterns as separate shapes | Рисувай шарките на площите поотделно | one line: "като отделни фигури" wrapped and pushed "Other folder" out | CourseAppearanceDialog_checkBoxStandardSizes_Text "Използвай..." |
| K11 | Cannot create SVG files for the following reason: | Не може да се създадат SVG файлове поради следната причина: | error heading | MiscText CannotCreatePdfs "Не може да се създадат PDF файлове поради следната причина:" |

#### cs
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Vytvořit soubory S_VG... | infinitive, noun before the format as in the menu | MainFrame_createRouteGadgetFilesMenu_Text "Vytvořit soubory Route_Gadget..." |
| K2 | Create SVG Files | Vytvořit soubory SVG | the PDF title "Vyrob PDF soubory" uses a familiar imperative no other title uses | CreateOcadFiles_Text "Vytvořit soubory OCAD" |
| K3 | Bitmaps | Bitmapy | group header | NewEventBitmapScale_labelTitle_Text "Rozlišení bitmapu" |
| K4 | Bitmaps in the map: | Bitmapy v mapě: | label beside the combo | CreatePdfCourses_filesLabel_Text "Soubory:" |
| K5 | Embed in each file | Vložit do souboru | infinitive; "each file" as "the file" for room | CreatePdfCourses_comboBoxMultiPage_Items "Oříznout na jednu stránku" |
| K6 | Save beside each file | Uložit vedle souboru | "each file" as "the file" for room | MainFrame_saveMenu_Text "_Uložit" |
| K7 | Leave out | Vynechat | infinitive | CreatePdfCourses_comboBoxMultiPage_Items1 "Tisknout na více stránek" |
| K8 | JPEG quality: | Kvalita JPEG: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Kvalita mapy" |
| K9 | Compress the files (.svgz) | Komprimovat soubory (.svgz) | infinitive as the check boxes | CreatePdfCourses_checkBoxMergeParts_Text "Tisknout výměny map na té samé mapě" |
| K10 | Draw area patterns as separate shapes | Kreslit vzory ploch jako samostatné tvary | plocha for area symbols | CreatePdfCourses_checkBoxMergeParts_Text "Tisknout..." |
| K11 | Cannot create SVG files for the following reason: | Nelze vytvořit soubory SVG z tohoto důvodu: | without the colloquial "PDFko" | MiscText CannotCreatePdfs "Nelze vytvořit PDFko z tohoto důvodu:" |

#### da
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Opret S_VG-filer... | File menu item, opens the dialog | MainFrame_createImageFilesMenu_Text "Opret bill_edfiler...", CreatePdfCourses_Text "Opret PDF-filer" |
| K2 | Create SVG Files | Opret SVG-filer | window title | CreatePdfCourses_Text "Opret PDF-filer" |
| K3 | Bitmaps | Bitmaps | group header | NewEventBitmapScale_labelTitle_Text "Bitmap opløsning" |
| K4 | Bitmaps in the map: | Bitmaps i kortet: | label beside the combo | CreatePdfCourses_filesLabel_Text "Filer:" |
| K5 | Embed in each file | Indlejr i filen | imperative; "the file" as in K6 | CreatePdfCourses_comboBoxMultiPage_Items "Skrump til en enkelt side" |
| K6 | Save beside each file | Gem ved siden af filen | "each file" as "the file" for room | MainFrame_saveMenu_Text "_Gem" |
| K7 | Leave out | Udelad | imperative | CreatePdfCourses_comboBoxMultiPage_Items1 "Udskriv på flere sider" |
| K8 | JPEG quality: | JPEG-kvalitet: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "Kort_kvalitet" |
| K9 | Compress the files (.svgz) | Komprimér filerne (.svgz) | imperative, accented as "Skalér" | CourseAppearanceDialog_checkBoxStandardSizes_Text "Brug IOF standard størrelser", CourseAppearanceDialog_comboBoxScaleItemSizes_Items "Skalér ikke" |
| K10 | Draw area patterns as separate shapes | Tegn flademønstre som separate figurer | flade for area symbols | CreatePdfCourses_checkBoxMergeParts_Text "Udskriv kortUdvekslinger på samme kort" |
| K11 | Cannot create SVG files for the following reason: | Kan ikke oprette SVG-filer af følgende grund: | error heading | MiscText CannotCreatePdfs "Kan ikke oprette PDFs af følgende grund:" |

#### de
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | S_VG-Dateien erstellen... | File menu item, opens the dialog | MainFrame_createKMLFileMenu_Text "_KML-Datei erstellen...", CreateOcadFiles_Text "OCAD-Dateien erstellen" |
| K2 | Create SVG Files | SVG-Dateien erstellen | the PDF dialog's title "Bahnen drucken" is the print dialog's | CreateOcadFiles_Text "OCAD-Dateien erstellen" |
| K3 | Bitmaps | Bilder | the file calls a bitmap "Bild" | NewEventBitmapScale_labelTitle_Text "Bildauflösung", MainFrame_addImageMenu_Text "Bild" |
| K4 | Bitmaps in the map: | Bilder in der Karte: | label beside the combo | CreatePdfCourses_filesLabel_Text "Dateien:" |
| K5 | Embed in each file | Einbetten | infinitive; "In jede Datei einbetten" (133 px) doesn't fit 121 | CreatePdfCourses_comboBoxMultiPage_Items "Auf eine Seite zuschneiden" |
| K6 | Save beside each file | Separat speichern | "Neben jeder Datei speichern" (167 px) doesn't fit; "Daneben" alone could mean beside the map | MainFrame_saveMenu_Text "_Speichern" |
| K7 | Leave out | Weglassen | infinitive | CreatePdfCourses_comboBoxMultiPage_Items1 "Auf mehrere Seiten drucken" |
| K8 | JPEG quality: | JPEG-Qualität: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "Karten_qualität" |
| K9 | Compress the files (.svgz) | Dateien komprimieren (.svgz) | infinitive as the check boxes | CreatePdfCourses_checkBoxMergeParts_Text "Kartenwechsel auf gleicher Karte drucken" |
| K10 | Draw area patterns as separate shapes | Flächenmuster als Einzelformen zeichnen | Fläche for area; one line, where "als einzelne Formen" wraps | MainFrame_whiteOutMenu_Text "Weiße Fläche" |
| K11 | Cannot create SVG files for the following reason: | SVG-Dateien können aus folgendem Grund nicht erstellt werden: | error heading | MiscText CannotCreatePdfs "PDFs können aus folgendem Grund nicht erzeugt werden:", CannotCreateOcadFiles "...nicht erstellt werden:" |

#### en-GB
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Create S_VG Files... | no British spelling differs | MainFrame_createImageFilesMenu_Text "Create Ima_ge Files..." |
| K2 | Create SVG Files | Create SVG Files | this file titles the PDF dialog "Print Courses" | CreateOcadFiles_Text "Create OCAD Files" |
| K3 | Bitmaps | Bitmaps | group header | NewEventBitmapScale_labelTitle_Text "Bitmap Resolution" |
| K4 | Bitmaps in the map: | Bitmaps in the map: | label beside the combo | — |
| K5 | Embed in each file | Embed in each file | combo item | — |
| K6 | Save beside each file | Save beside each file | clipped in English too (123 px of 113) | — |
| K7 | Leave out | Leave out | combo item | — |
| K8 | JPEG quality: | JPEG quality: | label beside the 1–100 box | — |
| K9 | Compress the files (.svgz) | Compress the files (.svgz) | check box | — |
| K10 | Draw area patterns as separate shapes | Draw area patterns as separate shapes | check box | — |

#### es
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Crear archivos S_VG... | File menu item, opens the dialog | MainFrame_createImageFilesMenu_Text "Crear archivos de _imagen...", CreatePdfCourses_Text "Crear archivos PDF" |
| K2 | Create SVG Files | Crear archivos SVG | window title | CreatePdfCourses_Text "Crear archivos PDF" |
| K3 | Bitmaps | Mapas de bits | group header | NewEventBitmapScale_labelTitle_Text "Resolución del mapa de bits" |
| K4 | Bitmaps in the map: | Mapas de bits: | "Mapas de bits del mapa:" repeats "mapa" and left the combo 90 px | CreatePdfCourses_filesLabel_Text "Archivos:" |
| K5 | Embed in each file | Incrustar en el archivo | infinitive; "cada" as "el" for room (1 px over) | CreatePdfCourses_comboBoxMultiPage_Items "Recortar a una página" |
| K6 | Save beside each file | Guardar junto al archivo | "each file" as "the file" for room | MainFrame_saveMenu_Text "_Guardar" |
| K7 | Leave out | Omitir | infinitive | CreatePdfCourses_comboBoxMultiPage_Items1 "Imprimir en múltiples páginas" |
| K8 | JPEG quality: | Calidad JPEG: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Calidad del mapa" |
| K9 | Compress the files (.svgz) | Comprimir los archivos (.svgz) | infinitive as the check boxes | CreatePdfCourses_checkBoxMergeParts_Text "Imprimir los cambios de mapa en el mismo mapa" |
| K10 | Draw area patterns as separate shapes | Dibujar las tramas como formas separadas | trama, an area's fill pattern; "los patrones de área" wrapped and pushed "Other folder" out | CreatePdfCourses_checkBoxMergeParts_Text |
| K11 | Cannot create SVG files for the following reason: | No se pueden crear archivos SVG por el siguiente motivo: | error heading | MiscText CannotCreatePdfs "No se pueden crear PDF por el siguiente motivo:" |

#### et
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Salvesta S_VG failid... | the neighbouring export items say "Salvesta" | MainFrame_createKMLFileMenu_Text "Salvesta _KML fail...", MainFrame_createImageFilesMenu_Text "Salvesta pil_difaild ..." |
| K2 | Create SVG Files | Salvesta SVG failid | the PDF title "Trüki rajad" is the print dialog's | CreateImageFiles_Text "Salvesta pildifailid" |
| K3 | Bitmaps | Rasterpildid | group header | NewEventBitmapScale_labelTitle_Text "Rasterkaardi resolutsioon", NewEventBitmapScale_resolutionLabel_Text "Pildi resolutsioon:" |
| K4 | Bitmaps in the map: | Pildid kaardil: | the header says raster; "Rasterpildid kaardil:" took 9 px the combo needs | NewEventBitmapScale_resolutionLabel_Text "Pildi resolutsioon:" |
| K5 | Embed in each file | Manusta igasse faili | imperative | CreatePdfCourses_comboBoxMultiPage_Items "Mahuta ühele lehele" |
| K6 | Save beside each file | Salvesta faili kõrvale | "each file" as "the file" for room | MainFrame_saveMenu_Text "_Salvesta" |
| K7 | Leave out | Jäta välja | imperative | CreatePdfCourses_comboBoxMultiPage_Items1 "Trüki mitmele lehele" |
| K8 | JPEG quality: | JPEG kvaliteet: | no hyphen, as "KML fail", "Värvi mudel:" | MainFrame_mapQualityMenu_Text "Kaardi _kvaliteet" |
| K9 | Compress the files (.svgz) | Tihenda failid (.svgz) | imperative as the check boxes | CourseAppearanceDialog_checkBoxStandardSizes_Text "Kasuta IOF standard suuruseid" |
| K10 | Draw area patterns as separate shapes | Joonista alade mustrid eraldi kujunditena | ala for area | MainFrame_addOutOfBoundsMenu_Text "Keeluala" |
| K11 | Cannot create SVG files for the following reason: | Ei saa luua SVG faile järgmisel põhjusel: | error heading | MiscText CannotCreateImageFiles "Ei saa luua pildifaile järgmisel põhjusel:" |

#### fi
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Lu_o SVG-tiedostoja... | key O: S, V and G are taken, V by the image item next to it | MainFrame_createImageFilesMenu_Text "Luo ku_vatiedostoja...", MainFrame_createKMLFileMenu_Text "Luo _KML-tiedosto..." |
| K2 | Create SVG Files | Luo SVG-tiedostoja | the PDF title "Tulosta radat" is the print dialog's | CreateImageFiles_Text "Luo kuvatiedostoja" |
| K3 | Bitmaps | Kuvat | the file calls a bitmap "kuva" | NewEventBitmapScale_labelTitle_Text "Kuvan resoluutio" |
| K4 | Bitmaps in the map: | Kartan kuvat: | label beside the combo | MainFrame_mapQualityMenu_Text "Kartan _laatu" |
| K5 | Embed in each file | Upota tiedostoon | imperative; "jokaiseen" doesn't fit | CreatePdfCourses_comboBoxMultiPage_Items "Rajaa yhdelle sivulle" |
| K6 | Save beside each file | Tallenna erikseen | "save separately"; "tiedoston viereen" is 4 px too wide | MainFrame_saveMenu_Text "_Tallenna" |
| K7 | Leave out | Jätä pois | imperative | CreatePdfCourses_comboBoxMultiPage_Items1 "Tulosta useille sivuille" |
| K8 | JPEG quality: | JPEG-laatu: | label beside the 1–100 box (dev's own Finnish has "Kuvanlaatu:") | MainFrame_mapQualityMenu_Text "Kartan _laatu" |
| K9 | Compress the files (.svgz) | Pakkaa tiedostot (.svgz) | imperative as the check boxes | CourseAppearanceDialog_checkBoxStandardSizes_Text "Käytä IOF:n standardimittoja" |
| K10 | Draw area patterns as separate shapes | Piirrä alueiden kuviot erillisinä muotoina | alue for area | MainFrame_addOutOfBoundsMenu_Text "Kielletty alue" |
| K11 | Cannot create SVG files for the following reason: | SVG-tiedostoja ei voi luoda seuraavasta syystä: | error heading | MiscText CannotCreateImageFiles "Kuvatiedostoja ei voi luoda seuraavasta syystä:" |

#### fr
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Créer des fichiers S_VG... | File menu item, opens the dialog | MainFrame_createImageFilesMenu_Text "Créer des fichiers Ima_ge..." |
| K2 | Create SVG Files | Créer des fichiers SVG | the PDF title "Imprimer les circuits" is the print dialog's | CreateOcadFiles_Text "Créer des fichiers OCAD", CreateImageFiles_Text "Créer des fichiers image" |
| K3 | Bitmaps | Bitmaps | group header | NewEventBitmapScale_labelTitle_Text "Résolution Bitmap" |
| K4 | Bitmaps in the map: | Bitmaps de la carte&nbsp;: | no-break space before the colon, as "Fichiers :" | CreatePdfCourses_filesLabel_Text "Fichiers&nbsp;:" |
| K5 | Embed in each file | Intégrer au fichier | infinitive; "dans chaque fichier" doesn't fit | CreatePdfCourses_comboBoxMultiPage_Items "Réduire à une seule page" |
| K6 | Save beside each file | Enregistrer à part | "save separately"; "à côté du fichier" doesn't fit | MainFrame_saveMenu_Text "_Enregistrer" |
| K7 | Leave out | Omettre | infinitive | CreatePdfCourses_comboBoxMultiPage_Items1 "Imprimer sur plusieurs pages" |
| K8 | JPEG quality: | Qualité JPEG&nbsp;: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Qualité de la Carte" |
| K9 | Compress the files (.svgz) | Compresser les fichiers (.svgz) | infinitive as the check boxes | CourseAppearanceDialog_checkBoxStandardSizes_Text "Utiliser les tailles IOF standards" |
| K10 | Draw area patterns as separate shapes | Dessiner les motifs en formes séparées | one line: "motifs surfaciques" wrapped and pushed "Other folder" out | CourseAppearanceDialog_checkBoxStandardSizes_Text |
| K11 | Cannot create SVG files for the following reason: | Impossible de créer des fichiers SVG pour la raison suivante : | plain space before the colon, as in that file | MiscText CannotCreateImageFiles "Impossible de créer des fichiers image pour la raison suivante :" |

#### hu
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | _SVG fájlok létrehozása... | key S: V is taken by "A program nyel_ve:" | MainFrame_createGPXFileMenu_Text "_GPX fájl készítése...", CreatePdfCourses_Text "PDF fájlok létrehozása" |
| K2 | Create SVG Files | SVG fájlok létrehozása | window title | CreatePdfCourses_Text "PDF fájlok létrehozása" |
| K3 | Bitmaps | Képek | the file calls a bitmap "kép" | NewEventBitmapScale_labelTitle_Text "Kép felbontása:" |
| K4 | Bitmaps in the map: | Képek a térképen: | label beside the combo | CreatePdfCourses_filesLabel_Text "Fájlok:" |
| K5 | Embed in each file | Beágyazni a fájlba | infinitive as the combo items | CreatePdfCourses_comboBoxMultiPage_Items "Levágni egy oldalra" |
| K6 | Save beside each file | A fájl mellé menteni | "each file" as "the file" for room | MainFrame_saveMenu_Text "Men_tés" |
| K7 | Leave out | Kihagyni | infinitive | CreatePdfCourses_comboBoxMultiPage_Items1 "Több oldalra nyomtatni" |
| K8 | JPEG quality: | JPEG minőség: | no hyphen, as "PDF fájlok", "Szín modell:" | MainFrame_mapQualityMenu_Text "Térkép_minőség" |
| K9 | Compress the files (.svgz) | Fájlok tömörítése (.svgz) | noun phrase as the check boxes | CreatePdfCourses_checkBoxMergeParts_Text "Térképcserék nyomtatása ugyanarra a térképre" |
| K10 | Draw area patterns as separate shapes | Területminták rajzolása különálló alakzatokként | terület for area; wraps to two lines, room remains | MainFrame_addOutOfBoundsMenu_Text "Elzárt terület" |
| K11 | Cannot create SVG files for the following reason: | Az SVG fájlok nem hozhatók létre az alábbi okból: | plural | MiscText CannotCreatePdfs "A PDF fájl nem hozható létre az alábbi okból:" |

#### it
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Crea file S_VG... | File menu item, opens the dialog | MainFrame_createImageFilesMenu_Text "Crea file imma_gine..." |
| K2 | Create SVG Files | Crea file SVG | the PDF's "Creare Files PDF" is the only title in the infinitive | CreateImageFiles_Text "Crea file immagine", CreateKmlFiles_Text "Crea file KML" |
| K3 | Bitmaps | Bitmap | group header | NewEventBitmapScale_labelTitle_Text "Risoluzione Bitmap" |
| K4 | Bitmaps in the map: | Bitmap nella carta: | carta, the orienteering map; "mappa" left the combo 7 px short | CreatePdfCourses_checkBoxMergeParts_Text "Stampa i Cambi Carta..." |
| K5 | Embed in each file | Incorpora nel file | imperative; "each file" as "the file" | CreatePdfCourses_comboBoxMultiPage_Items "Ritaglia su una singola pagina" |
| K6 | Save beside each file | Salva accanto al file | combo item | MainFrame_saveMenu_Text "_Salva" |
| K7 | Leave out | Ometti | imperative | CreatePdfCourses_comboBoxMultiPage_Items1 "Stampa su piu' pagine" |
| K8 | JPEG quality: | Qualità JPEG: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Qualità Mappa" |
| K9 | Compress the files (.svgz) | Comprimi i file (.svgz) | imperative as the check boxes | CourseAppearanceDialog_checkBoxStandardSizes_Text "Usa le dimensioni standard IOF" |
| K10 | Draw area patterns as separate shapes | Disegna i motivi areali come forme separate | areale as in simbolo areale | MainFrame_addOutOfBoundsMenu_Text "Area Fuori Dai Limiti" |

#### ja
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | SVGファイルを作成(_V)... | File menu item, key in the file's (_X) form | MainFrame_createGPXFileMenu_Text "GPXファイルを作成(_P)..." |
| K2 | Create SVG Files | SVGファイルを作成 | window title | CreatePdfCourses_Text "PDFファイルを作成" |
| K3 | Bitmaps | ビットマップ | group header | NewEventBitmapScale_labelTitle_Text "ビットマップの解像度" |
| K4 | Bitmaps in the map: | 地図のビットマップ: | ASCII colon as the file | CreatePdfCourses_filesLabel_Text "ファイル:" |
| K5 | Embed in each file | 埋め込む | dictionary form; "各ファイルに" doesn't fit 100 px | CreatePdfCourses_comboBoxMultiPage_Items1 "複数のページに印刷する" |
| K6 | Save beside each file | 別に保存する | "save separately"; "各ファイルの横に" doesn't fit | MainFrame_saveMenu_Text "保存(_S)" |
| K7 | Leave out | 含めない | combo item | — |
| K8 | JPEG quality: | JPEGの画質: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "地図の画質(_Q)" |
| K9 | Compress the files (.svgz) | ファイルを圧縮する (.svgz) | dictionary form | PrintCourses_checkBoxPausePrinting_Text "コースパートごとに印刷を休止する" |
| K10 | Draw area patterns as separate shapes | 面のパターンを個別の図形で描画する | 面 for area symbols; one line, where "として" wrapped and pushed "Other folder" out | PrintCourses_checkBoxPausePrinting_Text |
| K11 | Cannot create SVG files for the following reason: | SVGファイルを作成できません。以下の理由: | without the "{0}" the PDF message wrongly has | MiscText CannotCreatePdfs "PDF '{0}' を作成できません。以下の理由:" |

#### nb-NO
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Lag S_VG-filer... | File menu item, opens the dialog | MainFrame_createRouteGadgetFilesMenu_Text "Lag RouteGadget-filer", MainFrame_createImageFilesMenu_Text "Lag bildefiler..." |
| K2 | Create SVG Files | Lag SVG-filer | the PDF title "Skriv ut løyper" is the print dialog's | CreateKmlFiles_Text "Lag KML-fil", CreateImageFiles_Text "Lag bildefiler" |
| K3 | Bitmaps | Bilder | the file also has "Bitmap oppløsning"; "Bilder" reads naturally and leaves room | NewEventBitmapScale_resolutionLabel_Text "Bilde oppløsning:", MainFrame_addImageMenu_Text "Bilde" |
| K4 | Bitmaps in the map: | Bilder i kartet: | label beside the combo | CreatePdfCourses_filesLabel_Text "Filer:" |
| K5 | Embed in each file | Bygg inn i filen | imperative; "the file" as in K6 | CreatePdfCourses_comboBoxMultiPage_Items "Beskjær til en side" |
| K6 | Save beside each file | Lagre ved siden av filen | "each file" as "the file" for room | MainFrame_saveMenu_Text "_Lagre" |
| K7 | Leave out | Utelat | imperative | CreatePdfCourses_comboBoxMultiPage_Items1 "Skriv ut på flere sider" |
| K8 | JPEG quality: | JPEG-kvalitet: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Kartkvalitet" |
| K9 | Compress the files (.svgz) | Komprimer filene (.svgz) | imperative as the check boxes | CourseAppearanceDialog_checkBoxStandardSizes_Text "Bruk IOF-standardstørrelse" |
| K10 | Draw area patterns as separate shapes | Tegn flatemønstre som separate figurer | flate as in flatesymbol | CreatePdfCourses_checkBoxMergeParts_Text "Skriv ut kartbytte på samme kart" |
| K11 | Cannot create SVG files for the following reason: | Kan ikke lage SVG-filer på grunn av: | error heading | MiscText CannotCreateOcadFiles "Kan ikke lage OCAD filer på grunn av:" |

#### nn-NO
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Lag S_VG-filer... | the File menu is English in this file | CreateOcadFiles_Text "Lag OCAD filer" |
| K2 | Create SVG Files | Lag SVG-filer | the PDF title "Skriv ut løyper" is the print dialog's | CreateOcadFiles_Text "Lag OCAD filer" |
| K3 | Bitmaps | Bilde | plural of bilde | NewEventBitmapScale_resolutionLabel_Text "Bildeoppløysing:" |
| K4 | Bitmaps in the map: | Bilde i kartet: | label beside the combo | — |
| K5 | Embed in each file | Bygg inn i kvar fil | imperative | CreatePdfCourses_comboBoxMultiPage_Items "Beskjær til ein side" |
| K6 | Save beside each file | Lagra ved sida av kvar fil | fits in full (144 of 148 px) | MainFrame_saveMenu_Text "_Lagra" |
| K7 | Leave out | Utelat | imperative | CreatePdfCourses_comboBoxMultiPage_Items1 "Skriv ut på fleire sider" |
| K8 | JPEG quality: | JPEG-kvalitet: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Kartkvalitet" |
| K9 | Compress the files (.svgz) | Komprimer filene (.svgz) | imperative | CourseAppearanceDialog_checkBoxStandardSizes_Text "Bruk IOF-standardstørrelse" |
| K10 | Draw area patterns as separate shapes | Teikn flatemønster som separate figurar | flate as in flatesymbol | — |
| K11 | Cannot create SVG files for the following reason: | Kan ikkje lage SVG-filer på grunn av: | error heading | MiscText CannotCreateOcadFiles "Kan ikkje lage OCAD filer på grunn av:" |

#### nl
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Creëer S_VG bestanden... | File menu item, opens the dialog | MainFrame_createGPXFileMenu_Text "Creëer G_PX bestand...", CreatePdfCourses_Text "Creëer PDF bestanden" |
| K2 | Create SVG Files | Creëer SVG bestanden | window title | CreatePdfCourses_Text "Creëer PDF bestanden" |
| K3 | Bitmaps | Bitmaps | group header | NewEventBitmapScale_labelTitle_Text "Bitmap resolutie" |
| K4 | Bitmaps in the map: | Bitmaps in de kaart: | label beside the combo | CreatePdfCourses_filesLabel_Text "Bestanden:" |
| K5 | Embed in each file | Insluiten | infinitive; "In het bestand insluiten" doesn't fit | CreatePdfCourses_comboBoxMultiPage_Items "Bijsnijden tot enkele pagina" |
| K6 | Save beside each file | Apart opslaan | "save separately"; "Naast het bestand opslaan" doesn't fit | MainFrame_saveMenu_Text "Op_slaan" |
| K7 | Leave out | Weglaten | infinitive | CreatePdfCourses_comboBoxMultiPage_Items1 "Afdrukken op meerdere pagina's" |
| K8 | JPEG quality: | JPEG kwaliteit: | no hyphen, as "PDF bestanden" | — |
| K9 | Compress the files (.svgz) | Bestanden comprimeren (.svgz) | infinitive last, as the PDF check box | CreatePdfCourses_checkBoxMergeParts_Text "Kaartwissels afdrukken op Dezelfde Kaart" |
| K10 | Draw area patterns as separate shapes | Vlakpatronen als losse vormen tekenen | vlak as in vlaksymbool | CreatePdfCourses_checkBoxMergeParts_Text "Kaartwissels afdrukken op Dezelfde Kaart" |
| K11 | Cannot create SVG files for the following reason: | Kan geen SVG bestanden aanmaken vanwege: | error heading | MiscText CannotCreateOcadFiles "Kan geen OCAD bestanden aanmaken vanwege:" |

#### pl
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Utwórz pliki S_VG... | the File menu is English in this file | CreatePdfCourses_Text "Utwórz pliki PDF" |
| K2 | Create SVG Files | Utwórz pliki SVG | window title | CreatePdfCourses_Text "Utwórz pliki PDF" |
| K3 | Bitmaps | Bitmapy | group header | NewEventBitmapScale_labelTitle_Text "Rozdzielczość bitmapy" |
| K4 | Bitmaps in the map: | Bitmapy na mapie: | label beside the combo | CreatePdfCourses_filesLabel_Text "Pliki:" |
| K5 | Embed in each file | Osadź w pliku | imperative; "each file" as "the file" | CreatePdfCourses_comboBoxMultiPage_Items "Przytnij do pojedynczej strony" |
| K6 | Save beside each file | Zapisz obok pliku | combo item | MainFrame_saveMenu_Text "_Zapisz" |
| K7 | Leave out | Pomiń | imperative | CreatePdfCourses_comboBoxMultiPage_Items1 "Drukuj na kliku stronach" |
| K8 | JPEG quality: | Jakość JPEG: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "Jak_ość mapy" |
| K9 | Compress the files (.svgz) | Skompresuj pliki (.svgz) | perfective imperative as "Wydrukuj", "Użyj" | CreatePdfCourses_checkBoxMergeParts_Text "Wydrukuj zmiany map na tej samej mapie" |
| K10 | Draw area patterns as separate shapes | Rysuj wzory obszarów jako osobne kształty | obszar for area | MainFrame_whiteOutMenu_Text "Wymaż obszar" |
| K11 | Cannot create SVG files for the following reason: | Nie udało się utworzyć plików SVG z następującego powodu: | error heading | MiscText CannotCreatePdfs "Nie udało się utworzyć plików PDF z następującego powodu:" |

#### pt
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Criar Ficheiros S_VG... | File menu item, opens the dialog | MainFrame_createGPXFileMenu_Text "Criar Ficheiros G_PX..." |
| K2 | Create SVG Files | Exportar para ficheiro SVG | the titles say "Exportar para ficheiro" where the menu says "Criar Ficheiros" | CreatePdfCourses_Text "Exportar para ficheiro PDF" |
| K3 | Bitmaps | Bitmaps | group header | NewEventBitmapScale_labelTitle_Text "Resolução de Bitmap" |
| K4 | Bitmaps in the map: | Bitmaps no mapa: | label beside the combo | CreatePdfCourses_filesLabel_Text "Ficheiros:" |
| K5 | Embed in each file | Incorporar | infinitive; "no ficheiro" is 5 px too wide | CreatePdfCourses_comboBoxMultiPage_Items "Recortar para uma página" |
| K6 | Save beside each file | Gravar à parte | "save separately"; "junto ao ficheiro" doesn't fit | MainFrame_saveMenu_Text "_Gravar" |
| K7 | Leave out | Omitir | infinitive | CreatePdfCourses_comboBoxMultiPage_Items1 "Imprimir em múltiplas páginas" |
| K8 | JPEG quality: | Qualidade JPEG: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Qualidade do Mapa" |
| K9 | Compress the files (.svgz) | Comprimir os ficheiros (.svgz) | infinitive as the check boxes | CreatePdfCourses_checkBoxMergeParts_Text "Imprimir trocas de mapa no mesmo mapa" |
| K10 | Draw area patterns as separate shapes | Desenhar padrões de área forma a forma | "shape by shape" on one line; "como formas separadas" wrapped and pushed "Other folder" out | MainFrame_whiteOutMenu_Text "Área Branca" |
| K11 | Cannot create SVG files for the following reason: | Não é possível criar ficheiros SVG pelo seguinte motivo: | error heading | MiscText CannotCreateImageFiles "Não é possível criar ficheiros de imagem pelo seguinte motivo:" |

#### pt-BR
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Criar Arquivos S_VG... | File menu item, opens the dialog | MainFrame_createImageFilesMenu_Text "Criar Arquivos de Ima_gem...", CreatePdfCourses_Text "Criar Arquivos PDF" |
| K2 | Create SVG Files | Criar Arquivos SVG | window title | CreatePdfCourses_Text "Criar Arquivos PDF" |
| K3 | Bitmaps | Bitmaps | group header | NewEventBitmapScale_labelTitle_Text "Resolução de Bitmap" |
| K4 | Bitmaps in the map: | Bitmaps no mapa: | label beside the combo | CreatePdfCourses_filesLabel_Text "Arquivos:" |
| K5 | Embed in each file | Incorporar | infinitive; "no arquivo" is 5 px too wide | CreatePdfCourses_comboBoxMultiPage_Items "Cortar para uma única página" |
| K6 | Save beside each file | Salvar à parte | "save separately"; "junto ao arquivo" doesn't fit | MainFrame_saveMenu_Text "_Salvar" |
| K7 | Leave out | Omitir | infinitive | CreatePdfCourses_comboBoxMultiPage_Items1 "Imprimir em várias páginas" |
| K8 | JPEG quality: | Qualidade JPEG: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Qualidade do mapa" |
| K9 | Compress the files (.svgz) | Comprimir os arquivos (.svgz) | infinitive, sentence case as that check box | CourseAppearanceDialog_checkBoxStandardSizes_Text "Usar tamanhos padrão da IOF" |
| K10 | Draw area patterns as separate shapes | Desenhar padrões de área forma a forma | as pt, on one line | MainFrame_whiteOutMenu_Text "Área Branca" |
| K11 | Cannot create SVG files for the following reason: | Não é possível criar arquivos SVG pelo seguinte motivo: | error heading | MiscText CannotCreatePdfs "Não é possível criar arquivos PDF pelo seguinte motivo:" |

#### ro
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Creare Fișiere S_VG... | noun form and capitals as that title; the File menu is English here | CreateOcadFiles_Text "Creare Fișier OCAD" |
| K2 | Create SVG Files | Creare Fișiere SVG | the PDF title "Tipărire Trasee" is the print dialog's | CreateOcadFiles_Text "Creare Fișier OCAD" |
| K3 | Bitmaps | Imagini | no bitmap term in this file | NewEventBitmapScale_resolutionLabel_Text "Rezoluția imaginii:" |
| K4 | Bitmaps in the map: | Imagini din hartă: | label beside the combo | MainFrame_mapQualityMenu_Text "_Calitate hartă" |
| K5 | Embed in each file | Încorporare în fișier | noun form; "each file" as "the file" | CreatePdfCourses_comboBoxMultiPage_Items "Decupare într-o singură pagină" |
| K6 | Save beside each file | Salvare lângă fișier | combo item | MainFrame_saveMenu_Text "_Salvare" |
| K7 | Leave out | Omitere | noun form | CreatePdfCourses_comboBoxMultiPage_Items "Decupare..." |
| K8 | JPEG quality: | Calitate JPEG: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Calitate hartă" |
| K9 | Compress the files (.svgz) | Comprimă fișierele (.svgz) | verb as the check boxes | CourseAppearanceDialog_checkBoxStandardSizes_Text "Utilizează standardele de dimensiune IOF" |
| K10 | Draw area patterns as separate shapes | Desenează modelele de suprafață ca forme separate | suprafață as in simbol de suprafață; wraps, room remains | CourseAppearanceDialog_checkBoxStandardSizes_Text |
| K11 | Cannot create SVG files for the following reason: | Nu se pot crea fișierele SVG deoarece: | plural | MiscText CannotCreateOcadFiles "Nu se poate crea fișierul OCAD deoarece:" |

#### sl
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Ustvari S_VG datoteke... | the File menu is English in this file | CreateOcadFiles_Text "Ustvari OCAD datoteko" |
| K2 | Create SVG Files | Ustvari SVG datoteke | window title | CreateOcadFiles_Text "Ustvari OCAD datoteko" |
| K3 | Bitmaps | Bitne slike | no bitmap term in this file | — |
| K4 | Bitmaps in the map: | Slike v karti: | the header says bitne; "Bitne slike v karti:" left the combo 3 px short | MainFrame_mapQualityMenu_Text "Karta _Kvaliteta" |
| K5 | Embed in each file | Vdelaj v datoteko | imperative; "each file" as "the file" | CourseAppearanceDialog_checkBoxStandardSizes_Text "Uporabi IOF standardne velikosti" |
| K6 | Save beside each file | Shrani poleg datoteke | combo item | MainFrame_saveMenu_Text "_Shrani" |
| K7 | Leave out | Izpusti | imperative | — |
| K8 | JPEG quality: | Kvaliteta JPEG: | "kvaliteta" as the file, where "kakovost" is standard | MainFrame_mapQualityMenu_Text "Karta _Kvaliteta" |
| K9 | Compress the files (.svgz) | Stisni datoteke (.svgz) | imperative as the check boxes | CourseAppearanceDialog_checkBoxStandardSizes_Text "Uporabi IOF standardne velikosti" |
| K10 | Draw area patterns as separate shapes | Nariši ploskovne vzorce kot ločene oblike | ploskovni as in ploskovni znak | CustomSymbolText_checkBoxShowKey_Text "Pokaži pomen pod opisom simbola" |

#### sv
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Skapa S_VG-filer... | File menu item, opens the dialog | MainFrame_createGPXFileMenu_Text "Skapa G_PX-fil...", MainFrame_createRouteGadgetFilesMenu_Text "Skapa RouteGadget-filer..." |
| K2 | Create SVG Files | Skapa SVG-filer | window title | CreatePdfCourses_Text "Skapa PDF-filer" |
| K3 | Bitmaps | Bitmappar | group header | NewEventBitmapScale_labelTitle_Text "Upplösning på bitmap" |
| K4 | Bitmaps in the map: | Bitmappar i kartan: | label beside the combo | CreatePdfCourses_filesLabel_Text "Filer:" |
| K5 | Embed in each file | Bädda in i filen | imperative; "the file" as in K6 | CreatePdfCourses_comboBoxMultiPage_Items "Beskär till en enskild sida" |
| K6 | Save beside each file | Spara bredvid filen | "each file" as "the file" for room | MainFrame_saveMenu_Text "_Spara" |
| K7 | Leave out | Utelämna | imperative | CreatePdfCourses_comboBoxMultiPage_Items1 "Skriv ut på fler sidor" |
| K8 | JPEG quality: | JPEG-kvalitet: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Kartkvalitet" |
| K9 | Compress the files (.svgz) | Komprimera filerna (.svgz) | imperative as the check boxes | CourseAppearanceDialog_checkBoxStandardSizes_Text "Använd IOF-normens storlek" |
| K10 | Draw area patterns as separate shapes | Rita ytmönster som separata former | yt- as in ytsymbol | CreatePdfCourses_checkBoxMergeParts_Text "Skriv ut kartbyten på samma karta" |
| K11 | Cannot create SVG files for the following reason: | Kan inte skapa SVG-filer av följande orsaker: | error heading | MiscText CannotCreatePdfs "Kan inte skapa PDF av följande orsaker:" |

#### tr
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | S_VG Dosyaları Oluştur... | File menu item, opens the dialog | MainFrame_createGPXFileMenu_Text "G_PX Dosyası Oluştur...", MainFrame_createRouteGadgetFilesMenu_Text "RouteGadget Dosyaları Oluşturun..." |
| K2 | Create SVG Files | SVG Dosyaları Oluştur | title case as the other export titles (the PDF one is lower case) | CreateOcadFiles_Text "OCAD Dosyası Oluştur", CreateKmlFiles_Text "KML Dosyası Oluştur" |
| K3 | Bitmaps | Bitmapler | group header | NewEventBitmapScale_labelTitle_Text "Bitmap Çözünürlüğü" |
| K4 | Bitmaps in the map: | Haritadaki bitmapler: | label beside the combo | CreatePdfCourses_filesLabel_Text "Dosyalar:" |
| K5 | Embed in each file | Dosyaya göm | imperative; "each file" as "the file" | CreatePdfCourses_comboBoxMultiPage_Items "Tek bir sayfaya yerleştir" |
| K6 | Save beside each file | Ayrı kaydet | "save separately"; "Dosyanın yanına kaydet" doesn't fit | MainFrame_saveMenu_Text "_Kaydet" |
| K7 | Leave out | Dahil etme | negative imperative, "do not include" | — |
| K8 | JPEG quality: | JPEG kalitesi: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "Harita _Kalite" |
| K9 | Compress the files (.svgz) | Dosyaları sıkıştır (.svgz) | imperative as the check boxes | CourseAppearanceDialog_checkBoxStandardSizes_Text "IOF standart boyutlarını kullan" |
| K10 | Draw area patterns as separate shapes | Alan desenlerini ayrı şekiller olarak çiz | alan for area | MainFrame_addOutOfBoundsMenu_Text "Sınır Dışı Alan" |
| K11 | Cannot create SVG files for the following reason: | Aşağıdaki nedenden dolayı SVG dosyaları oluşturulamıyor: | error heading | MiscText CannotCreateOcadFiles "Aşağıdaki nedenden dolayı OCAD dosyaları oluşturulamıyor:" |

#### uk
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Створити файли S_VG... | File menu item, opens the dialog | MainFrame_createGPXFileMenu_Text "Створити файл G_PX..." |
| K2 | Create SVG Files | Створити файли SVG | noun before the format, as the menu (the PDF title says "PDF файли") | CreateKmlFiles_Text "Створити файл KML" |
| K3 | Bitmaps | Растрові зображення | group header | NewEventBitmapScale_labelTitle_Text "Роздільна здатність растрового зображення" |
| K4 | Bitmaps in the map: | Зображення на карті: | the header says растрові; with it the combo had 43 px | MainFrame_addImageMenu_Text "Зображення" |
| K5 | Embed in each file | Вбудувати | infinitive; "в кожен файл" doesn't fit | CreatePdfCourses_comboBoxMultiPage_Items "Кадрувати до розміру сторінки" |
| K6 | Save beside each file | Зберегти поруч | "save alongside"; "із кожним файлом" doesn't fit | MainFrame_saveMenu_Text "_Зберегти" |
| K7 | Leave out | Пропустити | infinitive | CreatePdfCourses_comboBoxMultiPage_Items1 "Друкувати на кількох сторінках" |
| K8 | JPEG quality: | Якість JPEG: | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "_Якість карти" |
| K9 | Compress the files (.svgz) | Стиснути файли (.svgz) | infinitive as the check boxes | TeamVariationsForm_checkBoxHideVariationsFromMap_Text "Приховати коди розсіювання на карті" |
| K10 | Draw area patterns as separate shapes | Малювати візерунки площ як окремі фігури | площа for area symbols; wraps, room remains | CreatePdfCourses_checkBoxMergeParts_Text "Надрукувати пункт зміни карт..." |
| K11 | Cannot create SVG files for the following reason: | Не вдалося створити файли SVG через наступні причини: | error heading | MiscText CannotCreatePdfs "Не вдалося створити PDF через наступні причини:" |

#### zh-CN
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | 创建SVG文件(_V)... | key in the file's (_X) form | MainFrame_createGPXFileMenu_Text "创建GPX文件...", MainFrame_saveMenu_Text "保存(_S)" |
| K2 | Create SVG Files | 创建SVG文件 | window title | CreatePdfCourses_Text "创建PDF文件" |
| K3 | Bitmaps | 位图 | group header | NewEventBitmapScale_labelTitle_Text "位图的分辨率" |
| K4 | Bitmaps in the map: | 地图中的位图： | full-width colon | CreatePdfCourses_fileNamePrefixLabel_Text "名称前缀：" |
| K5 | Embed in each file | 嵌入每个文件 | combo item | — |
| K6 | Save beside each file | 保存在每个文件旁 | combo item | MainFrame_saveMenu_Text "保存(_S)" |
| K7 | Leave out | 不包含 | combo item | — |
| K8 | JPEG quality: | JPEG质量： | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "地图质量" |
| K9 | Compress the files (.svgz) | 压缩文件 (.svgz) | check box | — |
| K10 | Draw area patterns as separate shapes | 将区域图案绘制为单独的形状 | check box | MainFrame_whiteOutMenu_Text "白色外部区域" |
| K11 | Cannot create SVG files for the following reason: | 无法创建SVG文件，原因如下： | error heading | MiscText CannotCreateImageFiles "无法创建图像文件，原因如下：" |

#### zh-TW
| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | 建立 SVG 檔案(_V)... | File menu item, opens the dialog | MainFrame_createGPXFileMenu_Text "建立 GPX 檔案(_P)..." |
| K2 | Create SVG Files | 建立SVG檔案 | window title | CreatePdfCourses_Text "建立PDF檔案" |
| K3 | Bitmaps | 點陣圖 | group header | NewEventBitmapScale_labelTitle_Text "點陣圖解析度" |
| K4 | Bitmaps in the map: | 地圖中的點陣圖： | full-width colon | CreatePdfCourses_filesLabel_Text "檔案：" |
| K5 | Embed in each file | 嵌入每個檔案 | combo item | — |
| K6 | Save beside each file | 儲存在每個檔案旁 | combo item | MainFrame_saveMenu_Text "儲存(_S)" |
| K7 | Leave out | 不包含 | combo item | — |
| K8 | JPEG quality: | JPEG品質： | label beside the 1–100 box | MainFrame_mapQualityMenu_Text "地圖品質(_Q)" |
| K9 | Compress the files (.svgz) | 壓縮檔案 (.svgz) | check box | — |
| K10 | Draw area patterns as separate shapes | 將區域圖樣繪製為個別圖形 | check box | MainFrame_addOutOfBoundsMenu_Text "禁制區域" |
| K11 | Cannot create SVG files for the following reason: | 由於下列原因無法建立 SVG 檔案： | error heading | MiscText CannotCreatePdfs "由於下列原因無法建立 PDF：" |

#### The five near-empty files (commit 60e28d5a)
None of these files has a neighbour term to mirror, so each uses the language's common software terms. zh-HK mirrors zh-TW's keys, as marked.

| Key | English | ca | he | id | ru | zh-HK |
| --- | --- | --- | --- | --- | --- | --- |
| K1 | Create S_VG Files... | Crea fitxers S_VG... | יצירת קובצי S_VG... | Buat Berkas S_VG... | Создать файлы S_VG... | 建立 SVG 檔案(_V)... (zh-TW GPX item) |
| K2 | Create SVG Files | Crea fitxers SVG | יצירת קובצי SVG | Buat Berkas SVG | Создать файлы SVG | 建立SVG檔案 (zh-TW CreatePdfCourses_Text) |
| K3 | Bitmaps | Mapes de bits | מפות סיביות | Bitmap | Растровые изображения | 點陣圖 (zh-TW NewEventBitmapScale_labelTitle_Text) |
| K4 | Bitmaps in the map: | Mapes de bits del mapa: | מפות סיביות במפה: | Bitmap di peta: | Растры на карте: (room) | 地圖中的點陣圖： (the file's "顯示得分在：" colon) |
| K5 | Embed in each file | Incrusta (room) | הטמעה בכל קובץ | Sematkan di berkas | Встроить (room) | 嵌入每個檔案 |
| K6 | Save beside each file | Desa a part (room) | שמירה לצד כל קובץ | Simpan terpisah (room) | Сохранить рядом (room) | 儲存在每個檔案旁 |
| K7 | Leave out | Omet | השמטה | Jangan sertakan | Не включать | 不包含 |
| K8 | JPEG quality: | Qualitat JPEG: | איכות JPEG: | Kualitas JPEG: | Качество JPEG: | JPEG品質： |
| K9 | Compress the files (.svgz) | Comprimeix els fitxers (.svgz) | דחיסת הקבצים (.svgz) | Kompres berkas (.svgz) | Сжать файлы (.svgz) | 壓縮檔案 (.svgz) |
| K10 | Draw area patterns as separate shapes | Dibuixa els patrons d'àrea com a formes separades (wraps) | ציור דוגמאות שטח כצורות נפרדות | Gambar pola area sebagai bentuk terpisah | Рисовать узоры площадей отдельными фигурами (wraps) | 將區域圖樣繪製為個別圖形 |

Conventions used: ca uses the imperative, as Catalan software does. he uses the noun form, mirroring AboutForm_licenseButton_Text "הצגת הרישיון המלא". id uses the imperative in title case, mirroring "Lihat Lisensi Penuh".

#### Held drafts, not committed: ku and vec
| Key | English | ku (Kurmanji) | vec (Venetian) |
| --- | --- | --- | --- |
| K1 | Create S_VG Files... | Pelên S_VG Çêke... | Crea i file S_VG... |
| K2 | Create SVG Files | Pelên SVG Çêke | Crea i file SVG |
| K3 | Bitmaps | Wêneyên bitmap | Bitmap |
| K4 | Bitmaps in the map: | Wêne di nexşeyê de: | Bitmap de la mapa: |
| K5 | Embed in each file | Têxe nav pelî | Incorpora |
| K6 | Save beside each file | Cuda tomar bike | Salva a parte |
| K7 | Leave out | Nexe nav | Lassa fora |
| K8 | JPEG quality: | Kalîteya JPEG: | Calità JPEG: |
| K9 | Compress the files (.svgz) | Pelan biguvêşe (.svgz) | Comprimi i file (.svgz) |
| K10 | Draw area patterns as separate shapes | Nimûneyên deveran wek şeklên cuda xêz bike | Dessegna i motivi de le aree come forme separae |

- **ku:** I'm unsure of most words, especially "nimûne" for pattern, "deveran" for areas, "Nexe nav" for "leave out" and "biguvêşe" for compress.
- **vec:** the spelling (with or without ł) and the Italianisms "Incorpora" and "Comprimi" are uncertain.

## @Pauli's choices and the Special options group

The same agent's report on carrying out @Pauli's four choices and the "Special options" group (Nov 13, 12:13 to 13:18).

@Pauli's four choices and the Special options layout are done, and `dev` is published with them. Every full run passed. One item is for @Pauli: in five languages the area-patterns check box keeps its shorter wording, because the full wording would wrap and push "Other folder" partly out of its group.

### Branch, commits and runs
**`feature/svg-dialog-choices`**, rooted at `feature/svg-target` 1da0e9e2:
- eff86b8a reverts 60e28d5a: ca, he, id, ru and zh-HK lose the SVG strings.
- bceede1f adds the Kurmanji and Venetian drafts, as reported.
- 528362cc builds the Special options group.
  - The new key `CreateSvgCourses_specialOptionsGroupBox_Text` ("Special options") is in `UIText.resx` and `UIText.Designer.cs`.
  - `CreateSvgCourses_bitmapsGroupBox_Text` is gone from `UIText.resx` and all 27 satellites. Its entry in each file was replaced in place by the new heading.
  - The area-patterns check box left Appearance, so Appearance now matches the PDF dialog.
  - The bitmaps label stands above its combo, whose text now has 240 px, so even the English "Save beside each file" fits.
  - The window grew from 610 to 642 px, exactly what the new row adds. "Other folder" stays inside its group in every language, checked on the rendered dialog.
- 97af1438 gives the bitmaps label and combo items their full wording (lists below).
- 929a5bb3 gives the folder picker its own key, `CreateSvgCourses_folderBrowserDialog_Description` ("Select a folder for the SVG files"). It is in `UIText.resx`, has a Designer property, is used in the code-behind, and is translated in all 27 satellites.

**Runs and merges:**
- **`wip/linux/svg-dialog-choices`** 2ee4f202 is `base` 68228505 with the branch merged in, plus ce9f4e3's two baselines. Tests 43410815324 and Build 43410815370 passed, and trial 4.0.266.220 downloads.
- **`feature/svg-target`**: fast-forwarded 1da0e9e2 → 929a5bb3.
- **`wip/linux/svg-target-into-dev`** da5a5883 is `dev` 8042a0da with the branch merged in, without conflict. The message follows the new rule: a subject naming the work, the five commits one per line, then the conflict note. Tests 43412467419 and Build 43412467460 passed.
- **`dev`**: fast-forwarded to da5a5883. Build 43413967327 published `channel-beta` 4.0.268.220 for Linux, macOS and Windows, and the .exe downloads. Its Tests run skipped its jobs because the commit was already tested.
- Renders of the new dialog in every language were sent to @Pauli in the session.

### Restored combo items (Embed in each file / Save beside each file)
| | was | now |
|---|---|---|
| bg | Вгради / Запиши отделно | Вгради във всеки файл / Запиши до всеки файл |
| cs | Vložit do souboru / Uložit vedle souboru | Vložit do každého souboru / Uložit vedle každého souboru |
| da | Indlejr i filen / Gem ved siden af filen | Indlejr i hver fil / Gem ved siden af hver fil |
| de | Einbetten / Separat speichern | In jede Datei einbetten / Neben jeder Datei speichern |
| es | Incrustar en el archivo / Guardar junto al archivo | Incrustar en cada archivo / Guardar junto a cada archivo |
| et | (Manusta igasse faili, already full) / Salvesta faili kõrvale | Salvesta iga faili kõrvale |
| fi | Upota tiedostoon / Tallenna erikseen | Upota jokaiseen tiedostoon / Tallenna jokaisen tiedoston viereen |
| fr | Intégrer au fichier / Enregistrer à part | Intégrer dans chaque fichier / Enregistrer à côté de chaque fichier |
| hu | Beágyazni a fájlba / A fájl mellé menteni | Beágyazni minden fájlba / Minden fájl mellé menteni |
| it | Incorpora nel file / Salva accanto al file | Incorpora in ogni file / Salva accanto a ogni file |
| ja | 埋め込む / 別に保存する | 各ファイルに埋め込む / 各ファイルの横に保存する |
| nb-NO | Bygg inn i filen / Lagre ved siden av filen | Bygg inn i hver fil / Lagre ved siden av hver fil |
| nl | Insluiten / Apart opslaan | In elk bestand insluiten / Naast elk bestand opslaan |
| pl | Osadź w pliku / Zapisz obok pliku | Osadź w każdym pliku / Zapisz obok każdego pliku |
| pt | Incorporar / Gravar à parte | Incorporar em cada ficheiro / Gravar ao lado de cada ficheiro |
| pt-BR | Incorporar / Salvar à parte | Incorporar em cada arquivo / Salvar ao lado de cada arquivo |
| ro | Încorporare în fișier / Salvare lângă fișier | Încorporare în fiecare fișier / Salvare lângă fiecare fișier |
| sl | Vdelaj v datoteko / Shrani poleg datoteke | Vdelaj v vsako datoteko / Shrani poleg vsake datoteke |
| sv | Bädda in i filen / Spara bredvid filen | Bädda in i varje fil / Spara bredvid varje fil |
| tr | Dosyaya göm / Ayrı kaydet | Her dosyaya göm / Her dosyanın yanına kaydet |
| uk | Вбудувати / Зберегти поруч | Вбудувати в кожен файл / Зберегти поруч із кожним файлом |
| ku | Têxe nav pelî / Cuda tomar bike | Têxe nav her pelî / Li kêleka her pelî tomar bike |
| vec | Incorpora / Salva a parte | Incorpora in ogni file / Salva darente a ogni file |

en-GB, nn-NO, zh-CN and zh-TW were already full. The longest item is uk's at 215 px of the 240 available.

### Revised labels ("Bitmaps in the map:")
The heading no longer says bitmaps, and the label now has the group's whole width, so these labels now name the bitmaps themselves:
- **bg:** Изображения: → **Растерни изображения в картата:**
- **es:** Mapas de bits: → **Mapas de bits del mapa:**
- **et:** Pildid kaardil: → **Rasterpildid kaardil:**
- **sl:** Slike v karti: → **Bitne slike v karti:**
- **uk:** Зображення на карті: → **Растрові зображення на карті:**
- **ku:** Wêne di nexşeyê de: → **Wêneyên bitmap di nexşeyê de:**

All other labels already used their file's word for bitmap (de, fi, hu, nb-NO, nn-NO and ro use the word their files use for image), so they stay. All fit on one line; the longest is bg's at 210 of 284 px.

### The new heading "Special options"
| | Translation | Mirrors |
|---|---|---|
| bg | Специални настройки | AllControlsProperties_Text "Настройки на всички КТ", ChangeText_insertSpecialButton_Text "Вмъкни специален текст" |
| cs | Speciální nastavení | AutoNumbering_newControlsOnlyRadioButton_Text "Použít toto nastavení…", ChangeText_insertSpecialButton_Text "Vložit speciální text" |
| da | Særlige indstillinger | AutoNumbering_newControlsOnlyRadioButton_Text "…disse indstillinger…", ChangeText_insertSpecialButton_Text "Indsæt særlig tekst" |
| de | Spezielle Einstellungen | AllControlsProperties_Text "Einstellungen für alle Posten", ChangeText_insertSpecialButton_Text "Speziellen Text einfügen" |
| en-GB | Special options | as English |
| es | Opciones especiales | AutoNumbering_newControlsOnlyRadioButton_Text "Aplicar opciones…", ChangeText_insertSpecialButton_Text "Insertar texto especial" |
| et | Eriseaded | MiscText_CoursePropertiesTitle "Raja seaded", MainFrame_specialItemToolStripMenu_Text "Lisa erisümbol" |
| fi | Erityisasetukset | CourseAppearanceDialog_groupBoxOcadMap_Text "Lisäasetukset", ChangeText_insertSpecialButton_Text "Lisää erityisteksti" |
| fr | Options spéciales | CourseAppearanceDialog_groupBoxOcadMap_Text "Options avancées", MainFrame_specialItemToolStripMenu_Text "Ajouter un Symbole Spécial" |
| hu | Speciális beállítások | AllControlsProperties_Text "…beállításai", ChangeText_insertSpecialButton_Text "Speciális szöveg beszúrása" |
| it | Impostazioni speciali | AutoNumbering_newControlsOnlyRadioButton_Text "Applica queste impostazioni…", ChangeText_insertSpecialButton_Text "Inserisci Testo Speciale" |
| ja | 特殊な設定 | AutoNumbering_Text "番号付けの設定", ChangeText_insertSpecialButton_Text "特殊なテキストを入力" |
| nb-NO | Spesielle innstillinger | AllControlsProperties_Text "Innstillinger alle poster", MainFrame_specialItemToolStripMenu_Text "Legg til spesielt symbol" |
| nn-NO | Spesielle innstillingar | AutoNumbering_newControlsOnlyRadioButton_Text "…desse innstillingane…", MainFrame_specialItemToolStripMenu_Text "Legg til spesielt symbol" |
| nl | Speciale instellingen | AutoNumbering_newControlsOnlyRadioButton_Text "Pas deze instellingen…", ChangeText_insertSpecialButton_Text "Voer speciale tekst in" |
| pl | Ustawienia specjalne | AllControlsProperties_Text "Ustawienia Wszystkich Punktów", MainFrame_specialItemToolStripMenu_Text "Dodaj obiekt specjalny" |
| pt | Configurações especiais | PrinterMargins_Text "Configuração de página", ChangeText_insertSpecialButton_Text "Inserir Texto Especial" |
| pt-BR | Configurações especiais | AutoNumbering_newControlsOnlyRadioButton_Text "…essas configurações…", ChangeText_insertSpecialButton_Text "Inserir Texto Especial" |
| ro | Setări speciale | AutoNumbering_newControlsOnlyRadioButton_Text "Aplică aceste setări…", ChangeText_insertSpecialButton_Text "Adaugă Text Special" |
| sl | Posebne nastavitve | AutoNumbering_newControlsOnlyRadioButton_Text "Uporabi te nastavitve…" (the file has no word for "special") |
| sv | Specialinställningar | AllControlsProperties_Text "Inställningar Alla Kontroller", MainFrame_specialItemToolStripMenu_Text "Lägg till specialsymbol" |
| tr | Özel ayarlar | AutoNumbering_newControlsOnlyRadioButton_Text "Bu ayarları…", MainFrame_specialItemToolStripMenu_Text "Özel Nesne Ekle" |
| uk | Спеціальні параметри | PrinterMargins_Text "Параметри сторінки", ChangeText_insertSpecialButton_Text "Додати спеціальний текст" |
| zh-CN | 特殊选项 | CourseAppearanceDialog_groupBoxOcadMap_Text "高级选项", ChangeText_insertSpecialButton_Text "插入特殊文本" |
| zh-TW | 特定選項 | CourseAppearanceDialog_groupBoxOcadMap_Text "進階選項", MainFrame_specialItemToolStripMenu_Text "增加特定項目" |
| ku | Vebijêrkên taybet | nothing in the file (new draft) |
| vec | Opsion speciali | nothing in the file (new draft) |

### The folder picker's title "Select a folder for the SVG files"
| | Translation | Mirrors (`CreatePdfCourses_folderBrowserDialog_Description` unless noted) |
|---|---|---|
| bg | Избери папка за SVG файловете | "Избери папка за PDF файловете" |
| cs | Vyberte adresář pro SVG soubory | "Vyberte adresář pro PDF soubory" |
| da | Vælg mappe til SVG-filerne | no PDF picker; CreateOcadFiles_folderBrowserDialog_Description "Vælg mappe til OCADfilerne" |
| de | Wählen Sie einen Ordner für die SVG-Dateien aus | "…für die PDF-Dateien aus" |
| en-GB | Select a folder for the SVG files | "Select a folder for the PDF files" |
| es | Escoja una carpeta para los archivos SVG | "…para los archivos PDF" |
| et | Vali kaust SVG failide jaoks | "Vali kaust PDF failide jaoks" |
| fi | Valitse kansio SVG-tiedostoille | "Valitse kansio PDF-tiedostoille" |
| fr | Sélectionner un répertoire pour les fichiers SVG | "…pour les fichiers PDF" |
| hu | Válassza ki az SVG fájlok könyvtárát | "Válassza ki a PDF fájlok könyvtárát" |
| it | Selezionare una cartella per i file SVG | "…per i files PDF", with "file" as the OCAD picker spells it |
| ja | SVGファイルを保存するフォルダを選択してください | "PDFファイルを保存するフォルダを選択してください" |
| nb-NO | Velg en mappe for SVG-filene | "Velg en mappe for PDF filene" |
| nn-NO | Vel mappe for SVG-filene | no PDF picker; NewEventDirectory_folderBrowserDialog_Description "Vel mappe for arrangmentsfil" |
| nl | Kies een folder voor de SVG bestanden | "…voor de PDF bestanden" |
| pl | Wybierz katalog dla plików SVG | "Wybierz Katalog dla plików PDF", with "katalog" in lower case |
| pt | Selecionar a pasta para os ficheiros SVG | "…os ficheiros PDF" |
| pt-BR | Selecionar uma pasta para os arquivos SVG | "…os arquivos PDF" |
| ro | Selectați un director pentru fișierele SVG | no PDF picker; CreateOcadFiles_folderBrowserDialog_Description "…pentru fișierul OCAD" |
| sl | Izberi mapo za SVG datoteke | no PDF picker; CreateOcadFiles_folderBrowserDialog_Description "Izberi mapo za OCAD datoteke" |
| sv | Välj en mapp för SVG-filerna | "…för PDF-filerna" |
| tr | SVG dosyaları için bir klasör seçin | "PDF dosyaları için bir klasör seçin" |
| uk | Виберіть папку для файлів SVG | "…для PDF файлів", noun first as the OCAD picker and the SVG title |
| zh-CN | 选择一个SVG文件的文件夹 | "选择一个PDF文件的文件夹" |
| zh-TW | 選擇SVG檔案的資料夾 | "選擇PDF檔案的資料夾" |
| ku | Peldankekê ji bo pelên SVG hilbijêre | nothing in the file (new draft) |
| vec | Selesiona na cartela par i file SVG | nothing in the file (new draft) |

### For @Pauli
1. **Five check boxes keep their one-line wording.** "Draw area patterns as separate shapes" stays shortened in bg, es, fr, ja and pt. At 642 px those five dialogs have 6 to 9 px to spare, and a second line costs 20, so the full wording would push "Other folder" partly out of its group.
   - Option (a): keep them as they are.
   - Option (b): make the dialog 20 px taller (662) and restore the full wording:
     - bg: Рисувай шарките на площите като отделни фигури
     - es: Dibujar los patrones de área como formas separadas
     - fr: Dessiner les motifs surfaciques en formes séparées
     - ja: 面のパターンを個別の図形として描画する
     - pt: Desenhar padrões de área como formas separadas
   - I recommend (b): those wordings name the area symbols the option is about.
2. **Kurmanji and Venetian go beyond the shipped drafts.** I applied the "each file" restoration to their combo items like every other language, and their heading and picker titles are new drafts. The same doubts as before apply to all of them.
