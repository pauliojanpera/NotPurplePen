# LUONNOS – Vaihe 1: alueiden, yhdistelmien ja grafiikkaobjektien geometriametodit

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus (luonnos)**
Aluesymbolit (`AreaSymDef` ja `AreaComboSymDef`) sekä grafiikkaobjektit (`GraphicsAreaSymbol` ja `GraphicsLineSymbol`) tuottavat leikkausobjektinsa geometriametodeilla, ja vaihe 1 käyttää niitä kerääjän tilalla. Viivoitukset ja aluekuviot ovat toistuvaa geometriaa, eivätkä ne kuulu tähän. Tämä on geometriametodien kolmas osa, ja se on erotettu tiketistä [008](008-symbol-geometry.md) (@Pauli, 18.11.).

**Päätökset (sitovia)**
- Tiketin [008](008-symbol-geometry.md) päätökset tehtiin koko työlle ennen sen jakoa neljään osaan (@Pauli, 18.11.), ja ne koskevat tätäkin osaa: kerääjän ilmentymät ovat oraakkeli, vanha piirtokoodi pysyy muuttumattomana, ja ajot tehdään ilman `--collect`-valintaa, kun muutos ei voi muuttaa piirtoa.

**Hyväksymiskriteerit (luonnos: ehdotuksia, ei sitovia)**
- [ ] Ehdotus: aluesymbolin geometriametodi tuottaa alueen täytön, joka on polku reikineen ja täyttösääntöineen, ja reunan, jonka sen reunasymboli tuottaa viivojen geometriametodilla (tiketti [011](011-line-geometry.md)). `AreaComboSymDef` kokoaa osiensa geometrian.
- [ ] Ehdotus: grafiikkaobjektien geometriametodit tuottavat oman värinsä täytön (`GraphicsAreaSymbol`) ja viivan (`GraphicsLineSymbol`).
- [ ] Ehdotus: geometriametodit tuottavat ne leikkausobjektit ilman `IGraphicsTarget`-kutsuja ja ilman piirtoasetuksia, eivätkä ne tuota viivoituksia eivätkä aluekuvioita.
- [ ] Ehdotus: testikartoilla alueiden ja grafiikkaobjektien geometriametodien ilmentymät ovat rakenteena samat kuin kerääjän ilmentymät (tiketin 008 vertailu).
- [ ] Ehdotus: `LayerInstanceTests` ja `LayerInstanceCoverageTests` menevät läpi.
- [ ] Ehdotus: piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot (luonnos)**
- **Havaintoja nykyisestä koodista:**
  - `AreaSymDef.Draw` täyttää alueen, piirtää viivoituksen (`DrawHatchings`) ja kuviot (`DrawPatterns`) sekä reunan reunasymbolillaan. Vaihe 1 jättää viivoitukset ja kuviot pois asetuksella `omitHatchingsAndPatterns`, ja kerääjä tarvitsee asetuksen oraakkelina niin kauan kuin se on käytössä.
  - `AreaComboSymDef` kokoaa osiaan.
  - `GraphicsAreaSymbol` täyttää polkunsa täyttövärillään, ja `GraphicsLineSymbol` piirtää polkunsa omalla kynällään (väri, leveys, päät ja liitokset).
  - Kerääjän täytetty muoto pitää täyttösäännön (`FilledShape`).
- **Rajaus (luonnos):** viivoitukset ja aluekuviot prototyyppeinä ovat tiketti [009](009-pattern-instances.md). Kuva- ja asettelukerroksen symbolit eivät kuulu ilmentymiin.
- **Haara (ehdotus):** `feature/area-geometry` juurtuu kokoavaan haaraan `feature/palette-layers` tiketin 011 jälkeen. Täysi ajo tehdään basen päällä, ja ilman `--collect`-valintaa, kun muutos ei koske muuta koodia, jota piirto ajaa.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
