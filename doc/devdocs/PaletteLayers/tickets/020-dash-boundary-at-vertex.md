# Taitepisteen lähellä alkava katko saa taitepisteen ja viisteen kulman

**Konteksti**  
`SymPath.DrawDashedCore` piirtää jokaisen katkon murtoviivana katkon alusta tasoitetun polun välipisteiden kautta katkon loppuun. Kun katkon alku osuu hyvin vähän ennen taitepistettä, katkoon tulee taitepiste ja sen edelle lähes nollan mittainen osa. Viiva saa silloin liitoksen taitepisteessä, ja kaksoisviivan reunan katko, joka tehdään siirtämällä keskiviivan katko sivuun (`OffsetPointsRight`), saa ulkoreunalla viisteen kulman. OCAD aloittaa tällaisen katkon taitepisteestä ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md), neljäs näyte).

**Toistamisohjeet**  
1. Piirrä oraakkelin neljännen näytteen tiedosto `dashes-4-ocad12-euclidean.ocd`.
2. Mittaa solun DB16-2 vasemman reunan katkot reunan polkua pitkin ja vertaa niitä OCAD-viewerin vientiin.
3. Rajan mittaa kuudes näyte: `measure-corners.py dashes-6-cells.json ocad-viewer-dashes-6-ocad12-euclidean-1200dpi.png` kertoo kunkin solun kulman ja katkon rajan piirroksessa.

**Odotettu vs. Todellinen käytös**  
`odotettu:` keskiviivan katko alkaa 0,004 mm ennen toista taitepistettä, ja OCADin tavoin ulkoreunan katko alkaa viisteen kulman jälkeen, reunan polulla kohdasta 15,74 mm, ja on 2,0 mm pitkä.  
`todellinen:` Purple Penin ulkoreunan katko alkaa kohdasta 14,01 mm, ennen viisteen kulmaa, ja on 1,74 mm pidempi. Solussa DB6-2 ero on 0,76 mm.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Katkon raja, joka on OCADin toleranssin sisällä taitepisteestä, on taitepisteessä. Taitepisteestä alkava katko alkaa seuraavalta osalta, ja taitepisteeseen päättyvä katko päättyy siihen. Viidennessä näytteessä OCAD ei ottanut viisteen kulmaa yhteenkään katkoon, jonka alku oli enintään 0,02 mm ennen taitepistettä tai loppu enintään 0,004 mm sen jälkeen.

Kuudes näyte, jonka @Pauli vei 600 ja 1200 dpi:llä, näytti, että OCAD jättää taitepisteen yli menevän lyhyen pätkän kokonaan pois: katko alkaa tai päättyy taitepisteeseen. Raja riippuu viivan leveydestä. 0,3 mm:n viivalla ja 0,15 mm:n reunoilla OCAD jättää pätkän pois noin 0,055 mm:iin asti ja piirtää sen noin 0,06 mm:stä alkaen, alussa ja lopussa samoin, kummallakin tarkkuudella ja kummallakin reunojen etäisyydellä. 1,0 mm:n viivalla raja riippuu viennin tarkkuudesta: 600 dpi:llä se on noin 0,09–0,10 mm ja 1200 dpi:llä 0,16–0,21 mm. Nämä rajat ovat etäisyyksiä OCADin piirroksessa. OCADin katkojen rajat ovat taitepisteen kohdalla 0,01–0,02 mm aiemmin kuin tarkassa laskussa, joten esimerkiksi tarkka etäisyys 0,05 mm on piirroksessa alussa 0,048–0,067 mm ja lopussa 0,029–0,046 mm. Tarkkuudesta riippumaton sääntö voi siis noudattaa OCADia ohuilla viivoilla, mutta leveällä viivalla vain toista vientiä. @Pauli päätti, että OCADin oma raja selvitetään ensin näytteen PDF-viennistä, jossa katkojen päät ovat koordinaatteina ilman pikselöintiä, ja sääntö valitaan sen jälkeen (19.11.). Myös tiketin [011](011-line-geometry.md) koostettu katkoviiva noudattaa samaa sääntöä.

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun kaksoisviivan reunoja verrattiin OCADiin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
