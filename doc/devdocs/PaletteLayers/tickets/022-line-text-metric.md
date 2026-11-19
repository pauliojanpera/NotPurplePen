# Viivan varren teksti mitataan aina OCADin metriikalla

**Konteksti**  
`TextSymDef.GetLineTextGraphemePlacement` sijoittaa viivan varren tekstin (`LineTextSymbol`) grafeemit polulle mittaamalla polkua aina OCADin metriikalla (`BizzarroLength` ja `PointAtLengthBizzarro`), kartan metriikasta riippumatta. Koodin kommentin mukaan näin toimi OCAD silloin, kun koodi kirjoitettiin. Nykyinen OCAD mittaa viivat euklidisesti, ellei kartassa ole näkymäparametria `p1` (korjaus [016](016-ocad-metric-parameter.md)), ja Mapper-kartat mitataan aina euklidisesti. Katkot, glyyfit ja viivojen lyhennykset käyttävät jo kartan metriikkaa (`Map.MapDistanceMetric`).

**Toistamisohjeet**  
1. Piirrä euklidisen kartan viivan varren teksti vinolla polulla, esimerkiksi 45 asteessa.
2. Vertaa kirjainten paikkoja OCADin piirtoon.

**Odotettu vs. Todellinen käytös**  
`odotettu:` euklidisessa kartassa kirjainten etäisyydet polulla ovat euklidisia, kuten nykyisessä OCADissa (tarkistetaan näytteestä).  
`todellinen:` Purple Pen mittaa ne OCADin metriikalla, joten 45 asteen polulla teksti on noin 6 % tiiviimpi, ja keskitetyn ja oikealle tasatun tekstin alku siirtyy.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
`GetLineTextGraphemePlacement` mittaa polun ja sijoittaa grafeemit kartan metriikalla (`map.MapDistanceMetric`): euklidisessa kartassa euklidisesti ja kartassa, jonka `p` on 1, OCADin metriikalla kuten nyt. Testi vertaa kirjainten paikkoja OCADin PDF-vientiin näytteestä, jossa on viivan varren tekstejä vinoilla, mutkittelevilla ja kaarevilla poluilla.

**Päätökset**  
- Korjaus tarkistetaan OCADia vasten näytteellä ja sen PDF-viennillä (@Pauli, 19.11.).
- OCADin metriikka toteutetaan best effort -periaatteella: euklidinen metriikka on referenssi, eikä OCADin metriikan yksityiskohtiin tehdä enää näytteitä eikä korjauksia (@Pauli, 19.11.). Siksi näytteessä on vain euklidinen kartta.

**Haara (ehdotus)**  
`fix/line-text-metric` juurtuu kohtaan, jossa `base` kohtaa upstreamin (d48f4456), kuten korjaukset 015–021.
