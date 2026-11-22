# LUONNOS – Symbolin avain kartassa

> **LUONNOS (23.11.), @Paulin hyväksyttävänä.** Tämä tiketti on luonnos, kunnes @Pauli hyväksyy sen, eikä luonnosta toteuteta. Hyväksynnän jälkeen istunto poistaa tämän merkinnän ja otsikon sanan LUONNOS.

**Kuvaus**
Jokaisella kartan symbolilla on symbolin avain (`Symbol.Key`), joka on sen identiteetti. Symbolin tekijä antaa sen, ja kartta antaa sen laskurista symbolille, jolle tekijä ei antanut avainta. Korvaava symboli saa korvatun symbolin avaimen, ja kumoaminen palauttaa avaimen. Piirto ei muutu.

**Hyväksymiskriteerit**
- [ ] `Symbol.Key` on symbolin avain. Symbolilla ei ole avainta, ennen kuin sen tekijä tai kartta antaa sen. Avaimella on tekstimuoto, jonka itsensä kannatteleva vektoritiedosto voi kantaa, ja kaksi avainta ovat samat, kun niiden tekstimuodot ovat samat.
- [ ] Kun symboli lisätään karttaan (`Map.AddSymbol`) ilman avainta, kartta antaa sille avaimen laskurista. Kartta ei käytä laskurin arvoa uudelleen, ei symbolin poiston eikä `Map.Clear`in jälkeen. Tiedostosta luetun OCAD- tai Mapper-kartan symbolit saavat avaimensa näin lukujärjestyksessä.
- [ ] Symbolin tekijä voi antaa avaimen, ennen kuin symboli lisätään karttaan. Avain on yksikäsitteinen kartassaan: lisäys, jonka avain on jo kartan toisella symbolilla, on käyttövirhe (`MapUsageException`).
- [ ] Muokkaus, joka korvaa kartan symbolin toisella (`Map.ReplaceSymbol`), antaa korvaavalle symbolille korvatun symbolin avaimen. Kumoaminen palauttaa korvatun symbolin avaimineen, ja uudelleen tekeminen palauttaa korvaavan symbolin samoin avaimin.
- [ ] Symbolin kopio (`Symbol.CloneDetached`) on uusi symboli, jolla ei ole avainta.
- [ ] Alueen reikä (`HoleSymbol`) on kartan symboli, ja sekin saa avaimen.
- [ ] Avain ei ole symbolimäärittelyn numero (`SymDef.SymbolId`). Piirto ei käytä avainta: piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.
- [ ] Test_MapModelin testit tarkistavat laskurin antamat avaimet, tekijän antaman avaimen, yksikäsitteisyyden, korvaamisen, kumoamisen ja uudelleen tekemisen, kopion ja reiän avaimet sekä tiedostosta luettujen karttojen avaimet.

**Lisätiedot**
- **Päätökset:**
  - Neljä tikettiä korvaa tiketit 005, 007 ja 008, ja tämä on niistä kolmas (@Pauli, 23.11., Progress.md: "The new tickets of pass 1").
  - Symbolin avain (@Pauli, 21.11., "The symbol key"): `Symbol`illa on symbolin avain, joka on sen identiteetti, ja symbolin tekijä antaa sen. Kartta, joka on luettu tiedostosta tai jota muokataan, antaa avaimen laskurista, jonka arvoja se ei käytä uudelleen. Kun muokkaus korvaa symbolin, korvaava symboli saa saman avaimen. Kumoaminen palauttaa avaimen, ja itsensä kannatteleva vektoritiedosto kantaa sen. Ratakartassa avaimen antaa `CourseLayout.RenderToMap` (tiketti 026).
  - Laskurin avainten tekstimuoto on laskurin arvo kannassa 62, numeroin 0–9, A–Z ja a–z tässä järjestyksessä (@Pauli, 23.11.).
  - Avaimen perusta on `Map`in ja SVG-kanavan tulevaisuus (@Pauli, 21.11., "The future of Map and the SVG channel"): SVG-DOMin mutaatiot selaimessa tarvitsevat avaimia, jotka pysyvät samoina muokkausten yli.
- **Speksi:** [Specification.md](../Specification.md), luvun 02 kohta "The symbol keys and the course object keys". Avain on yksikäsitteinen kartassaan, joten piirrosobjektin viittaus antaa myös kartan. Symbolin kaikki piirrosobjektit jakavat sen avaimen, joten avain ei ole SVG-elementin `id`. Nimet ovat [sanastossa](../Glossary.md): `Symbol.Key` · symbolin avain.
- **Nykytila** (`feature/palette-stack`):
  - Symbolilla ei ole avainta eikä muuta tunnistetta. Palettivärillä on `Name` ja `OcadId`, ja symbolimäärittelyllä `Name` ja `SymbolId`.
  - Kaikki symbolit tulevat karttaan metodilla `Map.AddSymbol`: myös OCAD- ja Mapper-tiedostojen luku (`OcadImport`, `OpenMapperImport`) ja rataobjektit (`CourseObj.AddToMap`). Lisäys ja poisto (`RemoveSymbol`) kirjaavat kumoamisen toimenpiteen (`SymbolChangeAction`), joka lisää tai poistaa saman symbolin olion, kun kartalla on kumoamisen hallinta (`Map.UndoMgr`). Kartassa ei ole symbolin korvaavaa muokkausta.
  - Symboli ei muutu sen jälkeen, kun se on lisätty karttaan (`CheckModifiable`). `CloneDetached` tekee symbolista kopion, jota ei ole lisätty karttaan, ja voi vaihtaa sen symbolimäärittelyn ja muuntaa sen paikan.
  - Reikä (`HoleSymbol`) lisätään karttaan samalla metodilla: kartta liittää sen alueeseen (`AreaLikeSymbol`), jonka polkuun se kuuluu.
- **Ratkaisut** (nimet valitsee istunto, @Pauli, 23.11.):
  - Avaimen tyyppi on `SymbolKey`: muuttumaton arvo, jolla on tekstimuoto ja ordinaalinen yhtäsuuruus. Laskurin avainten tekstimuoto on laskurin arvo kannassa 62 (päätös 23.11.), ja laskuri ohittaa arvon, jonka tekstimuoto on jo kartan symbolin avain. 300 000 avaimen keskipituus on kannassa 62 3,19 merkkiä ja kymmenjärjestelmässä 5,63 merkkiä, ja gzip pakkaa molemmat 3–5 prosentin päähän toisistaan. Avaimissa on vain ASCII-kirjaimia ja -numeroita, joten välimerkit jäävät ratakartan avainten rakenteelle. Tekstimuoto sopii sekä laskurin avaimille että ratakartan avaimille, jotka tiketti 026 tekee rataobjektin avaimesta ja indeksistä, ja sen voi kirjoittaa tiedostoon ja SVG-DOMiin.
  - `Symbol.Key` asetetaan ennen kuin symboli lisätään karttaan, kuten symbolin muutkin tiedot (`CheckModifiable`).
  - `Map.ReplaceSymbol(Symbol symbol, Symbol replacement)` poistaa symbolin, antaa korvaavalle symbolille sen avaimen ja lisää korvaavan symbolin samassa kirjoituksessa. Kumoaminen kulkee lisäyksen ja poiston toimenpiteillä, jotka pitävät symbolien oliot ja siten niiden avaimet.
- **Rajaus:** ratakartan symbolien avaimet ja rataobjektin avain ovat tiketti 026. Avainten kirjoitus SVG- ja PDF-tiedostoihin kuuluu koodauksiin, ja tiedoston avaaminen kartaksi ei kuulu epiciin. Tiketit 023 ja 024 eivät käytä avaimia.
- **Testit:** testit ovat Test_MapModelin tiedostossa `SymbolKeyTests.cs`, joka lisätään tools-kansion `LinuxMapModelTestRun.csproj`-projektiin, ja ne ajetaan Linuxilla skriptillä `tools/run-map-tests.sh`. Piirto ei käytä avainta, joten täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack` ilman keräävää ajoa: `tools/push-wip.sh --base-baselines prs/000-palette-stack-base <klooni> feature/symbol-keys`.
- **Haara:** `feature/symbol-keys` juurtuu haaraan `feature/palette-stack`. Täysi ajo tehdään haarassa `wip/linux/symbol-keys` basen päällä valitsimella `--base-baselines prs/000-palette-stack-base`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md), [../README.md](../README.md), [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja "Decisions", speksin esipuhe ja luvun 02 kohta "The symbol keys and the course object keys", [../Glossary.md](../Glossary.md) sekä tämä tiketti. [../tools/README.md](../tools/README.md) kertoo tiketin työkierron.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
