# Aluesymbolien ja grafiikkaobjektien geometriametodit
**Kuvaus**
Alueen symbolimäärittelyt (`AreaSymDef` ja `AreaComboSymDef`) ja grafiikkaobjektien symbolimäärittely
(`GraphicsSymDef`) antavat symboliensa täyden peiton palettiväripeittojen leikkausobjektit omalla geometriametodillaan
ilman piirtoa: alueen täytön täytettynä muotona, alueen reunan viivojen geometriametodilla (tiketti
[011](011-line-geometry.md)) pääpolulla ja jokaisella reiällä, `AreaComboSymDef`in yhdistelmän osamäärittelyjen
leikkausobjektit osamäärittelyjen järjestyksessä sekä grafiikkaobjektin täytetyn muodon tai viivamuodon. Kerääjä on
oraakkeli, ja tekstit ja suorakulmiot saavat leikkausobjektinsa edelleen kerääjältä. Viivoitukset ja aluekuviot eivät
kuulu tähän. Piirto ei muutu.

**Hyväksymiskriteerit**
- [ ] Geometriametodien rajapinta: `AreaLikeSymDef` ja `GraphicsSymDef` korvaavat tiketin
  [024](024-point-symbol-geometry.md) rajapinnan `SymDef.MakeClippingObjects(Symbol, SymColor, CoveragePrototype)`.
  `AreaLikeSymDef` antaa aluesymbolin polun reikineen (`AreaLikeSymbol.Path`) alueen symbolimäärittelyn sisäiselle
  geometriametodille `MakeClippingObjects(SymPathWithHoles, SymColor)`, jonka `AreaSymDef` ja `AreaComboSymDef`
  toteuttavat. Aluesymbolin ja grafiikkaobjektin palettiväripeiton leikkausobjektit tulevat symbolimäärittelyltä ilman
  piirtokohdetta (`IGraphicsTarget`) ja ilman piirtoasetuksia (`RenderOptions`), eikä vaihe 1 kerää yhtään
  aluesymbolia eikä grafiikkaobjektia kerääjällä.
- [ ] `AreaSymDef`: geometriametodi antaa palettivärissä alueen täytön ja alueen reunan siinä järjestyksessä kuin
  `AreaSymDef.Draw` piirtää ne. Alueen täyttö (`FillColor`) on sen värissä täytetty muoto: pääpolku ja reiät yhtenä
  polkuna vuorottelevalla täyttösäännöllä (`AreaFillMode.Alternate`), kuten `SymPathWithHoles.Fill` täyttää ne. Muoto
  on polku, joka pitää polkujen Bezier-käyrät, tai monikulmio samalla ehdolla kuin `SymPath.FillCore` täyttää
  monikulmion. Alueen reunan (`BorderSymdef`) leikkausobjektit ovat viivan leikkausobjektit, joiden peruspolku on
  ensin pääpolku ja sitten kukin reikä reikien järjestyksessä, kuten `Draw` piirtää ne. Ne tulevat viivojen
  geometriametodilla: koostetut katkoviivat, kaksoisviivan reunat siirrettyinä polkuina ja viivan varren glyyfit
  toistosijoituksina ja yksittäisinä sijoituksina.
- [ ] `AreaComboSymDef`: geometriametodi antaa yhdistelmän osamäärittelyjen leikkausobjektit osamäärittelyjen
  järjestyksessä, kuten `AreaComboSymDef.Draw` piirtää ne. Viivan symbolimäärittely (`LineLikeSymDef`) antaa viivan
  leikkausobjektit, joiden peruspolku on ensin pääpolku ja sitten kukin reikä, ja alueen symbolimäärittely
  (`AreaLikeSymDef`) antaa oman geometriametodinsa leikkausobjektit samalle polulle reikineen. Yhdistelmän
  osamäärittely voi olla `LineSymDef`, `LineComboSymDef`, `AreaSymDef` tai toinen `AreaComboSymDef`, kuten Mapperin
  tuonti tekee ne (`OpenMapperImport.DelayedComboSymDef.CreateSymDef`).
- [ ] `LineLikeSymDef`: viivan symbolimäärittelyllä on sisäinen abstrakti geometriametodi
  `MakeClippingObjects(SymPath, SymColor)`, joka antaa viivan leikkausobjektit mille tahansa peruspolulle.
  `LineSymDef` toteuttaa sen tiketin 011 mukaan, ja `LineComboSymDef` antaa yhdistelmän osamäärittelyjensä
  leikkausobjektit osamäärittelyjen järjestyksessä.
- [ ] Grafiikkaobjektit: `GraphicsSymDef`in geometriametodi antaa `GraphicsAreaSymbol`in värissä (`FillColor`)
  täytetyn muodon, joka tehdään polusta reikineen kuten alueen täyttö, ja `GraphicsLineSymbol`in värissä
  (`LineColor`) viivamuodon. Viivamuodon kynällä on grafiikkaobjektin leveys (`Thickness`), päät (`LineCapMode`) ja
  liitos (`LineJoinMode`) sekä jiirausraja `GraphicsUtil.MITER_LIMIT`, kuten `GraphicsLineSymbol.Draw` luo kynänsä, ja
  muoto pitää polun Bezier-käyrät kuten `SymPath.Draw`.
- [ ] Täysi peitto: geometriametodit antavat vain täyden peiton leikkausobjektit. Viivoitukset ja aluekuviot ovat
  jaksollisia peittoja (luonnos [009](009-pattern-instances.md)), eikä niistä tule leikkausobjekteja. Palettiväri,
  jota alueen symbolimäärittely piirtää vain viivoituksella tai aluekuviolla, ei saa aluesymbolilta täyden peiton
  palettiväripeittoa, kuten tiketissä [023](023-palette-separations.md).
- [ ] Kerääjä on oraakkeli: testit vertaavat geometriametodien leikkausobjekteja kerääjän leikkausobjekteihin
  (`SymDef.CollectClippingObjects`) rakenteena jokaisessa aluesymbolin ja grafiikkaobjektin palettiväripeitossa.
  Vertailu laajentaa alueen reunojen katkojuosteet, siirretyt polut ja toistosijoitukset, ja sen sallitut erot ovat
  tiketin 011 sallitut erot. Muut leikkausobjektit, myös täytetyt muodot, ovat täsmälleen samat. Vertailu kattaa
  kansion `skia_render` jokaisen OCAD- ja Mapper-kartan sekä katkoviivanäytteiden tiedostot `dashes-*.ocd`.
- [ ] Rakennetestit tarkistavat ilman piirtoa alueen täytön monikulmiona, Bezier-käyrin, reikineen ja tyhjällä
  reikien taulukolla ja sen täyttösäännön, alueen reunan pääpolulla ja reiillä, katkotun alueen reunan, alueen reunan
  kaksoisviivana ja alueen reunan glyyfit, `AreaComboSymDef`in yhdistelmän osamäärittelyjen järjestyksen,
  `LineComboSymDef`in ja toisen `AreaComboSymDef`in yhdistelmän osamäärittelynä, aluesymbolin, jonka palettiväri on
  vain viivoituksessa tai aluekuviossa, sekä `GraphicsAreaSymbol`in ja `GraphicsLineSymbol`in. Testit rakentavat
  kartan tapauksille, joita vertailun kartoissa ei ole: alueen reuna kaksoisviivana sekä yhdistelmän osamäärittelyt
  `LineComboSymDef` ja `AreaComboSymDef` (ks. "Testit").
- [ ] Tiketin 023 testit menevät läpi, myös peiton vertailu, jossa aluesymbolien ja grafiikkaobjektien
  leikkausobjektit tulevat geometriametodilta. Tikettien 024 ja 011 testit menevät läpi.
- [ ] Tämän tiketin haara ei muuta piirtoa, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot**
- **Päätökset:**
  - @Pauli hyväksyi tämän tiketin ja sen istunnon valinnat (ks. "Ratkaisut") 23.11.
  - Geometriametodit tehdään neljänä osana symbolilajeittain, ja tämä on aluesymbolien ja grafiikkaobjektien osa,
    erotettu tiketistä 008 (@Pauli, 18.11.). Tiketit 023–026 korvaavat tiketit 005, 007 ja 008 (@Pauli, 23.11.),
    joten tämä tiketti käyttää tiketin 024 rajapintaa ja oraakkelia.
  - Vanha piirtokoodi pysyy ennallaan geometriametodien ajan, jotta kerääjä pysyy riippumattomana oraakkelina.
    Geometriametodit kirjoitetaan sen rinnalle, ja symbolin kokoava logiikka saa olla kahdessa paikassa (@Pauli,
    18.11.). Kun muutos ei voi muuttaa piirtoa, täysi ajo tehdään ilman keräävää ajoa (@Pauli, 18.11.).
  - Kerääjä on testien oraakkeli tikettien 008–013 ajan, ja tiketti 009 poistaa sen ja asetuksen
    `omitHatchingsAndPatterns` (@Pauli, 18.11.).
  - Alueen reunan peruspolku on pääpolku tai reikä (speksin luku 02, "The lines"). Alueen reunan leikkausobjektit
    ovat viivan leikkausobjektit, ja tiketin 011 päätökset koskevat niitä: koostettu katkoviiva, kaksoisviivan reunat
    siirrettyinä polkuina ja viivan varren glyyfit toistosijoituksina.
  - Alueen reunan katkoille sallittu ero on tiketin 011 ero: siirretyn polun laajennuksen leikkausobjektit 0,01 mm ja
    muut laajennetut leikkausobjektit 0,0001 mm (@Pauli, 23.11.).
- **Speksi:** [Specification.md](../Specification.md), luvut 02 ja 03. Luvun 02 kohta "The hatchings and the area
  patterns" antaa alueen täytön, jonka G on täysi peitto, ja alueen reunan, jonka `AreaSymDef.Draw` piirtää
  pääpolulla ja jokaisella reiällä. Kohta "The lines" antaa alueen reunan peruspolun, ja kohta "The coverage
  prototypes" täyden peiton ja jaksolliset peitot. Luvun 03 kohdat "The clipping objects and the union" ja "The forms
  of the derived values" antavat täytetyn muodon täyttösääntöineen ja viivamuodon kynineen. Nimet ovat
  [sanastossa](../Glossary.md).
- **Nykytila** (`feature/palette-stack` tiketin 011 jälkeen):
  - `AreaSymDef.Draw` täyttää palettivärissä alueen täytön, kun palettiväri on `FillColor` (`SymPathWithHoles.Fill`),
    piirtää viivoitukset (`DrawHatchings`) ja aluekuviot (`DrawPatterns`) ja sitten alueen reunan pääpolulle ja
    jokaiselle reiälle (`LineSymDef.Draw`). `SymPath.FillCore` täyttää vuorottelevalla täyttösäännöllä monikulmion,
    kun pääpolulla ei ole Bezier-käyriä ja reikien taulukko on null, ja muuten polun. Vaihe 1 jättää viivoitukset ja
    aluekuviot pois kerääjän asetuksella `omitHatchingsAndPatterns`, ja `AreaSymDef.HasFullCoverage` ottaa vain
    alueen täytön ja alueen reunan palettivärit.
  - Reiät ovat kartassa omia symbolejaan (`HoleSymbol`), joiden symbolimäärittelyllä (`HoleSymDef`) ei ole väriä.
    `AreaLikeSymbol.Path` kokoaa pääpolun ja reikien polut polukseen reikineen (`SymPathWithHoles`). Reikien taulukko
    on null, kun aluesymbolilla ei ole koskaan ollut reikiä, ja tyhjä, kun sen kaikki reiät on poistettu.
  - `AreaComboSymDef.Draw` piirtää yhdistelmän osamäärittelynsä järjestyksessä: viivan symbolimäärittelyn
    (`LineLikeSymDef`) pääpolulle ja jokaiselle reiälle ja alueen symbolimäärittelyn (`AreaLikeSymDef`) sen omalla
    `Draw`-metodilla samalle polulle reikineen. Mapperin tuonti tekee `LineComboSymDef`in, kun yhdistelmän kaikki
    osamäärittelyt ovat `LineSymDef`ejä, ja muuten `AreaComboSymDef`in, jonka osamäärittely voi olla myös
    `LineComboSymDef` tai toinen `AreaComboSymDef` (`OpenMapperImport.DelayedComboSymDef.CreateSymDef`).
  - Grafiikkaobjektien symbolimäärittely `GraphicsSymDef` on yksi kaikille kartan grafiikkaobjekteille, eikä sillä ole
    omia arvoja: värit ja kynän arvot ovat grafiikkaobjektissa. `GraphicsAreaSymbol.Draw` täyttää polkunsa reikineen
    värissään (`FillColor`), ja `GraphicsLineSymbol.Draw` piirtää polkunsa värissään (`LineColor`) kynällä, jolla on
    sen leveys, päät ja liitos sekä jiirausraja `GraphicsUtil.MITER_LIMIT`. OCADin tuonti tekee grafiikkaobjektit
    (`OcadImport.CreateGraphicsObject`).
  - Tiketin 011 jälkeen `LineSymDef.MakeClippingObjects(SymPath, SymColor)` (internal) antaa viivan
    leikkausobjektit mille tahansa peruspolulle, ja `LineComboSymDef` antaa leikkausobjektinsa vain viivasymbolille.
    Täytetyn muodon tekee `SymPathWithHoles.MakeFilledShape` (tiketti 024). `LineLikeSymDef`illä ja
    `AreaLikeSymDef`illä ei ole aliluokkia MapModelin ulkopuolella.
- **Vanha työ** (`feature/area-geometry`, ca753598, juurtuu vanhaan viivahaaraan `feature/line-geometry`):
  - Sopii: alueen täytön täytetty muoto (`SymPathWithHoles.MakeFilledShape`), alueen reuna pääpolulla ja jokaisella
    reiällä (`AddClippingObjectsOfBorder`), `AreaComboSymDef`in yhdistelmän osamäärittelyt niiden järjestyksessä,
    `GraphicsAreaSymbol`in täytetty muoto ja `GraphicsLineSymbol`in viivamuoto sekä testit `SymbolGeometryTests`issä.
    Ne sopivat, kun geometriametodi antaa yhden palettivärin leikkausobjektit tiketin 024 rajapinnalla ja alueen reuna
    käyttää tiketin 011 viivojen geometriametodia.
  - Ei sovi: rakentaja (`LayerInstanceBuilder`) ja `TryAddClippingObjects`, koska geometriametodi antaa yhden
    palettivärin leikkausobjektit, sekä vanha viivojen geometriametodi (`LineLikeSymDef.AddClippingObjects`), koska
    tiketin 011 metodi korvaa sen.
- **Ratkaisut** (istunnon valinnat, @Pauli hyväksyi ne tiketin kanssa 23.11.):
  - `AreaLikeSymDef` ja `GraphicsSymDef` korvaavat metodin `SymDef.MakeClippingObjects`. `AreaLikeSymDef` antaa
    aluesymbolin polun reikineen sisäiselle abstraktille metodille `MakeClippingObjects(SymPathWithHoles, SymColor)`,
    jonka `AreaSymDef` ja `AreaComboSymDef` toteuttavat. Näin `AreaComboSymDef` antaa alueen symbolimäärittelynä
    olevan osamäärittelyn leikkausobjektit samalle polulle reikineen, myös kun osamäärittely on toinen
    `AreaComboSymDef`.
  - `LineLikeSymDef` saa sisäisen abstraktin metodin `MakeClippingObjects(SymPath, SymColor)`, ja `LineComboSymDef`
    toteuttaa sen yhdistelmän osamäärittelyjensä järjestyksessä. Näin `AreaComboSymDef`in osamäärittely voi olla mikä
    tahansa viivan symbolimäärittely.
  - Alueen reunan leikkausobjektit tulevat alueen reunan metodilla `MakeClippingObjects(SymPath, SymColor)`
    peruspolkuina ensin pääpolku ja sitten kukin reikä, kuten `Draw`. Alueen reunalle ei tule omaa muotoa: alueen reuna
    on viivan symbolimäärittely, ja sen leikkausobjektit ovat viivan leikkausobjektit.
  - `GraphicsLineSymbol`in kynä on `Stroke` grafiikkaobjektin leveydellä, päillä ja liitoksella sekä jiirausrajalla
    `GraphicsUtil.MITER_LIMIT`, kuten `GraphicsLineSymbol.Draw` luo kynänsä. `GraphicsUtil.MakeSolidStroke` ei sovi,
    koska se ottaa päät ja liitoksen viivatyylinä (`LineStyle`).
- **Rajaus:**
  - Viivoitukset ja aluekuviot peittoprototyyppeinä ovat luonnos [009](009-pattern-instances.md). Kerääjä jättää ne
    pois asetuksella `omitHatchingsAndPatterns`, eivätkä geometriametodit tee niitä.
  - Tekstit, viivan varren tekstit ja suorakulmiot ovat luonnos [013](013-text-geometry.md). Kerääjä on niiden
    leikkausobjektien lähde, kunnes @Pauli on hyväksynyt luonnoksen tiketiksi ja se on toteutettu (@Pauli, 23.11.).
  - Kuvatason ja asettelutason objekteilla (`ImageSymDef`) ja rei'illä (`HoleSymDef`) ei ole palettiväripeittoja.
  - Vaihe 2 on luonnos [010](010-palette-composition.md). SVG-koodaus ja litistys eivät kuulu tähän.
- **Testit:** Map_Skia.Tests toimii Linuxilla sellaisenaan (`TEST_SILENTRUN=True`).
  - Vertailun kartoissa (kansion `skia_render` 162 karttaa ja 10 katkoviivanäytettä) on 21 503 `AreaSymDef`in ja 29
    `AreaComboSymDef`in aluesymbolia sekä 94 `GraphicsAreaSymbol`ia ja 44 `GraphicsLineSymbol`ia. Aluesymboleista
    789:llä on reikiä, yhteensä 2 187, ja 9:llä on alueen reuna. Alueen reunoissa ja `AreaComboSymDef`ien viivan
    symbolimäärittelyissä on 4 katkojuostetta ja 5 leikkausobjektia toistosijoituksilla mutta ei siirrettyä polkua.
    `AreaComboSymDef`ien yhdistelmän osamäärittelyt ovat `LineSymDef` ja `AreaSymDef`.
  - Istunto kokeili ratkaisuja testillä, jota se ei commitoinut: niiden mukaiset leikkausobjektit ovat samat kuin
    kerääjän kaikissa näiden karttojen 18 782 `AreaSymDef`in, 58 `AreaComboSymDef`in, 94 `GraphicsAreaSymbol`in ja 44
    `GraphicsLineSymbol`in palettiväripeitossa.
  - Vanha piirtokoodi ei muutu, joten täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack` ilman keräävää ajoa:
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
