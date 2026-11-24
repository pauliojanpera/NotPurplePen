# Vaihe 2 ja Skian koodaus
**Kuvaus**
Vaihe 2 piirtää kartan palettivärit välikerroksesta. Se ottaa palettierottelut (`Map.PaletteSeparations`)
palettijärjestyksessä, ja kanavan koodaus kirjoittaa kunkin palettivärin läpinäkyvyysryhmän kanavansa pinnalle: ryhmän
sisältö on palettivärin väri C palettierottelun peitossa, ja ryhmä koostetaan kerran palettivärin peittävyydellä α ja
sekoitustilalla B, jonka ratojen ulkoasun päällepainatustila (`OverprintMode`) valitsee. Tämä tiketti tekee vaiheen 2
yhteisen osan ja Skian koodauksen, joten näyttö, Windowsin tulostus ja tulostuksen esikatselu piirtävät palettivärit
vaiheella 2. Vaihe 2 ei kutsu symbolien piirtokoodia (`Draw`). Pohjakartat, kuvataso ja asettelutaso piirtyvät
palettipinon rinnalla kuten nyt. PDF:n koodaus on oma tikettinsä, ja siihen asti PDF:n ja legacy-kanavien
piirtokohteet piirtävät palettivärit vanhalla piirtokoodilla. Skian piirto muuttuu kartoissa, joiden palettivärin α
on alle 1, siellä, missä vanha kuviosivellin sijoitti aluekuvion toisin kuin M, uusissa päällepainatustiloissa ja
yksittäisissä reunapikseleissä, joissa laajennukset pyöristävät katkot ja glyyfit toisin kuin vanha piirto.

**Hyväksymiskriteerit**
- [ ] Vaihe 2 `Map.Draw`issa: kun piirtokohde antaa koodauksen (`IPaletteStackEncodingProvider`), `Map.Draw` piirtää
  palettivärit vaiheella 2 eikä kutsu niille vanhaa piirtoa (`DrawColor`) eikä symbolien piirtokoodia
  (`Symbol.Draw`). Vaihe 2 ottaa palettierottelut palettijärjestyksessä niiden palettivärien osalta, jotka värialueen
  asetukset (`RenderOptions.colorBeginDrawExclusive` ja `colorEndDrawInclusive`) valitsevat, ja kustakin ne symbolien
  palettiväripeitot, joiden symboli voi leikata piirron suorakulmion, samalla ehdolla kuin `DrawColor`
  (`Symbol.BoundingBox` ja `MayIntersectRect`). Pohjakartat kartan alla, kuvataso, palettipino, asettelutaso ja
  pohjakartat kartan päällä piirtyvät tässä järjestyksessä kuten nyt, ja kuvataso ja asettelutaso vanhalla
  piirtokoodilla (speksin luku 04, "The graphics target beside the palette stack"). OCAD-pohjakartta on oma `Map`insa,
  ja sen palettivärit piirtyvät samoin.
- [ ] Koodauksen rajapinta: MapModelin rajapinta `IPaletteStackEncoding` kirjoittaa palettivärin läpinäkyvyysryhmän
  (`WriteTransparencyGroup(TransparencyGroup)`) kanavansa pinnalle. Skian piirtokohde (`Skia_GraphicsTarget`) on
  `IPaletteStackEncodingProvider` ja antaa Skian koodauksen (`SkiaEncoding`), joka kirjoittaa saman `SKCanvas`in.
  Koodaus ja piirtokohde jakavat kanavan pinnan, sen muunnokset ja leikkaukset, värimuunnoksen, intensiteetin ja
  reunanpehmennyksen. `IGraphicsTarget` ei muutu.
- [ ] Palettivärin läpinäkyvyysryhmä (`TransparencyGroup`): vaihe 2 tekee kullekin piirrettävälle palettierottelulle
  ryhmän, jossa ovat palettiväri, sen peittävyys α (`SymColor.Opacity`), sekoitustila B päällepainatustilan mukaan,
  piirron resoluutio (`RenderOptions.minResolution`) ja symbolien palettiväripeitot palettierottelun järjestyksessä.
  Ryhmän väri C on palettivärin väri kartan värimatriisin kanssa (`SymColor.ColorValue`), jonka koodaus muuntaa
  piirtokohteen värimuunnoksella ja intensiteetillä kuten `SymColor.GetBrushKey`. Skian koodaus koostaa ryhmän
  menetelmällä `SKCanvas.SaveLayer`, jonka sivellin antaa α:n ja B:n, kun α on alle 1 tai B on Multiply. Kun α on 1 ja
  B on Normal tai Darken, ryhmän koostaminen antaa saman tuloksen kuin sen sisällön piirto suoraan B:llä, ja koodaus
  piirtää niin kuten vanha piirto (ks. "Ratkaisut").
- [ ] Ryhmän sisältö: Skian koodaus kirjoittaa kunkin symbolin palettiväripeiton erikseen palettierottelun
  järjestyksessä, ja palettierottelun unioni jää implisiittiseksi (speksin luku 04). Täyden peiton palettiväripeiton
  leikkausobjektit se kirjoittaa sijoitustensa sisäkkäisillä affiinisilla muunnoksilla (`Placement.Steps`): täytetyn
  muodon täyttösäännöllään, viivamuodon kynällään ja tekstijuosteen Y-akselin peilauksella. Katkojuosteet, siirretyt
  polut ja toistosijoitukset se laajentaa yksittäisiksi katkoiksi ja sijoituksiksi (`StrokedShape.ExpandDashes`,
  `RepeatedPlacement.Expand`), kuten vanha piirto piirtää ne (päätös 24.11.). Se piirtää muodot samoilla Skian
  kutsuilla kuin `Skia_GraphicsTarget` piirtää vanhan piirron vastaavat muodot.
- [ ] Leikkausobjektit piirron resoluutiolla: `Map` antaa kanavamuunnoksena symbolin palettiväripeiton
  leikkausobjektit piirron resoluutiolla (`SymbolCoverage.GetClippingObjects(float)`): viivan katkottu osatekijä,
  jonka kaikki välit ovat resoluutiota lyhyempiä, on yhtenäinen samalla ehdolla kuin vanha piirto piirtää sen
  (`LineSymDef.DrawDashedWithOffset`). Tämä koskee viivasymboleita, alueen reunoja ja yhdistelmien osamäärittelyjä.
  Resoluutiolla 0 ne ovat symbolin palettiväripeiton leikkausobjektit (`SymbolCoverage.ClippingObjects`). Tiketin 024
  rajapinta saa resoluution (`SymDef.MakeClippingObjects(Symbol, SymColor, CoveragePrototype, float)`), ja Skian
  koodaus kirjoittaa leikkausobjektit ryhmän resoluutiolla (päätös 23.11.).
- [ ] Kuviotäytöt: viivoituksen ja leikatun aluekuvion palettiväripeitolle Skian koodaus kirjoittaa sen
  leikkausobjektin, alueen, leikkauksena. Leikkauksen sisään se kirjoittaa kuvioruudun kuviosiveltimenä samoissa
  tapauksissa kuin vanha piirto (`RenderOptions.usePatternBitmaps`, säännöllinen leikattu aluekuvio,
  `PatternBrushTooPixelated`) ja muuten kuviotäytön laajennuksen (`SymbolCoverage.ExpandPrototype`). Kuvioruutu
  (`AreaPatternPrototype.Tile`) on määrittelyn johdos: aluekuvion solu ja elementin glyyfin osat palettivärissä niiden
  paikoilla, kuten `CreatePatternBrushes` piirtää siveltimen. Koodaus rasteroi sen siveltimen kokoiseksi
  bittikartaksi ja täyttää alueen sen varjostimella symbolin palettiväripeiton M:n mukaan
  (`SymbolCoverage.Transformation`).
- [ ] Päällepainatustila (`OverprintMode`): `RenderOptions.overprintMode` korvaa asetuksen `blendOverprintedColors`.
  Tilassa `None` B on Normal, tilassa `ByPaletteColor` B on kunkin palettivärin sekoitustila (`SymColor.BlendMode`), ja
  tiloissa `Darken` ja `Multiply` päällepainatusväreillä (`SymColor.OverPrint`) on Darken tai Multiply ja muilla Normal
  (päätös 23.11.). α ei riipu tilasta. Skian koodaus sekoittaa RGB:ssä, jossa Darken on värikomponenttien minimi ja
  Multiply niiden tulo. Ratakartta sekoittaa päällepainatusvärinsä Darkenilla kuten nyt (`MapDisplay.DrawCourseMap`).
- [ ] Ratojen ulkoasu: `CourseAppearance.overprintMode` korvaa kentän `useOcadOverprint`, ja `MapDisplay.OverprintMode`
  ja `Controller.OverprintMode` korvaavat `OcadOverprintEffect`in. Ratatiedosto pitää attribuutin `overprint-colors`
  kuten nyt ja kirjoittaa uuden attribuutin `overprint-mode` vain tiloille `ByPaletteColor` ja `Multiply`, joten
  tiedostot tiloissa `None` ja `Darken` pysyvät tavulleen ennallaan. Lukiessa `overprint-mode` ratkaisee, ja muuten
  `overprint-colors` antaa `Darken`in tai `None`n. Avalonian `CourseAppearanceDialog` valitsee tilan valikosta, jossa
  on neljä vaihtoehtoa: ei käytössä, palettivärin mukaan, tummenna ja kerro. Tekstit ovat `UIText.resx`:ssä.
- [ ] Tulostuksen esikatselu: Graphics2D:n rajapinta `IDeferredDrawingTarget` antaa piirtokohteen tallentaa piirron,
  joka tehdään toistossa (`DrawDeferred(Action<IGraphicsTarget>)`), ja `RecordingGraphicsTarget` toteuttaa sen.
  Tällaisella piirtokohteella `Map.Draw` tallentaa palettivärien piirron. Toistossa piirto lukee kartan (`Map.Read`) ja
  kirjoittaa palettivärit toistavan piirtokohteen koodauksella, tai vanhalla piirtokoodilla, jos sillä ei ole
  koodausta. Skian renderöintitestien tallennettu piirto (`RenderingUtil.RenderAndCompare`, `alsoTestRecording`) antaa
  saman kuvan kuin suora piirto.
- [ ] Piirtokohteet ilman koodausta: PDF-, GDI+-, WPF- ja muut piirtokohteet piirtävät palettivärit vanhalla
  piirtokoodilla (`DrawColor`) kuten nyt. Tilassa `Darken` ne sekoittavat päällepainatusvärit Darkenilla
  (`PushBlending`) kuten nyt, eivätkä ne sovella muita tiloja eivätkä α:aa (päätökset 23.11.). WinFormsin
  `CourseAppearanceDialog` pitää valintaruutunsa, joka valitsee `Darken`in tai `None`n.
- [ ] Peiton vertailu (`PaletteSeparationCoverageTests`): oraakkeli on edelleen vanha piirto. Sisäinen piirtoasetus
  `RenderOptions.drawColorsDirectly` saa `Map.Draw`in piirtämään palettivärit vanhalla piirtokoodilla myös
  piirtokohteelle, jolla on koodaus, ja vain testit käyttävät sitä. Vertailun kaikki tapaukset menevät läpi.
- [ ] Rakennetestit tarkistavat ilman vanhaa piirtoa seuraavat:
  - Vaihe 2 ei kutsu symbolien piirtokoodia.
  - Palettivärin kahden symbolin palettiväripeiton päällekkäinen osa saa α:n vain kerran (speksin kuva 6).
  - Ryhmät koostetaan palettijärjestyksessä.
  - Värialueen asetukset ja piirron suorakulmio rajaavat ryhmät ja niiden sisällön.
  - Päällepainatustilat `None`, `ByPaletteColor`, `Darken` ja `Multiply` antavat palettiväreille B:n kriteerin
    mukaan. Darkenilla pikseli on värikomponenttien minimi ja Multiplyllä niiden tulo, ja α < 1 yhdessä B:n kanssa
    koostetaan menetelmällä `SaveLayer`.
  - Katkoviiva on yhtenäinen, kun välit ovat resoluutiota lyhyempiä, myös kaksoisviivan reunoilla ja alueen reunalla.
  - Kuviosivellin on M:n mukaan, myös kiertokeskuksen kanssa kulmalla 0.
  - Tallennettu piirto toistuu.
  - Ratatiedosto kirjoittaa ja lukee päällepainatustilat, ja vanha tiedosto luetaan.

  Testit rakentavat kartan tapauksille, joita vertailun kartoissa ei ole (ks. "Testit").
- [ ] Kuvat: keräävä ajo antaa kuvat, jotka siirtyvät. Jokainen siirtynyt kuva selittyy yhdellä näistä syistä, ja
  tiketti kertoo niiden määrät:
  1. palettivärin α on alle 1
  2. vanha kuviosivellin sijoitti aluekuvion toisin kuin M
  3. leikkausobjektit eroavat vanhasta piirrosta tikettien 011–013 sallituissa eroissa, eli katkojuosteiden,
     siirrettyjen polkujen ja toistosijoitusten laajennuksissa.

  Muut kuva- ja tekstibaselinet eivät siirry. Siirtyneet baselinet tallennetaan @Paulin luvalla, jonka hän antoi
  hyväksyessään tiketin (24.11.).
- [ ] Tikettien 023–026, 011–013 ja 009 testit menevät läpi.

**Lisätiedot**
- **Päätökset:**
  - @Pauli hyväksyi tämän tiketin ja sen istunnon valinnat (ks. "Ratkaisut") 24.11. Istunto kirjoitti sen luonnoksesta
    @Paulin vastattua viiteen kysymykseen. Luonnos hyväksytään ensin tiketiksi, ja vasta sitten se toteutetaan
    (@Pauli, 21.11. ja 23.11.).
  - Vaihe 2 korvaa palettivärien suoran piirron, ja normaali näkymä on vaiheen 2 piirtotila (@Pauli, 18.11.). Vaihe 2
    lukee palettierotteluja, jotka viittaavat symboleihin, eikä kutsu symbolien piirtokoodia (`Draw`) (20.11.).
  - Palettivärin unioni saa jäädä piirrossa implisiittiseksi, mutta symbolien palettiväripeitot eivät saa jäädä pois
    (speksin luku 04, @Pauli, 18.11.). feature/multiply-blending on vain ulkoasun oraakkeli, eikä työ lue sen koodia.
  - Koodaus kirjoittaa kanavansa suoraan: Skian koodaus `SKCanvas`in. `IGraphicsTarget` jää palettipinon rinnalle
    piirtämään pohjakartat, korostukset, rastikuvaukset ja vanhan piirtokoodin (@Pauli, 21.11., "The encodings and
    IGraphicsTarget"). Korostukset piirtyvät suoraan kuten nyt.
  - Vaihe 2 jaetaan kanavittain. Tämä tiketti tekee yhteisen osan ja Skian koodauksen, ja PDF:n koodaus on oma tiketti
    tämän jälkeen. GDI+ ja WPF ovat legacya: ne eivät saa koodausta, ja työ voi hylätä ne, missä se on mahdollista
    (@Pauli, 23.11., "Pass 2 by channel").
  - Ratojen ulkoasun päällepainatusvalinta valitsee piirron sekoitustilan kaikissa kanavissa. Vaihtoehdot ovat ei
    käytössä, palettivärin mukaan, tummenna ja kerro, ja tummenna ja kerro koskevat päällepainatusvärejä. Valinta
    tehdään epicissä upstreamin ratojen ulkoasuun (@Pauli, 23.11., "The blend mode of the drawing").
  - Skian koodaus piirtää katkoviivan, jonka välit ovat pikseliä lyhyempiä, yhtenäisenä kuten nyt, ja `Map` tarjoaa
    sitä varten kanavamuunnoksen (@Pauli, 23.11., "The dashes shorter than a pixel").
  - Skian koodaus laajentaa katkojuosteet ja toistosijoitukset tässä tiketissä, ja Skian abstraktiot niille saavat oman
    tiketin (@Pauli, 24.11., "The expansions in the Skia encoding").
  - Kuviosiveltimen kiertokeskus verrataan oraakkeleihin vähintään jälkikäteen (@Pauli, 24.11., "The rotation center
    of the pattern brush"). OCADin sekoitustilat testataan omassa tiketissään ennen PDF:n koodausta (23.11.).
  - Täydet ajot tehdään basen päällä, ja palettipinohaarat saavat olla ristiriidassa devin kanssa (@Pauli, 18.11.).
- **Speksi:** [Specification.md](../Specification.md), luvut 01, 03 ja 04. Luvun 01 kohta "The two passes" ja luvun 04
  kohdat "Pass 2 and the transparency groups", "The rendering modes", "The encodings and the channels" ja "The graphics
  target beside the palette stack" antavat vaiheen 2, palettivärin läpinäkyvyysryhmän, piirtotilat, koodauksen ja
  piirtokohteen. Luvun 03 kohdat "The derived values" ja "The forms of the derived values" antavat kanavamuunnokset,
  laajennuksen ja kuvioruudun. Nimet ovat [sanastossa](../Glossary.md).
- **Nykytila** (`feature/palette-stack`, e51b81a2):
  - `Map.Draw` piirtää pohjakartat kartan alla, kuvatason, palettivärit alimmasta ylimpään, asettelutason ja pohjakartat
    kartan päällä. Kuvataso, palettivärit ja asettelutaso piirtyvät samalla silmukalla (`DrawColor`) pseudovärien
    `ImageColor` ja `LayoutColor` kautta. `DrawColor` ottaa näkyvät symbolimäärittelyt, joilla on palettiväri, ja niiden
    näkyvät symbolit, jotka voivat leikata suorakulmion. Se kytkee päällepainatusvärille Darkenin (`PushBlending`), kun
    `blendOverprintedColors` on päällä, ja piirtää kunkin symbolin (`Symbol.Draw`). Piirto ei sovella α:aa eikä B:tä
    (`SymColor.Opacity`, `SymColor.BlendMode`).
  - Välikerros (tiketit 023, 024, 011–013 ja 009): `Map.PaletteSeparations` antaa palettierottelut palettijärjestyksessä
    ja niissä symbolien palettiväripeitot `DrawColor`in järjestyksessä. Leikkausobjektit tulevat symbolimäärittelyjen
    geometriametodeilta resoluutiolla 0, ja viivoitusten ja aluekuvioiden laajennus tulee peittoprototyypiltä.
  - `Skia_GraphicsTarget.PushBlending` asettaa Darkenin kunkin muodon siveltimeen. Sen kommentin mukaan tämä toimii vain
    Darkenilla, koska Darken on idempotentti, ja muut sekoitustilat vaativat oman piirtopinnan (`SaveLayer`).
  - `MapDisplay` piirtää kartan asetuksella `blendOverprintedColors = ocadOverprintEffect`
    (`CourseAppearance.useOcadOverprint`, ratatiedoston attribuutti `overprint-colors`) ja ratakartan aina asetuksella
    `true`. Ratojen alempi purppura jakaa kartan piirron värialueen asetuksilla (`lowerPurpleMapLayer`). Se käyttää
    kuviosiveltimiä paitsi tulostuksessa ja silloin, kun piirto on reunanpehmennetty ja resoluutio alle 0,007 mm.
  - Toimitettava sovellus piirtää kartan näin:
    - näyttö (`CacheableMapDisplay`) `Skia_GraphicsTarget`ille `SwopColorConverter`illa
    - Windowsin tulostus (`WindowsPrinter`) `Skia_GraphicsTarget`ille kaistoittain
    - PDF-vienti sekä Linuxin ja macOS:n tulostus `PdfPrintTarget`in kautta `Pdf_GraphicsTarget`ille
    - tulostuksen esikatselu tallentaa piirron (`RecordingPrintTarget`, `RecordingGraphicsTarget`) ja toistaa sen
      `Skia_GraphicsTarget`ille (`RecordedPageDrawing`).

    `RecordingGraphicsTarget` tallentaa piirtokutsut kartan lukulukon aikana ja toistaa ne lukon jälkeen.
  - Vanha piirto piirtää katkoviivan yhtenäisenä, kun sen välit (`gapLength` ja toissijaiset välit) ovat lyhyempiä kuin
    `RenderOptions.minResolution` (`LineSymDef.DrawDashedWithOffset`). Näyttö ja Skian renderöintitestit antavat
    resoluutioksi pikselin koon.
  - `AreaSymDef.DrawPatterns` piirtää aluekuvion kuviosiveltimellä (`CreatePatternBrushes`,
    `DrawPatternWithTexBrush`), kun `usePatternBitmaps` on päällä, piirtokohde tukee kuviosiveltimiä, aluekuvio on
    säännöllinen ja leikattu eikä sivellin ole liian karkea (`PatternBrushTooPixelated`), ja muuten glyyfi kerrallaan
    (`DrawPattern`). Sivellin jättää kiertokeskuksen pois, kun alueobjektin kulma on 0 (tiketti 009). OCADin tuonti
    antaa kiertokeskukseksi kartan origon, joten tapaus tulee Mapper-kartoista (`rotationCoord`).
  - Suurin osa Skian renderöintitesteistä (`RenderingTests.CheckTest`) piirtää kuviosiveltimin ilman
    päällepainatusvaikutusta ja myös tallennuksen kautta (`alsoTestRecording`). `CheckTestNoPatternBitmaps` piirtää
    ilman kuviosiveltimiä, ja `CheckTestOverprinting` päällepainatusvaikutuksen kanssa: `MapperOverprinting` Hakolan
    Mapper-kartan spottiväreineen (`skia_render/Hakola malli.omap`, baseline `hakolaoverprinting.png`, tiketti 001) ja
    `Overprinting` OCAD-kartan (`ocad11overprinting`). Tilassa `Darken` niiden kuvat pysyvät, koska Darken piirretään
    ilman menetelmää `SaveLayer` kuten nyt, ja ne tarkistavat sen.
  - Testikartat: palettivärin α on alle 1 kartoissa `skia_render/LordHill.ocd` ja `LordHill OCAD11.ocd` (väri 201,
    `t80`) ja `openmapper/LordHill.omap` (0,8) sekä PurplePen_Testsin kartoissa
    `courseprinting/LordHill_ver16_2024Jan_scaled.omap` (0,75) ja `McHugh Creek.omap` (0,5). Sekoitustila Darken on
    väreillä 3, 6, 50 ja 52 kartoissa `skia_render/irregular1–4` ja `wholestructure1–4` sekä
    `ocadimport/ssl_issprom2019.ocd`. Niiden testit eivät käytä päällepainatusvaikutusta, joten B ei muuta niitä
    tiloissa `None` ja `Darken`.
- **Vanha työ:** vaiheelle 2 ei ole. Vanha epicin haara `feature/palette-layers` ja tikettien 011–013 vanhat haarat
  tekevät vain vaiheen 1. Sopii: peiton vertailun oma leikkausobjektien piirto (`PaletteSeparationCoverageTests`:
  `DrawClippingObjects`, sijoituksen sisäkkäiset affiiniset muunnokset ja tekstijuosteen peilaus) ja laajennukset
  (`SymbolCoverageComparison.Expand`). Skian koodaus ottaa niistä askelten järjestyksen ja laajennukset. Vertailu
  piirtää `IGraphicsTarget`in kautta, joten se ei ole koodaus.
- **Luonnoksen kysymykset:** 1 ratkesi päällepainatustilalla, 2 päätöksellä 23.11., 3 kriteerillä "Kuvat", 4 tiketillä
  023, 5 `DrawColor`in rajausehdolla, 6 päätöksillä 21.11. ja koodauksen rajapinnalla, 7 päätöksellä 23.11. (Skia
  sekoittaa RGB:ssä, musteittainen B kuuluu PDF:ään ja piirtotiloihin), 8 päätöksellä 23.11. ja 9 tiketillä 011 ja
  päätöksellä 24.11.
- **Ratkaisut** (istunnon valinnat, @Pauli hyväksyi ne tiketin kanssa 24.11.):
  - Nimet: `IPaletteStackEncoding`, `IPaletteStackEncodingProvider`, `SkiaEncoding`, `TransparencyGroup`,
    `OverprintMode` (`None`, `ByPaletteColor`, `Darken`, `Multiply`), `RenderOptions.overprintMode`,
    `CourseAppearance.overprintMode`, `MapDisplay.OverprintMode`, `Controller.OverprintMode`,
    `SymbolCoverage.GetClippingObjects(float)`, `AreaPatternPrototype.Tile` ja `PatternTile`, `IDeferredDrawingTarget`
    ja sisäinen `RenderOptions.drawColorsDirectly`. Ne ovat [sanastossa](../Glossary.md).
  - Ryhmä ilman menetelmää `SaveLayer`: kun α on 1 ja B on Normal, ryhmän koostaminen on sama kuin sisällön piirto
    suoraan, koska lähde päällä (source over) on liitännäinen. Kun α on 1 ja B on Darken, sama pätee, koska saman
    peittävän värin C Darken on idempotentti: jos C on komponentissa tummempi kuin tausta, kumpikin antaa
    (1 − A) · tausta + A · C, missä A = 1 − ∏(1 − aᵢ) on muotojen peittojen aᵢ kertymä, ja muuten taustan. Näin näyttö
    ja testit pysyvät pikselilleen ennallaan, myös ratakartta, joka sekoittaa aina Darkenilla. Multiply ei ole
    idempotentti, joten se ja α < 1 koostetaan menetelmällä `SaveLayer`.
  - Sisältö palettiväripeitoittain: Skian koodaus kirjoittaa kunkin palettiväripeiton erikseen eikä palettileikkuria
    yhdellä täytöllä ja leikkauksella. Näin piirto on sama kuin vanhan piirron siellä, missä leikkausobjektit ovat
    samat, ja palettivärin peittojen reunanpehmennys kertyy kuten nyt. Koodaus käyttää muotojen piirtoon samoja
    `Map_SkiaStd`in apufunktioita kuin `Skia_GraphicsTarget`, ja tekstit samoja fontteja.
  - Piirron suorakulmio ja keskeytys: vaihe 2 rajaa symbolien palettiväripeitot samalla ehdolla kuin `DrawColor` ja
    kutsuu keskeytystä (`throwOnCancel`) samalla tavalla, joten samat symbolit piirtyvät.
  - Resoluution kanavamuunnos riippuu resoluutiosta vain sen kautta, mitkä katkotut osatekijät ovat yhtenäisiä, ja
    symbolin palettiväripeitto säilyttää tuloksen kullekin tällaiselle joukolle kuten leikkausobjektinsa.
  - Kuvioruutu on määrittelyn johdos, joka tehdään kerran kuten glyyfin jaetut muodot. Koodaus rasteroi sen siveltimen
    kokoiseksi kuten `CreatePatternBrushes` laskee koon resoluutiosta, joten siveltimen kuvat pysyvät siellä, missä
    vanha sivellin oli M:n mukaan.
  - Ratatiedosto kirjoittaa attribuutin `overprint-colors` arvolla true tiloissa `Darken` ja `Multiply` ja arvolla false
    tiloissa `None` ja `ByPaletteColor`, joten vanhempi Purple Pen lukee päällepainatusvaikutuksen lähimpänä tilana.
  - Tallennettu piirto lukee kartan vasta toistossa, joten se ei ole palettivärien tilannekuva: kartan muutos
    tallennuksen ja toiston välillä näkyy toistossa. Esikatselu on modaalinen, ja testit toistavat heti.
  - Legacy-piirtokohteet ja PDF pitävät vanhan piirron, koska GDI+:n ja WPF:n hylkääminen veisi mukanaan WinFormsin
    sovelluksen ja Test_MapModelin GDI+-renderöinnit, jotka Windowsin CI rakentaa ja ajaa. Toimitettava sovellus ei
    tarvitse niitä. CI:ssä ne ovat, koska base rakentaa ja testaa upstreamin sellaisenaan ja epicin haarat tarjotaan
    upstreamiin, jonka ratkaisussa WinFormsin sovellus on. PurplePen_Tests ja Test_MapModel testaavat myös paljon
    muuta koodia. Niiden poisto olisi oma muutoksensa baseen ja CI:hin, ja sen päättää @Pauli (24.11.).
  - Peiton vertailun oraakkeli valitaan sisäisellä piirtoasetuksella eikä testin omalla piirtokohteella, koska
    `IGraphicsTarget`in kaikkien jäsenten välittäminen olisi suurempi muutos.
- **Rajaus:**
  - PDF:n koodaus on oma tiketti tämän jälkeen. Siihen asti PDF-vienti sekä Linuxin ja macOS:n tulostus piirtävät
    palettivärit vanhalla piirtokoodilla.
  - Skian abstraktiot katkojuosteille ja toistosijoituksille (katkoefekti ja 1D-polkuefekti) ovat oma tiketti tämän
    jälkeen.
  - OCADin sekoitustilojen testit ovat oma tiketti ennen PDF:n koodausta. Piirtotilat (musteiden simuloima
    päällepainatus ja värierottelut) ja musteittainen B ovat oma tikettinsä.
  - Yksi palettipino kaikille kartoille on oma tikettinsä. Siihen asti ratakartta piirtyy omana karttanaan ja sekoittaa
    päällepainatusvärinsä Darkenilla kuten nyt.
  - Kuviosiveltimen kiertokeskuksen vertailu Mapperiin ja OCADiin tehdään jälkikäteen.
  - SVG-koodaus ja litistys eivät kuulu tähän.
- **Testit:** Paikallinen Map_Skia.Testsin ajo on `tools/linux-renderings.sh` integraatioworktreessa
  (`tools/int-worktree.sh`) Windows-koneen fonteilla (`tools/windows-fonts.sh`). Sen jokainen testi menee läpi
  (Progress.md, "Local tests"). `tools/compare-renderings.py` vertaa ajon renderöintejä haaran `feature/palette-stack`
  (e51b81a2) renderöinteihin ja antaa kuvat, jotka siirtyvät Linuxilla, ennen keräävää ajoa.
  - Vertailun karttoja ovat kansion `skia_render` kartat ja katkoviivanäytteet. Rakennetestit rakentavat palettivärin,
    jonka α on alle 1 ja B on Multiply, ratakartan purppuran Darkenilla, aluesymbolin, jonka kiertokeskus ei ole origo
    ja kulma on 0, kuviosiveltimellä, sekä katkoviivan, kaksoisviivan reunan ja alueen reunan, joiden välit ovat
    resoluutiota lyhyempiä.
  - Piirto muuttuu, joten täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack` keräävän ajon kanssa:
    `tools/push-wip.sh --collect --base-baselines prs/000-palette-stack-base <klooni> feature/palette-composition`.
    Siirtyneet kuvat tallennetaan haaraan `record-baselines.py`:llä kuten tools/README.md kertoo.
- **Haara:** `feature/palette-composition` juurtuu haaraan `feature/palette-stack` (e51b81a2). Täysi ajo tehdään
  haarassa `wip/linux/palette-composition` basen päällä valitsimella `--base-baselines prs/000-palette-stack-base`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md),
  [../README.md](../README.md), [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja
  "Decisions", speksin esipuhe ja luvut 01–04, [../Glossary.md](../Glossary.md), tiketit
  [023](023-palette-separations.md), [024](024-point-symbol-geometry.md), [011](011-line-geometry.md),
  [012](012-area-geometry.md), [013](013-text-geometry.md) ja [009](009-pattern-instances.md) sekä tämä tiketti.
  [../tools/README.md](../tools/README.md) kertoo tiketin työkierron.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
* PDF:n koodaus: oma tiketti tämän jälkeen (@Pauli, 23.11.), ei vielä kirjoitettu.
* Skian abstraktiot katkojuosteille ja toistosijoituksille: oma tiketti tämän jälkeen (@Pauli, 24.11.), ei vielä
  kirjoitettu.
* [027](027-ocad-blend-mode-tests.md) OCADin sekoitustilojen ja peittävyyden testit: oma tiketti ennen PDF:n koodausta
  (23.11.), odottaa @Paulin hyväksyntää.
