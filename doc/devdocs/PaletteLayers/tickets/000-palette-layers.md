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
* **Arkkitehtuuri ennen ulkoasua (18.11.).** Feature/multiply-blending saavuttaa ulkoasunsa sekoittamalla musteen kerroksena symbolien piirron ympärillä. Tämä epic toteuttaa speksin erottelemat vaiheet: vaihe 1 tuottaa ilmentymät tietorakenteena, ja vaihe 2 koostaa ne. Siksi multiply-blending on vain ulkoasun oraakkeli, eikä työ lue sen koodia. Speksin luvun 05 implisiittinen unioni koskee piirtoa eikä rakennetta: ilmentymät eivät saa jäädä pois. Osat tehdään vaiheiden järjestyksessä: palettimerkinnän tiedot ensin pelkkänä mallina, sitten vaihe 1, vaihe 2 ja piirtotilat.
* **Mallin muutokset ensin (18.11.).** Inks-vaiheessa pelkän mallin muutokset tarkistuivat kierrostesteillä ilman baselineja, kun taas piirron muutokset tarvitsivat CI:n baselinet. Formaattien tulkinta tarkistetaan kirjoittajien omista tiedostoista: Hakolan OCAD-kartat olivat Mapperin kirjoittamia, ja OCADin omat tiedostot erosivat niistä. Matkalla löytyvät upstreamin viat erotetaan omiksi fix-tiketeikseen.
* **SVG-koodaus (18.11.).** Luvun 04 mukaan SVG-primitiivit ovat saman loogisen mallin koodaustapoja eivätkä osa arkkitehtuuria. Arkkitehtuuri rakennetaan ja todennetaan ilman SVG:tä, ja vaiheiden 1 ja 2 tiketit saavat lukua 04 vastaavat rakenteelliset kriteerit: prototyyppi jaetaan muunnoksin kopioimatta, ja palettimerkinnällä on yksi ryhmä. Feature/svg-targetin kokemus koskee enimmäkseen rakenteen ulkopuolisia asioita: tekstiä, fontteja, kuvia, georeferointia ja selainten eroja. Osan siitä, kuten kuvioiden haamut, ilmentymät korvaavat, ja haamujen seuraaminen olisi samanlainen luisu kuin multiply-blendingin seuraaminen. SVG-koodauksen paikka päätetään, kun vaihe 2 on valmis: koodataanko suoraan ilmentymistä vai sovitetaanko NotPurplePenin `Map_Svg` integraatiossa, ja saako työ käyttää svg-targetin dokumentoituja havaintoja ilman sen koodia.
* **Litistys (18.11.)** ei kuulu epiciin. Se on oma optimointitehtävänsä (luku 08).

### Erotetut alatehtävät
* [001](001-inks.md) Musteet palettimerkintöjen perustana: haara `feature/inks`, yhdistetty haaraan `feature/palette-layers`.
* [002](002-mapper-export-color-numbers.md) Mapper-viennin värinumerot, erotettu tiketistä 001: haara `fix/mapper-export-color-numbers`, yhdistetty haaraan `feature/palette-layers` haaran `feature/inks` mukana.
* [003](003-ocad11-utf8-strings.md) OCAD 11:n ja uudempien merkkijonojen UTF-8-koodaus, löytyi tiketin 001 testikartasta: haara `fix/ocad11-utf8-strings`, yhdistetty haaraan `feature/palette-layers`.
