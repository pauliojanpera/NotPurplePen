# OCAD-viivan vähimmäisvälit 2 ja 3 luetaan kolmeksi ja neljäksi

**Konteksti**  
`OcadImport` lukee viivasymbolin katkojen vähimmäisvälien määräksi `MinSym + 1` (`dashInfo.minGaps`) ja välein sijoitettujen glyyfien vähimmäismääräksi `max(1, MinSym + 1)`. OCAD lukee `MinSym`in −1 niin, ettei vähimmäismäärää ole, 0:n yhdeksi ja 1:n kahdeksi, kuten Purple Pen. Arvot 2 ja 3 OCAD kuitenkin lukee yhdeksi, kuten 0:n, sekä katkoille toissijaisen välin kanssa ja ilman sitä että glyyfeille ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md), kolmas näyte).

**Toistamisohjeet**  
1. Piirrä oraakkelin kolmannen näytteen tiedosto `dashes-3-ocad12-euclidean.ocd` 600 dpi:llä.
2. Mittaa rivien M3, M4, MS3, MS4 ja GS short katkot ja glyyfit `measure-dashes.py`:llä, ja vertaa niitä OCAD-viewerin vientiin.

**Odotettu vs. Todellinen käytös**  
`odotettu:` rivien M3 ja M4 1,0–6,0 mm:n viivoilla on 2, 2, 2, 2, 2, 3 ja 3 katkoa, rivien MS3 ja MS4 viivoilla 2, 2, 2, 2, 2, 4 ja 4, ja symbolin GS3 1,0–4,0 mm:n viivoilla 1, 1, 1 ja 2 glyyfiä, kuten OCAD 2020.5.18 Viewer piirtää.  
`todellinen:` Purple Pen piirtää riville M3 0, 0, 4, 4, 4, 4 ja 4 katkoa, riville M4 0, 0, 0, 5, 5, 5 ja 5, riville MS3 0, 0, 4, 4, 4, 4 ja 6, riville MS4 0, 0, 0, 5, 5, 5 ja 8, ja symbolin GS3 viivoille 3 glyyfiä.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
`OcadImport` lukee `MinSym`in 2 ja suuremmat kuten 0: katkoille yhden välin ja glyyfeille yhden glyyfin. Näytteessä olivat arvot −1–3, joten suurempia arvoja ei ole tarkistettu. `OcadExport` kirjoittaa vähimmäismäärän yhdellä vähennettynä `MinSym`iksi. Siksi Purple Penin kirjoittama vähintään kolmen välin vähimmäismäärä on OCADissa yksi, eikä sitä voi kirjoittaa OCAD-tiedostoon.

Korjaus on haarassa `fix/ocad-minimum-gaps` (c512ad72 ja 6805bf5c). `OcadImport.MinimumNumberOfGaps` lukee `MinSym`in, ja katkojen vähimmäisvälit ja glyyfien vähimmäismäärä käyttävät sitä. `OcadMinimumGapsTests` laskee kolmannen näytteen rivien M3, M4, MS3 ja MS4 katkot ja symbolin GS3 glyyfit OCAD-viewerin piirtoa vasten. Ilman korjausta sen kaikki 7 tapausta epäonnistuvat. Yksikään baseline ei siirry (keräävä ajo 44412813635). Täysi ajo basen päällä meni läpi (Tests 44415599153, Build 44415599108). Haaraa ei ole yhdistetty haaraan `feature/palette-layers`, koska vanha piirtokoodi pysyy siellä ennallaan tikettien 008–013 ajan.

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun katkoviivoja verrattiin OCADiin. Toisen näytteen symboli G2 (tiketti [017](017-ocad-dash-count.md)) näytti sen ensin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
