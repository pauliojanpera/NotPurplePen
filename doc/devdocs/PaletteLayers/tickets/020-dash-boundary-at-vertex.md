# Taitepisteen lähellä alkava katko saa taitepisteen ja viisteen kulman

**Konteksti**  
`SymPath.DrawDashedCore` piirtää jokaisen katkon murtoviivana katkon alusta tasoitetun polun välipisteiden kautta katkon loppuun. Kun katkon alku osuu hyvin vähän ennen taitepistettä, katkoon tulee taitepiste ja sen edelle lähes nollan mittainen osa. Viiva saa silloin liitoksen taitepisteessä, ja kaksoisviivan reunan katko, joka tehdään siirtämällä keskiviivan katko sivuun (`OffsetPointsRight`), saa ulkoreunalla viisteen kulman. OCAD aloittaa tällaisen katkon taitepisteestä ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md), neljäs näyte).

**Toistamisohjeet**  
1. Piirrä oraakkelin neljännen näytteen tiedosto `dashes-4-ocad12-euclidean.ocd`.
2. Mittaa solun DB16-2 vasemman reunan katkot reunan polkua pitkin ja vertaa niitä OCAD-viewerin vientiin.

**Odotettu vs. Todellinen käytös**  
`odotettu:` keskiviivan katko alkaa 0,004 mm ennen toista taitepistettä, ja OCADin tavoin ulkoreunan katko alkaa viisteen kulman jälkeen, reunan polulla kohdasta 15,74 mm, ja on 2,0 mm pitkä.  
`todellinen:` Purple Penin ulkoreunan katko alkaa kohdasta 14,01 mm, ennen viisteen kulmaa, ja on 1,74 mm pidempi. Solussa DB6-2 ero on 0,76 mm.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Katkon raja, joka on OCADin toleranssin sisällä taitepisteestä, on taitepisteessä. Taitepisteestä alkava katko alkaa seuraavalta osalta, ja taitepisteeseen päättyvä katko päättyy siihen. Toleranssi selvitetään viidennellä näytteellä, jossa katkon alku tai loppu on 0–0,02 mm:n päässä taitepisteestä. OCADin koordinaattien yksikkö on 0,01 mm. Myös tiketin [011](011-line-geometry.md) koostettu katkoviiva noudattaa samaa sääntöä.

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun kaksoisviivan reunoja verrattiin OCADiin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
