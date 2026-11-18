# Palettikerrokset upstreamin karttamalliin
### Tavoite
Upstreamin karttamalli toteuttaa [speksin](../Specification.md) palettikerrosarkkitehtuurin. Palettimerkinnät koostuvat musteista, symbolit instansioivat geometrian, ja piirto koostaa palettimerkinnät palettijärjestyksessä. Kartan piirron auktoriteetteja ovat OCAD ja Mapper omille kartoilleen ja muuten feature/multiply-blending. Se on vain ulkoasun oraakkeli eikä toteutuksen malli: työ ei lue sen koodia, ja unioni saa jäädä piirrossa implisiittiseksi, mutta ilmentymät eivät saa jäädä pois (speksin esipuhe ja luku 05). Arkkitehtuurin osien tiketeillä on rakenteelliset hyväksymiskriteerit. Osat kootaan haaraan `feature/palette-layers`, joka juurtuu kohtaan, jossa base kohtaa upstreamin (d48f4456). Tiedosto [Progress.md](../Progress.md) seuraa haaroja.

### Laajuus  
✅ Sisältyy
* Musteet palettimerkintöjen perustana, luku 06
* Palettimerkinnän opasiteetti ja sekoitustila, luvut 01 ja 05
* Vaihe 1: symbolien ohjaama ilmentäminen tietorakenteeksi sekä hilan ja elementtien muunnokset, luvut 02 ja 03
* Vaihe 2: koostaminen palettimerkinnöittäin pelkistä ilmentymistä, luku 05
* Piirtotilat vaiheen 2 väriasetuksina: normaali näkymä, musteiden simuloima päällepainatus ja separaatiot, luvut 06 ja 07
* Yksi palettipino kaikille kartoille, radat mukaan lukien
* Korjaukset upstreamin koodiin, joita osat tarvitsevat

❌ Ei sisälly
* SVG-koodaus, luku 04, ennen kuin vaiheet 1 ja 2 ovat valmiit: sen paikka ja feature/svg-targetin kokemuksen käyttö päätetään silloin (ks. Muistiinpanot)
* Läpinäkyvyyden litistys vektoreiksi, luku 08: se on oma optimointitehtävänsä
* Integraatio NotPurplePenin dev-haaraan: se tehdään @Paulin luvalla, kun osat ovat valmiit

### Muistiinpanot
* **Arkkitehtuuri ennen ulkoasua (18.11.).** Vanha feature/multiply-blending -haara saavuttaa ulkoasunsa sekoittamalla musteen kerroksena symbolien piirron ympärillä. Tämä epic sen sijaan toteuttaa speksin erottelemat vaiheet: vaihe 1 tuottaa ilmentymät tietorakenteena, ja vaihe 2 koostaa ne. Siksi multiply-blending on tälle työlle vain ulkoasun oraakkeli, eikä työ lue sen koodia. Speksin luvun 05 implisiittinen unioni koskee piirtoa eikä rakennetta: ilmentymät eivät saa jäädä pois. (Tekstissä mainintaa tarkoitetaan suhteutettavaksi lähinnä litistämiseen.) Osat tehdään vaiheiden järjestyksessä: palettimerkinnän tiedot ensin pelkkänä mallina, sitten vaihe 1, vaihe 2 ja piirtotilat.
* **Mallin muutokset ensin (18.11.).** Inks-vaiheessa pelkän mallin muutokset tarkistuivat kierrostesteillä ilman baselineja, kun taas piirron muutokset tarvitsivat CI:n baselinet. Formaattien tulkinta tarkistetaan ensisijaisesti kirjoittajien (OCAD, OOM) omista tiedostoista: Hakolan OCAD-kartat ovat Mapperin kirjoittamia, ja OCADin omat tiedostot eroavat niistä. Jos osan työssä korjataan upstreamin vika, korjaus erotetaan omaksi fix-tiketikseen ja fix-haarakseen (@Pauli, 18.11.).
* **SVG-koodaus (18.11.).** Luvun 04 mukaan SVG-primitiivit ovat saman loogisen mallin koodaustapoja eivätkä osa kartta-arkkitehtuuria. Arkkitehtuuri rakennetaan ja todennetaan ilman SVG:tä, ja vaiheiden 1 ja 2 tiketit saavat lukua 04 vastaavat rakenteelliset kriteerit: prototyyppi jaetaan muunnoksin kopioimatta, ja palettimerkinnällä on yksi ryhmä. Feature/svg-targetin kokemus koskee enimmäkseen rakenteen ulkopuolisia asioita: tekstiä, fontteja, kuvia, georeferointia ja selainten eroja. Osan siitä, kuten kuvioiden haamut, ilmentymät korvaavat, ja haamujen seuraaminen olisi samanlainen luisu kuin multiply-blendingin seuraaminen. SVG-koodauksen paikka päätetään, kun vaihe 2 on valmis: koodataanko suoraan ilmentymistä vai sovitetaanko NotPurplePenin `Map_Svg` integraatiossa, ja saako työ käyttää svg-targetin dokumentoituja havaintoja tai sen koodia.
* **Litistys (18.11.)** ei kuulu epiciin. Se on oma optimointitehtävänsä (luku 08).
* **Vaihe 2 korvaa palettimerkintöjen suoran piirron (@Pauli, 18.11.).** Kun vaihe 2 on valmis, kartan palettimerkinnät piirretään vain ilmentymistä. Nykyinen suora piirto, jossa `Map.Draw` piirtää värit `DrawColor`-metodilla ja kunkin symbolin (`Symbol.Draw`) suoraan piirtokohteelle, ei silloin enää piirrä niitä. Normaali näkymä on vaiheen 2 piirtotila, kuten musteiden simuloima päällepainatus ja separaatiot. Symbolien piirtokoodi on vaiheen 1 lähde kerääjän kautta (tiketti [005](005-layer-instances.md)), kunnes symbolien omat geometriametodit korvaavat kerääjän.
* **Piirto ei käytä symboleita (@Pauli, 18.11.).** Vaihe 2 lukee vain ilmentymiä, joten piirto ei valitse symboleita eikä kutsu niiden koodia. Symboleita käyttää vain vaihe 1.
* **Ilmentymät kuuluvat karttaan piirrosta riippumatta (@Pauli, 18.11.).** Vaihe 1 riippuu vain kartasta: se ei saa piirron suorakulmiota eikä resoluutiota. Suorakulmioon rajaaminen kuuluu vaiheeseen 2. Ilmentymien ajantasaisuus kartan muuttuessa ratkaistaan vaiheen 2 tiketissä.
* **Vaihe 1 ennen kuvioita (@Pauli, 18.11.).** Ennen kuviotikettiä tehdään kaksi tikettiä. Ensimmäinen irrottaa vaiheen 1 piirrosta. Toinen korvaa kerääjän symbolien omilla geometriametodeilla, ja kerääjän ilmentymät ovat sen oraakkeli.
* **Katkoviiva muotona (18.11.).** Geometriametodien tiketti ratkaisee, onko katkoviiva yksi muoto, polku ja sen katkojen pituudet, ja miten se esittää OCADin katkoviivan: viivaan sovitetut katkojen pituudet, OCADin oman metriikan ja kaksoisviivan sivuun siirretyt katkot.
* **Vaiheen 2 tiketti ratkaisee (18.11.):**
  * Ryhmän koostamisen. Vaihe 2 koostaa jokaisen palettimerkinnän ryhmän kerran α:lla ja B:llä, mutta `IGraphicsTarget`-rajapinnassa ei vielä ole ryhmän koostamista. Koostamista tarvitsevat tälle työlle merkitykselliset piirtokohteet: Skia, PDF, tulostus ja tuleva SVG-toteutus (@Pauli, 18.11.). AvPurplePenin tulostus piirtää sivut Windowsissa Skialla ja Linuxissa ja macOS:ssä PDF:nä. GDI+ ja WPF ovat legacya, eivätkä Direct2D ja iOS ole käytössä: toimitettava sovellus viittaa piirtokohteista vain Skiaan (`Map_SkiaStd`) ja PDF:ään (`Map_PDF`).
  * Pohjakarttojen, kuvakerroksen ja asettelukerroksen piirron palettipinon rinnalla. Ne eivät ole palettimerkintöjä, eikä niillä ole ilmentymiä, mutta `Map.Draw` piirtää ne nyt palettimerkintöjen alle ja päälle. OCAD-pohjakartta on oma `Map`-olionsa, jonka `Map.Draw` piirtää.
  * Peiton vertailun uuden perustan. `LayerInstanceCoverageTests` vertaa ilmentymien peittoa suoraan piirtoon, joka on sen oraakkeli, ja suora piirto ei enää piirrä palettimerkintöjä vaiheen 2 jälkeen.
* **Valintojen korostukset (18.11.)** (`Symbol.DrawHighlight` ja ratakohteiden `CourseObject.DrawHighlight`) piirretään suoraan piirtokohteelle eivätkä kulje `Map.Draw`n kautta. Auki on, jäävätkö ne suoraksi piirroksi. Se ei estä vaihetta 2.

### Erotetut alatehtävät
* [001](001-inks.md) Musteet palettimerkintöjen perustana: haara `feature/inks`, yhdistetty haaraan `feature/palette-layers`.
* [002](002-mapper-export-color-numbers.md) Mapper-viennin värinumerot, erotettu tiketistä 001: haara `fix/mapper-export-color-numbers`, yhdistetty haaraan `feature/palette-layers` haaran `feature/inks` mukana.
* [003](003-ocad11-utf8-strings.md) OCAD 11:n ja uudempien merkkijonojen UTF-8-koodaus, löytyi tiketin 001 testikartasta: haara `fix/ocad11-utf8-strings`, yhdistetty haaraan `feature/palette-layers`.
* [004](004-opacity-and-blend-mode.md) Palettimerkinnän peittävyys ja sekoitustila: haara `feature/opacity-and-blend-mode`, yhdistetty haaraan `feature/palette-layers`.
* [005](005-layer-instances.md) Vaihe 1: yhtenäisen geometrian ilmentymät (G = 1): haara `feature/layer-instances`, yhdistetty haaraan `feature/palette-layers`.
* [006](006-matrix-equals-offsets.md) Matriisin yhtäsuuruus vertaa siirtymiä float-lukuina, löytyi tiketin 005 testeistä: haara `fix/matrix-equals-offsets`, ei yhdistetty, koska mikään osa ei tarvitse sitä.
