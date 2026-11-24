# Palettileikkurin kuviotäyttö kerran alueidensa unionissa
**Kuvaus**
Kun saman palettileikkurin (`PaletteCutter`) alueet ovat päällekkäin tai koskettavat, kukin alue kirjoittaa nyt oman
viivoituksensa tai leikatun aluekuvionsa. Reunanpehmennetty piirto tummentaa siksi kuvion siellä, missä kopiot ovat
päällekkäin: kahdesti kirjoitettu peitto a antaa 1 − (1 − a)². Kääntymätön kuviosivellin jättää lisäksi koskettaviin
reunoihin vaalean sauman. Vika ei koske vain yhtä kanavaa: se on Skian koodauksessa (tiketti 010) ja PDF-piirtokohteen
vanhassa piirrossa, ja se tulisi myös SVG:n koodaukseen. Speksin mukaan saman kokoonpanon päällekkäiset alueet saavat
kuvion kerran niiden unionissa. Tässä tiketissä välikerros antaa kanavista riippumatta palettileikkurin alueryppäät,
joissa ovat alueet, joiden rajaavat suorakulmiot leikkaavat tai koskettavat, ja kunkin ryppään unionin. Skian koodaus
kirjoittaa usean alueen ryppään kuvion kerran unioniin. PDF:n ja SVG:n koodaukset kirjoittavat samat ryppäät ja unionit
omissa tiketeissään. Tiketti on erotettu tiketistä 010, ja sen haara yhdistetään tiketin 010 haaraan ennen kuin tiketti
010 yhdistetään eepoksen haaraan.

**Hyväksymiskriteerit**
- [x] Alueryväs (`AreaCluster`, ehdotettu nimi): viivoituksen (`HatchingPrototype`) tai leikatun aluekuvion
  (`AreaPatternPrototype`) palettileikkuri antaa johdoksena alueryppäänsä (`PaletteCutter.AreaClusters`, ehdotettu
  nimi). Ryppääseen kuuluvat ne palettileikkurin palettiväripeitot (`SymbolCoverage`), joiden alueiden rajaavat
  suorakulmiot leikkaavat tai koskettavat toisiaan suoraan tai ryppään muiden alueiden kautta. Alue, jonka ääriviivat
  kohtaavat toisensa tai itsensä (`SymPathWithHoles.ContoursMeet`), on oma ryppäänsä (päätös "Invalid shapes"). Samoin
  alue, joka ei kohtaa palettileikkurin muita alueita, on oma ryppäänsä. Ryväs pitää palettiväripeittonsa
  palettierottelun järjestyksessä ja alueidensa yhteisen rajaavan suorakulmion. Ryppäät tehdään kartasta ilman piirron
  suorakulmiota ja resoluutiota, kun niitä tarvitaan ensimmäisen kerran, ja ne vanhenevat kartan muuttuessa kuten muut
  johdokset.
- [x] Ryppään unioni (`AreaCluster.Union`, ehdotettu nimi): usean alueen ryppään alueiden unioni on johdos, leikkausobjekti,
  joka on täytetty polku (`FilledShape`, `PathShape`). Sen täyttösääntö on nonzero (`AreaFillMode.Winding`), ja siinä
  ovat ryppään kaikkien alueiden kaikki ääriviivat. Ääriviiva kiertää positiivisesti, kun sen oman alueen muista
  ääriviivoista parillinen määrä ympäröi sen, ja muuten toiseen suuntaan (`SymPathWithHoles.AddToUnionPathPartList`).
  Näin nonzero-sääntö täyttää kustakin alueesta juuri sen, minkä parillisuussääntö täyttää siitä yksinään, eikä mikään
  alue kumoa toisen lisäämää. Yhden alueen ryppään unioni on alue itse. Unioni ei käytä Skiaa eikä monikulmiokirjastoa,
  ja jokainen kanava leikkaa ja täyttää sen kuten minkä tahansa täytetyn polun.
- [x] Vaiheen 2 ryhmä: palettivärin läpinäkyvyysryhmä (`TransparencyGroup`) antaa koodaukselle viivoitusten ja
  leikattujen aluekuvioiden palettiväripeitot ryppäittäin. Ryppäästä tulevat kaikki palettiväripeitot, kun jokin
  niistä voi leikata piirron suorakulmion (tiketin 010 ehto `Symbol.BoundingBox` ja `MayIntersectRect`). Koodaus
  kirjoittaa ryppään sen ensimmäisen palettiväripeiton kohdalla palettierottelun järjestyksessä. Täyden peiton
  palettiväripeitot ryhmä antaa kuten tiketissä 010.
- [x] Laajennus suorakulmion yli: `SymbolCoverage.ExpandPrototype(RectangleF)` (ehdotettu nimi) antaa
  peittoprototyypin laajennuksen annetun suorakulmion yli palettiväripeiton M:llä (`CoveragePrototype.Expand(Symbol,
  RectangleF)`). `SymbolCoverage.ExpandPrototype()` on sama laajennus alueen rajaavan suorakulmion yli, kuten nyt.
- [x] Skian koodaus: yhden alueen ryppään koodaus kirjoittaa kuten tiketissä 010. Usean alueen ryppään kuvion se
  kirjoittaa kerran ryppään unioniin:
  - Viivoituksen ja glyyfi kerrallaan piirretyn aluekuvion laajennus tehdään ryppään rajaavan suorakulmion yli ryppään
    ensimmäisen palettiväripeiton M:llä, ja se kirjoitetaan unionin leikkauksen sisään. Leikkaus on ilman
    reunanpehmennystä, kuten tiketissä 010 (`SKCanvas.ClipPath`).
  - Kuviosivellin kirjoitetaan kuten tiketin 010 kriteerissä "Kuviotäytöt", mutta alueen sijasta unioniin. Kun
    sivellin ei käänny, koodaus täyttää unionin polun siveltimen varjostimella. Muuten se täyttää unionin leikkauksen
    sisällä suorakulmion, joka rajaa unionin sijoituksen koordinaatistossa.
- [x] Rakennetestit tarkistavat ilman vanhaa piirtoa seuraavat:
  - Unioni ilman piirtoa: piste on ryppään unionissa nonzero-säännöllä täsmälleen silloin, kun se on jonkin ryppään
    alueen sisällä parillisuussäännöllä. Tapaukset ovat päällekkäiset ja koskettavat alueet, alue, jonka reikää toinen
    alue peittää, alue, jonka reikä on ääriviivan ulkopuolella, ja saari reiässä.
  - Saman palettileikkurin kaksi päällekkäistä tai koskettavaa aluetta piirtyvät Skian koodauksella samoin kuin yksi
    alue, jonka ääriviiva on niiden unioni, enintään yhden värikomponentin tason erolla. Tapaukset ovat viivoitus, alueen
    mukana kääntyvä viivoitus, kiertotilan `Never` viivoitus eri kulmien alueilla, glyyfi kerrallaan piirretty
    aluekuvio, epäsäännöllinen aluekuvio, kuviosivellin päällekkäin ja vierekkäin, alue, jonka reikää toinen alue
    peittää, ja alue, jonka reikä on ääriviivan ulkopuolella (parillisuussääntö).
  - Kahden eri kokoonpanon alueet eroavat unionista: alueet, joilla on eri symbolimäärittely, ja alueet, joiden M:t
    eivät eroa G:n symmetrian verran.
  - Ryppäät: rajaavien suorakulmioiden ketju tekee yhden ryppään, koskettavat alueet kuuluvat samaan ryppääseen,
    erilliset alueet ovat eri ryppäissä, ja alue, jonka ääriviivat kohtaavat, on oma ryppäänsä.
  - Alue, joka ei kohtaa palettileikkurin muita alueita, piirtyy pikselilleen kuten vanha piirto
    (`RenderOptions.drawColorsDirectly`).
- [x] Renderöintitesti: `Map_Skia.Tests` piirtää @Paulin tapaukset `suo.ocd` ja `suo2.ocd`
  ([SvgHatchingOverlap/case](../../SvgHatchingOverlap/README.md)). Niiden vertailukuvat otetaan keräävästä ajosta.
- [x] Kuvat: keräävä ajo antaa kuvat, jotka siirtyvät. Jokainen siirtynyt pikseli on usean alueen ryppään unionissa tai
  sen reunan pikselissä, ja tiketti kertoo kuvat ja niiden pikselien määrät. Syy on se, että ryppään kuvio piirtyy
  kerran unioniinsa ryppään ensimmäisen palettiväripeiton M:llä: päällekkäisissä kohdissa kuvio ei kerrostu, ja
  kosketusreunoilla ei ole saumaa. Vain Skian koodauksen kuvat siirtyvät. PDF-, GDI+- ja WPF-piirtokohteiden kuvat
  eivätkä muut kuva- ja tekstibaselinet siirry. Siirtyneet baselinet tallennetaan @Paulin luvalla, jonka hän antaa
  hyväksyessään tiketin.
- [x] Tikettien 010, 023–026, 011–013 ja 009 testit menevät läpi.

**Lisätiedot**
- **Lähtö:**
  - @Pauli kokeili tiketin 010 koepakettia `notpurplepen-4.0.415.220-trial-palette-composition-linux-x64.deb`
    (24.11.). Päällekkäisten vaakaviivoitettujen soiden viivoitus tummui niiden päällekkäisissä kohdissa. Sama vika
    korjattiin vanhassa piirrossa ennen palettipinotyötä. Tiketin 010 kriteeri "Ryhmän sisältö" jätti palettierottelun
    unionin implisiittiseksi myös kuviotäytöille, mikä on ristiriidassa speksin ja päätöksen "Overlapping patterns"
    kanssa. @Pauli hyväksyi, että työ erotetaan tiketistä 010 omaksi alatehtäväkseen ennen toteutusta (24.11.).
  - @Pauli: vika ei koske vain Skiaa, vaan sama vika on PDF:ssä ja tulevassa SVG-kanavassa (24.11.). Siksi ryppäät ja
    niiden unioni ovat välikerroksen johdoksia, jotka kaikki kanavat lukevat.
- **Speksi:** [Specification.md](../Specification.md), luku 03, kohta "The clipping objects and the union": "The
  clipping objects of a clipped pattern fill are the areas of the palette color coverages of the symbols in a palette
  cutter. So the overlapping areas of one configuration get the pattern once, in their union." ja "The union of the
  clipping objects of a palette cutter is a true union, also where each area fills with the even-odd rule." Luvun 04
  kohta "Pass 2 and the transparency groups": "In the group, the encoding writes each palette cutter with one fill and
  one clip, or each palette color coverage of a symbol separately." Nimet ovat [sanastossa](../Glossary.md).
- **Päätökset:**
  - "A colour is a layer" (12.11., [NotPurplePenProgress.md](../../NotPurplePenProgress.md)): saman värin muotojen
    päällekkäisiä tai koskettavia reunanpehmennettyjä reunoja ei korjata, "except the patterns of task 2". Siksi
    täyden peiton palettiväripeitot kirjoitetaan erikseen kuten tiketissä 010, ja vain kuviotäytöt ryppäittäin.
  - "Overlapping patterns" (12.–13.11.): "A pattern drawn thicker where areas of one symbol overlap is not acceptable
    in any context." Kukin alue piirretään edelleen omasta ääriviivastaan, ja vain niiden unioni leikkaa kuvion.
  - "Invalid shapes" (13.11.): alue, jonka ääriviiva leikkaa itseään tai jonka reikä leikkaa ääriviivaa, piirtää oman
    kuvionsa, joka tuplautuu siellä, missä se on toisen alueen päällä. Monikulmiokirjastoa ei lisätä.
  - Koodaustaan odottavat piirtokohteet saavat olla rikki haarassa `feature/palette-stack`, koska sitä ei vielä käytetä
    mihinkään, eikä vanhaa piirtoa siirretä ryppäisiin (@Pauli, 24.11.).
  - Kuviotäyttöjen reunanpehmennys Skian koodauksessa on oma tikettinsä tiketin 010 jälkeen (päätös 24.11., Progress.md
    "The Skia calls of the pattern fills"). Se rakentuu tämän tiketin unioniin: alueittain reunanpehmennetyt
    leikkaukset jättäisivät koskettaviin reunoihin vaaleat saumat, ja unionin leikkauksessa niitä ei ole.
- **Toteutus** (haara `feature/palette-cutter-patterns`, 1530dabd, yhdistetty haaraan `feature/palette-composition`
  (bc9a4800)):
  - 8e382fcf: palettileikkuri antaa alueryppäät (`PaletteCutter.AreaClusters`, `AreaCluster`), ja palettierottelu antaa
    palettiväripeiton ryppään (`PaletteSeparation.GetAreaCluster`). Ryppäät tehdään kuten korjauksen `OverlappingAreas`
    ryhmät: rajaavat suorakulmiot yhdistetään pyyhkäisyllä ja unioni–etsi-rakenteella samalla koodilla (`Connect`,
    `Find`, `Join`, `CountMembers`), ja alue, jonka ääriviivat kohtaavat, on oma ryppäänsä. Ryppään unioni
    (`AreaCluster.Union`) on täytetty polku säännöllä `AreaFillMode.Winding`, ja sen osat antaa
    `SymPathWithHoles.AddToUnionPathPartList`. `SymPath.cs`:n geometria (`AddToUnionPathPartList`, `ContoursMeet`,
    `AddClosedContourToPathPartList`, `SignedArea` ja niiden apufunktiot) on korjauksen koodi sellaisenaan.
    `SymbolCoverage.ExpandPrototype(RectangleF)` ja `CoveragePrototype.Expand(Symbol, RectangleF)` laajentavat
    suorakulmion yli, ja `AreaSymDef.ExpandHatching` ja `ExpandAreaPattern` saavat alueen polun sijasta suorakulmion.
    `TransparencyGroup.GetAreaCluster` antaa koodaukselle palettiväripeiton ryppään, ja `SkiaEncoding` kirjoittaa usean
    alueen ryppään kuvion kerran unioniin ensimmäisen palettiväripeitonsa M:llä.
  - Testit: `AreaClusterTests` (16 testiä) ja `RenderingTests.OverlappingMarshes` ja `OverlappingMarshes2` (@Paulin
    `suo.ocd` ja `suo2.ocd`, kuvaukset `suo.txt` ja `suo2.txt`).
  - Paikallinen ajo (`tools/linux-renderings.sh` Windows-koneen fonteilla): haarassa 786 testiä menee läpi (tiketin 010
    haarassa 1b282046 766), ja `tools/compare-renderings.py` antaa 9 siirtynyttä kuvaa. Testi, jota ei commitoitu,
    piirsi kunkin kuvan kartan usean alueen ryppäiden unionit testin pikseleihin reunaa 2,5 pikseliä levennettynä, ja
    jokainen siirtynyt pikseli on niissä: `LordHill` 19, `clouds` 1 612, `irregular3` 48, `lksamm3` 91 ja `teanwest`
    1 903 pikseliä. Karttojen usean alueen ryppäitä on 7, 19, 1, 4 ja 131.
  - Keräävä ajo 45400058882 (Windows), jonka kuvat 1530dabd tallentaa:
    - ryhmä Skia: samat 9 kuvaa ja pikselimäärät kuin paikallisessa ajossa (`LordHill` ja `LordHill OCAD11` 19, `clouds`
      ja `clouds11` 1 612, `irregular3` 48, `lksamm3` ja `lksamm3_ocadmetric` 91, `teanwest` ja `teanwest11` 1 903), sekä
      uudet `suo` ja `suo2` (valitsin `--blank-baseline`).
    - ryhmä PurplePen, 28 kuvaa: `bitmapcreate/create3`:n `Relay AC`, `AD`, `BC` ja `BD` (26 pikseliä kukin),
      `bitmapcreate/create2`:n `BM_All Controls` (213) ja `BM_Course 2` (232), `routegadget`in `GRC GIF` (24),
      `courseprinting`in 17 sivua (2–7 pikseliä) ja `bitmapcreate/blend`in `Course 5` neljässä tilassa. Tilojen `none`,
      `layer` ja `layersprint` kuvat eroavat vertailukuvistaan noin 1,08 miljoonassa pikselissä, joista 8 ylittää
      testin toleranssin 30. Nämä vertailukuvat ovat vanhoja koko kartan alalla: vertailukuvat `none` ja `blend` eroavat
      toisistaan 1 081 446 pikselissä, mutta ajon renderöinnit vain purppuran 47 277 pikselissä. Muutoksen osuus on
      enintään `blend`-kuvan 1 070 pikseliä, ja toleranssin ylittävät 8 pikseliä ovat samassa kohdassa kaikissa neljässä
      tilassa.
    - Skian koodaus muuttaa vain usean alueen ryppäiden kirjoitusta, ja se kirjoittaa ne unionin leikkauksen sisään tai
      täyttää unionin polun. Kaikki muu kirjoitetaan samoilla kutsuilla kuin tiketissä 010. Siksi PurplePen-ryhmänkin
      siirtyneet pikselit ovat ryppäiden unioneissa; Skian kuvissa se on tarkistettu pikseleittäin.
    - PDF-, GDI+-, WPF- ja MapModel-ryhmissä yksikään kuva ei siirtynyt.
    - PurplePen-ryhmän `ExportRouteGadget2` sai keräävässä ajossa `IOException`in, koska `GRC GIF` siirtyi ja
      `base`n GDI+-vertailu ei vapauta tiedostoa keräävässä tilassa, kuten tiketin 010 keräävässä ajossa.
  - Täysi ajo meni läpi kohdassa 1530dabd (`wip/linux/palette-cutter-patterns` kohdassa f9ab3e68, basen päällä,
    valitsimella `--base-baselines prs/000-palette-stack-base`: Tests 45402806490, Build 45402806370): kaikki
    testiryhmät menivät läpi.
  - Haara on yhdistetty haaraan `feature/palette-composition` (bc9a4800), ja yhdistämisen puu on haaran puu. Tiketin
    010 haara on yhdistetty haaraan `feature/palette-stack` (0d2148b3), ja epicin täysi ajo meni läpi
    (`wip/linux/palette-stack` kohdassa 78770fb2: Tests 45405076724, Build 45405076683).
- **Nykytila** (`feature/palette-composition`, 1b282046):
  - `SkiaEncoding.WriteTransparencyGroup` kirjoittaa ryhmän palettiväripeitot yksi kerrallaan palettierottelun
    järjestyksessä. Kuviotäytön kirjoittaa `WritePatternFill` kunkin alueen leikkauksen sisään tai
    kuviosiveltimenä (`WritePatternBrush`). `SymbolCoverage.ExpandPrototype()` laajentaa peittoprototyypin alueen
    rajaavan suorakulmion yli (`AreaSymDef.ExpandHatching`, `ExpandAreaPattern`). Laajennus käyttää alueesta vain
    sen rajaavaa suorakulmiota, kulmaa ja kiertokeskusta. Epäsäännöllisen aluekuvion siirto riippuu vain hilapisteestä
    (`HandleIrregularity`).
  - PDF-, GDI+- ja WPF-piirtokohteet piirtävät palettivärit vanhalla piirrolla (`DrawColor`), jossa kukin alue piirtää
    oman kuvionsa (`AreaSymDef.Draw`), kuten upstream.
  - Palettileikkuri (`PaletteCutter`) antaa peittoprototyyppinsä, M:n normaalimuodon ja palettiväripeittonsa.
    Saman palettileikkurin palettiväripeittojen M:t eroavat toisistaan G:n symmetrian ja normaalimuodon tarkkuuden
    verran (`CoveragePrototype.TranslationPrecision` 0,0001 mm, `LinearPrecision` 10⁻⁶).
- **Vanha työ:** `fix/overlapping-patterns` (c544ed87–7d2699b0) on haaroissa `basewithfixes`, `dev` ja
  `feature/svg-target`. Se ryhmittelee vanhassa piirrossa saman symbolin alueet, joilla on sama kehys ja joiden
  rajaavat suorakulmiot leikkaavat tai koskettavat (`OverlappingAreas`), ja piirtää kuvion kerran unionin polun sisään
  (`AreaLikeSymDef.DrawPatternsOnce`). Unionin polku (`OverlappingAreas.Group.GetUnionPathKey`) on tämän tiketin
  unioni: osat tulevat metodista `SymPathWithHoles.AddToUnionPathPartList`, ja polku täytetään säännöllä
  `AreaFillMode.Winding`. `SymPathWithHoles.ContoursMeet` jättää ryhmistä pois alueet, joiden ääriviivat kohtaavat
  tai vain koskettavat. Ryhmittely on vanhassa piirrossa, jota vaihe 2 ei kutsu, mutta näiden kahden metodin
  geometria ja testitapaukset (`OverlappingPatternsTests`, 7d2699b0, `Map_Skia.Tests`) siirtyvät. Tämä tiketti ottaa
  metodit samoin nimin ja samalla koodilla, joten eepos ja `dev` kohtaavat saman koodin. Tapaukset, mittaukset ja
  skriptit ovat kansiossa [SvgHatchingOverlap](../../SvgHatchingOverlap/README.md), ja suunnitelma on
  [SvgExportDesign.md](../../SvgExportDesign.md):n kohdassa "Overlapping patterns, and a colour's layer". Mittauksen
  mukaan päällekkäisissä kohdissa oli 60 % enemmän mustetta 100 dpi:llä ja 7,7 % Windowsin tulostuksen 720 dpi:llä.
- **Ratkaisut:**
  - Ryppäät rajaavien suorakulmioiden mukaan, kuten vanha korjaus: speksin unioni on erillisten ryppäiden unionien
    unioni. Alue, joka ei kohtaa muita, kirjoitetaan kuten tiketissä 010, joten kartta ilman päällekkäisiä alueita
    piirtyy pikselilleen ennallaan, ja siirtyneet pikselit rajautuvat ryppäisiin.
  - Koskettavat alueet kuuluvat ryppääseen, koska kääntymätön kuviosivellin täyttää alueen reunanpehmennettynä, ja
    kahden alueen yhteinen reuna saa silloin peiton 1 − c(1 − c) eli vaalean sauman.
  - Unioni on yksi nonzero-säännön polku, koska jokainen kanava osaa leikata ja täyttää sellaisen: Skia
    (`SKCanvas.ClipPath`), PDF (leikkausoperaattori `W`), SVG (`clip-rule="nonzero"`), GDI+ ja WPF. PDF:n
    leikkauspolut leikkaavat toisiaan, joten PDF:ssä unioni on tehtävä yhtenä polkuna. Sama polku pitää kanavat
    samoina.
  - Ryppään ensimmäisen palettiväripeiton M: muiden palettiväripeittojen kuvio siirtyy enintään normaalimuodon
    tarkkuuden verran, mikä on pikselin murto-osa kaikilla piirron resoluutioilla.
  - Koodaus ei säilytä mitään piirtojen yli, kuten tiketissä 010. Ryppäät ja unioni säilyvät johdoksina.
- **Rajaus:**
  - Täyden peiton palettiväripeitot kirjoitetaan erikseen kuten tiketissä 010 (päätös "A colour is a layer").
  - Aluekuviot, jotka eivät ole leikkaustilassa vaan valitsevat glyyfin osat täyttötilalla, ovat täyden peiton
    palettiväripeittoja (speksin luku 03, "The selected elements"), ja ne kirjoitetaan alue kerrallaan kuten vanhassa
    korjauksessa.
  - Eri kokoonpanojen alueet, esimerkiksi kahden symbolimäärittelyn samanlaiset viivoitukset, kirjoitetaan erikseen.
  - PDF-, GDI+- ja WPF-piirtokohteet piirtävät vanhalla piirrolla kuten nyt, ja PDF:n ja SVG:n koodaukset kirjoittavat
    ryppäät omissa tiketeissään (@Pauli, 24.11.). `fix/overlapping-patterns` ei tule haaraan `feature/palette-stack`.
    Asetus "Draw area patterns as separate shapes" (`RenderOptions.patternsWithEachArea`) kuuluu SVG:hen eikä tähän.
  - Kuviotäyttöjen reunanpehmennys on oma tikettinsä tämän jälkeen.
- **Haara:** `feature/palette-cutter-patterns`, juurtuu haaraan `feature/palette-composition` (1b282046). Täydet ajot
  tehdään haarassa `wip/linux/palette-cutter-patterns` basen päällä valitsimella `--base-baselines
  prs/000-palette-stack-base`, ensin keräävänä ajona. Kun tiketti on valmis, haara yhdistetään haaraan
  `feature/palette-composition`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md),
  [../README.md](../README.md), [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja
  "Decisions", speksin luvut 03 ja 04, [../Glossary.md](../Glossary.md), tiketit
  [010](010-palette-composition.md) ja [009](009-pattern-instances.md), [SvgExportDesign.md](../../SvgExportDesign.md):n
  kohta "Overlapping patterns, and a colour's layer" sekä tämä tiketti.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
