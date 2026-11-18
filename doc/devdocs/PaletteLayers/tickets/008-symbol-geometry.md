# LUONNOS – Vaihe 1: geometriametodien perusrakenne ja pistesymbolit

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus (luonnos)**
Symbolit tuottavat leikkausobjektinsa omilla geometriametodeillaan, ja vaihe 1 käyttää niitä kerääjän tilalla. Kerääjän ilmentymät ovat oraakkeli. Työ on neljä osaa. Tämä ensimmäinen osa tekee geometriametodien perusrakenteen ja pistesymbolien geometriametodit. Osat [011](011-line-geometry.md), [012](012-area-geometry.md) ja [013](013-text-geometry.md) tekevät viivojen, alueiden ja tekstien geometriametodit.

**Päätökset (sitovia)**
- Kerääjä korvataan symbolien omilla geometriametodeilla ennen kuviotikettiä, ja tämä tiketti tehdään tiketin [007](007-map-instances.md) jälkeen (@Pauli, 18.11.).
- Kerääjän ilmentymät ovat geometriametodien oraakkeli (@Pauli, 18.11.).
- Vanha piirtokoodi pysyy tämän työn ajan muuttumattomana, jotta kerääjä kerää sen kutsut ja pysyy geometriametodien riippumattomana oraakkelina. Geometriametodit kirjoitetaan sen rinnalle, ja symbolien kokoava logiikka saa olla kahdessa paikassa (@Pauli, 18.11.).
- Työ jaetaan neljään osaan symbolilajeittain: perusrakenne ja pistesymbolit (tämä tiketti), viivat ([011](011-line-geometry.md)), alueet, yhdistelmät ja grafiikkaobjektit ([012](012-area-geometry.md)) sekä tekstit ja suorakulmiot ([013](013-text-geometry.md)) (@Pauli, 18.11.).
- Piirto ei käytä symboleita, vaan symboleita käyttää vain vaihe 1. Ilmentymät kuuluvat karttaan piirrosta riippumatta (@Pauli, 18.11., epic [000](000-palette-layers.md)).
- Kun muutos ei voi muuttaa piirtoa, esimerkiksi koska se ei koske koodia, jota piirto ajaa, täysi ajo tehdään komennolla `tools/push-wip.sh` ilman `--collect`-valintaa, sillä kuvat pysyvät silloin ennallaan (@Pauli, 18.11.).
- Katkoviivan päätökset ovat viivojen tiketissä [011](011-line-geometry.md).

**Hyväksymiskriteerit (luonnos: ehdotuksia, ei sitovia)**
- [ ] Ehdotus: geometriametodien rajapinta: symbolilaji, jolla on geometriametodi, tuottaa symbolin leikkausobjektit palettimerkinnöittäin ilman `IGraphicsTarget`-kutsuja ja ilman piirtoasetuksia (`RenderOptions`).
- [ ] Ehdotus: pistesymbolilla (`PointSymDef` ja sen glyyfi) on geometriametodi. Se tuottaa glyyfin viivat, alueet, ympyrät ja täytetyt ympyrät, aukkojen tekemät kaaret sekä origossa olevan yhden ympyrän glyyfin symbolin paikassa ilman muunnosta.
- [ ] Ehdotus: vaihe 1 tekee ilmentymät geometriametodeilla niille symbolilajeille, joilla geometriametodi on, ja kerääjällä muille.
- [ ] Ehdotus: kerääjä jää testien oraakkeliksi. Testit vertaavat testikartoilla geometriametodien ilmentymiä kerääjän ilmentymiin rakenteena: samat palettimerkinnät ja samat leikkausobjektit samassa järjestyksessä (laji, muoto, viiva, täyttösääntö ja sijoituksen vaiheet), tai tiketti kertoo sallitun eron ja sen syyn. Myöhemmät osat käyttävät samaa vertailua.
- [ ] Ehdotus: `LayerInstanceTests` ja `LayerInstanceCoverageTests` menevät läpi.
- [ ] Ehdotus: piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot (luonnos)**
- **Havaintoja nykyisestä koodista** (tiketit 005 ja 007; SymDef.cs, Symbol.cs ja Glyph.cs, noin 7 500 riviä, sekä SymPath.cs, noin 2 100 riviä):
  - `PointSymDef` piirtää glyyfinsä (`Glyph`), jonka osat ovat viivoja, alueita, ympyröitä ja täytettyjä ympyröitä. Yhden ympyrän glyyfi origossa piirretään suoraan symbolin paikkaan. Aukot (`gaps`) tekevät ympyrästä kaaria.
  - Glyyfin paikka ja kierto ovat muunnospinossa (`PushTransform`). Lisämuunnos (`extraTransform`), esimerkiksi viivan glyyfin venytys kulmissa ja pienenevät glyyfit, muuntaa valmiiksi pisteet ja paikat mutta ei viivanleveyksiä.
  - Kerääjän leikkausobjektit ovat muotoja (polku, ellipsi, kaari, tekstiajo), täytettyinä tai viivattuina, ja niiden sijoitus pitää muunnosten ketjun (`LayerInstances.cs`). Kerääjä tekee jokaisesta piirtokutsusta uuden muodon.
  - Täsmällinen peiton vertailu vaatii samat sijoituksen vaiheet samassa järjestyksessä, koska Skia kokoaa sisäkkäiset muunnokset omalla pyöristyksellään (tiketti 005).
  - Symbolien piirtokoodia kutsuvat vain `Map.DrawColor` ja vaihe 1 kerääjän kautta. Korostukset käyttävät osin samaa koodia (`LineSymDef.DrawHighlight` kutsuu `DrawCore`a).
  - Vaiheen 1 piirtokoodi lukee piirtoasetuksista vain resoluution, jota katkoviivat käyttävät, ja asetuksen `omitHatchingsAndPatterns` (tiketti 007). Vaihe 1 valitsee symbolit väri kerrallaan (`SelectVisibleSymbolsOfColor`), joten sama symboli piirretään kerran jokaista väriään kohden.
  - Kerääjä on `internal`, ja MapModelin `InternalsVisibleTo` nimeää vain Test_MapModelin. Map_Skia.Tests näkee vain julkiset jäsenet.
- **Avoimia kysymyksiä:**
  1. Oraakkelin vertailu. Ehdotus: rakenne ensin (kriteerit), ja peitto siellä, missä esitys muuttuu.
  2. Geometriametodin muoto. Ehdotus: symbolikohtainen metodi tuottaa kaikki symbolin palettimerkinnät yhdellä läpikäynnillä, ja leikkausobjektit ovat kunkin palettimerkinnän sisällä samassa järjestyksessä kuin nyt.
  3. Leikkausobjektin alkuperä. Ilmentymien päivitys kartan muuttuessa kuuluu vaiheen 2 tikettiin (päätös 18.11.). Symbolikohtainen geometriametodi tekee alkuperän kirjaamisesta helppoa.
  4. Glyyfin muodon uudelleenkäyttö. Jaettu muoto, jonka sijoitukset asettavat, vastaa speksin luvun 04 uudelleenkäytettäviä määrittelyjä, ja se sopii glyyfeille ilman lisämuunnosta. Pistesymboli ei käytä lisämuunnosta. Rakenteellinen vertailu vertaa silloin muotoja arvoina, koska kerääjä tekee jokaiselle sijoitukselle uuden muodon.
  5. Kerääjän paikka. Ehdotus: kerääjä jää MapModeliin testien oraakkeliksi, ja `omitHatchingsAndPatterns` jää sen mukana, koska kerääjä ei tunne kuvioita. Map_Skia.Tests näkee kerääjän, kun `InternalsVisibleTo` nimeää myös sen.
- **Rajaus (luonnos):** viivat, alueet ja tekstit ovat tiketit [011](011-line-geometry.md), [012](012-area-geometry.md) ja [013](013-text-geometry.md). Viivoitukset ja kuviot ovat tiketti [009](009-pattern-instances.md), ja vaihe 2 on tiketti [010](010-palette-composition.md).
- **Haara (ehdotus):** `feature/symbol-geometry` juurtuu kokoavaan haaraan `feature/palette-layers`. Täysi ajo tehdään basen päällä. Vanha piirtokoodi pysyy ennallaan (päätös 18.11.), joten täysi ajo tehdään ilman `--collect`-valintaa, kun muutos ei koske muuta koodia, jota piirto ajaa.

**Erotetut alatehtävät**
- [011](011-line-geometry.md) Viivojen geometriametodit, erotettu tästä tiketistä (@Pauli, 18.11.): haara `feature/line-geometry` (ehdotus), ei vielä tehty. Se juurtuu kokoavaan haaraan tämän tiketin jälkeen, eikä sitä yhdistetä tämän tiketin haaraan.
- [012](012-area-geometry.md) Alueiden, yhdistelmien ja grafiikkaobjektien geometriametodit, erotettu tästä tiketistä (@Pauli, 18.11.): haara `feature/area-geometry` (ehdotus), ei vielä tehty. Se juurtuu kokoavaan haaraan tiketin 011 jälkeen, eikä sitä yhdistetä tämän tiketin haaraan.
- [013](013-text-geometry.md) Tekstien ja suorakulmioiden geometriametodit, erotettu tästä tiketistä (@Pauli, 18.11.): haara `feature/text-geometry` (ehdotus), ei vielä tehty. Se juurtuu kokoavaan haaraan tiketin 012 jälkeen, eikä sitä yhdistetä tämän tiketin haaraan.
