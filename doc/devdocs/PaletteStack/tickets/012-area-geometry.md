# Alueiden, yhdistelmäalueiden ja grafiikkaobjektien geometriametodit
**Kuvaus**
Alueen symbolimäärittelyt (`AreaSymDef` ja `AreaComboSymDef`) ja grafiikkaobjektien symbolimäärittely
(`GraphicsSymDef`) antavat symboliensa täyden peiton palettiväripeittojen leikkausobjektit omalla geometriametodillaan
ilman piirtoa: alueen täytön polkuna reikineen ja täyttösääntöineen, alueen reunan reunasymbolin viivojen
geometriametodilla (tiketti [011](011-line-geometry.md)) pääpolulla ja jokaisella reiällä, yhdistelmäalueen osat
osien järjestyksessä sekä grafiikkaobjektin täytön tai viivan. Kerääjä on oraakkeli, ja tekstit ja suorakulmiot
saavat leikkausobjektinsa edelleen kerääjältä. Viivoitukset ja aluekuviot eivät kuulu tähän. Piirto ei muutu.

**Hyväksymiskriteerit**
- [ ] Geometriametodien rajapinta: `AreaSymDef`, `AreaComboSymDef` ja `GraphicsSymDef` korvaavat tiketin
  [024](024-point-symbol-geometry.md) rajapinnan `SymDef.MakeClippingObjects(Symbol, SymColor, CoveragePrototype)`.
  Alueen ja grafiikkaobjektin palettiväripeiton leikkausobjektit tulevat symbolimäärittelyltä ilman piirtokohdetta
  (`IGraphicsTarget`) ja ilman piirtoasetuksia (`RenderOptions`), eikä vaihe 1 kerää yhtään aluesymbolia eikä
  grafiikkaobjektia kerääjällä.
- [ ] Alue: `AreaSymDef` antaa palettivärissä ne osatekijät, joiden väri tämä palettiväri on, siinä järjestyksessä
  kuin `AreaSymDef.Draw` piirtää ne: täytön, kun palettiväri on täyttöväri (`FillColor`), ja reunan, kun
  reunasymboli (`BorderSymdef`) piirtää palettivärin. Täyttö on täytetty muoto: pääpolku ja sen reiät yhtenä polkuna
  vuorottelevalla täyttösäännöllä, Bezier-käyrät säilyttäen, kuten `SymPathWithHoles.Fill` piirtää sen. Reuna on
  reunasymbolin leikkausobjektit ensin pääpolulla ja sitten jokaisella reiällä reikien järjestyksessä, viivojen
  geometriametodilla: koostetut katkoviivat, siirretyt polut ja toistosijoitukset kuten viivasymbolilla.
- [ ] Yhdistelmäalue: `AreaComboSymDef` antaa osamäärittelyjensä leikkausobjektit osien järjestyksessä, kuten
  `AreaComboSymDef.Draw` piirtää ne. Viivaosa antaa leikkausobjektinsa pääpolulla ja jokaisella reiällä, ja alueosa
  oman geometriametodinsa leikkausobjektit. Viivaosa voi olla myös yhdistelmäviiva (`LineComboSymDef`).
- [ ] Grafiikkaobjektit: `GraphicsSymDef` antaa grafiikka-alueen (`GraphicsAreaSymbol`) täytön täyttövärissään
  täytettynä muotona, kuten alueen täytön, ja grafiikkaviivan (`GraphicsLineSymbol`) viivan värissään viivattuna
  muotona, jonka kynällä on viivan leveys, päät ja liitos sekä jiirausraja `GraphicsUtil.MITER_LIMIT`, kuten
  `GraphicsLineSymbol.Draw` piirtää sen. Viivan muoto pitää polun Bezier-käyrät.
- [ ] Täysi peitto: geometriametodit antavat vain täyden peiton leikkausobjektit. Viivoitukset ja aluekuviot ovat
  jaksollisia peittoja (luonnos [009](009-pattern-instances.md)), eikä niistä tule leikkausobjekteja. Palettiväri,
  jota alue piirtää vain viivoituksella tai kuviolla, ei saa alueelta täyden peiton palettiväripeittoa, kuten
  tiketissä [023](023-palette-separations.md).
- [ ] Kerääjä on oraakkeli: testit vertaavat geometriametodien leikkausobjekteja kerääjän leikkausobjekteihin
  (`SymDef.CollectClippingObjects`) rakenteena jokaisessa alueen, yhdistelmäalueen ja grafiikkaobjektin
  palettiväripeitossa. Vertailu laajentaa reunojen katkojuosteet, siirretyt polut ja toistosijoitukset, ja sen
  sallitut erot ovat tiketin 011 sallitut erot. Muut leikkausobjektit, myös täytöt, ovat täsmälleen samat. Vertailu
  kattaa kansion `skia_render` jokaisen OCAD- ja Mapper-kartan sekä katkoviivanäytteiden tiedostot `dashes-*.ocd`.
- [ ] Rakennetestit tarkistavat ilman piirtoa alueen täytön reikineen ja täyttösääntöineen, reunan pääpolulla ja
  reiillä, katkotun reunan ja kaksoisviivareunan, yhdistelmäalueen osien järjestyksen ja yhdistelmäviivan osana, alueen,
  jonka palettiväri on vain viivoituksessa tai kuviossa, sekä grafiikka-alueen ja grafiikkaviivan. Grafiikkaobjektit
  tarkistetaan myös testiä varten rakennetulla kartalla, jos kansion `skia_render` kartoissa ei ole niitä.
- [ ] Tiketin 023 testit menevät läpi, myös peiton vertailu, jossa alueiden ja grafiikkaobjektien leikkausobjektit
  tulevat geometriametodilta. Tikettien 024 ja 011 testit menevät läpi.
- [ ] Tämän tiketin haara ei muuta piirtoa, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot**
- **Päätökset:**
  - @Pauli ei ole vielä hyväksynyt tätä tikettiä. Istunto kirjoitti luonnoksen tiketiksi 23.11. @Paulin pyynnöstä,
    ja se toteutetaan vasta hyväksynnän jälkeen (@Pauli, 23.11.).
  - Geometriametodit tehdään neljänä osana symbolilajeittain, ja tämä on alueiden, yhdistelmien ja grafiikkaobjektien
    osa, erotettu tiketistä 008 (@Pauli, 18.11.). Tiketit 023–026 korvaavat tiketit 005, 007 ja 008 (@Pauli, 23.11.),
    joten tämä tiketti käyttää tiketin 024 rajapintaa ja oraakkelia.
  - Vanha piirtokoodi pysyy ennallaan geometriametodien ajan, jotta kerääjä pysyy riippumattomana oraakkelina.
    Geometriametodit kirjoitetaan sen rinnalle, ja symbolin kokoava logiikka saa olla kahdessa paikassa (@Pauli,
    18.11.). Kun muutos ei voi muuttaa piirtoa, täysi ajo tehdään ilman keräävää ajoa (@Pauli, 18.11.).
  - Kerääjä on testien oraakkeli tikettien 008–013 ajan, ja tiketti 009 poistaa sen ja asetuksen
    `omitHatchingsAndPatterns` (@Pauli, 18.11.).
  - Alueen reuna on viiva, jonka peruspolku on alueen pääpolku tai reikä (speksin luku 02, "The lines"). Sen
    leikkausobjektit ovat viivan leikkausobjektit, ja tiketin 011 päätökset koskevat niitä: koostettu katkoviiva,
    kaksoisviivan reunat siirrettyinä polkuina ja viivan varren glyyfit toistosijoituksina.
  - Reunojen katkoille sallittu ero on tiketin 011 ero: siirretyn polun laajennuksen leikkausobjektit 0,01 mm ja muut
    laajennetut 0,0001 mm (@Pauli, 23.11.).
- **Speksi:** [Specification.md](../Specification.md), luvut 02 ja 03. Luvun 02 kohta "The hatchings and the area
  patterns" antaa alueen täytön, jonka G on täysi peitto, ja alueen reunan, jonka `AreaSymDef.Draw` piirtää pääpolulla
  ja jokaisella reiällä. Kohta "The lines" antaa reunan peruspolun. Luvun 02 kohta "The coverage prototypes" antaa
  täyden peiton ja jaksolliset peitot. Luvun 03 kohdat "The clipping objects and the union" ja "The forms of the
  derived values" antavat täytetyn muodon täyttösääntöineen ja viivatun muodon kynineen. Nimet ovat
  [sanastossa](../Glossary.md).
- **Nykytila** (`feature/palette-stack` tiketin 011 jälkeen):
  - `AreaSymDef.Draw` täyttää palettivärissä alueen, kun palettiväri on täyttöväri (`SymPathWithHoles.Fill`,
    `AreaFillMode.Alternate`), piirtää viivoitukset (`DrawHatchings`) ja kuviot (`DrawPatterns`) ja sitten reunan
    reunasymbolillaan pääpolulle ja jokaiselle reiälle (`LineSymDef.Draw`). Vaihe 1 jättää viivoitukset ja kuviot pois
    kerääjän asetuksella `omitHatchingsAndPatterns`, ja `AreaSymDef.HasFullCoverage` ottaa vain täyttövärin ja reunan
    värit.
  - `AreaComboSymDef.Draw` piirtää osamäärittelynsä järjestyksessä: viivaosan (`LineLikeSymDef`) pääpolulle ja
    jokaiselle reiälle ja alueosan (`AreaLikeSymDef`) sen omalla `Draw`-metodilla.
  - Grafiikkaobjektien symbolimäärittely `GraphicsSymDef` on kartan yhteinen, ja värit ja kynä ovat symbolissa.
    `GraphicsAreaSymbol.Draw` täyttää polkunsa täyttövärillään, ja `GraphicsLineSymbol.Draw` piirtää polkunsa
    kynällä, jolla on viivan leveys, päät ja liitos sekä jiirausraja `GraphicsUtil.MITER_LIMIT`. OCADin tuonti tekee
    grafiikkaobjektit (`OcadImport.CreateAreaGraphicsObject` ja `CreateLineGraphicsObject`).
  - Tiketin 011 jälkeen `LineSymDef.MakeClippingObjects(SymPath, SymColor)` (internal) antaa viivan leikkausobjektit
    mille tahansa peruspolulle, jotta alueen reuna voi käyttää sitä. `LineComboSymDef` antaa leikkausobjektinsa vain
    viivasymbolille. Täytetyn muodon tekee `SymPathWithHoles.MakeFilledShape` (tiketti 024) ja kynän
    `GraphicsUtil.MakeSolidStroke`.
- **Vanha työ** (`feature/area-geometry`, ca753598, juurtuu vanhaan viivahaaraan `feature/line-geometry`):
  - Sopii: alueen täyttö (`SymPathWithHoles.MakeFilledShape`), reuna pääpolulla ja jokaisella reiällä
    (`AddClippingObjectsOfBorder`), yhdistelmäalueen osat osien järjestyksessä, grafiikka-alueen täyttö ja
    grafiikkaviivan viivattu muoto sekä testit `SymbolGeometryTests`issä. Ne sopivat, kun geometriametodi antaa yhden
    palettivärin leikkausobjektit tiketin 024 rajapinnalla ja reuna käyttää tiketin 011 viivojen geometriametodia.
  - Ei sovi: rakentaja (`LayerInstanceBuilder`) ja `TryAddClippingObjects`, koska geometriametodi antaa yhden
    palettivärin leikkausobjektit, sekä vanha viivojen geometriametodi (`LineLikeSymDef.AddClippingObjects`), koska
    tiketin 011 metodi korvaa sen.
- **Ratkaisut** (istunnon ehdotukset, @Paulin hyväksyttävinä tiketin kanssa):
  - `AreaSymDef`, `AreaComboSymDef` ja `GraphicsSymDef` korvaavat metodin `SymDef.MakeClippingObjects`. Sisäinen
    `AreaLikeSymDef.MakeClippingObjects(SymPathWithHoles, SymColor)` antaa alueen leikkausobjektit polulle reikineen,
    jotta yhdistelmäalue voi koota alueosiensa leikkausobjektit samalla polulla.
  - `LineLikeSymDef` saa sisäisen abstraktin metodin `MakeClippingObjects(SymPath, SymColor)`, ja `LineComboSymDef`
    toteuttaa sen osamäärittelyjensä järjestyksessä. Näin yhdistelmäalueen viivaosa voi olla mikä tahansa viiva.
  - Alueen reunan leikkausobjektit tehdään reunasymbolin metodilla `MakeClippingObjects(SymPath, SymColor)` ensin
    pääpolulle ja sitten jokaiselle reiälle, kuten `Draw`. Reunalle ei tule omaa muotoa: reuna on viiva.
  - Grafiikkaviivan kynä on `Stroke` viivan leveydellä, päillä ja liitoksella sekä jiirausrajalla
    `GraphicsUtil.MITER_LIMIT`, kuten `GraphicsLineSymbol.Draw` luo kynänsä. `GraphicsUtil.MakeSolidStroke` ei sovi,
    koska se ottaa päät ja liitoksen viivatyylinä (`LineStyle`) eikä erikseen.
- **Rajaus:**
  - Viivoitukset ja aluekuviot peittoprototyyppeinä ovat luonnos [009](009-pattern-instances.md). Kerääjä jättää ne
    pois asetuksella `omitHatchingsAndPatterns`, ja geometriametodit eivät tee niitä.
  - Tekstit, viivan varren tekstit ja suorakulmiot ovat luonnos [013](013-text-geometry.md). Kerääjä on niiden
    leikkausobjektien lähde, kunnes @Pauli on hyväksynyt luonnoksen tiketiksi ja se on toteutettu (@Pauli, 23.11.).
  - Kuva- ja asettelukerroksen symbolit (`ImageSymDef`) eivät ole palettivärejä, eikä niillä ole palettiväripeittoja.
  - Vaihe 2 on luonnos [010](010-palette-composition.md). SVG-koodaus ja litistys eivät kuulu tähän.
- **Testit:** Map_Skia.Tests toimii Linuxilla sellaisenaan (`TEST_SILENTRUN=True`). Vanha piirtokoodi ei muutu,
  joten täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack` ilman keräävää ajoa:
  `tools/push-wip.sh --base-baselines prs/000-palette-stack-base <klooni> feature/area-symbol-geometry`.
- **Haara:** `feature/area-symbol-geometry` juurtuu haaraan `feature/palette-stack` sen jälkeen, kun tiketin 011 haara
  `feature/line-symbol-geometry` on yhdistetty siihen. Täysi ajo tehdään haarassa `wip/linux/area-symbol-geometry`
  basen päällä valitsimella `--base-baselines prs/000-palette-stack-base`. Vanha haara `feature/area-geometry` jää
  vanhan vaiheen 1 tallenteeksi.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md),
  [../README.md](../README.md), [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja
  "Decisions", speksin esipuhe ja luvut 01–03, [../Glossary.md](../Glossary.md), tiketit
  [023](023-palette-separations.md), [024](024-point-symbol-geometry.md) ja [011](011-line-geometry.md) sekä tämä
  tiketti. [../tools/README.md](../tools/README.md) kertoo tiketin työkierron.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
