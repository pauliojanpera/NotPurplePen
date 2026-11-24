# OCAD-vienti kirjoittaa värien prosenttiosuudet float-kertolaskun virheineen

**Konteksti**  
OCAD 9 ja uudemmat pitävät palettivärin (`SymColor`) CMYK-osuudet värin parametrimerkkijonon (tyyppi 9) kentissä
`c`, `m`, `y` ja `k` prosentteina. `OcadExport` kirjoittaa ne lausekkeella
`(cyan * 100F).ToString(CultureInfo.InvariantCulture)`: float-kertolaskun tulo pyöristyy floatiksi, ja `ToString`
kirjoittaa pyöristyneen tulon lyhimmän esityksen. Tuonti lukee osuuden lausekkeella `GetParamFloat(...) / 100.0F`,
joten OCADin kirjoittama `m27` palaa viennissä muotoon `m27.000002`.

- Kokonaisluvuista 0–100 muuttuvat näin seitsemän: 15, 27, 30, 53, 54, 59 ja 60 kirjoittuvat muotoihin
  15.000001, 27.000002, 30.000002, 52.999996, 54.000004, 58.999996 ja 60.000004. Puolen prosentin välein olevista
  arvoista muuttuvat lisäksi esimerkiksi 7.5, 13.5 ja 26.5.
- OCAD itse kirjoittaa osuudet kokonaislukuina (OCAD 12 ja 2018) tai yhden desimaalin tarkkuudella (OCAD 9 ja 11,
  esimerkiksi `c30.0` ja `m13.5` tiedostossa `LordHill.ocd`).
- Ero on enintään 0,000004 prosenttiyksikköä, joten se ei näy piirrossa. Se muuttaa kuitenkin tiedostoon kirjoitetun
  luvun: kun OCAD-tiedosto kulkee Purple Penin kautta, sen osuudet muuttuvat, vaikka värit eivät muutu.
- Vika on upstreamissa (d48f4456, `CreateColorStringParameters`). Upstreamin omat ratojen tekstibaselinet
  sisältävät sen: `ControllerTests`in `CreateOcadFiles`-testien 13 odotettua dumppia antavat värien Purple ja
  Lower purple osuudeksi `code=c, value=15.000001`. Dumpit ovat kansioissa `src/TestFiles/controller/ocad_create2`
  (3), `ocad_create8` (1), `ocad_create9` (5) ja `ocad_create10` (4).
- Haarassa `feature/palette-stack` samalla tavalla kirjoitetaan myös tiketin 001 lisäämät spottivärierottelujen kentät:
  värin osuus spottivärierottelussa (`p`, `component.Factor * 100F`), spottivärierottelun parametrimerkkijonon
  (tyyppi 10) ulkoasun `c`, `m`, `y` ja `k` (`ink.Appearance.Cyan * 100F`) sekä rasterin taajuus kymmenyksinä (`f`,
  `screen.Frequency * 10F`).

**Toistamisohjeet**  
1. Lue OCAD 12 -kartta `src/MapModel/TestFiles/rendering/wholestructure.ocd` (`InputOutput.ReadFile`).
2. Kirjoita se OCAD 12 -tiedostoksi (`InputOutput.WriteFile`).
3. Lue kirjoitetun tiedoston värien parametrimerkkijonot.

**Odotettu vs. Todellinen käytös**  
`odotettu:` osuudet ovat samat luvut kuin lähdetiedostossa: värillä 50 "Upper Purple for Course Overprint" `c30 m100`
ja värillä 24 "Yellow 100%/Green 50%" `c40 m30 y90`. OCAD 9 -kartan `rendering/LordHill.ocd` värillä 27 "Yellow
50%/Green 50%" `m13.5`. Ratojen OCAD-viennissä purppuralla `c15`.  
`todellinen:` vienti kirjoittaa `c30.000002`, `m30.000002` ja `m13.500001`, ja ratojen OCAD-vienti `c15.000001`.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes, feature/palette-stack / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Vienti kirjoittaa osuuden desimaalilukuna, jota float-osuus edustaa. Muunnos `decimal`iksi pyöristää floatin seitsemään
merkitsevään numeroon, ja kertolasku 100:lla tehdään desimaaliluvuilla, joten osuus 0,27 kirjoittuu muotoon `27`
ja osuus 0,135 muotoon `13.5`. Yksi apufunktio kirjoittaa näin kaikki prosentti- ja kymmenyskentät, joten osuus,
jonka tuonti luki OCADin merkkijonosta, kirjoittuu takaisin samana lukuna, kun merkkijonossa on enintään seitsemän
merkitsevää numeroa.

- Haara `fix/ocad-color-shares` juurtuu kohtaan, jossa base kohtaa upstreamin (d48f4456), jotta korjauksen voi tarjota
  upstreamiin sellaisenaan. Se korjaa värin parametrimerkkijonon kentät `c`, `m`, `y` ja `k` ja päivittää 13
  tekstidumppia (`15.000001` → `15`).
- Testi kierrättää kartat `rendering/wholestructure.ocd` (OCAD 12) ja `rendering/LordHill.ocd` (OCAD 9) ja vertaa
  kirjoitettujen värien osuuksia lähdetiedoston osuuksiin lukuina.
- Spottivärierottelujen kentät ovat vain haarassa `feature/palette-stack`, joten ne saavat saman kirjoitustavan, kun
  korjaus yhdistetään siihen. Tiketin 004 `t`-rivit ovat samoissa 13 dumpissa, joten yhdistäminen ratkaisee niiden
  ristiriidat.
- Korjaus ei muuta piirtoa eikä kuvabaselineja. Tekstidumppien muutos näkyy täydessä ajossa, koska keräävä ajo ei kerää
  niitä.

Vika löytyi tiketin [027](027-ocad-blend-mode-tests.md) näytteestä: väri Yellow (5) C0 M27 Y79 kirjoittui muotoon
`m27.000002`, ja näyte käyttää sen tilalla arvoa C0 M25 Y80. Tiketti 027 ei odota korjausta.

Luonnos: @Pauli pyysi tämän tiketin 24.11. Se toteutetaan vasta, kun hän hyväksyy sen.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
