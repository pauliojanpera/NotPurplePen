# LUONNOS – Vaihe 1: tekstien ja suorakulmioiden geometriametodit

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus (luonnos)**
Tekstisymbolit (`TextSymDef`), tekstit viivan varrella (`LineTextSymbol`) ja suorakulmiosymbolit (`RectangleSymDef`) tuottavat leikkausobjektinsa geometriametodeilla. Tämän osan jälkeen jokaisella symbolilajilla, joka tuottaa yhtenäistä geometriaa, on geometriametodi, eikä vaihe 1 käytä kerääjää. Tämä on geometriametodien neljäs osa, ja se on erotettu tiketistä [008](008-symbol-geometry.md) (@Pauli, 18.11.).

**Päätökset (sitovia)**
- Koko työlle ennen sen jakoa neljään osaan tehdyt päätökset (tiketti [008](008-symbol-geometry.md), @Pauli, 18.11.) koskevat tätäkin osaa: kerääjän ilmentymät ovat oraakkeli, vanha piirtokoodi pysyy muuttumattomana, ja ajot tehdään ilman `--collect`-valintaa, kun muutos ei voi muuttaa piirtoa.
- Hyödyn mittari on renderöintikanavissa tarjolla olevan abstraktion hyödyntäminen perustilanteiden ratkaisuna (@Pauli, 19.11., tiketti [011](011-line-geometry.md)).
- Tekstin rakenne: jokainen sana, tai grafeemi, kun merkkiväli on suurempi kuin 0, on tekstiajo omassa paikassaan, kuten piirrossa ja kerääjässä. Kanavat ottavat paikoitetut ajot sellaisinaan (SVG:n `tspan` x ja y, PDF:n `Td` ja `TJ`, Skian paikoitettu tekstiblobi), ja paikat pysyvät Purple Penin mittojen mukaisina (kysymys 1, @Pauli, 19.11.).

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
- **Avoimia kysymyksiä:**
  2. Kehysviivan välikynät. Piirto piirtää kehysviivan tekstiajon ääriviivana usealla kynällä: välikynät levenevät 0,33 × fonttikoon välein, ja viimeinen on täysi leveys (2 × kehyksen leveys). Kapeammat viivat ovat leveimmän sisällä, joten geometrisesti kehys on yksi viiva. Koodin kommentti on vuoden 2008 GDI+-koodista, mutta kokeessa (kartta `frametext1`, 10 kehysviivallista tekstiä) yksi leveä viiva jättää reikiä kehykseen, esimerkiksi o-kirjaimen sisään, Skialla (41 pikseliä 1500 px:n leveydellä) ja Cairolla (pdftocairo 264 ja Inkscape 127 pikseliä 1200 dpi:llä), mutta ei Popplerin Splashilla eikä Ghostscriptillä. Välikynät paikkaavat reiät. Ennen päätöstä OCADin piirros näyttää, ovatko reiät ylipäätään virheitä: `frametext1.ocd` on @Paulin tikulla kansiossa `ocad-framing` PDF- ja PNG-vientiä varten ([oracles/ocad-framing.md](../oracles/ocad-framing.md), @Pauli, 19.11.).
- **Ratkaistut kysymykset:** tekstin rakenne (kysymys 1) ratkaistiin @Paulin päätöksellä 19.11.
- **Rajaus (luonnos):** kerääjä on testien oraakkeli tikettien 008–013 ajan, ja asetus `omitHatchingsAndPatterns` jää sen mukana. Tiketti [009](009-pattern-instances.md) poistaa molemmat (@Pauli, 18.11.). Vaihe 2 on tiketti [010](010-palette-composition.md).
- **Haara (ehdotus):** `feature/text-geometry` juurtuu kokoavaan haaraan `feature/palette-layers` tiketin 012 jälkeen. Täysi ajo tehdään basen päällä, ja ilman `--collect`-valintaa, kun muutos ei koske muuta koodia, jota piirto ajaa.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
