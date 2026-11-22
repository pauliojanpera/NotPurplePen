# Vaihe 1 tekee kartan ilmentymät piirrosta riippumatta
**Kuvaus**
Vaihe 1 tekee ilmentymät pelkästä kartasta. `Map.CreateLayerInstances` ei saa piirron suorakulmiota eikä resoluutiota, ja ilmentymissä ovat kartan kaikki näkyvät symbolit sijainnistaan riippumatta. Kerääjä on edelleen ilmentymien lähde, ja piirto ei muutu.

**Hyväksymiskriteerit**
- [x] `Map.CreateLayerInstances` ei saa parametreina piirron suorakulmiota eikä resoluutiota. Sen tulos riippuu vain kartasta: sen väreistä, symbolimäärittelyistä, symboleista, näkyvyydestä ja tekstin mitoista (`TextMetricsProvider`).
- [x] Ilmentymissä ovat kartan kaikki näkyvät symbolit: näkyvät symbolimäärittelyt, joilla on palettimerkinnän väri, ja niiden näkyvät symbolit sijainnistaan riippumatta. Symbolien valinta ei käytä symbolien rajoja (`BoundingBox`) eikä `MayIntersectRect`-metodia.
- [x] Katkoviivan katkot ovat ilmentymissä katkoja kaikilla katkojen ja välien pituuksilla. Vaihe 1 antaa symbolien piirtokoodille resoluution 0, joten piirtokoodi ei piirrä katkoviivaa yhtenäisenä.
- [x] Peiton vertailu (`LayerInstanceCoverageTests`) piirtää oraakkelin, nykyisen piirron yksi väri kerrallaan, resoluutiolla 0, ja vertaa sitä koko kartan ilmentymiin. Vertailu on täsmällinen samoilla kartoilla kuin tiketissä [005](005-layer-instances.md).
- [x] Rakennetestit (`LayerInstanceTests`) tarkistavat, että ilmentymissä on symboli kaukana muista symboleista ja että katkoviiva, jonka välit ovat lyhyempiä kuin tavallinen pikselikoko, koostuu katkoista.
- [x] Piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot**
- **Päätökset (@Pauli, 18.11.):** piirto ei käytä symboleita, ja ilmentymät kuuluvat karttaan piirrosta riippumatta (epic [000](000-palette-stack.md), muistiinpanot). Suorakulmioon rajaaminen kuuluu vaiheeseen 2. Ilmentymien säilytys kartassa ja niiden päivitys kartan muuttuessa kuuluvat vaiheen 2 tikettiin. Tämä tiketti tehdään ennen geometriametodien tikettiä, ja sen ilmentymät ovat geometriametodien oraakkeli.
- **Toteutus:**
  - `Map.CreateLayerInstances()` ei saa parametreja. Se valitsee symbolit metodilla `SelectVisibleSymbolsOfColor`: näkyvät symbolimäärittelyt, joilla on väri, ja niiden näkyvät symbolit siinä järjestyksessä, jossa `DrawColor` piirtää ne. Valinta ei käytä symbolien rajoja eikä `MayIntersectRect`-metodia.
  - Vaihe 1 antaa symbolien piirtokoodille piirtoasetukset, joissa `minResolution` on 0 ja `omitHatchingsAndPatterns` on päällä. Silloin `LineSymDef.DrawDashedWithOffset` piirtää katkot kaikilla välien pituuksilla.
  - `LayerInstanceCoverageTests` piirtää oraakkelin resoluutiolla 0 ja vertaa sitä koko kartan ilmentymiin. Vertailu on täsmällinen tiketin 005 kaikilla 20 kartalla. Yhdessäkään niistä ei ole katkoviivaa, jonka kaikki välit ovat kartan pikseliä lyhyempiä, ja jokaisen kartan piirto on pikselikoolla ja resoluutiolla 0 pikselilleen sama. Siksi vertailu piirtää lisäksi kartat dashline, isomlines ja doublelines9 pikseleillä, jotka ovat niiden välejä suurempia (0,3, 0,4 ja 0,8 mm): pää- ja toissijaiset välit sekä kaksoisviivojen välit. Kukin näistä tapauksista tarkistaa ensin, että piirto pikselikoolla eroaa piirrosta resoluutiolla 0, ja vertaa sitten kaikkien värien peiton. Kun vaihe 1 saa resoluution 1 mm, kaikki kolme tapausta kaatuvat.
  - `LayerInstanceTests.InstancesHoldTheVisibleSymbolsWhereverTheyLie` tarkistaa, että ilmentymissä on symboli 6,4 metrin päässä muista symboleista kartan paperilla. `DashedLineIsItsDashesAlsoWhenItsGapsAreShorterThanAPixel` tarkistaa, että 10 mm:n viiva, jonka välit ovat 0,01 mm, on kymmenen katkoa, joiden välissä on väli. 1200 dpi:n tulosteen pikseli on 0,021 mm.
- **Nykytila:**
  - `Map.CreateLayerInstances(rect, minResolution)` (Map.cs) valitsee symbolit metodilla `SymbolsDrawn`. Se ottaa näkyvät symbolimäärittelyt (`IsSymdefVisible`), joilla on väri (`HasColor`), ja niiden näkyvät symbolit, joiden rajat leikkaavat suorakulmion ja jotka voivat osua siihen (`MayIntersectRect`). Kuva- ja asettelukerroksen määrittelyt se lajittelee (`SortSymbolsForDrawing`).
  - Vaihe 1 antaa symbolien piirtokoodille piirtoasetukset, joissa on `minResolution` ja sisäinen `omitHatchingsAndPatterns`. Symbolien piirtokoodi käyttää `minResolution`ia vain katkoviivoissa (`LineSymDef.DrawDashedWithOffset`): se piirtää viivan yhtenäisenä, kun välit ja toissijaiset välit ovat sitä lyhyempiä. Kuviot käyttävät sitä myös, mutta vaihe 1 jättää ne pois.
  - `Map.DrawColor` valitsee symbolit omalla silmukallaan, ja se pysyy ennallaan, koska sen valinta poistuu vaiheen 2 mukana.
  - `LayerInstanceCoverageTests` antaa sekä oraakkelille että vaiheelle 1 testikartan pikselikoon (`MinResolution`). `LayerInstanceTests.InstancesHoldTheSymbolsThatTheDrawingDraws` tarkistaa nyt, että suorakulmion ulkopuolinen symboli ei ole ilmentymissä. Molemmat ovat Map_Skia.Testsissä.
- **Peiton vertailu:** oraakkeli piirtää testikartan alueen kuten nyt, mutta resoluutiolla 0. Ilmentymät ovat koko kartasta, ja niiden piirto rajautuu bittikarttaan, joten alueen ulkopuoliset symbolit eivät muuta vertailua.
- **Rajaus:** kerääjä pysyy ilmentymien lähteenä, ja symbolien geometriametodit ovat seuraava tiketti. Vaihe 2, ilmentymien säilytys ja päivitys sekä viivoitukset ja kuviot eivät kuulu tähän.
- **Testit:** Map_Skia.Tests toimii Linuxilla sellaisenaan (`TEST_SILENTRUN=True`). Paikallinen integraatioworktree tehdään basen päälle skriptillä `tools/int-worktree.sh`, ja Test_MapModelin tiedostot ajetaan skriptillä `tools/run-map-tests.sh`. Täysi ajo tehdään komennolla `tools/push-wip.sh --collect`, joka yhdistää haaran baseen.
- **Haara:** `feature/map-instances` juurtuu kokoavaan haaraan `feature/palette-stack`. Täysi ajo tehdään haarassa `wip/linux/map-instances` basen päällä.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md), [../README.md](../README.md) ja sen kohta "The architecture comes first", [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja "Decisions", epicin [000](000-palette-stack.md) muistiinpanot, tiketti [005](005-layer-instances.md) sekä tämä tiketti. [../tools/README.md](../tools/README.md) kertoo tiketin työkierron.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
