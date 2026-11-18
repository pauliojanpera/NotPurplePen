# OCAD-viivan katkojen määrä poikkeaa OCADin piirrosta

**Konteksti**  
OCAD laskee viivan katkojen ja välein sijoitettujen glyyfien määrän toisin kuin `LineSymDef.ComputeDashDistances` viidessä tilanteessa ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md)):
- **Bezier-käyrä.** OCAD laskee välien määrän pituudesta, jossa jokainen Bezier-käyrä on neljä jännettä parametrin arvojen 0, ¼, ½, ¾ ja 1 pisteiden välillä, kartan metriikalla mitattuna. Purple Pen mittaa hienoksi tasoitetun käyrän (`SymPath.GetFlattenedPoints`). Katkot OCAD sijoittaa koko käyrälle. Tämä pituus sopii näytteiden kaikkiin 28 Bezier-käyrään kummallakin metriikalla, eikä mikään muu jänteiden määrä väliltä 2–16 sovi kaikkiin kummallakin.
- **Puolikas.** Kun päätykatkot ovat katkon mittaisia, OCAD antaa viivalle katkoja lähimmän kokonaisluvun verran lausekkeesta (L + väli + 2 × katko − 2 × päätykatko) / (katko + väli), ja tasan puolikkaassa parillisen määrän. Purple Pen pyöristää välien määrän puolikkaassa parilliseksi (`Math.Round`).
- **Lyhyt viiva.** Kun viiva ei ole päätykatkojaan ja väliä pidempi, OCAD antaa sille välin vasta, kun se on pidempi kuin päätykatkojen puolikkaat ja väli. Purple Pen antaa välin jo samanpituiselle viivalle (`pathLength >= firstDashLength / 2 + lastDashLength / 2 + gapLength`).
- **Päätykatkot 0.** Kun sääntö antaa viivalle yhden välin ja päätykatkot ovat 0, OCAD piirtää viivan ehjänä, mutta Purple Pen ei piirrä mitään, koska koko viiva on väliä.
- **Toissijainen väli.** OCAD ei lisää toissijaista väliä katkoon, joka on tasan puolet katkon pituudesta. Purple Pen lisää sen katkoon, joka on vähintään puolet (`actualDashLength >= dashLength / 2`).

Välein sijoitetut glyyfit noudattavat samoja sääntöjä kuin katkot: tasan puolikkaassa glyyfien määrä plus yksi on parillinen, ja Bezier-käyrä mitataan neljänä jänteenä (kolmas näyte).

Lisäksi 64-osainen murtoviiva sopii vain, jos OCAD pyöristää jokaisen osan pituuden 0,01 mm:iin. Sen tarkka pituus antaa 9,5009, ja pyöristetyt osat tasan 9,5.

**Toistamisohjeet**  
1. Piirrä oraakkelin toisen näytteen tiedostot `dashes-2-ocad12-euclidean.ocd` ja `dashes-2-ocad12-ocad-metric.ocd` sekä kolmannen näytteen tiedosto `dashes-3-ocad12-euclidean.ocd` 600 dpi:llä.
2. Mittaa katkot ja glyyfit `measure-dashes.py`:llä ja vertaa niitä OCAD-viewerin vientiin.

**Odotettu vs. Todellinen käytös**  
`odotettu:` solujen katkojen määrät ovat kuten OCAD-viewerin viennissä: A1-1 ja A1-7 10, A2-8 1, B1-4, B1-16 ja B2-15 1, euklidisessa tiedostossa DS-3, DS-4, Darch-3 ja Darch-4 10, DS-9 ja Darch-9 11 ja D4F-1 10, ja OCADin metriikan tiedostossa Darch-4 11 ja Darch-9 12. Kolmannen näytteen solussa MS2-5 on 3 katkoa, ja soluissa GSstraight-3 ja GSstraight-6 5 glyyfiä, GSshort-6:ssa 1 sekä GScurve-3:ssa ja GScurve-4:ssä 9.  
`todellinen:` Purple Pen piirtää A1-1:een 9 katkoa, A1-7:ään 11, A2-8:aan ei mitään, B1-4:ään ja B1-16:een 2, B2-15:een 4 ja MS2-5:een 4, ja muihin toisen näytteen soluihin yhden katkon enemmän kuin OCAD. Glyyfejä se piirtää GSstraight-3:een 4, GSstraight-6:een 6, GSshort-6:een 2 sekä GScurve-3:een ja GScurve-4:ään 10.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
`ComputeDashDistances` saa välien määrän laskuun OCADin pituuden, jossa jokainen Bezier-käyrä mitataan neljänä jänteenä ja jokaisen osan pituus pyöristetään 0,01 mm:iin. Katkojen sovitettuihin pituuksiin se käyttää edelleen tasoitetun polun pituutta. Katkojen määrä pyöristetään puolikkaassa parilliseksi, lyhyt viiva saa välin vasta rajan ylittäessään, yhden välin viiva, jonka päätykatkot ovat 0, piirretään ehjänä, ja toissijainen väli tulee vain katkoon, joka on yli puolet katkon pituudesta. Korjaus koskee OCADin katkojen sovitusta (`SpacingMethod.OCAD`) sekä katkoille että glyyfeille, ei Mapperin.

Toisen näytteen symboli G2 (vähintään 3 väliä ja toissijainen väli) ei saanut OCADissa vähimmäisvälejä, koska OCAD lukee `MinSym`in 2 kuten 0. Se on oma vikansa, tiketti [019](019-ocad-minimum-gaps.md).

Korjaus on haarassa `fix/ocad-dash-count` (844cb7f8 ja b99067b5). `SymPath.GetCornerAndDashPointLengthsForCounting` antaa kunkin kulma- ja katkopisteiden välisen osan pituuden OCADin laskentatapaan, ja `ComputeDashDistances` laskee siitä välien määrän OCADin säännöillä. Katkojen sovitetut pituudet tulevat edelleen tasoitetun polun pituudesta. `OcadDashCountTests` laskee näytteiden 28 solun katkot ja glyyfit OCAD-viewerin piirtoa vasten. Ilman korjausta 21 tapausta epäonnistuu, ja 7 vertailutapausta menee läpi. Korjaus siirtää 107 kuvabaselinea keräävästä ajosta 44414319636, ja kaksi basen omaa route gadget -baselinea on kansiossa `prs/017-ocad-dash-count-base/` basen yhdistämistä varten. Täysi ajo basen päällä meni läpi (Tests 44417098812, Build 44417098893). Haaraa ei ole yhdistetty haaraan `feature/palette-layers`, koska vanha piirtokoodi pysyy siellä ennallaan tikettien 008–013 ajan.

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun katkoviivoja verrattiin OCADiin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
