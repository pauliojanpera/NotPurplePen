# Palettikerrokset upstreamin karttamalliin
### Tavoite
Upstreamin karttamalli toteuttaa [speksin](../Specification.md) palettikerrosarkkitehtuurin. Palettimerkinnät koostuvat musteista, symbolit instansioivat geometrian, ja piirto koostaa palettimerkinnät palettijärjestyksessä. Kartan piirron auktoriteetteja ovat OCAD ja Mapper omille kartoilleen ja muuten feature/multiply-blending. Osat kootaan haaraan `feature/palette-layers`, joka juurtuu kohtaan, jossa base kohtaa upstreamin (d48f4456). Tiedosto [Progress.md](../Progress.md) seuraa haaroja.

### Laajuus  
✅ Sisältyy
* Musteet palettimerkintöjen perustana, luku 06
* Koostaminen musteiden mukaan: separaatiot, knockout ja päällepainatus, luvut 05 ja 06
* Palettimerkinnän opasiteetti ja sekoitustila, luvut 01 ja 05
* Symbolien ohjaama kerrosten instansiointi sekä hilan ja elementtien muunnokset, luvut 02 ja 03
* Värien esitystavat, luku 07
* Korjaukset upstreamin koodiin, joita osat tarvitsevat

❌ Ei sisälly
* SVG-koodaus, luku 04: upstreamissa ei ole SVG-kohdetta, ja NotPurplePenin SVG-kohde kuuluu integraatioon
* Läpinäkyvyyden litistys vektoreiksi, luku 08: se on oma optimointitehtävänsä
* Integraatio NotPurplePenin dev-haaraan: se tehdään @Paulin luvalla, kun osat ovat valmiit

### Erotetut alatehtävät
* [001](001-inks.md) Musteet palettimerkintöjen perustana: haara `feature/inks`, yhdistetty haaraan `feature/palette-layers`.
* [002](002-mapper-export-color-numbers.md) Mapper-viennin värinumerot, erotettu tiketistä 001: haara `fix/mapper-export-color-numbers`, yhdistetty haaraan `feature/palette-layers` haaran `feature/inks` mukana.
* [003](003-ocad11-utf8-strings.md) OCAD 11:n ja uudempien merkkijonojen UTF-8-koodaus, löytyi tiketin 001 testikartasta: haara `fix/ocad11-utf8-strings`, yhdistetty haaraan `feature/palette-layers`.
