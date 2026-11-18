# OCAD-kaksoisviivan katkoissa on aina vähintään yksi väli

**Konteksti**  
`OcadImport` antaa kaksoisviivan katkoille vähintään yhden välin (`doubleDashes.minGaps = 1`). OCAD ei anna niille vähimmäisväliä: se piirtää 1,0–2,45 mm:n katkotut kaksoisviivat, joiden katko on 2,0 mm ja väli 0,5 mm, ehjinä, kun sekä täyttö että reunat ovat katkottuja ja kun vain reunat ovat ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md), molemmat näytteet).

**Toistamisohjeet**  
1. Piirrä oraakkelin toisen näytteen tiedosto `dashes-2-ocad12-euclidean.ocd` 600 dpi:llä.
2. Mittaa solujen B2-1–B2-4 (symboli C, täyttö ja reunat katkottuja) ja B2-12 (symboli C3, reunat katkottuja) katkot `measure-dashes.py`:llä, ja vertaa niitä OCAD-viewerin vientiin.

**Odotettu vs. Todellinen käytös**  
`odotettu:` jokainen viiva on yksi ehjä katko, kuten OCAD 2020.5.18 Viewer piirtää.  
`todellinen:` Purple Pen piirtää jokaiseen viivaan välin, eli kaksi katkoa.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
`OcadImport` ei anna kaksoisviivan katkoille vähimmäisväliä (`minGaps = 0`). Kolmas näyte osoitti, ettei OCAD käytä katkotun pääviivan vähimmäisvälejä kaksoisviivan reunojen katkoihin: reunat jäivät ilman väliä sekä pääviivan vähimmäismäärällä 0 että 3 (rivi DM).

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun katkoviivoja verrattiin OCADiin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
