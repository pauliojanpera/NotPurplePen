# OCAD-viivan katkojen määrä poikkeaa OCADin piirrosta

**Konteksti**  
OCAD laskee viivan katkojen määrän toisin kuin `LineSymDef.ComputeDashDistances` neljässä tilanteessa ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md)):
- **Bezier-käyrä.** OCAD laskee välien määrän pituudesta, jossa jokainen Bezier-käyrä on neljä jännettä parametrin arvojen 0, ¼, ½, ¾ ja 1 pisteiden välillä, kartan metriikalla mitattuna. Purple Pen mittaa hienoksi tasoitetun käyrän (`SymPath.GetFlattenedPoints`). Katkot OCAD sijoittaa koko käyrälle. Tämä pituus sopii näytteiden kaikkiin 28 Bezier-käyrään kummallakin metriikalla, eikä mikään muu jänteiden määrä väliltä 2–16 sovi kaikkiin kummallakin.
- **Puolikas.** Kun päätykatkot ovat katkon mittaisia, OCAD antaa viivalle katkoja lähimmän kokonaisluvun verran lausekkeesta (L + väli + 2 × katko − 2 × päätykatko) / (katko + väli), ja tasan puolikkaassa parillisen määrän. Purple Pen pyöristää välien määrän puolikkaassa parilliseksi (`Math.Round`).
- **Lyhyt viiva.** Kun viiva ei ole päätykatkojaan ja väliä pidempi, OCAD antaa sille välin vasta, kun se on pidempi kuin päätykatkojen puolikkaat ja väli. Purple Pen antaa välin jo samanpituiselle viivalle (`pathLength >= firstDashLength / 2 + lastDashLength / 2 + gapLength`).
- **Päätykatkot 0.** Kun sääntö antaa viivalle yhden välin ja päätykatkot ovat 0, OCAD piirtää viivan ehjänä, mutta Purple Pen ei piirrä mitään, koska koko viiva on väliä.

Lisäksi 64-osainen murtoviiva sopii vain, jos OCAD pyöristää jokaisen osan pituuden 0,01 mm:iin. Sen tarkka pituus antaa 9,5009, ja pyöristetyt osat tasan 9,5.

**Toistamisohjeet**  
1. Piirrä oraakkelin toisen näytteen tiedostot `dashes-2-ocad12-euclidean.ocd` ja `dashes-2-ocad12-ocad-metric.ocd` 600 dpi:llä.
2. Mittaa katkot `measure-dashes.py`:llä ja vertaa niitä OCAD-viewerin vientiin.

**Odotettu vs. Todellinen käytös**  
`odotettu:` solujen katkojen määrät ovat kuten OCAD-viewerin viennissä: A1-1 ja A1-7 10, A2-8 1, B1-4, B1-16 ja B2-15 1, euklidisessa tiedostossa DS-3, DS-4, Darch-3 ja Darch-4 10, DS-9 ja Darch-9 11 ja D4F-1 10, ja OCADin metriikan tiedostossa Darch-4 11 ja Darch-9 12.  
`todellinen:` Purple Pen piirtää A1-1:een 9 katkoa, A1-7:ään 11, A2-8:aan ei mitään, B1-4:ään ja B1-16:een 2 ja B2-15:een 4, ja muihin soluihin yhden katkon enemmän kuin OCAD.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
`ComputeDashDistances` saa välien määrän laskuun OCADin pituuden, jossa jokainen Bezier-käyrä mitataan neljänä jänteenä ja jokaisen osan pituus pyöristetään 0,01 mm:iin. Katkojen sovitettuihin pituuksiin se käyttää edelleen tasoitetun polun pituutta. Katkojen määrä pyöristetään puolikkaassa parilliseksi, lyhyt viiva saa välin vasta rajan ylittäessään, ja yhden välin viiva, jonka päätykatkot ovat 0, piirretään ehjänä. Korjaus koskee OCADin katkojen sovitusta (`SpacingMethod.OCAD`), ei Mapperin.

Korjaus odottaa kolmatta näytettä kahdesta syystä. Ensinnäkin viivan glyyfit (`GlyphLocation`) käyttävät samaa laskua, eikä näytteissä ollut glyyfejä. Toiseksi vähimmäisvälit (`MinSym`) vaikuttavat samaan laskuun: OCAD ei antanut toisen näytteen symbolille G2 (vähintään 3 väliä ja toissijainen väli) vähimmäisvälejä, eikä näytteissä ollut symbolia, jonka `MinSym` on 0.

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun katkoviivoja verrattiin OCADiin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
