# LUONNOS – Vaihe 1: tekstien ja suorakulmioiden geometriametodit

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus (luonnos)**
Tekstisymbolit (`TextSymDef`), tekstit viivan varrella (`LineTextSymbol`) ja suorakulmiosymbolit (`RectangleSymDef`) tuottavat leikkausobjektinsa geometriametodeilla. Tämän osan jälkeen jokaisella symbolilajilla, joka tuottaa yhtenäistä geometriaa, on geometriametodi, eikä vaihe 1 käytä kerääjää. Tämä on geometriametodien neljäs osa, ja se on erotettu tiketistä [008](008-symbol-geometry.md) (@Pauli, 18.11.).

**Päätökset (sitovia)**
- Koko työlle ennen sen jakoa neljään osaan tehdyt päätökset (tiketti [008](008-symbol-geometry.md), @Pauli, 18.11.) koskevat tätäkin osaa: kerääjän ilmentymät ovat oraakkeli, vanha piirtokoodi pysyy muuttumattomana, ja ajot tehdään ilman `--collect`-valintaa, kun muutos ei voi muuttaa piirtoa.

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
  1. Tekstin rakenne. Riittävätkö tekstiajot (sana tai grafeemi), vai tarvitaanko tekstisymbolin rakennetta enemmän?
- **Rajaus (luonnos):** kerääjä on testien oraakkeli tikettien 008–013 ajan, ja asetus `omitHatchingsAndPatterns` jää sen mukana. Tiketti [009](009-pattern-instances.md) poistaa molemmat (@Pauli, 18.11.). Vaihe 2 on tiketti [010](010-palette-composition.md).
- **Haara (ehdotus):** `feature/text-geometry` juurtuu kokoavaan haaraan `feature/palette-layers` tiketin 012 jälkeen. Täysi ajo tehdään basen päällä, ja ilman `--collect`-valintaa, kun muutos ei koske muuta koodia, jota piirto ajaa.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
