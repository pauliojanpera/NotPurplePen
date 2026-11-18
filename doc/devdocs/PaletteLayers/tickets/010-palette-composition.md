# LUONNOS – Vaihe 2: koostaminen palettimerkinnöittäin

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS. Tiketti jakautunee useaksi tiketiksi (ks. avoimet kysymykset).

**Kuvaus (luonnos)**
Vaihe 2 piirtää kartan palettimerkinnät ilmentymistä: se ryhmittelee ilmentymät palettimerkinnöittäin ja koostaa jokaisen ryhmän kerran palettimerkinnän värillä C, peittävyydellä α ja sekoitustilalla B palettijärjestyksessä (speksin luku 05).

**Päätökset (sitovia)**
- Vaihe 2 korvaa palettimerkintöjen suoran piirron, ja normaali näkymä on vaiheen 2 piirtotila (@Pauli, 18.11.).
- Piirto ei käytä symboleita, ja ilmentymät kuuluvat karttaan piirrosta riippumatta (@Pauli, 18.11.).
- Palettimerkinnän unioni saa jäädä piirrossa implisiittiseksi, mutta ilmentymät eivät saa jäädä pois (speksin luku 05, @Pauli, 18.11.). feature/multiply-blending on vain ulkoasun oraakkeli, eikä sen tapa sekoittaa muste kerroksena symbolien piirron ympärillä ole vaiheen 2 malli. Työ ei lue sen koodia (@Pauli, 18.11.).
- OCAD ja Mapper ovat omien karttojensa piirron auktoriteetit (@Pauli, 17.11.).
- Ryhmän koostamista tarvitsevat Skia, PDF, tulostus ja tuleva SVG-toteutus. GDI+ ja WPF ovat legacya, eivätkä Direct2D ja iOS ole käytössä (@Pauli, 18.11.).
- Täydet ajot tehdään basen päällä, ja palettikerroshaarat saavat olla ristiriidassa devin kanssa (@Pauli, 18.11.).

**Hyväksymiskriteerit (luonnos: ehdotuksia, ei sitovia)**
Rakenteelliset ehdotukset tulevat Progress.md:n kohdan "Next" esimerkeistä.
- [ ] Ehdotus: vaihe 2 lukee vain ilmentymiä, eikä se kutsu symbolien koodia.
- [ ] Ehdotus: piirtokohde saa vain koostettuja ryhmiä: jokaisen palettimerkinnän ryhmä koostetaan kerran C:llä, α:lla ja B:llä palettijärjestyksessä.
- [ ] Ehdotus: testi tarkistaa speksin kuvan 6 tilanteen: palettimerkinnän kahden ilmentymän päällekkäinen osa saa α:n vain kerran.
- [ ] Ehdotus: kun α on 1 ja B on normaali, piirto on sama kuin nyt, tai tiketti kertoo sallitun eron ja sen syyn.
- [ ] Ehdotus: kartat, joiden väreillä α on alle 1 tai B ei ole normaali, muuttuvat. Niiden baselinet siirtyvät vain @Paulin luvalla.

**Lisätiedot (luonnos)**
- **Epicin mukaan tämä tiketti ratkaisee** (muistiinpano "Vaiheen 2 tiketti ratkaisee", 18.11.):
  - ryhmän koostamisen, jota `IGraphicsTarget`-rajapinnassa ei vielä ole
  - pohjakarttojen, kuvakerroksen ja asettelukerroksen piirron palettipinon rinnalla
  - peiton vertailun uuden perustan, koska `LayerInstanceCoverageTests` vertaa nyt suoraan piirtoon.
- **Valintojen korostukset** jäävät epicin mukaan auki, eivätkä ne estä vaihetta 2.
- **Havaintoja nykyisestä koodista:**
  - `Map.Draw` piirtää ensin kartan alle kuuluvat pohjakartat, sitten kuvakerroksen, värit alimmasta ylimpään, asettelukerroksen ja kartan päälle kuuluvat pohjakartat. Kun `blendOverprintedColors` on päällä, `DrawColor` kytkee päällepainetulle värille Darken-sekoituksen (`PushBlending`). Se on rajapinnan ainoa sekoitus.
  - `MapDisplay` piirtää kartan ja ratakartan saman `Map.Draw`n kautta näyttöön, tulostukseen ja PDF:ään. Ratakartta piirretään aina sekoituksen kanssa. Ratojen alempi purppura jakaa kartan piirron OCAD-värin kohdalta (`lowerPurpleMapLayer`).
  - AvPurplePenin tulostus rasteroi sivut Skialla Windowsissa ja tulostaa PDF:n Linuxissa ja macOS:ssä.
  - Esimerkkikarttoja: LordHillin väri "Purple" on `t80` (tiketti [004](004-opacity-and-blend-mode.md)), ja `wholestructure.ocd` sisältää värin, jonka B on Darken.
- **Oraakkelit:** OCAD-kartalla B = Darken on kunkin musteen suurempi osuus eikä RGB-minimi, ja OCAD soveltaa B:tä vain PDF-viennissään ([oracles/ocad-darken.md](../oracles/ocad-darken.md)). Mapper soveltaa α:n jokaiseen muotoon, joten se ei ole α:n oraakkeli, kun saman värin muodot ovat päällekkäin ([oracles/mapper-opacity.md](../oracles/mapper-opacity.md)).
- **Avoimia kysymyksiä:**
  1. Miten päällepainatuksen simulointi (`blendOverprintedColors`) toimii, kunnes piirtotilat ovat valmiita?
  2. Tarvitaanko piirrossa katkoviivojen yksinkertaistusta, kun välit ovat pikseliä pienempiä?
  3. Mitkä baselinet siirtyvät, kun α ja B tulevat käyttöön, ja mitkä muut erot syntyvät kerroksittaisesta koostamisesta?
  4. Miten ilmentymät pysyvät ajan tasalla kartan muuttuessa? Vaihtoehtoja ovat säilytys kartassa ja mitätöinti muutoksessa sekä uusi teko jokaista piirtoa varten.
  5. Miten piirto rajautuu suorakulmioon leikkausobjektien rajojen avulla?
  6. Millainen ryhmän koostaminen tulee rajapintaan (`IGraphicsTarget`), ja miten Skia (kerros), PDF (läpinäkyvyysryhmä) ja tulostus toteuttavat sen?
  7. Mitä B = Darken tarkoittaa normaalissa näkymässä: RGB-kanavittain vai musteittain? Kuuluuko jälkimmäinen piirtotiloihin?
  8. Jaetaanko työ esimerkiksi kolmeen tikettiin: ryhmän koostaminen piirtokohteisiin, `Map.Draw` ilmentymistä sekä ilmentymien säilytys ja päivitys?
- **Rajaus (luonnos):** piirtotilat (päällepainatuksen simulointi ja separaatiot, luvut 06 ja 07) ja yksi palettipino kartalle ja radoille ovat omat tikettinsä. SVG-koodauksen paikka päätetään, kun vaihe 2 on valmis (epic).
- **Haara (ehdotus):** `feature/palette-composition` juurtuu kokoavaan haaraan `feature/palette-layers`. Täysi ajo tehdään basen päällä.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
