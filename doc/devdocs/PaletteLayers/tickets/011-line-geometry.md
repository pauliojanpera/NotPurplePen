# LUONNOS – Vaihe 1: viivojen geometriametodit

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus (luonnos)**
Viivasymbolit (`LineSymDef` ja `LineComboSymDef`) tuottavat leikkausobjektinsa geometriametodeilla, ja vaihe 1 käyttää niitä kerääjän tilalla. Katkoviivan perusmalli on yksi viivattu muoto, jolla on katkojen ja välien pituudet, ja eksoottisemmat katkoviivat pilkotaan katkoiksi. Tämä on geometriametodien toinen osa, ja se on erotettu tiketistä [008](008-symbol-geometry.md) (@Pauli, 18.11.).

**Päätökset (sitovia)**
- Koko työlle ennen sen jakoa neljään osaan tehdyt päätökset (tiketti [008](008-symbol-geometry.md), @Pauli, 18.11.) koskevat tätäkin osaa: kerääjän ilmentymät ovat oraakkeli, vanha piirtokoodi pysyy muuttumattomana, ja ajot tehdään ilman `--collect`-valintaa, kun muutos ei voi muuttaa piirtoa.
- Katkoviivan esityksen oikea ratkaisu etsitään tässä tiketissä (@Pauli, 18.11.).
- Katkoviiva noudattaa samaa periaatetta kuin kuviotäytöt [speksin](../Specification.md) luvussa 03: perusmalli esitetään standardivälineellä, katkoviivana, ja eksoottisemmat tapaukset, kuten OCADin metriikka, pilkotaan osiin, katkoviivassa yksittäisiksi katkoiksi (@Pauli, 18.11.). Luvun 03 mukaan leikattu kuviotäyttö pysyy toistuvana geometriana, ja kokonaisia elementtejä valitsevat täyttötilat ja epäsäännölliset täytöt tulevat yksittäisiksi leikkausobjekteiksi.

**Hyväksymiskriteerit (luonnos: ehdotuksia, ei sitovia)**
- [ ] Ehdotus: viivasymbolin geometriametodi tuottaa tiketin 008 rajapinnalla pääviivan, katkoviivat, terävät päät, toisen viivan, kaksoisviivan täytön ja reunat sekä glyyfit viivan varrella ilman `IGraphicsTarget`-kutsuja ja ilman piirtoasetuksia. `LineComboSymDef` kokoaa osiensa geometrian.
- [ ] Ehdotus: katkoviivan perusmalli on yksi viivattu muoto, jolla on katkojen ja välien pituudet, ja osiin pilkottu katkoviiva on yksittäisiä katkoja.
- [ ] Ehdotus: testikartoilla viivojen geometriametodien ilmentymät ovat rakenteena samat kuin kerääjän ilmentymät (tiketin 008 vertailu). Katkoviivan perusmalli verrataan katkoiksi laajennettuna, tai tiketti kertoo sallitun eron ja sen syyn.
- [ ] Ehdotus: rakennetestit tarkistavat ilman piirtoa katkoviivan perusmallin ja pilkotun katkoviivan.
- [ ] Ehdotus: `LayerInstanceTests` ja `LayerInstanceCoverageTests` menevät läpi, myös katkoviivojen tapaukset, joissa välit ovat pikseliä lyhyempiä (`ClippingObjectsCoverTheDashesOfGapsShorterThanAPixel`).
- [ ] Ehdotus: piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot (luonnos)**
- **Havaintoja nykyisestä koodista:**
  - `LineSymDef.DrawCore` piirtää pääviivan, katkoviivat (`ComputeDashDistances` ja `SymPath.DrawDashedCore`), terävät päät, toisen viivan, kaksoisviivan täytön ja reunat (`DrawDashedWithOffset`) sekä glyyfit viivan varrella (`GlyphLocation`: päät, kulmat, katkopisteet, välien ja katkojen keskikohdat, välimatkat ja pienenevät glyyfit). `LineComboSymDef` kokoaa osiaan.
  - `IGraphicsTarget`issa ei ole katkoviivoja: katkot laskee aina MapModel (`SymPath.DrawDashedCore`), ja se mittaa tasoitettua polkua (`GetFlattenedPoints`). Viiva jaetaan osiin OCADin kulmapisteistä (y & 1) ja katkopisteistä (y & 8). Katkot sovitetaan kuhunkin osaan erikseen, ja vierekkäisten osien päätykatkot yhdistyvät.
  - Kaksoisviivan reunojen katkot mitataan keskiviivalta ja siirretään sivuun yksitellen (`OffsetPointsRight`).
  - Katkot, viivan lyhennys ja glyyfien paikat viivan varrella mitataan kartan metriikalla (`MapDistanceMetric`). Se on OCADin oma (`BizzarroDistance`: suurempi koordinaattiero ja puolet pienemmästä), ellei kartta käytä euklidista metriikkaa (`UseEuclideanMetric`: Mapper-kartat sekä OCAD 11:n ja uudemmat kartat, joiden ViewPar-parametrin `p` on 0).
  - Koodissa on erikseen OCADin ja Mapperin katkojen sovitus (`SpacingMethod.OCAD` ja `OpenMapperDashes`).
  - Glyyfin lisämuunnos (`extraTransform`), esimerkiksi venytys kulmissa ja pienenevät glyyfit, muuntaa valmiiksi pisteet ja paikat mutta ei viivanleveyksiä, joten se ei ole viivatun muodon affiininen sijoitus.
  - Peiton vertailun tapaukset, joissa välit ovat pikseliä lyhyempiä, piirtävät kartat dashline, isomlines ja doublelines9 (tiketti 007).
- **Katkojen sijoittelu käytössä (@Pauli, 18.11.):** Purple Pen on sijoittanut esimerkiksi ajouraviivojen katkot havaittavasti huonommin kuin OCAD ja Mapper. Se on ilmeisesti yhdistänyt viivan päättäviä osia niin, että jäljelle on jäänyt pitkä katkoton pätkä. Mapperin päätösegmenttien malli poikkeaa ilmeisesti OCADin mallista.
- **Avoimia kysymyksiä:**
  1. Katkoviivan perusmallin rajat. Ehdotus: perusmalli on katkoviiva, jota ei siirretä sivuun ja joka mitataan euklidisella metriikalla, eli pääviiva ja kaksoisviivan täyttö. Sen katkojen ja välien pituudet ovat ne, jotka `ComputeDashDistances` antaa. Osiin pilkotaan OCADin metriikalla mitatut viivat ja kaksoisviivan reunat. Standardiväline mittaa käyrän pituutta, mutta Purple Pen mittaa tasoitetun polun pituutta, joten käyrällä katkot voivat siirtyä hieman. Vaihe 2 ratkaisee, piirretäänkö perusmalli piirtokohteen katkoviivana vai laajennetaanko se katkoiksi.
  2. Glyyfit viivan varrella. Viivan glyyfeillä ei ole symbolin aukkoja (`DrawGlyph` antaa aukoiksi `null`). Glyyfi ilman lisämuunnosta voi jakaa muotonsa kuten pistesymbolin glyyfi tiketissä 008, ja lisämuunnettu glyyfi tarvitsee oman muotonsa.
  3. Katkojen sijoittelu. Jos @Paulin havainto vahvistuu OCADia ja Mapperia vasten ja vika korjataan, korjaus on oma bug-tikettinsä ja fix-haaransa. Kun geometriametodit käyttävät `ComputeDashDistances`ia muuttamattomana, korjaus muuttaa piirron ja ilmentymät yhdessä paikassa.
- **Rajaus (luonnos):** alueiden reunat piirretään viivasymboleilla, ja alueet ovat tiketti [012](012-area-geometry.md). Viivan varren tekstit (`LineTextSymbol`) ovat tiketti [013](013-text-geometry.md). Vaihe 2, joka piirtää katkoviivan perusmallin, on tiketti [010](010-palette-composition.md).
- **Haara (ehdotus):** `feature/line-geometry` juurtuu kokoavaan haaraan `feature/palette-layers` tiketin 008 jälkeen. Täysi ajo tehdään basen päällä, ja ilman `--collect`-valintaa, kun muutos ei koske muuta koodia, jota piirto ajaa.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
