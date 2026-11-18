# LUONNOS – Vaihe 1: symbolien omat geometriametodit kerääjän tilalle

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus (luonnos)**
Symbolit tuottavat leikkausobjektinsa omilla geometriametodeillaan, ja vaihe 1 käyttää niitä kerääjän tilalla. Kerääjän ilmentymät ovat oraakkeli.

**Päätökset (sitovia)**
- Kerääjä korvataan symbolien omilla geometriametodeilla ennen kuviotikettiä, ja tämä tiketti tehdään tiketin [007](007-map-instances.md) jälkeen (@Pauli, 18.11.).
- Kerääjän ilmentymät ovat geometriametodien oraakkeli (@Pauli, 18.11.).
- Katkoviivan esityksen oikea ratkaisu etsitään tässä tiketissä (@Pauli, 18.11.).
- Piirto ei käytä symboleita, vaan symboleita käyttää vain vaihe 1. Ilmentymät kuuluvat karttaan piirrosta riippumatta (@Pauli, 18.11., epic [000](000-palette-layers.md)).
- Kun muutos ei voi muuttaa piirtoa, esimerkiksi koska se ei koske koodia, jota piirto ajaa, täysi ajo tehdään komennolla `tools/push-wip.sh` ilman `--collect`-valintaa, sillä kuvat pysyvät silloin ennallaan (@Pauli, 18.11.).

**Hyväksymiskriteerit (luonnos: ehdotuksia, ei sitovia)**
- [ ] Ehdotus: jokaisella symbolilajilla, joka tuottaa yhtenäistä geometriaa, on geometriametodi, joka tuottaa symbolin leikkausobjektit palettimerkinnöittäin ilman `IGraphicsTarget`-kutsuja.
- [ ] Ehdotus: vaihe 1 tuottaa ilmentymät geometriametodeilla, eikä se käytä kerääjää.
- [ ] Ehdotus: testikartoilla geometriametodien ilmentymien peitto on sama kuin kerääjän ilmentymien peitto, tai tiketti kertoo sallitun eron ja sen syyn.
- [ ] Ehdotus: rakennetestit tarkistavat geometriametodien ilmentymät ilman piirtoa, kuten tiketin [005](005-layer-instances.md) testit.
- [ ] Ehdotus: piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot (luonnos)**
- **Havaintoja nykyisestä koodista** (tiketti 005; SymDef.cs, Symbol.cs ja Glyph.cs, noin 7 500 riviä):
  - `PointSymDef` piirtää glyyfinsä (`Glyph`), jonka osat ovat viivoja, alueita, ympyröitä ja täytettyjä ympyröitä. Yhden ympyrän glyyfi origossa piirretään suoraan symbolin paikkaan. Aukot (`gaps`) tekevät ympyrästä kaaria.
  - `LineSymDef.DrawCore` piirtää pääviivan, katkoviivat (`ComputeDashDistances` ja `SymPath.DrawDashedCore`), terävät päät, toisen viivan, kaksoisviivan täytön ja reunat (`DrawDashedWithOffset`) sekä glyyfit viivan varrella (`GlyphLocation`: päät, kulmat, katkopisteet, välien ja katkojen keskikohdat, välimatkat ja pienenevät glyyfit).
  - `AreaSymDef` täyttää alueen ja piirtää reunan reunasymbolillaan. Viivoitukset ja kuviot ovat tiketti [009](009-pattern-instances.md). `AreaComboSymDef` ja `LineComboSymDef` kokoavat osiaan.
  - `TextSymDef.Draw` piirtää tekstin sanoittain tai merkeittäin. Kehysviiva piirtää tekstin ja sen ääriviivat usealla kynällä, varjo siirretyn tekstin, ja lisäksi on alleviivaukset, keskipiste ja kehyssuorakulmio. `DrawTextOnPath` sijoittaa merkit polun varrelle.
  - `RectangleSymDef` piirtää ääriviivan, ruudukon viivat ja ruudukon tekstit. Grafiikkasymbolit (`GraphicsAreaSymbol` ja `GraphicsLineSymbol`) piirtävät oman värinsä. Kuva- ja asettelukerroksen symbolit eivät kuulu ilmentymiin.
  - Kerääjän leikkausobjektit ovat muotoja (polku, ellipsi, kaari, tekstiajo), täytettyinä tai viivattuina, ja niiden sijoitus pitää muunnosten ketjun (`LayerInstances.cs`).
- **Avoimia kysymyksiä:**
  1. Katkoviivan esitys. Tunnetut yksityiskohdat ovat viivaan sovitetut katkojen pituudet, OCADin oma metriikka (`MapDistanceMetric`), tasoitetulta polulta mitatut katkot sekä kaksoisviivan katkot, jotka mitataan keskiviivalta ja siirretään sivuun.
  2. Suora piirto ennen vaihetta 2. Piirtääkö se jo geometriametodien muodot, jolloin geometria on yhdessä paikassa mutta piirto voi muuttua esitysten mukana? Vai jääkö vanha piirtokoodi rinnalle vaiheeseen 2 asti, jolloin geometria on hetken kahdessa paikassa?
  3. Oraakkelin vertailu. Verrataanko rakennetta (samat leikkausobjektit) vai peittoa (samat pikselit), jos jokin esitys, kuten katkoviiva, muuttuu?
  4. Leikkausobjektin alkuperä. Tarvitseeko leikkausobjekti tiedon symbolista, josta se tuli, kun karttaan kuuluvat ilmentymät päivitetään?
  5. Glyyfin muodon uudelleenkäyttö. Kerääjässä jokainen sijoitus kopioi muodon. Tarvitaanko jaettu muoto speksin luvun 04 uudelleenkäytettäviä määrittelyjä varten?
  6. Tekstin rakenne. Riittävätkö tekstiajot (sana tai merkki), vai tarvitaanko tekstisymbolin rakennetta enemmän?
  7. Kerääjän ja sisäisen asetuksen `RenderOptions.omitHatchingsAndPatterns` kohtalo. Poistuvatko ne tässä tiketissä vai kuviotiketissä?
  8. Jako osiin, esimerkiksi symbolilajeittain (pisteet, viivat, alueet ja tekstit).
- **Rajaus (luonnos):** viivoitukset ja kuviot ovat tiketti [009](009-pattern-instances.md), ja vaihe 2 on tiketti [010](010-palette-composition.md).
- **Haara (ehdotus):** `feature/symbol-geometry` juurtuu kokoavaan haaraan `feature/palette-layers`. Täysi ajo tehdään basen päällä. Piirto pysyy ennallaan, jos vanha piirtokoodi jää rinnalle muuttumattomana (kysymys 2).

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
