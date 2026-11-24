# SVG-vienti uuteen deviin ja SVG:n koodaus
**Kuvaus**
NotPurplePenin SVG-vienti on haarassa `feature/svg-target`, jota uusi dev (`dev-next`) ei sisällä. Tämä tiketti tuo
sen uuteen deviin ilman haaran `feature/svg-target` historiaa ja tekee SVG:n koodauksen: `Svg_GraphicsTarget` antaa
koodauksen (`IPaletteStackEncodingProvider`), joka kirjoittaa vaiheen 2 palettivärien läpinäkyvyysryhmät
(`TransparencyGroup`) samaan `SvgDocument`iin. SVG:n rakenne heijastaa `Map`in rakenteita mahdollisimman suoraan.
`Map_Svg`:n osat, jotka jäävät, tulevat sellaisinaan: dokumentti, fontit, tekstit, kuvat, georeferointi, sivut,
nykyiset metatiedot, selainten kiertotiet, `CourseSvg`, Avalonian dialogi ja niiden testit. Palettipinoa edeltävät
rakenteet väistyvät palettipinon käsitteiden tieltä: värin ryhmä (`BeginColorRun`, `data-c`) ja sen ympärille avattu sekoitustilan ryhmä palettivärin läpinäkyvyysryhmän tieltä,
objektinumerot (`data-o`) symbolin avainten (`Symbol.Key`), päällekkäisten alueiden unioni (`DrawOutline`,
`DescribeUnion`) palettileikkurin alueryppäiden (`AreaCluster`) ja toistuvien katkelmien uudelleenkäyttö (S-5) glyyfin
jaettujen muotojen tieltä. Vanhaa palettivärien polkua karttakoodissa ei tuoda. Testit tarkistavat SVG:n Chromiumissa
Skian koodauksen vaiheen 2 piirtoa vastaan. Vektoritiedostojen yhteinen rakenne `Map`in sisältöineen on oma tikettinsä
tämän jälkeen.

**Hyväksymiskriteerit**
- [ ] Tuonti: haara tuo uuteen deviin SVG-työn tilasta 1088d1295 (`feature/svg-target` ennen haaran
  `feature/multiply-blending` yhdistämistä) ne osat, jotka jäävät:
  - `Map_Svg`: dokumentti ja sen solmut, numerot, tyylit, värit ICC-profiileineen, fontit osajoukkoineen, tekstit,
    kuvat, JPEG-data ja pakkaus.
  - Graphics2D:n rakenteinen rajapinta (`IStructuredGraphicsTarget`) ratatietueineen (`CourseRecords`), georeferointi
    (`MapGeoreference`), fonttien osajoukot (`IFontSubsetProvider`) ja `IdentityComparer`.
  - `Map_SkiaStd`in fonttityö (`HarfBuzzSubsetter`, `CharacterMaps`, `OpenTypeFont`, `VariableFont`, `SymbolFont`,
    `SkiaFontSubsetProvider`, `MissingCharacterBox`, `EnhancedTypeface`, `FontFallback`) ja JPEG-data (`BitmapIO`,
    `SkiaFileLoader`, `Skia_GraphicsTarget`).
  - OCADin ruudukoiden EPSG-koodit (`MapProjection`, `Map.CoordinateSystemEpsg`) ja kartan georeferointi
    koordinaattimuuntimesta.
  - `PurplePenCore`n vienti: `CourseSvg`, PDF-viennin kanssa jaetut tiedostot ja sivut (`CourseOutputPages`),
    `MapDisplay`n kerroksittainen piirto (`MapDisplayLayer`) ja viennin asetukset (`CourseSvgSettings`).
  - Avalonian dialogi ja komento (`CreateSvgCoursesDialog`, `CreateSvgCoursesDialogViewModel`) käännöksineen.
  - Testit: `Map_Svg.Tests`, Skian renderöintitestien jaettu kirjasto (`RenderingTestCases`) ja muiden projektien
    testit, jotka koskevat näitä osia.

  Haaran `feature/svg-target` historiaa ei yhdistetä: osat tulevat uusina committeina (ks. "Ratkaisut").
- [ ] Mikä ei tule: `Map.DrawColor`in värin ryhmä (`BeginColorRun`), karttakoodin vektorisiveltimet aluekuvioille ja
  viivoituksille (`AreaSymDef`: `HatchingIsFilledWithBrush`, `CreateHatchBrush`, `FillInPatternFrame`), päällekkäisten
  alueiden ääriviivat ja unioni (`DrawOutline`, `DescribeUnion` ja `OverlappingAreas`in muutokset), kuviosiveltimen
  kiertokeskus vanhassa piirrossa (b217b9ddd), tietue siitä, mikä rataobjekti lisäsi kunkin ratakartan symbolin
  (711e089f9), ja haara `feature/multiply-blending`. `IStructuredGraphicsTarget`ista poistuvat kutsut, jotka
  palettipino korvaa: `BeginColorRun`, `EndColorRun`, `DrawOutline` ja `DescribeUnion`.
- [ ] SVG:n koodaus (`SvgEncoding`, ehdotettu nimi): `Svg_GraphicsTarget` on `IPaletteStackEncodingProvider` ja antaa
  koodauksen, joka kirjoittaa palettivärin läpinäkyvyysryhmän (`WriteTransparencyGroup(TransparencyGroup)`) samaan
  `SvgDocument`iin. Koodaus ja piirtokohde jakavat dokumentin, sen avoimen osan ja kartan sekä muunnokset ja
  leikkaukset, kuten Skian koodaus ja piirtokohde jakavat `SKCanvas`in (tiketti 010). Ryhmä on `<g>`, jolla on
  palettivärin asema kartan väritaulukossa (`data-c`) kuten värin ryhmällä nyt, peittävyys α (`opacity`), kun α on alle
  1, ja sekoitustila B (`mix-blend-mode`), kun B ei ole Normal (speksin luku 04, "The SVG channel"). Ryhmää ei
  yhdistetä naapureihinsa. Pohjakartat, kuvataso ja asettelutaso piirtyvät `Svg_GraphicsTarget`ille kuten nyt.
- [ ] Ryhmän sisältö: koodaus kirjoittaa kunkin symbolin palettiväripeiton erikseen palettierottelun järjestyksessä
  kuten Skian koodaus. Täyden peiton leikkausobjektit se kirjoittaa piirron resoluutiolla
  (`SymbolCoverage.GetClippingObjects(float)`) sijoitustensa sisäkkäisillä affiinisilla muunnoksilla
  (`Placement.Steps`): täytetyn muodon täyttösäännöllään, viivamuodon kynällään (`Stroke`) ja tekstijuosteen
  (`TextShape`) tekstinä upotetuin fontein ja muotoilluin merkkien paikoin. Muodot se kirjoittaa samoina SVG:n
  elementteinä ja tyyleinä kuin `Svg_GraphicsTarget` kirjoittaa vanhan piirron vastaavat muodot.
- [ ] Piirrosobjektit: jokainen elementti, jonka koodaus kirjoittaa symbolin palettiväripeitosta, kantaa symbolinsa
  avaimen tekstimuodon (`data-k`, ehdotettu nimi) objektinumeron (`data-o`) sijaan. Peräkkäiset saman
  symbolimäärittelyn palettiväripeitot jakavat ryhmän, jolla on symbolimäärittelyn numero (`data-s`), kuten nyt.
  Kuvatason ja asettelutason objektit kantavat samoin symboliensa avaimet (`BeginObject`).
- [ ] Toistuvat rakenteet: SVG:n rakenne heijastaa `Map`in rakenteita mahdollisimman suoraan (@Pauli, 25.11.).
  - Glyyfin jaetut muodot palettivärissä koodaus määrittelee kerran (`<defs>`), ja kukin sijoitus viittaa niihin
    (`<use>`). Sijoituksen sisäkkäiset affiiniset muunnokset (`Placement.Steps`) se kirjoittaa SVG:n muunnoslistana
    (`transform`) niiden järjestyksessä.
  - Katkojuosteet ja toistosijoitukset: luonnoksen kysymys 7.
  - Siirretyn polun (`OffsetPathShape`) koodaus kirjoittaa siirrettynä käyränä, koska SVG:llä ei ole siirtymää. Sen
    katkot se kirjoittaa yksittäisinä katkoina (`StrokedShape.ExpandDashes`), koska siirretyn polun katkokuvio mitataan
    peruspolkua pitkin ja SVG mittaa katkokuvion elementin omaa polkua pitkin.
- [ ] Kuviotäytöt:
  - Viivoituksen ja säännöllisen leikatun aluekuvion koodaus kirjoittaa kuviotäyttönä (`<pattern>`), kun ryhmä käyttää
    kuviosiveltimiä (`TransparencyGroup.UsePatternBrushes`). Peittoprototyyppi (`HatchingPrototype`,
    `AreaPatternPrototype`) on määrittely, joka on origoon sidottu ja yksivärinen ja jonka aukot jäävät maalaamatta, ja
    kunkin kokoonpanon kuviotäyttö viittaa siihen M:n kanssa (`patternTransform`). Aluekuvion määrittely on sen
    kuvioruutu (`AreaPatternPrototype.Tile`) vektoreina, ja viivoituksen määrittely on sen yksi viiva.
  - Usean alueen alueryppään kuviotäyttö kirjoitetaan kerran ryppään unionin (`AreaCluster.Union`) leikkaukseen
    (`clipPath`, nonzero-sääntö) kuten tiketissä 028.
  - Epäsäännöllisen aluekuvion koodaus kirjoittaa laajennuksena (`SymbolCoverage.ExpandPrototype`) leikkauksen
    sisään kuten Skian koodaus. Samoin se kirjoittaa kaikki kuviotäytöt, kun viennin asetus "Draw area patterns as
    separate shapes" (`CourseSvgSettings.DrawAreaPatternsAsSeparateShapes`) on päällä. Silloin kukin alue kirjoittaa
    oman laajennuksensa oman alueensa leikkaukseen eikä ryppään unioniin (ks. "Ratkaisut").
- [ ] Päällepainatustila: SVG-vienti käyttää ratojen ulkoasun päällepainatustilaa (`OverprintMode`) kuten muut kanavat,
  ja ratakartta sekoittaa päällepainatusvärinsä Darkenilla kuten nyt. Chromium sekoittaa sRGB:ssä kuten Skia: Darken
  on värikomponenttien minimi ja Multiply niiden tulo.
- [ ] Ratatiedot (CD-1–CD-3): ratakartan piirrosobjektit kantavat symboliensa avaimet `<rataobjektin avain>/<indeksi>`
  (tiketti 026). Ratatiedot kuvaavat kunkin rataobjektin sen avaimella (`CourseObj.Key`): `npp:object` on
  rataobjektia kohden objektinumeron sijaan, ja sen laji ja rastit ovat kuten nyt.
- [ ] Metatiedot: `npp:document`, `npp:map`, `npp:color`, `npp:symbol`, georeferointi ja sivujen näkymät pysyvät
  kuten nyt. `npp:color` kirjoitetaan kullekin palettivärille, jonka ryhmä kirjoitettiin, ja `npp:symbol` kullekin
  symbolimäärittelylle, jonka palettiväripeittoja kirjoitettiin.
- [ ] Vertailu: Chromiumin piirtämää SVG:tä verrataan Skian viitekuvaan pikselitoleranssilla (`PixelTolerance`) kuten
  nyt. Tapauksen tallennettu piirto (`StructuredRecordingGraphicsTarget`) toistetaan sekä `Svg_GraphicsTarget`ille että
  `Skia_GraphicsTarget`ille, ja kumpikin kirjoittaa palettivärit koodauksellaan (`IDeferredDrawingTarget`, tiketti
  010). Geometriatarkistus (V-10) ottaa palettivärien odotetun geometrian välikerroksesta, leikkausobjekteista piirron
  resoluutiolla ja niiden laajennuksista, ja piirtokutsuista sen, mikä piirtyy palettipinon rinnalla. Selainten
  tarkistus (V-6) ja sivujen vertailu sovelluksen omaan piirtoon pysyvät.
- [ ] Rakennetestit tarkistavat ilman selainta: palettivärin läpinäkyvyysryhmän α:n ja B:n kussakin
  päällepainatustilassa, palettiväripeitot palettierottelun järjestyksessä symbolien avaimin, kuviotäytön prototyypin
  määrittelynä ja kokoonpanon viittauksena, alueryppään kuviotäytön kerran unionissa ja asetuksella "Draw area patterns
  as separate shapes" alue kerrallaan, jaettujen muotojen `<use>`it, katkojuosteet ja toistosijoitukset kysymyksen 7
  mukaan, siirretyn polun katkot, kuvatason ja asettelutason
  objektit symbolien avaimin sekä ratatiedot rataobjektien avaimin. `Map_Svg.Tests`in testit, jotka koskevat
  väistyviä rakenteita, kirjoitetaan palettipinon käsitteillä, ja muut menevät läpi kuten nyt.
- [ ] Kuvat ja baselinet:
  - SVG-vertailun viitekuvat (`src/MapModel/TestFiles/svg_reference`) siirtyvät siellä, missä Skian koodaus piirtää
    toisin kuin vanha piirto rakenteen säilyttävälle piirtokohteelle: tiketin 010 kriteerin "Kuvat" syyt, tiketin 028
    alueryppäät ja vanhan polun vektorisiveltimet, joita Skian koodaus ei käytä. Tiketti kertoo kuvat ja syyt.
  - SVG-viennin baselinet (`src/TestFiles/svg_export`) ja selainten tarkistuksen luvut siirtyvät rakenteen mukana, ja
    tiketti kertoo muutokset.
  - Kuvavienti siirtää baselinet, jotka Skian JPEG-kooderi siirtää, kuten haarassa `feature/svg-target` (1088d1295).
  - Muut kuva- ja tekstibaselinet eivät siirry. Siirtyneet baselinet tallennetaan @Paulin luvalla, jonka hän antaa
    hyväksyessään tiketin.
- [ ] Ajot: täysi ajo menee läpi (`wip/linux/svg-encoding` valitsimella `--base dev-next`), Windowsin koonti ja
  `CourseSvgBaselineTests` ryhmässä Skia mukaan lukien. SVG-silmukka menee läpi Linuxilla ilman epäonnistuneita
  tapauksia, ja tiketti kertoo, jos odottavat tai selaimelle erotetut tapaukset eroavat tilan 1088d1295 silmukasta.
- [ ] Toimitettava sovellus: SVG-vienti on Avalonian valikossa ja dialogissa, ja haaran koepaketista voi viedä SVG:n.

**Lisätiedot**
- **Päätökset:**
  - SVG:n rakenne heijastaa `Map`in rakenteita mahdollisimman suoraan, ja rakenteet on mietitty SVG-tiedostomuodon
    edellytysten pohjalta (@Pauli, 25.11., "The SVG reflects the structures of `Map`").
  - SVG-vienti on kaksi tikettiä. Tämä on niistä ensimmäinen: `Map_Svg` uuteen deviin SVG:n koodauksella, ja sen
    testit tarkistavat SVG:n Chromiumissa Skian vaiheen 2 piirtoa vastaan. Toinen on vektoritiedostojen yhteinen
    rakenne `Map`in sisältöineen, ja se kirjoitetaan tämän jälkeen (@Pauli, 25.11., "The two tickets of the SVG
    export").
  - SVG:n koodaus on NotPurplePenin `Map_Svg`:ssä, joka tulee uuteen deviin: `Svg_GraphicsTarget` antaa koodauksen,
    joka kirjoittaa vaiheen 2 läpinäkyvyysryhmät `SvgDocument`iin. `Map_Svg`:n palettipinoa edeltävät rakenteet
    väistyvät palettipinon käsitteiden tieltä, ja muut osat ja niiden testit jäävät (@Pauli, 24.11., "The place of the
    SVG encoding").
  - Uusi dev rakentuu vahvasti palettipinolle (@Pauli, 24.11.). Uudessa basewithfixesissä ei ole korjausta
    `fix/overlapping-patterns`, `feature/multiply-blending` jää pois, ja `Map_Svg` tulee tässä tiketissä ilman haaran
    `feature/svg-target` historiaa (@Pauli, 25.11.).
  - PDF- ja SVG-viennillä on sama muoto, ja kumpikin kantaa `Map`in sisällön, jonka lippu voi jättää pois. SVG tehdään
    ennen PDF:ää (@Pauli, 25.11.). Ohjelmalla ei ole käyttäjiä (@Pauli, 25.11.).
  - Päällepainatustila valitsee sekoitustilan kaikissa kanavissa (@Pauli, 23.11.). Skian koodaus laajentaa
    katkojuosteet ja toistosijoitukset, ja niiden abstraktiot saavat oman tiketin, jotta piirron vaihtumisen ja
    abstraktioiden siirtämät kuvat tarkistetaan erikseen (@Pauli, 24.11.). Alueryppäät ja niiden unionit ovat
    välikerroksen johdoksia, samat kaikille kanaville (@Pauli, 24.11.).
  - SVG-työn päätökset ([SvgExportDesign.md](../../SvgExportDesign.md)): viivoitukset ja säännölliset aluekuviot ovat
    kuviotäyttöjä, ja asetus "Draw area patterns as separate shapes" piirtää ne elementti ja viiva kerrallaan
    lukijalle, joka ei osaa täyttää aluetta kuviolla (@Pauli, 6.11.). Kullakin symbolilla ja värillä on oma kuvion
    määrittely (@Pauli, 12.11.). Asetus "Draw area patterns as separate shapes" piirtää kunkin alueen oman kuvion,
    jotta Scribus ei menetä mitään (@Pauli, 13.11.).
- **Speksi:** [Specification.md](../Specification.md), luku 04: "Pass 2 and the transparency groups", "The encodings
  and the channels", "The SVG channel" ja "The drawn objects and the files". Luku 03 antaa johdokset ja
  kanavamuunnokset. Nimet ovat [sanastossa](../Glossary.md).
- **Nykytila:**
  - `dev-next` (a777b4aab): ei `Map_Svg`:tä eikä SVG-vientiä. Palettivärit piirtyvät vaiheella 2 piirtokohteelle, joka
    antaa koodauksen (`Skia_GraphicsTarget`), ja muille vanhalla piirrolla (`DrawColor`). Testien työnkulun ryhmä Skia
    ajaa `CourseSvgBaselineTests`in, kun projekti `Map_Svg.Tests` on koonnissa, ja SVG-silmukan työnkulku (`svg.yml`)
    on mukana.
  - SVG-työ tilassa 1088d1295: 100 committia vanhan basewithfixesin tilasta 7d2699b06, joista 10 on jo uudessa
    devissä basewithfixesin kautta: glyyfit muotoilluissa paikoissaan alikuvapisteen tarkkuudella ja niiden siirtämät
    baselinet, fonttidatan lukko ja puuttuva järjestelmäfontti. `Map_Svg`:ssä on 16 lähdetiedostoa (5 092 riviä) ja
    `Map_Svg.Tests`issä 39 lähdetiedostoa (10 303 riviä) sekä fontit ja skriptit. Viitekuvia on 193 (`svg_reference`)
    ja viennin baselineja 27 (`svg_export`). Tilan 3b5c7b7eb SVG-silmukka (43563468407): 868 tapausta meni läpi, 0
    epäonnistui, 2 odottaa kutsuja, joita piirtokohde ei koskaan noudata, ja 16 on erotettu selaimelle.
  - Vanha palettivärien polku: `Map.DrawColor` avaa rakenteen säilyttävälle piirtokohteelle värin ryhmän
    (`BeginColorRun`) ja objektin (`BeginObject`) ja kuvaa päällekkäisten alueiden unionin (`DrawOutline`,
    `DescribeUnion`). Päällepainatusväri sekoitetaan ryhmällä `mix-blend-mode:darken`, ja muut sekoitustilat ovat
    kutsuja, joita piirtokohde ei koskaan noudata. Karttakoodi piirtää vektorisiveltimen piirtokohteelle aluekuviot ja
    viivoitukset siveltimillä (`GraphicsUtil.PatternBrushesAreVectors`). Elementti kantaa objektinumeronsa (`data-o`),
    ja S-5 määrittelee kerran katkelmat, joiden teksti toistuu.
  - Koeyhdistäminen (`git merge-tree`) tilasta 1088d1295 haaraan `dev-next`: ristiriidat ovat kymmenessä
    kooditiedostossa (`Map.cs`, `SymDef.cs`, `GraphicsUtil.cs`, `OverlappingAreas.cs`, `CourseLayout.cs`,
    `MapDisplay.cs`, `RenderingTests.cs`, `RenderingUtil.cs`, `MainWindowViewModel.cs` ja `Directory.Packages.props`)
    ja 48 kuvassa.
  - Scribus 1.6.3 (tämän koneen paketti) leikkaa leikkauspolulla parillisuussäännöllä myös silloin, kun polulla on
    `clip-rule:nonzero` tai ei sääntöä: kahden päällekkäisen neliön yhden polun leikkaus jätti neliöiden yhteisen
    osan tyhjäksi. Inkscape 1.4 noudattaa sääntöä. Ryppään unioni tarvitsee nonzero-säännön (tiketti 028).
  - Toistuvien rakenteiden kokeet tämän koneen lukijoilla:
    - Chromium (Playwrightin headless shell 1243) ja Inkscape 1.4 piirtävät oikein markerit polun kaikkiin
      kärkipisteisiin (`orient="auto"`), nollan pituiset katkot pyöreillä päillä pisteinä ja katkokuviot
      (`stroke-dasharray`, `stroke-dashoffset`), myös ryhmän skaalauksen alla.
    - Scribus 1.6.3 piirtää markerin vain polun alkuun ja loppuun. Katkokuvion pituudet se lukee pisteinä (pt) eikä
      käyttäjäyksikköinä: millimetreinä kirjoitettu kuvio 3 2 tuli noin 1,7 mm:n jaksolla. Viennin rakenteessa, jossa
      sisältö on 0,001 mm:n askelina skaalauksen alla, katkoviivat ja pisteviiva tulivat yhtenäisinä.
    - `<use>` muunnoksineen piirtyy oikein kaikissa kolmessa.
- **Vanha työ:** `feature/svg-target` (1088d1295) ja [SvgExportDesign.md](../../SvgExportDesign.md). Sopii: kriteerin
  "Tuonti" osat sekä vertailu, toleranssi, geometriatarkistus ja silmukka, joiden vertailukohta on jo Skian piirto. Ei
  sovi: vanha palettivärien polku (ks. "Nykytila"), koska vaihe 2 ei kutsu symbolien piirtokoodia.
- **Ratkaisut** (istunnon valinnat, jotka @Pauli hyväksyy tiketin kanssa):
  - Osat tulevat uusina committeina haaraan, joka juurtuu haaraan `dev-next`: ensin osat, jotka jäävät, sellaisinaan
    tilasta 1088d1295 ilman vanhaa palettivärien polkua, ja sitten koodaus. Commitin viesti luettelee haaran
    `feature/svg-target` commitit, joiden työn se tuo, kuten yhdistämisen viesti luettelee commitit. Näin haara ei tuo
    korjausta `fix/overlapping-patterns` eikä haaraa `feature/multiply-blending`.
  - Palettiväripeitot kirjoitetaan erikseen kuten Skian koodauksessa. SVG:n leikkauspolku ottaa lapsistaan vain
    geometrian eikä kynää, joten palettileikkurin yksi täyttö ja yksi leikkaus vaatisi viivamuodoista kynänjäljen.
    Erikseen kirjoitettuna jokainen piirrosobjekti kantaa symbolinsa avaimen, ja Chromium kertaa reunanpehmennyksen
    kuten Skia.
  - Jaetut muodot viitataan aina, myös glyyfi, joka sijoitetaan kerran, ja yhden ympyrän glyyfi: viittaus heijastaa
    sitä, että muodot kuuluvat symbolimäärittelyyn. Vanha S-5 jätti kerran löytyneen katkelman ja ympyrän paikalleen
    tiedoston koon vuoksi.
  - Asetuksella "Draw area patterns as separate shapes" kukin alue kirjoittaa oman kuvionsa, päätöksen 13.11.
    mukaisesti. Sen peruste pätee edelleen: Scribus leikkaa parillisuussäännöllä, joten ryppään unioni, joka tarvitsee
    nonzero-säännön, jättäisi Scribusissa alueiden yhteiset osat ilman kuviota. Kuvio kertautuu vain alueiden yhteisissä
    osissa. Vaihe 2 antaa asetuksen ryhmälle (`TransparencyGroup.PatternsWithEachArea`, ehdotettu nimi, asetuksesta
    `RenderOptions.patternsWithEachArea`). Oletustilassa kuviotäyttö on kerran unionissa kuten muissa kanavissa.
  - `IStructuredGraphicsTarget.PatternBrushesAreVectors` jää: `MapDisplay` kytkee sen perusteella kuviosiveltimet
    viennissä päälle myös tulostettaessa, ja koodaus saa ne ryhmän asetuksena (`TransparencyGroup.UsePatternBrushes`).
    Vektorikuvio ei ole koskaan liian karkea, joten SVG:n koodaus ei kysy sitä (`PatternTile.IsTooPixelatedAt`).
  - `data-k` on symbolin avaimen tekstimuoto, ja `data-s` ja `data-c` pysyvät. Ratatietojen muodon versio pysyy 1:nä,
    koska yksikään julkaisu ei ole kirjoittanut ratatietoja (SvgExportDesign.md, "Course data").
  - Nimet `SvgEncoding`, `data-k` ja `TransparencyGroup.PatternsWithEachArea` ovat sanastossa.
- **Luonnoksen kysymykset:** 6 ratkesi päätöksellä "The two tickets of the SVG export" (25.11.). 7, katkojuosteet ja
  toistosijoitukset SVG:ssä, on avoin.
- **Rajaus:**
  - Vektoritiedostojen yhteinen rakenne `Map`in sisältöineen ja lippu, joka jättää sen pois, ovat oma tikettinsä tämän
    jälkeen. PDF:n koodaus on oma tikettinsä sen jälkeen.
  - Piirtotilat (värierottelut, musteiden simuloima päällepainatus ja musteittainen B) ja yksi palettipino kaikille
    kartoille ovat omat tikettinsä. Läpinäkyvyyden litistys ei kuulu tähän.
  - `feature/multiply-blending` ei tule, koska päällepainatustila korvaa sen.
- **Testit:**
  - Paikallisesti `../../SvgDevLoop/run-svg-tests.sh <worktree> <tulokset>` Linuxilla Chromiumin kanssa, ja selainten
    tarkistus myös Firefoxilla ja WebKitillä. `Map_Skia.Tests` ajetaan kuten Handover.md:n kohta "Local tests" kertoo.
  - SVG-silmukka: `../../SvgDevLoop/push-svg-loop.sh <worktree>` rakentaa haaran `svg/svg-encoding` basen ja haaran
    yhdistämisenä ja ajaa silmukan, ja tulokset tulevat haaraan `svg-results/svg-encoding`. Puuttuvat viitekuvat
    tulevat tulosten kansiosta `references/`.
  - Piirto muuttuu, joten täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack` keräävän ajon kanssa:
    `tools/push-wip.sh --collect --base dev-next <klooni> feature/svg-encoding`.
- **Haara:** `feature/svg-encoding` juurtuu haaraan `dev-next` (a777b4aab). Täydet ajot tehdään haarassa
  `wip/linux/svg-encoding` valitsimella `--base dev-next`, ja SVG-silmukka haarassa `svg/svg-encoding`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md),
  [../README.md](../README.md), [../Handover.md](../Handover.md), speksin esipuhe ja luvut 01–04,
  [../Glossary.md](../Glossary.md), tiketit [010](010-palette-composition.md) ja [028](028-palette-cutter-patterns.md),
  [SvgExportDesign.md](../../SvgExportDesign.md):n luvut "The SVG target's core design", "Where the export lives",
  "Drawing a map's parts and a course part's layers apart", "Course data", "Baseline tests", "Area patterns as pattern
  fills", "Symbol reuse (S-5)" ja "Overlapping patterns, and a colour's layer",
  [../../SvgDevLoop/README.md](../../SvgDevLoop/README.md) sekä tämä tiketti.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
