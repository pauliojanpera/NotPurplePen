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

Korjaus on haarassa `fix/ocad-metric-parameter` (dc02991c, 7ceb7543, 47471041, 3ab5d9ed ja 99ce8e7c). `OcadMetricTests` tarkistaa OCAD 6-, 8-, 9- ja 10-tiedostojen metriikan, 45°:n viivan katkot OCAD 9- ja 10-näytetiedostoissa OCAD-viewerin piirtoa vasten ja sen, että kirjoitettu tiedosto säilyttää kartan metriikan. Ilman korjausta 7 sen 15 tapauksesta epäonnistuu. Korjaus siirtää 210 kuvabaselinea keräävästä ajosta 44363913238: OCAD 6–10 -testikartat, joiden katkot, lyhennykset tai glyyfit ovat vinoilla tai kaarevilla viivoilla. Lake Sammamishin OCAD 11- ja 12-versioissa ja teanwestin OCAD 2018 -versiossa on p1, ja ne jakoivat baselinensa vanhempien versioiden kanssa, joissa p:tä ei ole. Samalla metriikalla versiot piirtyvät pikselilleen samoin (tarkistettu piirtämällä), joten ainoa ero on p. Niiden kuvaukset saavat omat baselinet `*_ocadmetric`, ja `LakeSammMap12` siirtää karttansa edestakaisin vain OCAD 9–12 -tiedostojen kautta, koska OCAD 6–8 -tiedostoissa ei ole näkymäparametreja. Basen omat route gadget -baselinet, jotka korjaus siirtää, ovat kansiossa `prs/016-ocad-metric-parameter-base/` basen yhdistämistä varten, ja `push-wip.sh --base-baselines` kirjoittaa ne wip-ajoon. Täysi ajo basen päällä meni läpi (Tests 44415489933, Build 44415489926). Haara on yhdistetty haaraan `feature/palette-layers` (bdec06f0) ennen tiketin 011 työtä (@Pauli, 19.11.), ja yhdistetyn haaran täysi ajo meni läpi (Tests 44429290119, Build 44429290123).

Kymmenes näyte vahvisti OCAD 6-, 7- ja 8-tiedostot ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md)): OCAD-viewer piirtää OCAD 7- ja 8-tiedostot kuten OCAD 12 -tiedoston euklidisella metriikalla 0,01 mm:n tarkkuudella, ja OCAD 6 -tiedoston samoin, kun sen katkopiste luetaan tavalliseksi pisteeksi, koska `OcadExport` kirjoittaa katkopisteet vasta OCAD 7 -tiedostoihin. Viewer mittaa siis OCAD 6–8 -tiedostot euklidisesti, kuten tämä korjaus.

Purple Pen mittaa tekstit viivan varrella aina OCADin metriikalla (`TextSymDef.GetLineTextGraphemePlacement`). Näytteissä ei ollut tekstejä, joten tämä tiketti ei koske niitä.

Vika löytyi tiketin [011](011-line-geometry.md) valmistelussa, kun katkoviivoja verrattiin OCADiin.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
