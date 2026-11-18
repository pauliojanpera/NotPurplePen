# OCAD 6–10 -tiedoston viivat mitataan aina OCADin metriikalla

**Konteksti**  
OCAD lukee viivojen metriikan ViewPar-parametrista `p` myös OCAD 9- ja 10-tiedostoista: `p1` on OCADin metriikka, ja ilman `p`:tä OCAD käyttää euklidista metriikkaa ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md), toinen näyte). Purple Pen lukee `p`:n vain OCAD 11+ -tiedostoista (`OcadImport`), joten se mittaa OCAD 6–10 -tiedostojen katkot, viivojen lyhennykset ja glyyfien paikat viivoilla aina OCADin metriikalla (`Map.UseEuclideanMetric` jää oletukseensa). `OcadExport` kirjoittaa `p`:n vain OCAD 11+ -tiedostoihin, joten OCADin metriikka katoaa, kun Purple Pen kirjoittaa kartan OCAD 9- tai 10-tiedostoksi.

**Toistamisohjeet**  
1. Avaa Purple Penissä oraakkelin tiedostot `dashes-2-ocad10.ocd`, jossa ei ole `p`:tä, ja `dashes-2-ocad10-ocad-metric.ocd`, jonka `p` on 1.
2. Laske rivin E 4.6 viivan 45° (solu E4.6-4) katkot ja vertaa niitä OCAD-viewerin vientiin.

**Odotettu vs. Todellinen käytös**  
`odotettu:` tiedostossa ilman `p`:tä viivalla on 5 katkoa euklidisella metriikalla, ja tiedostossa, jonka `p` on 1, 6 katkoa OCADin metriikalla, kuten OCAD 2020.5.18 Viewer piirtää.  
`todellinen:` Purple Pen piirtää kumpaankin 6 katkoa.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Purple Pen noudattaa nykyistä OCADia: OCAD 6–10 -tiedosto, jossa ei ole `p`:tä, mitataan euklidisella metriikalla, ja `p` luetaan, kun se on tiedostossa (@Pauli, 18.11.). `OcadImport` antaa jokaiselle OCAD-tiedostolle euklidisen metriikan ja lukee `p`:n jokaisen tiedoston ViewPar-parametrista. `OcadExport` kirjoittaa `p`:n jokaiseen tiedostoon, johon se kirjoittaa ViewPar-parametrin, eli OCAD 9+ -tiedostoihin.

Purple Pen mittaa tekstit viivan varrella aina OCADin metriikalla (`TextSymDef.GetLineTextGraphemePlacement`). Näytteissä ei ollut tekstejä, joten tämä tiketti ei koske niitä.

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun katkoviivoja verrattiin OCADiin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
