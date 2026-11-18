# LUONNOS – Vaihe 1: symbolien omat geometriametodit kerääjän tilalle

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus (luonnos)**
Symbolit tuottavat leikkausobjektinsa omilla geometriametodeillaan, ja vaihe 1 käyttää niitä kerääjän tilalla. Kerääjän ilmentymät ovat oraakkeli.

**Päätökset (sitovia)**
- Kerääjä korvataan symbolien omilla geometriametodeilla ennen kuviotikettiä, ja tämä tiketti tehdään tiketin [007](007-map-instances.md) jälkeen (@Pauli, 18.11.).
- Kerääjän ilmentymät ovat geometriametodien oraakkeli (@Pauli, 18.11.).
- Vanha piirtokoodi pysyy tämän tiketin ajan muuttumattomana, jotta kerääjä kerää sen kutsut ja pysyy geometriametodien riippumattomana oraakkelina. Geometriametodit kirjoitetaan sen rinnalle, ja symbolien kokoava logiikka saa olla kahdessa paikassa (@Pauli, 18.11.).
- Katkoviivan esityksen oikea ratkaisu etsitään tässä tiketissä (@Pauli, 18.11.).
- Katkoviiva noudattaa samaa periaatetta kuin kuviotäytöt: perusmalli esitetään standardivälineellä, katkoviivana, ja eksoottisemmat tapaukset, kuten OCADin metriikka, pilkotaan osiin, katkoviivassa yksittäisiksi katkoiksi. Kuviotäytöissä vastaavia tapauksia ovat stokastinen täyttö ja erikoiset reunavalinnat (@Pauli, 18.11.).
- Piirto ei käytä symboleita, vaan symboleita käyttää vain vaihe 1. Ilmentymät kuuluvat karttaan piirrosta riippumatta (@Pauli, 18.11., epic [000](000-palette-layers.md)).
- Kun muutos ei voi muuttaa piirtoa, esimerkiksi koska se ei koske koodia, jota piirto ajaa, täysi ajo tehdään komennolla `tools/push-wip.sh` ilman `--collect`-valintaa, sillä kuvat pysyvät silloin ennallaan (@Pauli, 18.11.).

**Hyväksymiskriteerit (luonnos: ehdotuksia, ei sitovia)**
- [ ] Ehdotus: jokaisella symbolilajilla, joka tuottaa yhtenäistä geometriaa, on geometriametodi, joka tuottaa symbolin leikkausobjektit palettimerkinnöittäin ilman `IGraphicsTarget`-kutsuja ja ilman piirtoasetuksia (`RenderOptions`).
- [ ] Ehdotus: vaihe 1 tuottaa ilmentymät geometriametodeilla, eikä se käytä kerääjää. Kerääjä jää testien oraakkeliksi.
- [ ] Ehdotus: testikartoilla geometriametodien ilmentymät ovat samat kuin kerääjän ilmentymät: samat palettimerkinnät ja samat leikkausobjektit samassa järjestyksessä (laji, muoto, viiva, täyttösääntö ja sijoituksen vaiheet). Katkoviivan perusmalli verrataan katkoiksi laajennettuna, tai tiketti kertoo sallitun eron ja sen syyn.
- [ ] Ehdotus: katkoviivan perusmalli on yksi viivattu muoto, jolla on katkojen ja välien pituudet, ja osiin pilkottu katkoviiva on yksittäisiä katkoja.
- [ ] Ehdotus: `LayerInstanceTests` ja `LayerInstanceCoverageTests` menevät läpi, kun vaihe 1 käyttää geometriametodeja, ja rakennetestit tarkistavat ilman piirtoa katkoviivan perusmallin ja pilkotun katkoviivan.
- [ ] Ehdotus: piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot (luonnos)**
- **Havaintoja nykyisestä koodista** (tiketti 005; SymDef.cs, Symbol.cs ja Glyph.cs, noin 7 500 riviä):
  - `PointSymDef` piirtää glyyfinsä (`Glyph`), jonka osat ovat viivoja, alueita, ympyröitä ja täytettyjä ympyröitä. Yhden ympyrän glyyfi origossa piirretään suoraan symbolin paikkaan. Aukot (`gaps`) tekevät ympyrästä kaaria.
  - `LineSymDef.DrawCore` piirtää pääviivan, katkoviivat (`ComputeDashDistances` ja `SymPath.DrawDashedCore`), terävät päät, toisen viivan, kaksoisviivan täytön ja reunat (`DrawDashedWithOffset`) sekä glyyfit viivan varrella (`GlyphLocation`: päät, kulmat, katkopisteet, välien ja katkojen keskikohdat, välimatkat ja pienenevät glyyfit).
  - `AreaSymDef` täyttää alueen ja piirtää reunan reunasymbolillaan. Viivoitukset ja kuviot ovat tiketti [009](009-pattern-instances.md). `AreaComboSymDef` ja `LineComboSymDef` kokoavat osiaan.
  - `TextSymDef.Draw` piirtää tekstin sanoittain tai merkeittäin. Kehysviiva piirtää tekstin ja sen ääriviivat usealla kynällä, varjo siirretyn tekstin, ja lisäksi on alleviivaukset, keskipiste ja kehyssuorakulmio. `DrawTextOnPath` sijoittaa merkit polun varrelle.
  - `RectangleSymDef` piirtää ääriviivan, ruudukon viivat ja ruudukon tekstit. Grafiikkasymbolit (`GraphicsAreaSymbol` ja `GraphicsLineSymbol`) piirtävät oman värinsä. Kuva- ja asettelukerroksen symbolit eivät kuulu ilmentymiin.
  - Kerääjän leikkausobjektit ovat muotoja (polku, ellipsi, kaari, tekstiajo), täytettyinä tai viivattuina, ja niiden sijoitus pitää muunnosten ketjun (`LayerInstances.cs`).
  - SymPath.cs (noin 2 100 riviä) sisältää katkojen, sivuun siirron ja tasoituksen geometrian (`DrawDashedCore`, `OffsetRight` ja `GetFlattenedPoints`).
  - Glyyfin paikka ja kierto ovat muunnospinossa (`PushTransform`). Lisämuunnos (`extraTransform`), esimerkiksi venytys kulmissa ja pienenevät glyyfit, muuntaa valmiiksi pisteet ja paikat mutta ei viivanleveyksiä.
  - Täsmällinen peiton vertailu vaatii samat sijoituksen vaiheet samassa järjestyksessä, koska Skia kokoaa sisäkkäiset muunnokset omalla pyöristyksellään (tiketti 005).
  - `IGraphicsTarget`issa ei ole katkoviivoja: katkot laskee aina MapModel (`SymPath.DrawDashedCore`). Viiva jaetaan osiin OCADin kulmapisteistä (y & 1) ja katkopisteistä (y & 8). Katkot sovitetaan kuhunkin osaan erikseen, ja vierekkäisten osien päätykatkot yhdistyvät. Kaksoisviivan reunojen katkot mitataan keskiviivalta ja siirretään sivuun yksitellen (`OffsetPointsRight`). Metriikka on OCADin oma (`BizzarroDistance`: suurempi koordinaattiero ja puolet pienemmästä), ellei kartta käytä euklidista metriikkaa (`UseEuclideanMetric`: Mapper-kartat sekä OCAD 11:n ja uudemmat kartat, joiden ViewPar-parametrin `p` on 0).
  - Symbolien piirtokoodia kutsuvat vain `Map.DrawColor` ja vaihe 1 kerääjän kautta. Korostukset käyttävät osin samaa koodia (`LineSymDef.DrawHighlight` kutsuu `DrawCore`a).
  - Vaiheen 1 piirtokoodi lukee piirtoasetuksista vain resoluution, jota katkoviivat käyttävät, ja asetuksen `omitHatchingsAndPatterns` (tiketti 007). Vaihe 1 valitsee symbolit väri kerrallaan (`SelectVisibleSymbolsOfColor`), joten sama symboli piirretään kerran jokaista väriään kohden.
  - Kerääjä on `internal`, ja MapModelin `InternalsVisibleTo` nimeää vain Test_MapModelin. Map_Skia.Tests näkee vain julkiset jäsenet.
- **Katkojen sijoittelu käytössä (@Pauli, 18.11.):** Purple Pen on sijoittanut esimerkiksi ajouraviivojen katkot havaittavasti huonommin kuin OCAD ja Mapper. Se on ilmeisesti yhdistänyt viivan päättäviä osia niin, että jäljelle on jäänyt pitkä katkoton pätkä. Mapperin päätösegmenttien malli poikkeaa ilmeisesti OCADin mallista. Koodissa on erikseen OCADin ja Mapperin katkojen sovitus (`SpacingMethod.OCAD` ja `OpenMapperDashes`).
- **Avoimia kysymyksiä:**
  1. Katkoviivan perusmallin rajat. Ehdotus: perusmalli on katkoviiva, jota ei siirretä sivuun ja joka mitataan euklidisella metriikalla, eli pääviiva ja kaksoisviivan täyttö. Sen katkojen ja välien pituudet ovat ne, jotka `ComputeDashDistances` antaa. Osiin pilkotaan OCADin metriikalla mitatut viivat ja kaksoisviivan reunat. Standardiväline mittaa käyrän pituutta, mutta Purple Pen mittaa tasoitetun polun pituutta, joten käyrällä katkot voivat siirtyä hieman. Vaihe 2 ratkaisee, piirretäänkö perusmalli piirtokohteen katkoviivana vai laajennetaanko se katkoiksi.
  2. Oraakkelin vertailu. Ehdotus: rakenne ensin (kriteerit), ja peitto siellä, missä esitys muuttuu.
  3. Leikkausobjektin alkuperä. Ilmentymien päivitys kartan muuttuessa kuuluu vaiheen 2 tikettiin (päätös 18.11.). Symbolikohtainen geometriametodi tekee alkuperän kirjaamisesta helppoa.
  4. Glyyfin muodon uudelleenkäyttö. Kerääjässä jokainen sijoitus tekee uuden muodon. Jaettu muoto sopii glyyfeille ilman lisämuunnosta. Lisämuunnettu glyyfi tarvitsee oman muotonsa, koska lisämuunnos ei muunna viivanleveyksiä eikä ole siksi viivatun muodon affiininen sijoitus.
  5. Tekstin rakenne. Riittävätkö tekstiajot (sana tai grafeemi), vai tarvitaanko tekstisymbolin rakennetta enemmän?
  6. Kerääjän paikka. Ehdotus: kerääjä jää MapModeliin testien oraakkeliksi, ja `omitHatchingsAndPatterns` jää sen mukana, koska kerääjä ei tunne kuvioita. Map_Skia.Tests näkee kerääjän, kun `InternalsVisibleTo` nimeää myös sen.
  7. Jako osiin. Ehdotus: perusrakenne ja pistesymbolit, viivat, alueet ja yhdistelmät, tekstit sekä suorakulmiot ja grafiikkaobjektit. Vaiheessa 1 kerääjä tekee niiden symbolilajien ilmentymät, joiden geometriametodit puuttuvat vielä.
  8. Katkojen sijoittelu. Jos @Paulin havainto vahvistuu OCADia ja Mapperia vasten ja vika korjataan, korjaus on oma bug-tikettinsä ja fix-haaransa. Kun geometriametodit käyttävät `ComputeDashDistances`ia muuttamattomana, korjaus muuttaa piirron ja ilmentymät yhdessä paikassa.
- **Rajaus (luonnos):** viivoitukset ja kuviot ovat tiketti [009](009-pattern-instances.md), ja vaihe 2 on tiketti [010](010-palette-composition.md).
- **Haara (ehdotus):** `feature/symbol-geometry` juurtuu kokoavaan haaraan `feature/palette-layers`. Täysi ajo tehdään basen päällä. Vanha piirtokoodi pysyy ennallaan (päätös 18.11.), joten täysi ajo tehdään ilman `--collect`-valintaa, kun muutos ei koske muuta koodia, jota piirto ajaa.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
