# Musteet palettimerkintöjen perustaksi

**Kuvaus**

Karttamalliin tulee eksplisiittinen muste, ja jokainen palettimerkintä (`SymColor`) määritellään musteiden koosteena speksin luvun 02 mukaisesti.

**Hyväksymiskriteerit**

- [x] Muste on oma entiteettinsä. Sillä on nimi, ulkoasun CMYK-arvo sekä rasteritiheys ja -kulma silloin, kun ne on määritelty. Prosessimusteet C, M, Y ja K ovat aina olemassa.
- [x] Palettimerkintä sisältää mustekomponentit, joista jokaisella on muste ja kerroin väliltä 0–1. Puuttuva komponentti ja kerroin 0 ovat eri asioita.
- [x] Palettimerkintä, jolla ei ole spottimäärittelyä, koostuu prosessimusteista C, M, Y ja K, ja kertoimina ovat sen CMYK-arvot.
- [x] Spottimuste voi olla palettimerkintä sellaisenaan, täydellä sävyllä.
- [x] Palettimerkinnän CMYK-arvo säilyy, ja koodi, joka lukee värin CMYK-arvon, saa saman arvon kuin ennen.
- [x] OCAD 9 ja uudemmat: tuonti lukee spottiseparaatiot (merkkijonotyyppi 10, `si_SpotColor`) musteiksi ja värimerkkijonon `s`- ja `p`-parit komponenteiksi.
- [x] OCAD 6–8: tuonti lukee symboliotsakkeen `aColorSep`-taulukon musteiksi ja `TColorInfo.SepPercentage`-arvot komponenteiksi. Arvo 255 tarkoittaa, ettei komponenttia ole, ja arvot 0–200 ovat kaksinkertaisia prosentteja.
- [x] Mapper: tuonti lukee `namedcolor`-elementin spottimusteeksi (nimi, `screen_frequency` ja `screen_angle`) ja `component`-elementit komponenteiksi (`factor` ja `spotcolor`, joka on viitatun spottivärin prioriteetti).
- [x] Mapper-tuonti merkitsee värin päällepainetuksi vain, jos värillä on spottimäärittely ja sen knockout on pois päältä. Muut Mapper-värit ovat knockout-värejä, kuten OOM ne piirtää.
- [x] OCAD- ja Mapper-vienti kirjoittavat musteet ja komponentit. Kun tiedosto tuodaan, viedään ja tuodaan uudelleen, musteet ja komponentit säilyvät samoina muodon rajoissa.
- [x] Piirto muuttuu vain niillä Mapper-väreillä, joilla ei ole spottimäärittelyä: päällepainatusvaikutus ei enää sekoita niitä. Näiden karttojen baselinet siirtyvät tässä tiketissä, eikä mikään muu piirron baseline siirry.
- [x] Testit kattavat OCAD- ja Mapper-testikarttojen tuonnin, tuonnin ja viennin kierroksen sekä tapauksen, jossa sama muste on kahdessa palettimerkinnässä, jotka eivät ole peräkkäisiä. Testikartoissa ei ole tekstiä aina, kun se on mahdollista.

**Lisätiedot**

- **Speksi:** [Specification.md](../Specification.md), luku 02. Prosessivärit ovat musteiden neliväritapaus. Siksi värit, joilla ei ole spottimäärittelyä, koostuvat prosessimusteista, ja tässä kohdassa speksi ratkaisee oraakkelin sijaan (@Pauli, 17.11.).
- **Nykytila:** `SymColor` sisältää vain CMYK-arvon, päällepainatuslipun, nimen ja OCAD-tunnuksen. `OcadFileFormat` lukee OCAD 6–8:n `aColorSep`- ja `SepPercentage`-kentät, mutta `OcadImport.CreateColors` ei käytä niitä eikä OCAD 9:n ja uudempien `s`- ja `p`-kenttiä ("we don't preserve the spot color information"). `OpenMapperColor.ReadFromXml` lukee `spotcolors`-elementistä vain `knockout`-määreen. OCAD-vienti kirjoittaa separaatiot tyhjinä, eikä Mapper-vienti kirjoita musteita.
- **OOM:n malli** ([map_color.h](https://github.com/OpenOrienteering/mapper/blob/master/src/core/map_color.h), [xml_file_format.cpp](https://github.com/OpenOrienteering/mapper/blob/master/src/fileformats/xml_file_format.cpp)): väri on spottiväri (`SpotColor`, jolla on nimi ja rasteri), spottivärien kooste (`CustomColor`, jonka komponenteilla on kerroin 0–1) tai pelkkä CMYK-väri. Komponentti viittaa spottiväriin tämän prioriteetilla.
- **OCADin malli:** separaatiot ovat erillinen lista, ja väri viittaa niihin prosenttiosuuksin. Speksin luvun 02 mukaan puuttuva osuus jättää separaation ennalleen, 0 % pyyhkii sen ja positiivinen osuus asettaa peiton.
- **Testiaineisto:** OCAD-tiedostoista (versio 9 tai uudempi) 265:ssä 493:sta on spottiseparaatioita, ja Mapper-tiedostoista 98:ssa 112:sta on spottivärejä.
- **Testikartat:** tekstin piirto riippuu koneen fonteista, ja tämän koneen fontit eroavat CI:n fonteista, joten testikartoissa ei ole tekstiä. Kartat ovat haarassa `feature/inks` tuontitestien kansioissa: Mapper-kartta kansiossa `src/MapModel/TestFiles/openmapper/` ja OCAD-kartat kansiossa `src/MapModel/TestFiles/ocadimport/`. Jos Skian piirtotesti käyttää karttaa, se kuuluu kansioon `src/MapModel/TestFiles/skia_render/` testikuvauksen ja baseline-kuvan viereen.
  - `Hakola malli.omap` (Mapper): 8 spottimustetta, 57 koostetta (rastereita, kahden musteen sekoituksia ja komponentteja, joiden kerroin on 0), värejä, joilla knockout on päällä ja pois, sama kooste kahdella prioriteetilla, jotka eivät ole peräkkäisiä, sekä 2 943 objektia. Väri "Ruskea 100% - Pistekohde" on pelkkä CMYK-väri ilman spottimäärittelyä, ja 214 objektia käyttää sitä.
  - `Hakola malli OCAD12.ocd` (OCAD 12): 8 separaatiota ja 67 väriä, joista 58:lla on `s`- ja `p`-pareja. Väri "Registration black (all printed colors)" kuuluu kaikkiin separaatioihin, ja väri "Ruskea 100% - Pistekohde" on päällepainettu (`o1`) ilman separaatioita. Merkkijonojen muoto on sama versioissa 9–2018.
  - `Hakola malli OCAD8.ocd` (OCAD 8): samat separaatiot ja värit OCAD 6–8:n binäärirakenteissa sekä 2 972 objektia. Prosenttiosuuksissa on sekä 0 että 255 (ei osuutta). Muoto rajaa separaation nimen 15 ja värin nimen 31 merkkiin, joten esimerkiksi "PMS136_Keltainen" on siinä "PMS136_Keltaine".
- **Prosessimusteiden rasterit:** prosessimusteet C, M, Y ja K ovat kaikkien karttojen yhteisiä, eikä niillä ole rasteria. Vain OCAD 6–8 määrittää prosessimusteille rasterit symboliotsakkeessaan, mutta OCAD 6–8 -tiedostoja ei enää käytetä painatukseen (@Pauli, 18.11.). Rasterit säilyvät OCAD 6–8 -kierroksessa symboliotsakkeen mukana.
- **Mapper-värien knockout:** OOM on Mapper-karttojen piirron auktoriteetti, ja siinä knockout koskee vain värejä, joilla on spottimäärittely. Upstream tulkitsee jokaisen Mapper-värin, jolla ei ole `spotcolors`-elementtiä, päällepainetuksi (`!knockout`). Tämä tiketti korjaa tulkinnan.
- **Rajaus:** tämä tiketti ei muuta koostamista, ja musteisiin perustuva koostaminen on oma tikettinsä. Sen pohjaksi sopii OOM:n päällepainatussimulaatio ([renderable.cpp](https://github.com/OpenOrienteering/mapper/blob/master/src/core/renderables/renderable.cpp)). Se piirtää jokaiselle musteelle oman separaation prioriteettijärjestyksessä: koosteet piirretään kertoimellaan, knockout pyyhkii ja myöhempi merkintä korvaa aiemman. Sen jälkeen separaatio sävytetään musteen CMYK-arvolla ja kerrotaan kuvaan. Speksin kuvan 7 mukaan päällepainatus säilyttää sen alla olevan musteen, jota päällä olevassa värissä ei ole. Myöskään palettimerkinnän opasiteetti ei kuulu tähän tikettiin.

**Erotetut alatehtävät**

- [002](002-mapper-export-color-numbers.md) korjaa Mapper-viennin värinumerot, joihin koosteiden viittaukset spottiväreihin perustuvat. Se eriytettiin tästä tiketistä haaraksi `fix/mapper-export-color-numbers`, ja haara on yhdistetty feature/inksiin.
- [003](003-ocad11-utf8-strings.md) korjaa OCAD 11:n ja uudempien merkkijonojen koodauksen, joka löytyi tämän tiketin OCAD 12 -testikartan värien nimistä. Sen haara `fix/ocad11-utf8-strings` ei ole feature/inksissä, ja tämän tiketin testit hakevat värit ASCII-nimillä.
