# LUONNOS – Vaihe 1: tekstien ja suorakulmioiden geometriametodit

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus (luonnos)**
Tekstisymbolit (`TextSymDef`), tekstit viivan varrella (`LineTextSymbol`) ja suorakulmiosymbolit (`RectangleSymDef`) tuottavat leikkausobjektinsa geometriametodeilla. Tämän osan jälkeen jokaisella symbolilajilla, joka tuottaa yhtenäistä geometriaa, on geometriametodi, eikä vaihe 1 käytä kerääjää. Tämä on geometriametodien neljäs osa, ja se on erotettu tiketistä [008](008-symbol-geometry.md) (@Pauli, 18.11.).

**Päätökset (sitovia)**
- Koko työlle ennen sen jakoa neljään osaan tehdyt päätökset (tiketti [008](008-symbol-geometry.md), @Pauli, 18.11.) koskevat tätäkin osaa: kerääjän ilmentymät ovat oraakkeli, vanha piirtokoodi pysyy muuttumattomana, ja ajot tehdään ilman `--collect`-valintaa, kun muutos ei voi muuttaa piirtoa.
- Hyödyn mittari on renderöintikanavissa tarjolla olevan abstraktion hyödyntäminen perustilanteiden ratkaisuna (@Pauli, 19.11., tiketti [011](011-line-geometry.md)).
- Tekstin rakenne: jokainen sana, tai grafeemi, kun merkkiväli on suurempi kuin 0, on tekstiajo omassa paikassaan, kuten piirrossa ja kerääjässä. Kanavat ottavat paikoitetut ajot sellaisinaan (SVG:n `tspan` x ja y, PDF:n `Td` ja `TJ`, Skian paikoitettu tekstiblobi), ja paikat pysyvät Purple Penin mittojen mukaisina (kysymys 1, @Pauli, 19.11.).
- Kehysviiva: tekstiajon kehys on ilmentymässä yksi viiva täydellä leveydellä ja symbolin liitoksilla, kuten OCADin PDF:ssä. OCADin oma piirros jättää kehykseen samoja reikiä kuin Skia ja Cairo yhdellä kynällä, joten Purple Penin välikynien paikkaamat reiät eivät ole virheitä OCADiin verrattuna. Vertailu kerääjään sallii eron, ja tiketti kertoo sen syyn (kysymys 2, @Pauli, 19.11., [oracles/ocad-framing.md](../oracles/ocad-framing.md)).

**Hyväksymiskriteerit (luonnos: ehdotuksia, ei sitovia)**
- [ ] Ehdotus: tekstisymbolin geometriametodi tuottaa tiketin 008 rajapinnalla tekstiajot, kehysviivan, varjon, alleviivaukset, keskipisteen ja kehyssuorakulmion ilman `IGraphicsTarget`-kutsuja ja ilman piirtoasetuksia.
- [ ] Ehdotus: viivan varren tekstin geometriametodi tuottaa jokaisen grafeemin tekstiajona omalla sijoituksellaan.
- [ ] Ehdotus: suorakulmiosymbolin geometriametodi tuottaa ääriviivan, ruudukon viivat ja ruudukon tekstit.
- [ ] Ehdotus: vaihe 1 tekee kaikki ilmentymät geometriametodeilla, eikä se käytä kerääjää. Kerääjä jää testien oraakkeliksi tikettiin 009 asti.
- [ ] Ehdotus: testikartoilla tekstien ja suorakulmioiden geometriametodien ilmentymät ovat rakenteena samat kuin kerääjän ilmentymät (tiketin 008 vertailu).
- [ ] Ehdotus: `LayerInstanceTests` ja `LayerInstanceCoverageTests` menevät läpi.
- [ ] Ehdotus: piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot (luonnos)**
- **Havaintoja nykyisestä koodista:**
  - `TextSymDef.Draw` piirtää tekstin osina: osa on sana, tai grafeemi, kun merkkiväli (`charSpacing`) on suurempi kuin 0. Kehysviiva piirtää tekstin ja sen ääriviivat usealla kynällä, varjo siirretyn tekstin, ja lisäksi on alleviivaukset, keskipiste ja kehyssuorakulmio.
  - Tekstin keskipiste ja suorakulmiosymbolin ruudukon tekstit sijoitetaan sisäkkäisillä muunnoksilla (tiketti 005).
  - `DrawTextOnPath` sijoittaa grafeemit polun varrelle, ja se mittaa polkua aina OCADin metriikalla (`BizzarroLength` ja `PointAtLengthBizzarro`), kartan metriikasta riippumatta.
  - Tekstin asettelu ja tekstiajojen rajat riippuvat kartan tekstin mitoista (`TextMetricsProvider`), ja koneen fontit eroavat CI:n fonteista.
  - `RectangleSymDef` piirtää ääriviivan, ruudukon viivat ja ruudukon tekstit.
- **Ratkaistut kysymykset:** tekstin rakenne (kysymys 1) ja kehysviiva (kysymys 2) ratkaistiin @Paulin päätöksillä 19.11. Kehysviivan pohjana oli koe: yksi leveä viiva jättää kehykseen reikiä Skialla ja Cairolla mutta ei Popplerin Splashilla eikä Ghostscriptillä, ja OCADin oma piirros jättää samanlaisia reikiä ([oracles/ocad-framing.md](../oracles/ocad-framing.md)).
- **Toteutus (kesken, haara `feature/text-geometry`, @Paulin tarkistettavana):**
  - `TextSymDef.AddClippingObjects` tuottaa tekstin leikkausobjektit `Draw`in rinnalla samoilla apumetodeilla: jokainen sana, tai grafeemi, kun merkkiväli on suurempi kuin 0, on tekstiajo omassa paikassaan varjoineen tai kehysviivoineen, ja kunkin rivin alleviivaukset, keskipiste ja kehyssuorakulmio tulevat `Draw`in järjestyksessä tekstin sijoituksessa. Kehysviivassa on vielä jokainen piirron kynä, kuten kerääjässä: päätös 19.11. yhdestä viivasta on toteuttamatta. `AddClippingObjectsOfTextOnPath` tuottaa viivan varren tekstin grafeemit omissa sijoituksissaan, ja `RectangleSymDef.AddClippingObjects` suorakulmion ääriviivan, ruudukon viivat ja ruudukon tekstit.
  - `Symbol.AddClippingObjects` on abstrakti, ja jokaisella symbolilajilla on se: kuva- ja asettelukerroksen symbolit eivät piirrä palettimerkintöihin, eikä reikä piirrä mitään, joten niiden metodit eivät lisää mitään. Vaihe 1 (`Map.CreateLayerInstances`) ei enää käytä kerääjää, ja kerääjä tekee testien oraakkelin (`Map.CollectLayerInstances`).
  - `PointSymDef.AddClippingObjects` ja `Glyph.AddClippingObjects` saavat sijoituksen, jonka sisällä glyyfi on, kuten tekstin keskipiste on tekstin sijoituksessa. `PathShape.FromRectangle` antaa suorakulmion kulmat kerääjän järjestyksessä, ja kerääjä käyttää sitä myös.
  - Vanha piirtokoodi ei muutu: haara lisää uusia metodeja, ja sen poistamat rivit ovat tikettien 008 ja 011 geometriakoodia.
  - Paikallisesti `SymbolGeometryTests` ja `LayerInstance`-testit menevät läpi (204 testiä), myös jokaisen `skia_render`-kartan rakennevertailu. Täysi ajo on kohdassa a20a13ac (`wip/linux/text-geometry`, basen päällä, valinnalla `--base-baselines prs/000-palette-layers-base`: Tests 44504633617, Build 44504633614).
- **Rajaus (luonnos):** kerääjä on testien oraakkeli tikettien 008–013 ajan, ja asetus `omitHatchingsAndPatterns` jää sen mukana. Tiketti [009](009-pattern-instances.md) poistaa molemmat (@Pauli, 18.11.). Vaihe 2 on tiketti [010](010-palette-composition.md).
- **Haara (ehdotus):** `feature/text-geometry` juurtuu kokoavaan haaraan `feature/palette-layers` tiketin 012 jälkeen. Koska tiketit 011 ja 012 eivät ole vielä kokoavassa haarassa, haara juurtuu nyt haaraan `feature/area-geometry` (ca753598). Täysi ajo tehdään basen päällä, ja ilman `--collect`-valintaa, kun muutos ei koske muuta koodia, jota piirto ajaa.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
