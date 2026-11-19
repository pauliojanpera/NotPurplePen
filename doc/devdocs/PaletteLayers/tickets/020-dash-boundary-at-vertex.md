# Taitepisteen lähellä alkava katko saa taitepisteen ja viisteen kulman

**Konteksti**  
`SymPath.DrawDashedCore` piirtää jokaisen katkon murtoviivana katkon alusta tasoitetun polun välipisteiden kautta katkon loppuun. Kun katkon alku osuu hyvin vähän ennen taitepistettä, katkoon tulee taitepiste ja sen edelle lähes nollan mittainen osa. Viiva saa silloin liitoksen taitepisteessä, ja kaksoisviivan reunan katko, joka tehdään siirtämällä keskiviivan katko sivuun (`OffsetPointsRight`), saa ulkoreunalla viisteen kulman. OCAD jättää tällaisesta katkosta taitepisteen pois, joten katko kulkee alustaan suoraan seuraavaan pisteeseen ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md), neljäs ja kuudes näyte).

**Toistamisohjeet**  
1. Piirrä oraakkelin neljännen näytteen tiedosto `dashes-4-ocad12-euclidean.ocd`.
2. Mittaa solun DB16-2 vasemman reunan katkot reunan polkua pitkin ja vertaa niitä OCAD-viewerin vientiin.
3. Rajan mittaa kuudes näyte: `measure-pdf-vertices.py dashes-6-cells.json ocad-viewer-dashes-6-ocad12-euclidean.pdf` kertoo kunkin solun PDF-viennistä, säilyykö taitepiste katkossa ja kuinka pitkä pätkä taitepisteen yli on OCADin yksikköinä.

**Odotettu vs. Todellinen käytös**  
`odotettu:` keskiviivan katko alkaa 0,004 mm ennen toista taitepistettä, ja OCADin tavoin ulkoreunan katko alkaa viisteen kulman jälkeen, reunan polulla kohdasta 15,74 mm, ja on 2,0 mm pitkä.  
`todellinen:` Purple Penin ulkoreunan katko alkaa kohdasta 14,01 mm, ennen viisteen kulmaa, ja on 1,74 mm pidempi. Solussa DB6-2 ero on 0,76 mm.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Kun katkon pätkä taitepisteen yli on lyhyempi kuin OCADin raja, katkoon ei oteta taitepistettä: katko kulkee rajastaan suoraan seuraavaan pisteeseen, kuten OCADin PDF-viennissä. Katkon raja ei siirry.

Kuudennen näytteen PDF-viennissä raja on yksinkertaisella viivalla puolet viivan leveydestä. 1,0 mm:n viivalla taitepiste jää pois 30,36 yksikön (0,01 mm) pätkästä ja säilyy 50,01 yksikön pätkässä, ja 0,3 mm:n viivalla se jää pois 10,77 yksikön pätkästä ja säilyy 15,52 yksikön pätkässä. Kaksoisviivalla, jolla ei ole pääviivaa, raja on 4,47–5,39 yksikköä kummallakin reunojen etäisyydellä. OCAD pyöristää katkojen pisteet 0,01 mm:n ruudukkoon. Näytteen kaikki taitteet ovat 90°, ja siinä kulmassa puolet leveydestä on myös viisteen ulottuma osan suuntaan. Seitsemäs näyte selvittää siksi, riippuuko raja taitteen kulmasta, kaksoisviivan reunojen leveydestä tai pääviivasta.

Bittikarttaviennit noudattavat yksinkertaisilla viivoilla omaa, tarkkuudesta riippuvaa sääntöään: 1200 dpi:llä 1,0 mm:n viiva saa kulman jo 0,20 mm:n pätkästä. PDF-vienti on OCADin geometria. @Pauli päätti, että OCADin raja selvitetään PDF-viennistä ja sääntö valitaan sen jälkeen (19.11.). Myös tiketin [011](011-line-geometry.md) koostettu katkoviiva noudattaa samaa sääntöä.

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun kaksoisviivan reunoja verrattiin OCADiin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
