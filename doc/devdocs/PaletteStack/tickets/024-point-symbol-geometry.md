# LUONNOS – Pistesymbolien geometriametodit

> **LUONNOS (23.11.), @Paulin hyväksyttävänä.** Tämä tiketti on luonnos, kunnes @Pauli hyväksyy sen, eikä luonnosta toteuteta. Hyväksynnän jälkeen istunto poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus**
Pistesymbolien symbolimäärittely antaa symboliensa palettiväripeittojen leikkausobjektit omalla geometriametodillaan ilman piirtoa, ja glyyfin jaetut muodot ovat määrittelyn johdos. Kerääjä on oraakkeli, ja muut symbolilajit saavat leikkausobjektinsa edelleen kerääjältä. Piirto ei muutu.

**Hyväksymiskriteerit**
- [ ] Geometriametodien rajapinta: symbolimäärittely antaa symbolin palettiväripeiton leikkausobjektit symbolista, palettiväristä ja peittoprototyypistä ilman piirtokohdetta (`IGraphicsTarget`) ja ilman piirtoasetuksia (`RenderOptions`). Symbolimäärittely, jolla ei ole geometriametodia, antaa ne symbolin piirtokoodista kerääjällä, kuten tiketissä [023](023-palette-separations.md).
- [ ] Pistesymbolin symbolimäärittelyllä (`PointSymDef`) on geometriametodi. Se antaa palettivärissä ne glyyfin osat, joiden väri tämä palettiväri on, osien järjestyksessä: viivat, alueet, ympyrät, täytetyt ympyrät ja aukkojen jättämät kaaret. Se noudattaa glyyfin piirron ehtoja (`Glyph.Draw` ja `DrawSimple`) korjauksen [015](015-thick-glyph-circles.md) kanssa: ympyrä, jonka viiva on vähintään halkaisijan levyinen, on täytetty ympyrä.
- [ ] Glyyfin jaetut muodot ovat glyyfin johdos: kukin glyyfin osa on yksi muoto, joka lasketaan, kun sitä tarvitaan ensimmäisen kerran. Saman symbolimäärittelyn pistesymbolit viittaavat samoihin muotoihin omilla sijoituksillaan. Poikkeuksia on kaksi: origossa oleva yhden ympyrän glyyfi ilman aukkoja on symbolin paikassa ilman muunnosta, kuten `Glyph.DrawSimple` piirtää sen, ja symbolin omien aukkojen (`PointSymbol.Gaps`) jättämät kaaret ovat symbolin omia muotoja, joilla on jaetun ympyrän kynä.
- [ ] Glyyfin osien sijoituksella on yksi sisäkkäinen affiininen muunnos: se kiertää glyyfiä origonsa ympäri symbolin kulmalla ja siirtää origon symbolin paikkaan, kuten `Glyph.Draw`.
- [ ] Kerääjä on oraakkeli: testit vertaavat geometriametodin leikkausobjekteja kerääjän leikkausobjekteihin rakenteena jokaisessa symbolin palettiväripeitossa: sama laji, muoto, kynä tai täyttösääntö ja sijoituksen sisäkkäiset affiiniset muunnokset samassa järjestyksessä, kukin luku tarkasti. Ero ei ole sallittu. Vertailu kattaa kansion `skia_render` jokaisen OCAD- ja Mapper-kartan.
- [ ] Rakennetestit tarkistavat jaetut muodot ja niiden kaksi poikkeusta sekä sen, että geometriametodi seuraa glyyfin piirron jokaista haaraa.
- [ ] Tiketin 023 testit menevät läpi, myös peiton vertailu, jossa pistesymbolien leikkausobjektit tulevat geometriametodilta.
- [ ] Piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot**
- **Päätökset:**
  - Neljä tikettiä korvaa tiketit 005, 007 ja 008, ja tämä on niistä toinen. Sen oraakkeli on kerääjä (@Pauli, 23.11., Progress.md: "The new tickets of pass 1"). Uudet commitit ottavat vanhasta vaiheesta 1 pistesymbolien geometriametodit ja testit siltä osin kuin ne sopivat (@Pauli, 22.11., "The new branch of the epic").
  - Pistesymbolit jakavat glyyfinsä muodot: kukin glyyfin osa on yksi muoto, jonka kukin symboli asettaa omalla sijoituksellaan (@Pauli, 18.11., "The shared shapes of the glyphs").
  - Vanha piirtokoodi pysyy ennallaan geometriametodien ajan, jotta kerääjä pysyy riippumattomana oraakkelina. Geometriametodit kirjoitetaan sen rinnalle, ja symbolin kokoava logiikka saa olla kahdessa paikassa (@Pauli, 18.11., "The old drawing code during the geometry methods").
  - Kun muutos ei voi muuttaa piirtoa, täysi ajo tehdään ilman keräävää ajoa (@Pauli, 18.11., "A full run without a collecting run").
- **Speksi:** [Specification.md](../Specification.md), luvut 01 ja 03. Luvun 01 mukaan symbolimäärittelyt antavat symboliensa palettiväripeittojen johdokset. Luvun 03 mukaan johdos, joka riippuu vain määrittelystä, kuuluu määrittelylle, esimerkiksi glyyfin jaetut muodot, ja kukin glyyfin sijoitus viittaa niihin. Venytetyt ja pienenevät glyyfit saavat omat muotonsa, ja ne ovat viivojen glyyfejä.
- **Nykytila** (`feature/palette-stack` ja tiketti 023):
  - `PointSymDef` piirtää glyyfinsä (`Glyph.Draw`), jonka osat ovat viivoja, alueita, ympyröitä ja täytettyjä ympyröitä. Glyyfin paikka ja kierto ovat muunnospinossa (`PushTransform`). Origossa olevan yhden ympyrän glyyfin `DrawSimple` piirtää suoraan symbolin paikkaan. Symbolin aukot tekevät ympyrästä kaaria.
  - Korjaus 015 on haarassa `feature/palette-stack`: ympyrä, jonka viiva on vähintään halkaisijan levyinen, piirretään täytettynä halkaisijansa kokoisena.
  - Tiketin 023 jälkeen kerääjä tekee kaikkien symbolilajien leikkausobjektit.
- **Vanha vaihe 1** (`feature/palette-layers`, d00c5c4e):
  - Geometriametodi kulkee `PointSymbol.TryAddClippingObjects`ista `PointSymDef.AddClippingObjects`in kautta `Glyph.AddClippingObjects`iin, joka lisää symbolin kaikkien palettivärien leikkausobjektit rakentajaan (`LayerInstanceBuilder`). Glyyfin osa tekee muotonsa metodeilla `GlyphPart.MakeClippingObject`, `MakeClippingObjectOfSimpleGlyph` ja `AddArcs`, ja `Glyph.GetSharedClippingObjects` julkaisee jaetut muodot kerran (`Interlocked.CompareExchange`). Polut tekevät `SymPath.MakeStrokedShape` ja `SymPathWithHoles.MakeFilledShape`, ja kiinteän kynän tekee `GraphicsUtil.MakeSolidStroke`. Commit 6bc52ba1 antaa geometriametodille korjauksen 015 täytetyt ympyrät.
  - Nämä sopivat, kun geometriametodi antaa yhden palettivärin leikkausobjektit eikä lisää kaikkia palettivärejä rakentajaan. Rakentaja ja `TryAddClippingObjects` eivät sovi.
  - Testit ovat Map_Skia.Testsissä: `SymbolGeometryTests` vertasi vaiheen 1 `LayerInstance`-olioita kerääjän olioihin 162 kartalla, joissa on 12 311 pistesymbolia, eikä yhtään eroa ollut. Sen muut testit tarkistavat jaetut muodot, kaksi poikkeusta ja piirron haarat. `LayerInstanceComparison` vertaa rakenteita. Ne sopivat, kun vertailu tehdään symbolin palettiväripeitoittain.
- **Ratkaisut** (nimet valitsee istunto, @Pauli, 23.11.):
  - `SymDef.MakeClippingObjects(Symbol, SymColor, CoveragePrototype)` on geometriametodien rajapinta. Perusluokan toteutus antaa kerääjän leikkausobjektit, ja `PointSymDef` korvaa sen omalla geometriametodillaan.
  - `SymDef.CollectClippingObjects(Symbol, SymColor)` (internal) antaa kerääjän leikkausobjektit myös symbolimäärittelylle, jolla on geometriametodi. Testit käyttävät sitä oraakkelina, ja perusluokan `MakeClippingObjects` käyttää samaa metodia.
  - Testien rakenteellinen vertailu on `SymbolCoverageComparison`, vanhan `LayerInstanceComparison`in tilalla.
  - MapModelin `InternalsVisibleTo` nimeää myös Map_Skia.Testsin, jotta testit näkevät oraakkelin.
- **Rajaus:** viivojen, alueiden ja tekstien geometriametodit ovat luonnokset [011](011-line-geometry.md), [012](012-area-geometry.md) ja [013](013-text-geometry.md), ja viivojen glyyfit kuuluvat niistä ensimmäiseen. Symbolin ja rataobjektin avaimet ovat tiketit 025 ja 026, viivoitukset ja aluekuviot luonnos [009](009-pattern-instances.md) ja vaihe 2 luonnos [010](010-palette-composition.md).
- **Testit:** Map_Skia.Tests toimii Linuxilla sellaisenaan (`TEST_SILENTRUN=True`). `SymbolGeometryTests` antaa `RenderingTests`in lisätä Roboto-fontit kiinnittimen alussa (`[OneTimeSetUp]`), jotta vertailun molemmat puolet mittaavat tekstit samoilla fonteilla (tiketti [014](014-basic-tests-private-fonts.md)). Paikallinen integraatioworktree tehdään basen päälle skriptillä `tools/int-worktree.sh`, ja Test_MapModelin tiedostot ajetaan skriptillä `tools/run-map-tests.sh`. Vanha piirtokoodi ei muutu, joten täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack` ilman keräävää ajoa: `tools/push-wip.sh --base-baselines prs/000-palette-stack-base <klooni> feature/point-symbol-geometry`.
- **Haara:** `feature/point-symbol-geometry` juurtuu haaraan `feature/palette-stack` sen jälkeen, kun tiketin 023 haara on yhdistetty siihen. Täysi ajo tehdään haarassa `wip/linux/point-symbol-geometry` basen päällä valitsimella `--base-baselines prs/000-palette-stack-base`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md), [../README.md](../README.md) ja sen kohta "The architecture comes first", [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja "Decisions", speksin esipuhe ja luvut 01 ja 03, [../Glossary.md](../Glossary.md), tiketti [023](023-palette-separations.md) sekä tämä tiketti. [../tools/README.md](../tools/README.md) kertoo tiketin työkierron.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
