# Viivoitusten ja aluekuvioiden peittoprototyypit
**Kuvaus**
Alueen symbolimäärittelyn (`AreaSymDef`) viivoitukset (`HatchInfo`) ja aluekuviot (`PatternInfo`) ovat
peittoprototyyppejä (`CoveragePrototype`). Aluesymbolilla on jokaisen viivoituksensa ja leikatun aluekuvionsa
palettivärissä palettiväripeitto, jonka peittoprototyyppi on viivoitus tai aluekuvio. Sen kokoonpanon affiininen
muunnos M tulee aluesymbolin kulmasta ja kiertokeskuksesta sekä peittoprototyypin kulmasta ja siirrosta, ja sen
leikkausobjekti on alue. Palettierottelu ryhmittelee symbolien palettiväripeitot palettileikkureiksi kokoonpanojen
normaalimuodon mukaan. Täyttötilan valitsemat glyyfin osat ovat täyden peiton leikkausobjekteja. Sisäinen asetus
`omitHatchingsAndPatterns` ja kerääjä poistuvat, ja peiton vertailu kattaa jokaisen palettivärin. Piirto ei muutu.

**Hyväksymiskriteerit**
- [ ] Peittoprototyypit: viivoituksen peittoprototyyppi (`HatchingPrototype`) ja aluekuvion peittoprototyyppi
  (`AreaPatternPrototype`) ovat peittoprototyyppejä (`CoveragePrototype`). Viivoituksen peittoprototyyppi viittaa alueen
  symbolimäärittelyynsä ja viivoituksen indeksiin, ja aluekuvion peittoprototyyppi alueen symbolimäärittelyynsä,
  aluekuvion indeksiin ja palettiväriin. Ne eivät kopioi määrittelyn arvoja. Peittoprototyyppi on symbolimäärittelyn
  johdos: määrittely tekee sen kerran, ja jokainen aluesymboli viittaa samaan olioon, myös kun määrittely on
  `AreaComboSymDef`in yhdistelmän osamäärittely.
- [ ] Peitto G alkupisteeseen sidotussa muodossaan (`CoveragePrototype.Covers`): viivoituksessa piste on peitossa, kun
  sen etäisyys lähimmästä suorasta y = k · `hatchSpacing` on enintään `hatchWidth` / 2 (k on kokonaisluku). Aluekuvio on
  palettivärin glyyfin osat (`patternGlyph`) kulmalla −`patternAngle` hilan pisteissä x = i · `patternWidth` ja y = j ·
  `patternHeight` (i ja j ovat kokonaislukuja). Kun aluekuvio siirtää rivejä (`offsetRows`), parittoman rivin pisteet
  ovat `patternWidth` / 2 sivussa. Epäsäännöllisen aluekuvion (`irregular`) elementti on siirretty kuten
  `HandleIrregularity` siirtää sen: satunnaisesti mutta deterministisesti hilan pisteen koordinaateista, jotka on
  pyöristetty 0,01 mm:iin.
- [ ] Kokoonpanon affiininen muunnos: symbolin palettiväripeitto antaa kokoonpanonsa affiinisen muunnoksen M
  (`SymbolCoverage.Transformation`). Täyden peiton M on identiteetti. Viivoituksen M on T(c) · R(a) · R(`hatchAngle`) ·
  T(0, `hatchOffset`) ja aluekuvion M on T(c) · R(a) · R(`patternAngle`) · T(`patternOffsetX`, `patternOffsetY`), missä
  c on aluesymbolin kiertokeskus (`AreaSymbol.RotationCenter`) ja a alueobjektin kulma (`AreaSymbol.Angle`). Piirto
  (`DrawOneHatching`, `DrawHatchLines` ja `DrawPattern`) asettaa samat muunnokset. Kiertotila `Never` jättää c:n ja a:n
  pois, ja `ManualOnly` on sama kuin `Always` (päätös 21.11.).
- [ ] Leikkausobjektit: viivoituksen ja leikatun aluekuvion (`PatternFillMode.Clip`) palettiväripeiton leikkausobjekti
  on alue: pääpolun ja reikien täytetty muoto vuorottelevalla täyttösäännöllä (`SymPathWithHoles.MakeFilledShape`). Se
  on sama muoto kuin alueen täytön muoto ja sama alue, jolla piirto leikkaa viivoituksen ja aluekuvion (`PushClip`).
  Palettiväripeiton peitto on G M:n alla alueen sisällä.
- [ ] Kuviotäytön laajennus: viivoituksen ja leikatun aluekuvion palettiväripeitto antaa peittoprototyyppinsä
  laajennuksen alueen rajojen yli (`SymbolCoverage.ExpandPrototype`): viivoituksen viivat viivamuotoina ja aluekuvion
  elementit glyyfin jaettujen muotojen yksittäisinä sijoituksina. Ne ovat samoissa paikoissa, samoissa sisäkkäisissä
  affiinisissa muunnoksissa ja samassa järjestyksessä kuin piirto piirtää ne (`DrawHatchLines` ja `DrawPatternRows`).
  Origossa olevan yhden ympyrän glyyfi on kohdassaan ilman asemaa, kuten `Glyph.DrawSimple` piirtää sen. Epäsäännöllisen
  aluekuvion laajennus on sen elementtien sijoitukset (speksin luku 03).
- [ ] Valitut glyyfin osat: täyttötilat `CompletelyInside`, `CenterInside` ja `PartiallyInside` valitsevat aluekuvion
  jokaisen glyyfin osan erikseen sen omien pisteiden ja keskipisteen perusteella, kuten piirto (`GlyphPartFilter`) ja
  Mapper valitsevat ne. Valitut glyyfin osat ovat aluesymbolin täyden peiton palettiväripeiton leikkausobjekteja:
  glyyfin jaetut muodot yksittäisessä sijoituksessa, jonka sisäkkäiset affiiniset muunnokset ovat aluekuvion M ja
  elementin asema, hilan piste kulmalla −`patternAngle`. Ne ovat alueen täytön ja alueen reunan välissä piirron
  järjestyksessä. Täyttötilan aluekuviolla ei ole omaa palettiväripeittoa.
- [ ] Peittoprototyypit palettivärissä: `SymDef.GetCoveragePrototypes(Symbol, SymColor)` antaa aluesymbolille ensin
  täyden peiton, kun määrittely piirtää palettivärin alueen täytöllä, alueen reunalla tai täyttötilan aluekuviolla, ja
  sitten palettivärin viivoitukset ja leikatut aluekuviot piirron järjestyksessä, yhdistelmän osamäärittelyt
  järjestyksessään. Ristiviivoitus antaa kaksi peittoprototyyppiä.
- [ ] Palettileikkurit: palettierottelu tekee palettileikkurin jokaiselle kokoonpanolle. Symbolien palettiväripeitot,
  joilla on sama peittoprototyyppi ja M:n sama normaalimuoto, ovat yksi palettileikkuri, jonka affiininen muunnos
  (`PaletteCutter.Transformation`) on normaalimuoto. Normaalimuoto vie M:n siirron G:n siirtosymmetrioiden
  perusalueeseen G:n koordinaateissa ja pyöristää M:n kiinteään tarkkuuteen. Viivoituksen siirto viivojen suuntaan on 0
  ja poikittain välillä [0, `hatchSpacing`). Aluekuvion siirto on hilan kantavektorien kertoimina välillä [0, 1).
  Epäsäännöllisen aluekuvion siirto pysyy, koska sen ainoa siirtosymmetria on identiteetti. Täyden peiton
  palettiväripeitot ovat edelleen yksi palettileikkuri, jonka M on identiteetti.
- [ ] Kerääjä poistuu: sisäinen piirtoasetus `RenderOptions.omitHatchingsAndPatterns` ja sen tarkistukset
  (`AreaSymDef.DrawHatchings` ja `DrawPatterns`), kerääjä (`ClippingObjectCollector`) ja `SymDef.CollectClippingObjects`
  poistuvat. Testit eivät vertaa kerääjään: vertailu `GeometryMethodsMakeTheClippingObjectsOfTheCollector` ja
  rakennetestien `AssertSameStructureAsTheCollector` poistuvat, ja rakennetestien omat tarkistukset jäävät. Jokaisen
  leikkausobjektin sijoitus antaa asemansa kansion `skia_render` jokaisessa kartassa ja katkoviivanäytteissä, kuten
  tiketissä 013.
- [ ] Peiton vertailu (`PaletteSeparationCoverageTests`): jokaisen vertailukartan jokaisen palettivärin peitto on
  täsmälleen sama kuin piirron, myös viivoitusten ja aluekuvioiden palettivärien. Peitto on täyden peiton
  leikkausobjektit sekä kunkin viivoituksen ja leikatun aluekuvion laajennus palettiväripeiton leikkausobjektien
  sisällä. Ero ei ole sallittu. Vertailukartat ovat tiketin 023 kartat ja kartat, joissa on viivoituksia tai
  aluekuvioita (ks. "Testit"). Kartassa, jossa on kuvatason objekteja, alin palettiväri jää pois kuten ennen.
- [ ] Rakennetestit tarkistavat ilman piirtoa viivoituksen ja aluekuvion peittoprototyypit ja niiden peiton (`Covers`),
  ristiviivoituksen, monivärisen glyyfin aluekuvion palettiväreittäin, M:n kiertotiloilla `Always`, `ManualOnly` ja
  `Never` kulman, kiertokeskuksen ja siirron kanssa, palettileikkurit (saman määrittelyn alueet samalla kulmalla,
  alueet, joiden M eroaa hilavektorin tai viivoituksen välin verran, eri kulmat ja epäsäännöllinen aluekuvio), alueen
  leikkausobjektina reikineen, laajennuksen viivat ja elementit sekä yhden ympyrän glyyfin, täyttötilojen valinnat
  glyyfin osittain, yhdistelmän osamäärittelyn ja sisäkkäisen yhdistelmän viivoitukset ja aluekuviot sekä palettivärin,
  jota sama määrittely piirtää täydellä peitolla ja viivoituksella. Testit rakentavat kartan tapauksille, joita
  vertailun kartoissa ei ole (ks. "Testit").
- [ ] Tikettien 023, 024, 011, 012 ja 013 testit menevät läpi, paitsi vertailut kerääjään, jotka tämä tiketti poistaa.
  Testit, jotka tarkistavat, että vaihe 1 jättää viivoitukset ja aluekuviot pois
  (`AreaHasItsFillAndBorderButNotItsHatchingsAndPatterns` ja
  `AreaHasNoFullCoverageInAPaletteColorOfItsHatchingsAndAreaPatternsAlone`), tarkistavat niiden peittoprototyypit.
- [ ] Piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot**
- **Päätökset:**
  - @Pauli hyväksyi tämän tiketin ja sen istunnon valinnat (ks. "Ratkaisut") 23.11. Istunto kirjoitti luonnoksen
    tiketiksi samana päivänä, ja @Pauli päätti sen kolme kysymystä (ks. alla). Luonnos hyväksytään ensin tiketiksi, joka
    ei ole enää luonnos, ja vasta sitten se toteutetaan (@Pauli, 21.11. ja 23.11.).
  - Ennen tätä tikettiä vaihe 1 irrotetaan piirrosta ja kerääjä korvataan symbolien geometriametodeilla (@Pauli, 18.11.,
    "Pass 1 before the patterns"). Tiketit 023–026 ja 011–013 ovat tehneet sen.
  - Kerääjä on testien oraakkeli tikettien 008–013 ajan. Tämä tiketti poistaa sisäisen asetuksen
    `omitHatchingsAndPatterns` ja lopettaa kerääjän oraakkelikäytön, ja peiton vertailu kattaa silloin myös viivoitusten
    ja aluekuvioiden palettivärit (@Pauli, 18.11.).
  - Kaksi kokoonpanoa ovat samat, kun ne antavat saman peiton: palettiväri ja peittoprototyyppi ovat samat, ja M₁⁻¹M₂ on
    G:n symmetria. Normaalimuoto ratkaisee sen (@Pauli, 21.11., "The same configuration"). Tämä vastaa luonnoksen
    kysymykseen 1.
  - Viivoitus on oma peittoprototyyppinsä, raidat, ja `ManualOnly` on speksissä sama kuin `Always` (@Pauli, 21.11., "The
    hatchings and ManualOnly"). Tämä vastaa luonnoksen kysymyksiin 3 ja 4.
  - Speksi antaa täyttötilan valitsemat elementit symbolin palettiväripeiton johdoksina, mutta se ei sano, mihin
    palettileikkuriin ne kuuluvat (luonnoksen kysymys 2), ja niiden koostetut esitykset jäävät myöhemmäksi (@Pauli,
    22.11., "The selected elements in the specification").
  - Täyttötilan (`PatternFillMode`) `CompletelyInside`, `CenterInside` tai `PartiallyInside` valitsemat glyyfin osat
    ovat aluesymbolin täyden peiton (`CoveragePrototype.FullCoverage`) palettiväripeiton leikkausobjekteja, eikä
    aluekuviolla ole niille omaa palettiväripeittoa (@Pauli, 23.11., "The selected glyph parts of an area pattern").
    Tämä vastaa luonnoksen kysymykseen 2. Käsitteellisesti @Pauli ottaisi aluekuvion palettiväripeiton, mutta täysi
    peitto on selvästi helpompi renderöidä: Skia, PDF ja SVG kirjoittavat valitut glyyfin osat muotoina kuten
    pistesymbolin glyyfin osat. Aluekuvion palettiväripeitossa koodaus kirjoittaisi jaksollisen peiton, jonka sen omien
    glyyfin osien muodot leikkaavat, jolloin reunanpehmennys tehdään osien reunoilla kahdesti. Muuten koodauksen pitäisi
    lukea täyttötila ja piirtää osat suoraan, jolloin vaihe 2 ratkaisisi täyttötilojen eron, jonka speksin luku 03 antaa
    vaiheelle 1. Valittujen glyyfin osien muodot ovat aluekuvion glyyfin (`PatternInfo.patternGlyph`) jaettuja muotoja.
  - Täyttötilat `CompletelyInside`, `CenterInside` ja `PartiallyInside` valitsevat aluekuvion elementin jokaisen glyyfin
    osan erikseen sen omien pisteiden ja keskipisteen perusteella, kuten upstreamin piirto (`GlyphPartFilter`) ja
    Mapper, eivätkä koko elementtiä (@Pauli, 23.11., "The fill modes select each glyph part"). Tämä tarkentaa speksin
    luvun 02 kohtaa "The hatchings and the area patterns" ("selects the complete elements") ja luvun 03 kohtaa "The
    selected elements" sekä sanaston merkintöjä `AreaSymDef.PatternFillMode` ja *valitut elementit*. Speksi ja sanasto
    saivat tarkennuksen, kun @Pauli hyväksyi tiketin.
  - Kun aluekuvion elementin glyyfin osat ovat useassa palettivärissä, aluekuviolla on peittoprototyyppi kussakin näistä
    palettiväreistä: `AreaPatternPrototype` viittaa alueen symbolimäärittelyyn, aluekuvion indeksiin ja palettiväriin
    (@Pauli, 23.11., "The coverage prototype of an area pattern in a palette color"). G on binäärinen valitsin, joka
    kertoo jokaisesta tason pisteestä, onko piste peitossa, ja aluekuvio peittää kussakin palettivärissä eri pisteet.
    `CoveragePrototype.Covers(PointF)` säilyy ennallaan. Tämä tarkentaa speksin luvun 02 kohtaa "The coverage
    prototypes", jossa aluekuvio on indeksi `AreaSymDef.patterns`-luettelossa, ja sanaston merkintää
    `CoveragePrototype`. Speksi ja sanasto saivat tarkennuksen, kun @Pauli hyväksyi tiketin.
  - OCAD ja Mapper ovat omien karttojensa piirron auktoriteetit (speksin esipuhe). feature/multiply-blending on vain
    ulkoasun oraakkeli, eikä työ lue sen koodia (@Pauli, 18.11.).
  - Kuviotäyttöjen kanavamuunnoksia ovat esimerkiksi epäsäännöllisen kuvion elementtien sijoitukset ja kuvioruutu
    (@Pauli, 20.11., "The conversions of the model", ja luennat 20.11.).
  - Kun muutos ei voi muuttaa piirtoa, täysi ajo tehdään ilman keräävää ajoa (@Pauli, 18.11.).
- **Speksi:** [Specification.md](../Specification.md), luvut 02 ja 03. Luvun 02 kohdat "The coverage prototypes" ja "The
  hatchings and the area patterns" antavat peittoprototyypit, hilan, rakennekulman, alueobjektin kulman,
  kiertokeskuksen, kiertotilan, täyttötilan ja epäsäännöllisen aluekuvion. Luvun 03 kohdat "The configurations", "The
  clipping objects and the union", "The selected elements" ja "The derived values" antavat kokoonpanon, normaalimuodon,
  unionin, valitut elementit ja kuviotäyttöjen kanavamuunnokset. Nimet ovat [sanastossa](../Glossary.md).
- **Nykytila** (`feature/palette-stack`, 86df0a59):
  - `AreaSymDef.Draw` piirtää palettivärissä alueen täytön, viivoitukset (`DrawHatchings`), aluekuviot (`DrawPatterns`)
    ja alueen reunan tässä järjestyksessä. `AreaComboSymDef.Draw` piirtää yhdistelmän osamäärittelynsä järjestyksessä
    samalla kulmalla ja kiertokeskuksella.
  - `DrawHatchings` leikkaa alueella (`PushClip(path.GetPathKey(g))`), ja `DrawOneHatching` asettaa muunnoksen T(c) ·
    R(a) · R(`hatchAngle`). `SymbolHelpers.DrawHatchLines` piirtää alueen rajojen yli viivat y = `hatchOffset` + k ·
    `hatchSpacing` kynällä, jonka leveys on `hatchWidth`, päät suorat ja liitos jiiri.
  - `DrawPatterns` piirtää aluekuvion kuviosiveltimellä (`CreatePatternBrushes` ja `DrawPatternWithTexBrush`), kun
    `usePatternBitmaps` on päällä, piirtokohde tukee kuviosiveltimiä ja aluekuvio on säännöllinen ja leikattu eikä liian
    karkea, ja muuten glyyfi kerrallaan (`DrawPattern`). Kuviosivellin jättää kiertokeskuksen pois, kun alueobjektin
    kulma on 0. NotPurplePenin korjaus `fix/pattern-brush-origin` on haarassa `dev` mutta ei basessa.
  - `DrawPattern` leikkaa alueella vain täyttötilassa `Clip`, ja se asettaa muunnoksen T(c) · R(a) · R(`patternAngle`) ·
    T(`patternOffsetX`, `patternOffsetY`). `DrawPatternRows` käy hilan pisteet alueen rajojen yli rivi kerrallaan ja
    summaa askeleet, `HandleIrregularity` siirtää epäsäännöllisen aluekuvion elementin, ja glyyfi piirretään kulmalla
    −`patternAngle`. Täyttötila suodattaa glyyfin osat (`GlyphPartFilter`): `CompletelyInside` piirtää osan, jonka
    jokainen piste on alueen sisällä, `CenterInside` osan, jonka keskipiste on alueen sisällä, ja `PartiallyInside`
    osan, jonka jokin piste on alueen sisällä. Viivan ja alueen osan pisteet ovat sen polun pisteet ja keskipiste sen
    pinnan painopiste (`AreaCentroid`). Ympyrän pisteet ovat kahdeksan pistettä sen halkaisijan kehällä, ja sen
    keskipiste on ympyrän keskipiste.
  - Vaihe 1 jättää viivoitukset ja aluekuviot pois. Kerääjä (`ClippingObjectCollector`) piirtää asetuksella
    `omitHatchingsAndPatterns` ja heittää poikkeuksen, kun symboli leikkaa. `AreaSymDef.HasFullCoverage` ottaa vain
    alueen täytön ja alueen reunan palettivärit, ja `PaletteSeparation` tekee vain täyden peiton palettileikkurin.
    Geometriametodit eivät käytä kerääjää (tiketti 013), mutta testit vertaavat niitä siihen.
  - OCADin tuonti antaa viivoitukselle ja aluekuviolle kiertotilan `Always` tai `ManualOnly` ja kiertokeskukseksi kartan
    origon, `hatchOffset` on 0, ja täyttötila ja epäsäännöllisyys tulevat OCAD 12:sta alkaen. Mapperin tuonti antaa
    kiertotilan `Always` tai `Never`, siirrot Mapperin siirroista, täyttötilan Mapperin valinnasta (`no_clipping`) ja
    kiertokeskuksen objektin kuvion origosta (`rotationCoord`).
  - `AreaSymDef.CopyToMap` kopioi aluekuvion glyyfin mutta ei lisää aluekuviota uuteen määrittelyyn, joten kopio
    menettää aluekuvionsa. Vika on upstreamin, eikä tämä tiketti korjaa sitä.
- **Mapper oraakkelina:** Mapper (`src/core/symbols/point_symbol.cpp`: `createRenderablesIfCompletelyInside`,
  `createRenderablesIfCenterInside` ja `createRenderablesIfPartiallyInside`) valitsee täyttötiloissa pistesymbolinsa
  pisteen, ympyrän ja jokaisen elementin erikseen, kuten upstream glyyfin osat. Ehdot poikkeavat yksityiskohdissa:
  Mapper katsoo ympyrää neljästä pisteestä viivan keskeltä ja elementin keskipistettä pisteiden keskiarvosta. Ero ei
  kuulu tähän tikettiin.
- **Vanha työ:** ei ole. Vanha epicin haara `feature/palette-layers` ja sen viivojen, alueiden ja tekstien haarat
  jättävät viivoitukset ja aluekuviot pois, eikä luonnosta 009 toteutettu.
- **Ratkaisut** (istunnon valinnat, nimet valitsee istunto, @Pauli, 23.11.):
  - Nimet ovat `HatchingPrototype`, `AreaPatternPrototype`, `SymbolCoverage.Transformation` ja
    `SymbolCoverage.ExpandPrototype`. Alueen symbolimäärittely antaa peittoprototyyppinsä sisäisillä metodeilla, jotka
    tekevät ne kerran (`Interlocked.CompareExchange`) kuten glyyfin jaetut muodot. Peittoprototyyppi antaa
    normaalimuodon sisäisellä metodilla `CoveragePrototype.GetNormalForm`. `SymbolCoverage.Transformation` ei tarkista
    kartan muutoslaskuria, kuten `PaletteCutter.Transformation`, ja laajennus tarkistaa sen, kuten leikkausobjektit.
  - M sisältää peittoprototyypin siirron, kuten speksin luku 03 sanoo, myös viivoituksen `hatchOffset`in, jonka piirto
    lisää viivojen paikkoihin (`DrawHatchLines`). Laajennus pitää piirron sisäkkäiset affiiniset muunnokset ja viivojen
    paikat, jotta peitto on pikselilleen sama kuin piirron (speksin luku 03, "The placements").
  - Epäsäännöllisen aluekuvion G on aluekuvio siirtoineen. Sen siirtosymmetrioista vain identiteetti on varma, joten
    normaalimuoto ei muuta sen siirtoa, ja palettileikkurin jakavat vain alueet, joiden M on sama. Leikatun
    epäsäännöllisen aluekuvion leikkausobjekti on alue, ja laajennus antaa elementtien sijoitukset.
  - Normaalimuodon tarkkuus: M:n lineaarisen osan alkiot pyöristetään 10⁻⁶:n ja siirto 0,0001 mm:n tarkkuuteen, joka on
    laajennusten sallittu ero tiketissä 011. Pyöristys yhdistää kokoonpanot, joiden M:t eroavat tätä vähemmän, ja voi
    erottaa lähes samat kokoonpanot pyöristysrajan eri puolilla. Kumpikaan ei näy piirrossa.
  - Symbolin palettiväripeittojen järjestys palettivärissä on täysi peitto ensin ja sitten viivoitukset ja leikatut
    aluekuviot piirron järjestyksessä. Palettivärin peitto on unioni, joten järjestys ei muuta sitä.
  - Peiton vertailun apufunktio leikkaa viivoituksen ja leikatun aluekuvion palettiväripeiton leikkausobjekteilla ja
    piirtää laajennuksen leikkauksen sisään. Apufunktio on testikoodia eikä vaihe 2. `SymbolCoverageComparison`
    säilyttää laajennuksen (`Expand`), jota apufunktio käyttää, ja sen kerääjän vertailun osat poistuvat.
- **Sallitut erot:** ei ole. Peiton vertailu on tarkka.
- **Rajaus:**
  - Vaihe 2 on luonnos [010](010-palette-composition.md). Kuvioruutu ja muut kuviotäyttöjen kanavamuunnokset tulevat
    koodausten mukana, kuten viivojen kanavamuunnokset tiketissä [011](011-line-geometry.md).
  - Valittujen elementtien koostetut esitykset jäävät myöhemmäksi (päätös 22.11.).
  - Kuviosiveltimen piirto (`usePatternBitmaps`) ei ole vaiheen 1 lähde, eikä tämä tiketti muuta sitä.
  - Mapperin ja upstreamin valintaehtojen erot, `AreaSymDef.CopyToMap`in vika ja kahden kartan kolme pikseliä (ks.
    "Testit") eivät kuulu tähän.
  - SVG-koodaus, litistys ja korostukset eivät kuulu tähän.
- **Testit:** Paikallinen Map_Skia.Testsin ajo on `tools/linux-renderings.sh` integraatioworktreessa
  (`tools/int-worktree.sh`) Windows-koneen fonteilla (`tools/windows-fonts.sh`), ja sen jokainen testi menee läpi
  (Progress.md, "Local tests"). Piirto ei muutu, joten `tools/compare-renderings.py` ei löydä yhtään siirtynyttä kuvaa,
  kun se vertaa ajon renderöintejä haaran `feature/palette-stack` renderöinteihin.
  - Kansion `skia_render` 162 kartassa ja 10 katkoviivanäytteessä on 46 karttaa, joiden näkyvillä aluesymboleilla on
    viivoituksia tai aluekuvioita: 163 viivoitusta 28 kartassa ja 181 aluekuviota 43 kartassa, ja niillä 1 876 ja 2 402
    aluesymbolia. Viivoituksista 158:n kiertotila on `ManualOnly` ja 5:n `Never`, ja 15 määrittelyllä on ristiviivoitus.
    Aluekuvioista 154 on leikattuja, 19 `PartiallyInside`, 4 `CompletelyInside` ja 4 `CenterInside`, ja 18 on
    epäsäännöllisiä (3 leikattuna ja 15 `PartiallyInside`). 59 aluekuviota siirtää rivejä, 111:n glyyfi on yksi ympyrä
    origossa, 25:n glyyfissä on useita osia, 9:n glyyfissä on useita palettivärejä ja 25:n glyyfi ulottuu hilan solun
    yli. Aluekuvioiden kiertotila on `ManualOnly` 142:lla, `Always` 33:lla ja `Never` 6:lla, ja yhdellä on siirto.
    Alueobjektin kulma on muu kuin 0 42 aluesymbolilla viivoituksineen ja 52 aluesymbolilla aluekuvioineen. Täyttötilan
    aluekuvion glyyfissä on useita osia 22 aluesymbolilla.
  - Kartoissa ei ole yhdistelmäalueen viivoitusta tai aluekuviota, kiertokeskusta, joka ei ole origo ja jota kiertotila
    käyttää, `hatchOffset`ia, joka ei ole 0, eikä palettiväriä, jota sama määrittely piirtää täydellä peitolla ja
    viivoituksella tai aluekuviolla. Rakennetestit rakentavat ne.
  - Peiton vertailun uudet kartat ovat kuvaukset rotarea-test, rotarea-test9, offsetpattern_nopatbm,
    offsetpatternrot_nopatbm, offsetpatternrot2_nopatbm, wholestructure, wholestructure2–4, irregular1–4, holes, holes9,
    isomarea_nopatbm, isomareabug_nopatbm, borderedarea9, glyphholes, glyphholes9, aiimport, FtCasey, PenistoneHill,
    punchboxbug, missingcolor ja clouds, ja vertailu kattaa nyt myös karttojen graphicobjects9, LordHill, lksamm1 ja
    marymoor jokaisen palettivärin.
  - Istunto kokeili ratkaisuja testillä, jota se ei lisännyt koodiin (`tools/ticket-009/PatternPrototypeCheck.cs`, ja
    `tools/ticket-009/CountPatterns.cs` laski tapaukset). Kun viivoituksen tai leikatun aluekuvion laajennus piirretään
    alueen leikkauksen sisään ja valitut glyyfin osat täyden peiton leikkausobjekteina, peitto on sama kuin piirron
    jokaisessa palettivärissä näissä 30 kartassa. Kartoissa hakolaoverprinting ja teanwest, joita peiton vertailu ei ole
    kattanut, kolme pikseliä eroaa piirrosta. Hakolan pikseli on palettivärissä, jossa ei ole viivoituksia eikä
    aluekuvioita. Teanwestissä täyden peiton leikkausobjektit peittävät toisen pikselin, jota piirto ei peitä, ja piirto
    peittää toisen, jota kumpikaan ei peitä. Nämä kartat jäävät vertailun ulkopuolelle.
  - Täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack` ilman keräävää ajoa, koska asetuksen
    `omitHatchingsAndPatterns` tarkistukset ovat piirrossa aina epätosia: `tools/push-wip.sh --base-baselines
    prs/000-palette-stack-base <klooni> feature/pattern-prototypes`.
- **Haara:** `feature/pattern-prototypes` juurtuu haaraan `feature/palette-stack` (86df0a59). Täysi ajo tehdään haarassa
  `wip/linux/pattern-prototypes` basen päällä valitsimella `--base-baselines prs/000-palette-stack-base`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md),
  [../README.md](../README.md), [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja
  "Decisions", speksin esipuhe ja luvut 01–03, [../Glossary.md](../Glossary.md), tiketit
  [023](023-palette-separations.md), [024](024-point-symbol-geometry.md), [011](011-line-geometry.md),
  [012](012-area-geometry.md) ja [013](013-text-geometry.md) sekä tämä tiketti. [../tools/README.md](../tools/README.md)
  kertoo tiketin työkierron.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
