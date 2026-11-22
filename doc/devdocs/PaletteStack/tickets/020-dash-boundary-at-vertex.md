# Taitepisteen lähellä alkava katko saa taitepisteen ja viisteen kulman

**Konteksti**  
`SymPath.DrawDashedCore` piirtää jokaisen katkon murtoviivana katkon alusta litistetyn polun välipisteiden kautta katkon loppuun. Kun katkon alku osuu hyvin vähän ennen taitepistettä, katkoon tulee taitepiste ja sen edelle lähes nollan mittainen osa. Viiva saa silloin liitoksen taitepisteessä, ja kaksoisviivan reunan katko, joka tehdään siirtämällä keskiviivan katko sivuun (`OffsetPointsRight`), saa ulkoreunalla viisteen kulman. OCAD jättää tällaisesta katkosta taitepisteen pois, joten katko kulkee alustaan suoraan seuraavaan pisteeseen ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md), neljäs ja kuudes näyte).

**Toistamisohjeet**  
1. Piirrä oraakkelin neljännen näytteen tiedosto `dashes-4-ocad12-euclidean.ocd`.
2. Mittaa solun DB16-2 vasemman reunan katkot reunan polkua pitkin ja vertaa niitä OCAD-viewerin vientiin.
3. Säännön mittaavat kuudes ja seitsemäs näyte: esimerkiksi `measure-pdf-vertices.py dashes-7-cells.json ocad-viewer-dashes-7-ocad12-euclidean.pdf` kertoo kunkin solun PDF-viennistä, säilyykö taitepiste katkossa ja kuinka pitkä pätkä taitepisteen yli on OCADin yksikköinä.

**Odotettu vs. Todellinen käytös**  
`odotettu:` keskiviivan katko alkaa 0,004 mm ennen toista taitepistettä, ja OCADin tavoin ulkoreunan katko alkaa viisteen kulman jälkeen, reunan polulla kohdasta 15,74 mm, ja on 2,0 mm pitkä.  
`todellinen:` Purple Penin ulkoreunan katko alkaa kohdasta 14,01 mm, ennen viisteen kulmaa, ja on 1,74 mm pidempi. Solussa DB6-2 ero on 0,76 mm.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Kun katkon pätkä taitepisteen yli on lyhyempi kuin OCADin raja, katkoon ei oteta taitepistettä: katko kulkee rajastaan suoraan seuraavaan pisteeseen, kuten OCADin PDF-viennissä. Katkon raja ei siirry.

Seitsemännen näytteen PDF-vienti antoi säännön. Yksinkertaisella viivalla raja on 0,4 × viivan leveys taitteen kulmasta riippumatta: leveyksillä 0,06–1,0 mm ja kulmilla 30–150° OCAD jättää taitepisteen pois, kun pätkä on lyhyempi kuin 0,4 × leveys, ja pitää sen, kun pätkä on tasan raja (1,0 mm:n viivalla 40,00 yksikköä). Sääntö koskee vain katkon alkua ja loppua lähinnä olevaa taitepistettä. Se ei toistu, eikä se koske katkon sisäisiä taitepisteitä eikä ehjää viivaa. Kaksoisviivalla keskiviivan katko jättää taitepisteen pois noin 0,05 mm:iin asti (4,5–5,8 yksikköä), ja kumpikin reuna soveltaa sen jälkeen omaan katkoonsa yksinkertaisen viivan sääntöä reunan leveydellä. Sisäreunan oma pätkä viisteen kulmaan on |pätkä − h·tan(kulma/2)|, missä h on reunan etäisyys polusta. OCAD pyöristää katkojen pisteet 0,01 mm:n ruudukkoon, joten rajan kohdalla tarkkoihin pätkiin perustuva sääntö ja OCAD voivat erota yksikön verran.

Bittikarttaviennit noudattavat yksinkertaisilla viivoilla omaa, tarkkuudesta riippuvaa sääntöään. PDF-vienti on OCADin geometria. @Pauli päätti, että OCADin raja selvitetään PDF-viennistä ja sääntö valitaan sen jälkeen, ja valitsi sitten OCADin säännön kaikkine osineen: 0,4 × leveys yksinkertaiselle viivalle, 0,05 mm kaksoisviivan keskiviivalle ja 0,4 × reunan leveys kummallekin reunalle (19.11.). Myös tiketin [011](011-line-geometry.md) koostettu katkoviiva noudattaa samaa sääntöä.

Korjaus on haarassa `fix/dash-boundary-at-vertex` (4b0b22a7). `SymPath.DrawDash` piirtää yhden katkon ja jättää siitä pois päätä lähimmän taitepisteen kahdella rajalla: polun katkolle ennen reunan siirtoa ja piirrettävälle katkolle. `LineSymDef` antaa rajat OCADin välistykselle: pääviivalle 0,4 × leveys, kaksoisviivan täytölle ja reunoille 0,05 mm ja sitten 0,4 × täytön tai reunan leveys. Mapperin välistykset pitävät kaikki taitepisteet. Käyrän litistyspisteitä sääntö ei koske. `OcadDashVertexTests` vertaa seitsemännen näytteen 74 solua OCADin PDF-vientiin, ja ilman korjausta 38 niistä epäonnistuu. Katkotun täytön sääntöä ei ole tarkistettu OCADista. Kahdeksas näyte tarkistaa sen. Korjaus siirtää 124 kuvabaselinea keräävästä ajosta 44441352883 (363615c7), ja kolme basen omaa route gadget -baselinea on kansiossa `prs/020-dash-boundary-at-vertex-base/` basen yhdistämistä varten. Täysi ajo basen päällä meni läpi (Tests 44443705594, Build 44443705749). Haara on yhdistetty haaraan `feature/palette-layers` (581e7588f) ennen tiketin 011 työtä (@Pauli, 19.11.). Yhdistetyn haaran keräävä ajo 44445217631 piirsi baselinet kaikkien korjausten kanssa (de72040a), ja sen täysi ajo meni läpi (Tests 44447019786, Build 44447019560).

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun kaksoisviivan reunoja verrattiin OCADiin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
