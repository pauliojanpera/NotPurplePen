# Vaihe 1: geometriametodien perusrakenne ja pistesymbolit
**Kuvaus**
Pistesymbolit tuottavat leikkausobjektinsa omalla geometriametodillaan, ja vaihe 1 käyttää sitä kerääjän tilalla. Kerääjän ilmentymät ovat oraakkeli, ja vanha piirtokoodi pysyy ennallaan. Tämä on neljästä osasta ensimmäinen: se tekee geometriametodien perusrakenteen, ja osat [011](011-line-geometry.md), [012](012-area-geometry.md) ja [013](013-text-geometry.md) tekevät viivojen, alueiden ja tekstien geometriametodit.

**Hyväksymiskriteerit**
- [ ] Geometriametodien rajapinta: symbolilaji, jolla on geometriametodi, tuottaa symbolin leikkausobjektit palettimerkinnöittäin ilman `IGraphicsTarget`-kutsuja ja ilman piirtoasetuksia (`RenderOptions`).
- [ ] Pistesymbolilla (`PointSymDef` ja sen glyyfi) on geometriametodi. Se tuottaa glyyfin viivat, alueet, ympyrät ja täytetyt ympyrät, aukkojen tekemät kaaret sekä origossa olevan yhden ympyrän glyyfin symbolin paikassa ilman muunnosta.
- [ ] Glyyfin muodot ovat jaettuja: saman symbolimäärittelyn pistesymbolit viittaavat samoihin muotoihin omilla sijoituksillaan, ja rakennetesti tarkistaa sen. Poikkeuksia on kaksi: origossa oleva yhden ympyrän glyyfi on symbolin paikassa ilman muunnosta, ja ympyrän kaaret, jotka symbolin omat aukot (`PointSymbol.Gaps`) tekevät, ovat symbolin omia muotoja.
- [ ] Vaihe 1 tekee ilmentymät geometriametodeilla niille symbolilajeille, joilla geometriametodi on, ja kerääjällä muille.
- [ ] Kerääjä on testien oraakkeli tikettien 008–013 ajan. Testit vertaavat testikartoilla geometriametodien ilmentymiä kerääjän ilmentymiin rakenteena: samat palettimerkinnät ja samat leikkausobjektit samassa järjestyksessä (laji, muoto, viiva, täyttösääntö ja sijoituksen vaiheet), tai tiketti kertoo sallitun eron ja sen syyn. Myöhemmät osat käyttävät samaa vertailua.
- [ ] `LayerInstanceTests` ja `LayerInstanceCoverageTests` menevät läpi.
- [ ] Piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot**
- **Päätökset (@Pauli, 18.11.):**
  - Kerääjä korvataan symbolien omilla geometriametodeilla ennen kuviotikettiä, ja tämä työ tehdään tiketin [007](007-map-instances.md) jälkeen.
  - Kerääjän ilmentymät ovat geometriametodien oraakkeli.
  - Vanha piirtokoodi pysyy tämän työn ajan muuttumattomana, jotta kerääjä kerää sen kutsut ja pysyy geometriametodien riippumattomana oraakkelina. Geometriametodit kirjoitetaan sen rinnalle, ja symbolien kokoava logiikka saa olla kahdessa paikassa.
  - Työ jaetaan neljään osaan symbolilajeittain: perusrakenne ja pistesymbolit (tämä tiketti), viivat ([011](011-line-geometry.md)), alueet, yhdistelmät ja grafiikkaobjektit ([012](012-area-geometry.md)) sekä tekstit ja suorakulmiot ([013](013-text-geometry.md)).
  - Pistesymbolin glyyfin muodot jaetaan sijoitusten kesken: kukin glyyfin osa on yksi muoto, ja kukin symboli asettaa sen omalla sijoituksellaan.
  - Piirto ei käytä symboleita, vaan symboleita käyttää vain vaihe 1. Ilmentymät kuuluvat karttaan piirrosta riippumatta (epic [000](000-palette-layers.md)).
  - Kun muutos ei voi muuttaa piirtoa, esimerkiksi koska se ei koske koodia, jota piirto ajaa, täysi ajo tehdään komennolla `tools/push-wip.sh` ilman `--collect`-valintaa, sillä kuvat pysyvät silloin ennallaan.
  - Kerääjä on testien oraakkeli tikettien 008–013 ajan. Tiketti [009](009-pattern-instances.md) poistaa sisäisen asetuksen `omitHatchingsAndPatterns` ja lopettaa kerääjän oraakkelikäytön, ja peiton vertailu kattaa silloin myös värit, joilla on viivoituksia tai kuvioita.
  - Katkoviivan päätökset ovat viivojen tiketissä [011](011-line-geometry.md).
- **Speksi:** [Specification.md](../Specification.md), luvut 02 ja 04. Luvun 02 vaihe 1 käsittelee symbolit ja kartan objektit ja tuottaa ilmentymät ja niiden leikkausobjektit. Luvun 04 mukaan muunnettu kuvion ilmentymä tarvitsee vain viittauksen ja muunnoksen eikä prototyypin kopiota. Jaetut glyyfin muodot noudattavat samaa periaatetta.
- **Nykytila** (tiketit 005 ja 007; SymDef.cs, Symbol.cs ja Glyph.cs, noin 7 500 riviä, sekä SymPath.cs, noin 2 100 riviä):
  - `PointSymDef` piirtää glyyfinsä (`Glyph`), jonka osat ovat viivoja, alueita, ympyröitä ja täytettyjä ympyröitä. Yhden ympyrän glyyfi origossa piirretään suoraan symbolin paikkaan. Aukot (`gaps`) tekevät ympyrästä kaaria.
  - Glyyfin paikka ja kierto ovat muunnospinossa (`PushTransform`). Lisämuunnos (`extraTransform`), esimerkiksi viivan glyyfin venytys kulmissa ja pienenevät glyyfit, muuntaa valmiiksi pisteet ja paikat mutta ei viivanleveyksiä.
  - Pistesymboli piirtää glyyfinsä ilman lisämuunnosta, glyyfin omissa koordinaateissa sijoituksen sisällä. Kerääjän kopiot saman symbolimäärittelyn glyyfin osista ovat siksi arvoltaan samat, paitsi ympyrän kaaret, jotka riippuvat kunkin symbolin aukoista.
  - Kerääjän leikkausobjektit ovat muotoja (polku, ellipsi, kaari, tekstiajo), täytettyinä tai viivattuina, ja niiden sijoitus pitää muunnosten ketjun (`LayerInstances.cs`). Kerääjä tekee jokaisesta piirtokutsusta uuden muodon.
  - Täsmällinen peiton vertailu vaatii samat sijoituksen vaiheet samassa järjestyksessä, koska Skia kokoaa sisäkkäiset muunnokset omalla pyöristyksellään (tiketti 005).
  - Symbolien piirtokoodia kutsuvat vain `Map.DrawColor` ja vaihe 1 kerääjän kautta. Korostukset käyttävät osin samaa koodia (`LineSymDef.DrawHighlight` kutsuu `DrawCore`a).
  - Vaiheen 1 piirtokoodi lukee piirtoasetuksista vain resoluution, jota katkoviivat käyttävät, ja asetuksen `omitHatchingsAndPatterns` (tiketti 007). Vaihe 1 valitsee symbolit väri kerrallaan (`SelectVisibleSymbolsOfColor`), joten sama symboli piirretään kerran jokaista väriään kohden.
  - Kerääjä on `internal`, ja MapModelin `InternalsVisibleTo` nimeää vain Test_MapModelin. Map_Skia.Tests näkee vain julkiset jäsenet.
- **Ratkaisuehdotukset:**
  - Symbolikohtainen geometriametodi tuottaa kaikki symbolin palettimerkinnät yhdellä läpikäynnillä, ja leikkausobjektit ovat kunkin palettimerkinnän sisällä samassa järjestyksessä kuin nyt.
  - Kerääjä jää MapModeliin testien oraakkeliksi tikettien 008–013 ajaksi, ja `omitHatchingsAndPatterns` jää sen mukana, koska kerääjä ei tunne kuvioita. Map_Skia.Tests näkee kerääjän, kun `InternalsVisibleTo` nimeää myös sen.
  - Rakenteellinen vertailu vertaa muotoja arvoina, koska kerääjä tekee jokaiselle sijoitukselle uuden muodon. Peittoa verrataan siellä, missä esitys muuttuu tarkoituksella.
  - Ilmentymien päivitys kartan muuttuessa kuuluu vaiheen 2 tikettiin (päätös 18.11.). Symbolikohtainen geometriametodi tekee leikkausobjektin alkuperän kirjaamisesta helppoa.
- **Rajaus:** viivat, alueet ja tekstit ovat tiketit [011](011-line-geometry.md), [012](012-area-geometry.md) ja [013](013-text-geometry.md). Viivoitukset ja kuviot ovat tiketti [009](009-pattern-instances.md), ja vaihe 2 on tiketti [010](010-palette-composition.md).
- **Testit:** Map_Skia.Tests toimii Linuxilla sellaisenaan (`TEST_SILENTRUN=True`). Paikallinen integraatioworktree tehdään basen päälle skriptillä `tools/int-worktree.sh`, ja Test_MapModelin tiedostot ajetaan skriptillä `tools/run-map-tests.sh`. Täysi ajo tehdään komennolla `tools/push-wip.sh`, joka yhdistää haaran baseen. Vanha piirtokoodi pysyy ennallaan, joten ajo tehdään ilman `--collect`-valintaa, kun muutos ei koske muuta koodia, jota piirto ajaa.
- **Haara:** `feature/symbol-geometry` juurtuu kokoavaan haaraan `feature/palette-layers`. Täysi ajo tehdään haarassa `wip/linux/symbol-geometry` basen päällä.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md), [../README.md](../README.md) ja sen kohta "The architecture comes first", [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja "Decisions", epicin [000](000-palette-layers.md) muistiinpanot, tiketit [005](005-layer-instances.md) ja [007](007-map-instances.md) sekä tämä tiketti. [../tools/README.md](../tools/README.md) kertoo tiketin työkierron.

**Erotetut alatehtävät**
- [011](011-line-geometry.md) Viivojen geometriametodit, erotettu tästä tiketistä (@Pauli, 18.11.): haara `feature/line-geometry` (ehdotus), ei vielä tehty. Se juurtuu kokoavaan haaraan tämän tiketin jälkeen, eikä sitä yhdistetä tämän tiketin haaraan.
- [012](012-area-geometry.md) Alueiden, yhdistelmien ja grafiikkaobjektien geometriametodit, erotettu tästä tiketistä (@Pauli, 18.11.): haara `feature/area-geometry` (ehdotus), ei vielä tehty. Se juurtuu kokoavaan haaraan tiketin 011 jälkeen, eikä sitä yhdistetä tämän tiketin haaraan.
- [013](013-text-geometry.md) Tekstien ja suorakulmioiden geometriametodit, erotettu tästä tiketistä (@Pauli, 18.11.): haara `feature/text-geometry` (ehdotus), ei vielä tehty. Se juurtuu kokoavaan haaraan tiketin 012 jälkeen, eikä sitä yhdistetä tämän tiketin haaraan.
