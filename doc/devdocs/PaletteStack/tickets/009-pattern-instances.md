# Viivoitukset ja aluekuviot peittoprototyyppeinä
**Kuvaus**
Alueen symbolimäärittelyn (`AreaSymDef`) viivoitukset (`HatchInfo`) ja aluekuviot (`PatternInfo`) ovat
peittoprototyyppejä. Aluesymbolilla on palettivärissä symbolin palettiväripeitto jokaiselle viivoitukselle ja
aluekuviolle, joka piirtää palettivärin. Peiton affiininen muunnos M tulee aluesymbolin kulmasta ja kiertokeskuksesta
sekä viivoituksen tai aluekuvion rakennekulmasta ja siirtymästä. Samanlaiset kokoonpanot ovat yksi palettileikkuri, ja
normaalimuoto ratkaisee sen. Viivoituksen ja leikatun aluekuvion leikkausobjekti on alue, ja täyttötilan valitsemat
elementin osat ovat leikkausobjekteja. Näin vaihe 1 tekee kartan jokaisen palettivärin. Sisäinen piirtoasetus
`omitHatchingsAndPatterns` ja kerääjä poistuvat, ja peiton vertailu vertaa jokaista palettiväriä piirtoon. Piirto ei
muutu.

**Hyväksymiskriteerit**
- [ ] Peittoprototyypit: täyden peiton (`CoveragePrototype.FullCoverage`) lisäksi peittoprototyyppejä ovat viivoitus
  (`HatchingPrototype`) ja aluekuvio palettivärissä (`AreaPatternPrototype`). Alueen symbolimäärittely säilyttää ne
  johdoksina: yhden kullekin viivoitukselle ja yhden kullekin aluekuviolle ja palettivärille, jossa sen elementillä on
  osia. Saman symbolimäärittelyn aluesymbolit viittaavat samoihin peittoprototyyppeihin. Ristiviivoitus on kaksi
  viivoitusta, joten sillä on kaksi peittoprototyyppiä.
- [ ] Symbolin palettiväripeitot: `SymDef.GetCoveragePrototypes(Symbol, SymColor)` antaa aluesymbolille palettivärissä
  ensin täyden peiton, kun alueen täyttö tai alueen reuna piirtää palettivärin, sitten jokaisen viivoituksen
  peittoprototyypin, jonka väri palettiväri on, viivoitusten järjestyksessä, ja sitten jokaisen aluekuvion
  peittoprototyypin tässä palettivärissä aluekuvioiden järjestyksessä. `AreaComboSymDef` antaa ensin täyden peiton,
  kun jokin yhdistelmän osamäärittely piirtää palettivärin täydellä peitolla, ja sitten yhdistelmän osamäärittelyjen
  viivoitusten ja aluekuvioiden peittoprototyypit osamäärittelyjen järjestyksessä: yhdistelmän osamäärittely on osa
  peittoprototyyppiä. Palettiväri, jota vain viivoitus tai aluekuvio piirtää, saa aluesymbolilta palettiväripeiton.
- [ ] Viivoituksen peitto G: viivoituksen viivat omissa koordinaateissaan, eli pisteet, joiden etäisyys lähimmästä
  viivasta y = `hatchOffset` + k · `hatchSpacing` on enintään puolet viivan leveydestä (`hatchWidth`), kuten
  `SymbolHelpers.DrawHatchLines` piirtää viivat litteillä päillä.
- [ ] Aluekuvion peitto G palettivärissä: elementin (`patternGlyph`) osat tässä palettivärissä jokaisessa hilan
  pisteessä (i · `patternWidth`, j · `patternHeight`), parittomilla riveillä siirrettyinä puolella leveydellä, kun
  `offsetRows` on päällä. Elementin affiininen muunnos E kiertää elementtiä hilan pisteensä ympäri rakennekulman
  vastakulmalla, kuten `DrawPatternRows` piirtää elementit. Epäsäännöllisen kuvion (`irregular`) elementtien siirrot
  (`HandleIrregularity`) ovat osa G:tä: siirto tulee hilan pisteen pyöristetystä paikasta, joten `Map` säilyttää
  satunnaisuuden määrittelynä.
- [ ] Kokoonpano: symbolin palettiväripeitolla on affiininen muunnos M (`SymbolCoverage.Transformation`). Viivoituksen M
  on T(kiertokeskus) · R(alueobjektin kulma) · R(rakennekulma), kuten `DrawOneHatching` asettaa sen, ja aluekuvion M on
  T(kiertokeskus) · R(alueobjektin kulma) · R(rakennekulma) · T(siirtymä), kuten `DrawPattern` asettaa sen. Kiertotila
  `Never` jättää aluesymbolin kulman ja kiertokeskuksen pois, ja `ManualOnly` on sama kuin `Always`. Täyden peiton M on
  identiteetti.
- [ ] Palettileikkurit: palettierottelun ne symbolien palettiväripeitot, joiden kokoonpanot ovat samat, ovat yksi
  palettileikkuri. Kokoonpanot ovat samat, kun niiden palettiväri, peittoprototyyppi ja M:n normaalimuoto ovat samat.
  Normaalimuodossa M:n lineaarinen osa on pyöristetty ja M:n siirto on redusoitu G:n symmetrioiden siirtojen
  perusalueeseen ja pyöristetty. Viivoituksen symmetrioita ovat kaikki siirrot viivojen suuntaan ja välin monikerrat
  viivojen poikki, aluekuvion symmetrioita hilan siirrot, ja epäsäännöllisen kuvion siirroista symmetria on vain nolla.
  Palettileikkurin M (`PaletteCutter.Transformation`) on normaalimuoto. Palettileikkurit ovat ensimmäisten
  palettiväripeittojensa järjestyksessä, kuten tiketissä [023](023-palette-separations.md).
- [ ] Leikkausobjektit: viivoituksen ja leikatun aluekuvion (`PatternFillMode.Clip`) leikkausobjekti on alue:
  pääpolku ja reiät täytettynä muotona vuorottelevalla täyttösäännöllä, kuten piirto rajaa ne
  (`SymPathWithHoles.GetPathKey`). Täyttötilojen `CompletelyInside`, `CenterInside` ja `PartiallyInside`
  leikkausobjektit ovat valitut elementin osat: niiden hilan pisteiden elementtien osat tässä palettivärissä, jotka
  täyttötila valitsee osan pisteiden ja keskipisteen perusteella, kuten `DrawPatternRows` valitsee ne
  (`GlyphPartFilter`). Ne ovat elementin jaettuja muotoja sijoituksessa, jonka askeleet ovat M ja elementin asema hilan
  pisteessä. Näin G M:n alla leikkausobjektien unionissa on täsmälleen valitut osat.
- [ ] Laajennus: peittoprototyyppi antaa elementtinsä annetussa suorakulmiossa omissa koordinaateissaan, siinä
  järjestyksessä ja niillä luvuilla, joilla piirto käy suorakulmion läpi: viivoitus viivansa viivamuotoina
  (`DrawHatchLines`) ja aluekuvio elementtinsä osat palettivärissä yksittäisissä sijoituksissaan (`DrawPatternRows`).
  Jokaisen leikkausobjektin ja laajennuksen sijoitus antaa asemansa (`Placement.Poses`).
- [ ] Kerääjän poisto: sisäinen piirtoasetus `RenderOptions.omitHatchingsAndPatterns`, kerääjä
  (`ClippingObjectCollector` ja `SymDef.CollectClippingObjects`) ja rakenteellinen vertailu kerääjään
  (`SymbolCoverageComparison.FindDifferences` ja `GeometryMethodsMakeTheClippingObjectsOfTheCollector`) poistuvat.
  Tikettien 024, 011, 012 ja 013 rakennetestit tarkistavat leikkausobjektit ilman kerääjää.
- [ ] Peiton vertailu: jokaisen palettierottelun peitto on täsmälleen sama kuin nykyisen piirron peitto tässä
  palettivärissä. Palettierottelun peitto on sen symbolien palettiväripeittojen peittojen unioni: kunkin G M:n alla
  leikkausobjektiensa unionissa. Piirto piirtää aluekuviot glyyfi kerrallaan (`usePatternBitmaps` pois). Vertailu kattaa
  tiketin [023](023-palette-separations.md) kartat ja jokaisen kansion `skia_render` kuvauksen kartan, jossa on
  viivoituksia tai aluekuvioita, kaikki palettivärit (ks. "Testit"). Vain kuvatasoa piirtävä alin palettiväri jää pois
  kartoilta, joissa on kuvaobjekteja, kuten tiketissä 023.
- [ ] Rakennetestit tarkistavat ilman piirtoa peittoprototyypit ja niiden jakamisen symbolien kesken, ristiviivoituksen,
  `AreaComboSymDef`in yhdistelmän osamäärittelyjen peittoprototyypit, palettivärin, jota vain viivoitus tai aluekuvio
  piirtää, M:n kiertotiloilla `Always`, `ManualOnly` ja `Never`, normaalimuodon (samanlaiset kokoonpanot yhdessä
  palettileikkurissa ja erilaiset kokoonpanot eri palettileikkureissa), leikkausobjektit jokaisella täyttötilalla,
  epäsäännöllisen kuvion ja laajennuksen. Testit rakentavat kartan tapauksille, joita vertailun kartoissa ei ole (ks.
  "Testit").
- [ ] Tikettien 023, 024, 011, 012 ja 013 testit menevät läpi ilman kerääjää.
- [ ] Piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot**
- **Päätökset:**
  - Tämä tiketti on @Paulin hyväksyttävänä. Luonnosta ei toteuteta ennen hyväksyntää (@Pauli, 21.11. ja 23.11.).
  - Ennen tätä tikettiä vaihe 1 tehdään piirrosta riippumatta, ja symbolien omat geometriametodit korvaavat kerääjän
    (@Pauli, 18.11., "Pass 1 before the patterns"). Tiketit 023–026 ja 011–013 ovat tehdyt.
  - Kerääjä on testien oraakkeli tikettien 008–013 ajan. Tämä tiketti poistaa sisäisen asetuksen
    `omitHatchingsAndPatterns` ja lopettaa kerääjän oraakkelikäytön, ja peiton vertailu kattaa silloin myös värit,
    joilla on viivoituksia tai aluekuvioita (@Pauli, 18.11., "The collector as the oracle until ticket 009"; epicin
    muistiinpano: "kuviotiketti 009 poistaa sen ja asetuksen `omitHatchingsAndPatterns`").
  - Kokoonpanot ovat samat, kun ne antavat saman peiton: palettiväri ja peittoprototyyppi ovat samat, ja M₁⁻¹M₂ on G:n
    symmetria. Normaalimuoto ratkaisee sen: M:n siirto redusoidaan G:n symmetrioiden siirtojen perusalueeseen ja
    pyöristetään kiinteään tarkkuuteen (@Pauli, 21.11., "The same configuration", luonnoksen kysymys 1).
  - Viivoitus on oma peittoprototyyppinsä: viivat, joiden symmetrioihin kuuluvat kaikki siirrot viivojen suuntaan.
    Speksissä `ManualOnly` on sama kuin `Always`: M ottaa aluesymbolin kulman ja kiertokeskuksen, ja vain `Never`
    jättää ne pois (@Pauli, 21.11., "The hatchings and `ManualOnly`", luonnoksen kysymykset 3 ja 4).
  - Speksin luku 03 antaa täyttötilan valitsemat elementit ja epäsäännöllisen kuvion elementtien sijoitukset symbolin
    palettiväripeiton johdoksina. Se ei kerro, mihin palettileikkuriin valitut elementit kuuluvat (luonnoksen kysymys
    2), ja jättää niiden kootut esitykset myöhempään tarkennukseen (@Pauli, 22.11., "The selected elements in the
    specification"). Tämä tiketti ratkaisee kysymyksen 2 (ks. "Ratkaisut").
  - Vanha piirtokoodi pysyy ennallaan, ja kun muutos ei voi muuttaa piirtoa, täysi ajo tehdään ilman keräävää ajoa
    (@Pauli, 18.11.). OCAD ja Mapper ovat omien karttojensa piirron auktoriteetit, ja feature/multiply-blending on vain
    ulkoasun oraakkeli, eikä työ lue sen koodia (@Pauli, 17.11. ja 18.11.).
- **Speksi:** [Specification.md](../Specification.md), luvut 02 ja 03. Luvun 02 kohdat "The coverage prototypes" ja
  "The hatchings and the area patterns" antavat peittoprototyypit, viivoitukset, aluekuviot, hilan, elementin, L:n ja
  E:n, rakennekulman, kiertokeskuksen, kiertotilat, täyttötilat ja epäsäännöllisen kuvion. Luvun 03 kohdat "The
  configurations", "The clipping objects and the union", "The selected elements" ja "The derived values" antavat
  kokoonpanon, normaalimuodon, palettileikkurin, leikatun kuviotäytön leikkausobjektit ja valitut elementit. Nimet ovat
  [sanastossa](../Glossary.md).
- **Nykytila** (`feature/palette-stack`, 86df0a59, tiketin 013 jälkeen):
  - `AreaSymDef.Draw` piirtää palettivärissä alueen täytön, viivoitukset (`DrawHatchings`), aluekuviot (`DrawPatterns`)
    ja alueen reunan. `DrawHatchings` rajaa piirron alueeseen (`PushClip(path.GetPathKey(g))`, pääpolku ja reiät
    vuorottelevalla täyttösäännöllä) ja piirtää jokaisen viivoituksen, jonka väri palettiväri on (`DrawOneHatching`):
    muunnoksessa T(kiertokeskus) · R(kulma) · R(`hatchAngle`) viivat y = k · `hatchSpacing` + `hatchOffset` alueen
    rajojen yli kynällä, jonka leveys on `hatchWidth`, päät litteät ja liitos jiiri (`DrawHatchLines`). Kiertotila
    `Never` asettaa kulmaksi 0 ja kiertokeskukseksi origon.
  - `DrawPatterns` piirtää jokaisen aluekuvion, jonka elementillä on osia palettivärissä. Kun `usePatternBitmaps` on
    päällä, piirtokohde tukee kuviosiveltimiä ja kuvio on säännöllinen ja leikattu eikä liian karkea, se täyttää alueen
    kuviosiveltimellä (`CreatePatternBrushes`). Muuten `DrawPattern` rajaa leikatun kuvion alueeseen ja piirtää
    muunnoksessa T(kiertokeskus) · R(kulma) · R(`patternAngle`) · T(`patternOffsetX`, `patternOffsetY`) hilan rivit
    (`DrawPatternRows`): rivit y = j · `patternHeight` ja sarakkeet x = i · `patternWidth`, parittomat rivit
    siirrettyinä puolella leveydellä, kun `offsetRows` on päällä, ja kuhunkin pisteeseen elementin kulmalla
    −`patternAngle`. Rivit ja sarakkeet se laskee alueen rajoista ja elementin rajoista, ja se kulkee niitä lisäämällä
    korkeuden ja leveyden edelliseen. Täyttötilat `CompletelyInside`, `CenterInside` ja `PartiallyInside` valitsevat
    elementin osat suodattimella (`GlyphPartFilter`), joka testaa osan pisteet ja keskipisteen alueen sisällä
    kuvion koordinaateissa. `HandleIrregularity` siirtää epäsäännöllisen kuvion elementtejä, ja siirron siemen tulee
    hilan pisteen paikasta pyöristettynä kahteen desimaaliin.
  - Kerääjä jättää viivoitukset ja aluekuviot pois sisäisellä asetuksella `omitHatchingsAndPatterns`, koska ne rajaavat
    piirron, ja `AreaSymDef.HasFullCoverage` ottaa vain alueen täytön ja alueen reunan palettivärit. Palettiväri, jota
    vain viivoitus tai aluekuvio piirtää, ei saa aluesymbolilta palettiväripeittoa (tiketti 012). `CoveragePrototype`lla
    on vain täysi peitto, ja palettierottelulla on yksi palettileikkuri, jonka M on identiteetti
    (`PaletteSeparation.MakeCutters`).
  - Tiketin 023 peiton vertailu (`PaletteSeparationCoverageTests`) jättää pois jokaisen palettivärin, jota näkyvän
    symbolin viivoitus tai aluekuvio piirtää, ja kuvatasoa piirtävän alimman palettivärin.
  - OCADin tuonti asettaa aluesymbolin kiertokeskukseksi kartan origon ja kiertotilaksi `Always` tai `ManualOnly`, ja se
    lukee täyttötilan ja epäsäännöllisyyden OCAD 12 -symboleista. Mapperin tuonti ottaa kiertokeskuksen kuvion
    origosta (`rotationCoord`) ja antaa kiertotilaksi `Always` tai `Never`.
  - Upstreamin `AreaSymDef.CopyToMap` ei lisää aluekuvioita uuteen määrittelyyn (`AddPattern` puuttuu). Metodilla ei
    ole kutsujia, eikä tämä tiketti tarvitse sitä.
- **Vanha työ:** luonnosta ei toteutettu, joten vanhaa työtä ei ole. Tiketin 023 peiton vertailu, tiketin 024 glyyfin
  jaetut muodot ja tiketin 011 yksittäiset sijoitukset sopivat.
- **Ratkaisut** (istunnon valinnat, @Paulin hyväksyttävinä):
  - Nimet: `HatchingPrototype` ja `AreaPatternPrototype` ovat `CoveragePrototype`n aliluokkia. Kumpikin viittaa
    alueen symbolimäärittelyynsä ja viivoituksen tai aluekuvion indeksiin, ja aluekuvion prototyyppi palettiväriinsä.
    `CoveragePrototype.MakeTransformation(Symbol)` antaa M:n, `CoveragePrototype.Normalize(Matrix)` normaalimuodon ja
    `CoveragePrototype.Expand(RectangleF)` laajennuksen. `SymbolCoverage.Transformation` on palettiväripeiton M.
  - Aluekuvion peittoprototyyppi on palettivärikohtainen, koska elementin osat ovat eri palettiväreissä ja G on joukko
    eli palettivärin osien unioni. Viivoituksella on yksi väri.
  - Valitut elementit (luonnoksen kysymys 2) ovat aluekuvion palettiväripeiton leikkausobjekteja, joten ne kuuluvat
    aluekuvion kokoonpanon palettileikkuriin. Jokainen valittu osa on G:n osa, joten G M:n alla osien unionissa on osat
    itse, eikä täyttä peittoa tarvita. Näin palettiväripeitto pysyy kolmikkona (`Symbol`, `SymColor`, G), ja valinta
    on G:n johdos. Valinnan yksikkö on elementin osa, kuten upstreamissa.
  - Epäsäännöllisen kuvion siirrot ovat osa G:tä, eivät leikkausobjekteja: G on määrittelyn antama joukko, jonka
    elementit ovat siirretyissä paikoissaan, eikä sillä ole siirtosymmetrioita. Näin leikatun epäsäännöllisen kuvion
    leikkausobjekti on alue, kuten säännöllisen.
  - Normaalimuodon tarkkuus: lineaarisen osan alkiot pyöristetään kuuteen desimaaliin ja siirto 0,0001 mm:n
    tarkkuuteen, joka on vertailun laajennettujen leikkausobjektien sallittu ero. Perusalue on viivoituksella
    0 ≤ siirto viivojen poikki < väli ja siirto viivojen suuntaan 0, ja aluekuviolla hilan perussolu. Pyöristys voi
    jakaa samat kokoonpanot kahteen palettileikkuriin, kun liukulukujen virhe vie siirron rajan yli, mutta se ei muuta
    peittoa.
  - Peiton vertailu piirtää kunkin palettiväripeiton G:n sen omalla M:llä laajennuksena alueen rajoista, kuten piirto,
    eikä palettileikkurin normaalimuodolla, jotta vertailu on tarkka. Normaalimuodon pyöristys siirtäisi kuvion vaihetta
    alle 0,0001 mm. Rakennetestit tarkistavat palettileikkurit.
  - Laajennus antaa elementit samoilla laskuilla kuin piirto, koska piirto laskee rivit ja sarakkeet suorakulmion
    rajoista ja kulkee niitä lisäämällä. Silloin kaksi suorakulmiota voivat antaa samalle hilan pisteelle liukuluvun
    viimeisessä numerossa eri paikan.
- **Rajaus:**
  - Vaihe 2 on luonnos [010](010-palette-composition.md). Kuvioruutu ja muut kuviotäyttöjen kanavamuunnokset tulevat
    koodausten mukana, kuten viivojen kanavamuunnokset tiketissä 011.
  - Kuviosivellin (`usePatternBitmaps`) on piirron nopeutus, eikä se kuulu välikerrokseen.
  - Kuvatason ja asettelutason objekteilla (`ImageSymDef`) ja rei'illä (`HoleSymDef`) ei ole palettiväripeittoja.
    SVG-koodaus ja litistys eivät kuulu tähän.
- **Testit:**
  - Paikallinen Map_Skia.Testsin ajo on `tools/linux-renderings.sh` integraatioworktreessa Windows-koneen fonteilla, ja
    sen jokainen testi menee läpi (Progress.md, "Local tests"). `tools/compare-renderings.py` ei löydä yhtään
    siirtynyttä kuvaa, kun se vertaa ajon renderöintejä haaran `feature/palette-stack` renderöinteihin.
  - Asetuksen `omitHatchingsAndPatterns` poisto koskee piirtokoodia (`DrawHatchings` ja `DrawPatterns`), mutta asetus
    on piirrossa aina pois päältä, joten piirto ei muutu, ja täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack`
    ilman keräävää ajoa: `tools/push-wip.sh --base-baselines prs/000-palette-stack-base <klooni> feature/pattern-fills`.
  - Kansion `skia_render` 162 kartasta 46:ssa on viivoituksia tai aluekuvioita, ja niistä 40:llä on kuvaus. Niiden
    näkyvissä aluesymboleissa on 1 876 viivoitusta ja 2 402 aluekuviota 271 viivoituksessa ja aluekuviossa, ja
    normaalimuoto antaa niille 320 kokoonpanoa. 70 palettiväriä piirtää vain viivoitus tai aluekuvio. Istunto laski
    nämä testillä, jota se ei commitoinut (23.11.).
  - Viivoituksista 50 on ristiviivoituksen osia, ja niiden kiertotila on `ManualOnly` (1 831) tai `Never` (45).
    Aluekuvioiden täyttötila on `Clip` (2 365), `CompletelyInside` (8), `CenterInside` (6) tai `PartiallyInside` (23).
    Epäsäännöllisiä kuvioita on 18, täyttötiloilla `Clip` (3) ja `PartiallyInside` (15). Aluekuvioista 212:lla on
    siirretyt rivit, 1 946:lla rakennekulma, 7:llä siirtymä ja 9:llä elementti useammassa palettivärissä, ja 2 107:n
    elementti on yksi ympyrä origossa. Kiertotila on `Always` (83), `ManualOnly` (2 194) tai `Never` (125). 92
    aluesymbolin kulma ei ole 0.
  - Kartoissa ei ole yhdistelmäalueen (`AreaComboSymDef`) osamäärittelyä, jolla on viivoitus tai aluekuvio, eikä
    aluesymbolia, jonka kiertokeskus ei ole origo, eikä viivoitusta kiertotilalla `Always`, eikä epäsäännöllistä
    kuviota täyttötiloilla `CompletelyInside` ja `CenterInside`. Rakennetestit rakentavat ne.
  - Peiton vertailun uusia karttoja ovat esimerkiksi `wholestructure.txt` (täyttötilat `CompletelyInside`,
    `CenterInside` ja `PartiallyInside`), `irregular1.txt` ja `irregular3.txt` (epäsäännölliset kuviot),
    `offsetpatternrot_nopatbm.txt` (siirtymät ja kierretyt alueet), `rotarea-test.txt` ja `isomarea_nopatbm.txt`.
- **Haara:** `feature/pattern-fills` juurtuu haaraan `feature/palette-stack` tiketin 013 jälkeen (86df0a59). Täysi
  ajo tehdään haarassa `wip/linux/pattern-fills` basen päällä valitsimella `--base-baselines
  prs/000-palette-stack-base`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md),
  [../README.md](../README.md), [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja
  "Decisions", speksin esipuhe ja luvut 01–03, [../Glossary.md](../Glossary.md), tiketit
  [023](023-palette-separations.md), [024](024-point-symbol-geometry.md), [012](012-area-geometry.md) ja
  [013](013-text-geometry.md) sekä tämä tiketti. [../tools/README.md](../tools/README.md) kertoo tiketin työkierron.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
