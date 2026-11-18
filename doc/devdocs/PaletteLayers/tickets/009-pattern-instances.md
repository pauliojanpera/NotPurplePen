# LUONNOS – Vaihe 1: viivoitukset ja aluekuviot toistuvana geometriana

> **LUONNOS (18.11.), ei sitova.** Tämä tiketti on luonnos. Sen kuvaus, ehdotetut kriteerit, havainnot ja kysymykset ovat lähtökohtia, eivät vaatimuksia eivätkä sääntöjä. Sitovia ovat vain kohdan "Päätökset" @Paulin päätökset päiväyksineen. Tiketin viimeistelevä sessio tarkistaa jokaisen kohdan koodia ja speksiä vasten, päättää kriteerit @Paulin kanssa ja poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus (luonnos)**
Viivoitukset ja aluekuviot ovat ilmentymissä toistuvan geometrian prototyyppejä, joilla on hilan ja elementtien muunnokset (speksin luku 03). Leikatun täytön leikkausobjektit ovat alueet, ja täyttötilan valitsemat kokonaiset elementit ovat omia leikkausobjektejaan.

**Päätökset (sitovia)**
- Ennen tätä tikettiä vaihe 1 irrotetaan piirrosta (tiketti [007](007-map-instances.md)), ja kerääjä korvataan symbolien omilla geometriametodeilla (tiketit [008](008-symbol-geometry.md), [011](011-line-geometry.md), [012](012-area-geometry.md) ja [013](013-text-geometry.md)) (@Pauli, 18.11.).
- Piirto ei käytä symboleita, ja ilmentymät kuuluvat karttaan piirrosta riippumatta (@Pauli, 18.11.).
- OCAD ja Mapper ovat omien karttojensa piirron auktoriteetit, ja Mapper-karttojen ulkoasun oraakkeli on Mapper (@Pauli, 17.11.). feature/multiply-blending on vain ulkoasun oraakkeli, eikä työ lue sen koodia (@Pauli, 18.11.).
- Kun muutos ei voi muuttaa piirtoa, esimerkiksi koska se ei koske koodia, jota piirto ajaa, täysi ajo tehdään komennolla `tools/push-wip.sh` ilman `--collect`-valintaa, sillä kuvat pysyvät silloin ennallaan (@Pauli, 18.11.).

**Hyväksymiskriteerit (luonnos: ehdotuksia, ei sitovia)**
- [ ] Ehdotus: geometriaprototyypillä on toistuvan geometrian tapaukset: viivoitus (viivan leveys, väli ja siirtymä) ja kuvio (elementin glyyfi, hilan leveys ja korkeus sekä rivien siirto).
- [ ] Ehdotus: ilmentymällä on hilan muunnos L ja elementtien muunnos E siten, että elementin piste e hilavektorissa v on kohdassa L(v) + E(e) (luku 03).
- [ ] Ehdotus: leikatun täytön leikkausobjektit ovat alueiden ääriviivat, ja samanlaiset kokoonpanot jakavat ilmentymän.
- [ ] Ehdotus: täyttötilojen `CompletelyInside`, `CenterInside` ja `PartiallyInside` valitsemat elementit ovat leikkausobjekteja, ja samoin epäsäännöllisten kuvioiden elementit.
- [ ] Ehdotus: sisäinen asetus `RenderOptions.omitHatchingsAndPatterns` poistuu, jos tiketti 008 ei ole jo poistanut sitä.
- [ ] Ehdotus: testikartoilla viivoitusten ja kuvioiden ilmentymien peitto on sama kuin nykyisen piirron peitto glyyfi kerrallaan (`usePatternBitmaps` pois päältä), tai tiketti kertoo sallitun eron ja sen syyn.
- [ ] Ehdotus: piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot (luonnos)**
- **Havaintoja nykyisestä koodista** (SymDef.cs):
  - `HatchInfo` sisältää värin, leveyden, välin, siirtymän, kulman ja kiertotilan (`RotateMode`: `Always`, `ManualOnly` ja `Never`). `DrawOneHatching` piirtää viivat muunnoksessa, joka siirtää kiertokeskukseen ja kiertää alueen kulmalla ja viivoituksen kulmalla. Tila `Never` ohittaa alueen kulman ja kiertokeskuksen.
  - `PatternInfo` sisältää glyyfin, hilan leveyden ja korkeuden, rivien siirron, kuvion kulman, siirtymän, kiertotilan, täyttötilan (`PatternFillMode`: `Clip`, `CompletelyInside`, `CenterInside` ja `PartiallyInside`) sekä epäsäännöllisyyden. `DrawPattern` siirtää kiertokeskukseen, kiertää alueen kulmalla ja kuvion kulmalla ja siirtää kuvion siirtymällä. `DrawPatternRows` piirtää glyyfit hilan pisteisiin kulmalla miinus kuvion kulma.
  - `HandleIrregularity` siirtää epäsäännöllisen kuvion elementtejä sattumanvaraisesti mutta deterministisesti, ja siirron siemen tulee elementin pyöristetystä paikasta.
  - Kuviosivellin (`CreatePatternBrush`) on käytössä, kun `usePatternBitmaps` on päällä ja kuvio on säännöllinen ja leikattu eikä liian karkea. Muuten glyyfit piirretään yksitellen.
  - Speksin luvun 03 mukaan OCADissa symbolin rakennekulma kiertää vain hilaa, ja alueobjektin kulma kiertää hilaa ja elementtejä yhdessä.
- **Havainto: päällekkäiset alueet.** basewithfixesin korjaus `fix/overlapping-patterns`, joka ei kuulu tämän työn pohjaan, piirtää saman symbolin päällekkäisten tai vierekkäisten, saman kehyksen alueiden viivoitukset ja leikatut kuviot kerran niiden unioniin. Mallissa sama voi seurata siitä, että samanlaiset kokoonpanot jakavat ilmentymän ja leikkausobjektit yhdistyvät unioniksi. Silloin prototyyppi piirretään kerran unioniin, ja unionin on oltava aito unioni, koska alueet täyttyvät parillisuussäännöllä. Tiketti ratkaisee, miten tämä toteutuu.
- **Avoimia kysymyksiä:**
  1. Mikä on samanlainen kokoonpano? Sama palettimerkintä, prototyyppi, L ja E, ja alueen mukana kääntyvillä kuvioilla myös sama kulma ja kiertokeskus?
  2. Mihin ilmentymään valitut elementit kuuluvat: palettimerkinnän G = 1 -ilmentymään sijoituksineen vai omaan ilmentymäänsä elementin muunnoksineen?
  3. Onko viivoitus oma prototyyppinsä (raidat) vai viivaelementin kuvio?
  4. Mitä tila `ManualOnly` merkitsee mallissa?
  5. Riittääkö täsmällinen vertailu glyyfi kerrallaan tehtyyn piirtoon, vai tarvitaanko sallittu ero esimerkiksi epäsäännöllisille kuvioille?
- **Rajaus (luonnos):** vaihe 2, joka piirtää prototyypit, on tiketti [010](010-palette-composition.md). SVG-koodaus odottaa vaiheen 2 valmistumista (epic).
- **Haara (ehdotus):** `feature/pattern-instances` juurtuu kokoavaan haaraan `feature/palette-layers`. Täysi ajo tehdään basen päällä. Piirto pysyy ennallaan, jos tiketti ei muuta viivoitusten ja kuvioiden piirtokoodia (esimerkiksi `DrawHatchings`, `DrawOneHatching`, `DrawPatterns`, `DrawPattern` ja `DrawPatternRows`). Asetuksen `omitHatchingsAndPatterns` poisto pitää piirron ennallaan, koska asetus on piirrossa aina pois päältä.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
