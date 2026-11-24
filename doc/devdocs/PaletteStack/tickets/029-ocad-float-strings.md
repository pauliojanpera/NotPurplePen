# OCAD-vienti kirjoittaa float-laskun virheen värien osuuksiin ja asettelutekstien fonttikokoihin

**Konteksti**  
`OcadExport` kirjoittaa parametrimerkkijonoihin float-laskun tuloja sellaisinaan: `ToString` kirjoittaa pyöristyneen
tulon lyhimmän esityksen. Tuonti on laskenut floatin OCADin luvusta käänteisellä laskulla, joten vienti kirjoittaa
monesti muun luvun kuin OCAD. Esimerkiksi `m27` kirjoittuu muotoon `m27.000002`, ja 12 pisteen fonttikoko muotoon
`12.000002`. Ero on luvun seitsemännessä merkitsevässä numerossa, joten se ei näy piirrossa. Se muuttaa kuitenkin
tiedostoon kirjoitetun luvun: kun OCAD-tiedosto kulkee Purple Penin kautta, sen luvut muuttuvat, vaikka kartta ei
muutu.

- **Värien osuudet.** OCAD 9 ja uudemmat pitävät palettivärin (`SymColor`) CMYK-osuudet värin parametrimerkkijonon
  (tyyppi 9) kentissä `c`, `m`, `y` ja `k` prosentteina. Vienti kirjoittaa ne lausekkeella
  `(cyan * 100F).ToString(CultureInfo.InvariantCulture)`, ja tuonti lukee ne lausekkeella `GetParamFloat(...) / 100.0F`.
  - Kokonaisluvuista 0–100 muuttuvat seitsemän: 15, 27, 30, 53, 54, 59 ja 60 kirjoittuvat muotoihin 15.000001,
    27.000002, 30.000002, 52.999996, 54.000004, 58.999996 ja 60.000004. Puolen prosentin välein olevista arvoista
    muuttuvat lisäksi esimerkiksi 7.5, 13.5 ja 26.5.
  - OCAD itse kirjoittaa osuudet kokonaislukuina (OCAD 12 ja 2018) tai yhden desimaalin tarkkuudella (OCAD 9 ja 11,
    esimerkiksi `c30.0` ja `m13.5` tiedostossa `LordHill.ocd`).
- **Asettelutekstien fonttikoot.** Asettelutason tekstin (`ImageTextSymbol`) fonttikoko on Purple Penissä millimetreinä
  (`FontSize`). Vienti kirjoittaa sen pisteinä lausekkeella
  `(FontSize / 25.4F * 72F).ToString(CultureInfo.InvariantCulture)` kahteen paikkaan: OCAD 12:n ja uudempien
  tekstiobjektin merkkijonon kenttään `s` (objektin merkkijonon tyyppi 3) ja OCAD 11:n asettelufonttien
  parametrimerkkijonon kenttään `s`. Tuonti muuntaa pisteet millimetreiksi lausekkeella `points / 72F * 25.4F`
  (`Geometry.MmFromPoints`).
  - Kokonaisista pistekoista 1–100 muuttuvat kymmenen: 3, 6, 12, 24, 48, 51, 55, 67, 95 ja 96. Esimerkiksi 12
    kirjoittuu muotoon `12.000002` ja 55 muotoon `54.999996`.
  - Testikartoista yhdeksässä (OCAD 11, 12 ja 2018, esimerkiksi `rendering/layout_objects12.ocd`) on 12 pisteen
    tekstejä. Lähdetiedostoissa ne ovat `s12`, ja Purple Penin kirjoittamassa OCAD 12 -tiedostossa
    `s12.000002`. Sama luku tulee OCAD 11 -tiedoston asettelufonttien merkkijonoon.
  - Symbolimäärittelyjen fonttikoot vienti pyöristää binäärisessä symbolitietueessa pisteen kymmenyksiin
    (`Math.Round(PointsFromMm(...) * 10)`), joten niissä vikaa ei ole.
- **Upstream.** Vika on upstreamissa (d48f4456): värin parametrimerkkijono (`CreateColorStringParameters`),
  asettelutekstin objektin merkkijono ja OCAD 11:n asettelufonttien merkkijonot. Upstreamin omat ratojen
  tekstibaselinet sisältävät sen: `ControllerTests`in `CreateOcadFiles`-testien 13 odotettua dumppia antavat värien
  Purple ja Lower purple osuudeksi `code=c, value=15.000001`. Dumpit ovat kansioissa
  `src/TestFiles/controller/ocad_create2` (3), `ocad_create8` (1), `ocad_create9` (5) ja `ocad_create10` (4).
- **Palettipinohaara.** Haarassa `feature/palette-stack` samalla tavalla kirjoitetaan myös tiketin 001 lisäämät
  spottivärierottelujen kentät: värin osuus spottivärierottelussa (`p`, `component.Factor * 100F`),
  spottivärierottelun parametrimerkkijonon (tyyppi 10) ulkoasun `c`, `m`, `y` ja `k` (`ink.Appearance.Cyan * 100F`)
  sekä rasterin taajuus kymmenyksinä (`f`, `screen.Frequency * 10F`).

**Toistamisohjeet**  
1. Lue OCAD 12 -kartat `src/MapModel/TestFiles/rendering/wholestructure.ocd` ja `rendering/layout_objects12.ocd`
   (`InputOutput.ReadFile`).
2. Kirjoita kumpikin OCAD 12 -tiedostoksi (`InputOutput.WriteFile`).
3. Lue kirjoitettujen tiedostojen värien parametrimerkkijonot ja asettelutekstien objektien merkkijonot.

**Odotettu vs. Todellinen käytös**  
`odotettu:` luvut ovat samat kuin lähdetiedostossa: `wholestructure.ocd`:n värillä 50 "Upper Purple for Course
Overprint" `c30 m100` ja värillä 24 "Yellow 100%/Green 50%" `c40 m30 y90`, ja `layout_objects12.ocd`:n kahdella
12 pisteen tekstillä `s12`. OCAD 9 -kartan `rendering/LordHill.ocd` värillä 27 "Yellow 50%/Green 50%" `m13.5`.
Ratojen OCAD-viennissä purppuralla `c15`.  
`todellinen:` vienti kirjoittaa `c30.000002`, `m30.000002`, `s12.000002` ja `m13.500001`, ja ratojen OCAD-vienti
`c15.000001`. OCAD 11 -tiedostoon vienti kirjoittaa asettelufontin koon samoin muotoon `s12.000002`.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes, feature/palette-stack / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Vienti kirjoittaa lyhimmän desimaaliluvun, jonka tuonti lukee takaisin samaksi floatiksi. Se laskee luvun
double-tarkkuudella, pyöristää sen yhteen, kahteen ja niin edelleen yhdeksään merkitsevään numeroon ja valitsee
ensimmäisen, jonka tuonnin lasku vie samaan floatiin. Näin osuus 0,27 kirjoittuu muotoon `27`, osuus 0,135 muotoon
`13.5` ja 12 pisteen fonttikoko (4,2333336 mm) muotoon `12`. Pelkkä pyöristys seitsemään merkitsevään numeroon ei riitä,
koska fonttikoon kaksi float-laskua siirtävät esimerkiksi 73 pisteen koon muotoon `72.99999`. Menetelmä antaa takaisin
kaikki osuudet 0–100 puolen prosentin välein, fonttikoot 1–200 pistettä puolen pisteen välein ja rasterin taajuudet
0–3000 lpi 0,1 lpi:n välein (laskettu float-aritmetiikalla 24.11.). Yksi apufunktio kirjoittaa näin kaikki nämä
kentät.

- Haara `fix/ocad-float-strings` juurtuu kohtaan, jossa base kohtaa upstreamin (d48f4456), jotta korjauksen voi
  tarjota upstreamiin sellaisenaan. Se korjaa värin parametrimerkkijonon kentät `c`, `m`, `y` ja `k` sekä
  asettelutekstien fonttikoot ja päivittää 13 tekstidumppia (`15.000001` → `15`).
- Testi kierrättää kartat `rendering/wholestructure.ocd` (OCAD 12), `rendering/LordHill.ocd` (OCAD 9),
  `rendering/layout_objects12.ocd` (OCAD 12) ja `rendering/layout_objects11.ocd` (OCAD 11) ja vertaa kirjoitettuja
  osuuksia ja fonttikokoja lähdetiedoston lukuihin.
- Spottivärierottelujen kentät ovat vain haarassa `feature/palette-stack`, joten ne saavat saman kirjoitustavan, kun
  korjaus yhdistetään siihen. Tiketin 004 `t`-rivit ovat samoissa 13 dumpissa, joten yhdistäminen ratkaisee niiden
  ristiriidat.
- Korjaus ei muuta piirtoa eikä kuvabaselineja. Tekstidumppien muutos näkyy täydessä ajossa, koska keräävä ajo ei
  kerää niitä.

Värien osuuksien vika löytyi tiketin [027](027-ocad-blend-mode-tests.md) näytteestä: väri Yellow (5) C0 M27 Y79
kirjoittui muotoon `m27.000002`, ja näyte käyttää sen tilalla arvoa C0 M25 Y80. Fonttikokojen vian istunto tarkisti
@Paulin pyynnöstä testikartoilla. Tiketti 027 ei odota korjausta.

Luonnos: @Pauli pyysi tämän tiketin 24.11. ja lisäsi siihen asettelutekstien fonttikoot. Se toteutetaan vasta, kun
hän hyväksyy sen.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
