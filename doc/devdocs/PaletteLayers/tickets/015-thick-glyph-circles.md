# Glyyfin ympyrä, jonka viiva on vähintään halkaisijan levyinen, jää piirtämättä

**Konteksti**  
OCAD piirtää ympyräelementin, jonka viiva on vähintään halkaisijan levyinen, täytetyksi ympyräksi, jonka halkaisija on elementin halkaisija ([oracles/ocad-thick-circles.md](../oracles/ocad-thick-circles.md)). Purple Penin OCAD-tuonti säilyttää tällaisen ympyrän halkaisijan. `GlyphPart.Draw` piirtää ympyrän kuitenkin vain, kun sen halkaisija on viivaa suurempi, ja `Glyph.DrawSimple` täyttää yksinäisen ympyrän origossa vain, kun viiva on halkaisijaa leveämpi.

**Toistamisohjeet**  
1. Avaa Purple Penissä tiedosto `oracles/ocad-thick-circles/circle-line-wider-than-diameter-ocad12.ocd`.
2. Katso symboleita B, C ja E kohdissa x = 10, 20 ja 40 mm.

**Odotettu vs. Todellinen käytös**  
`odotettu:` B:n ympyrä, C ja E ovat täytettyjä ympyröitä, joiden halkaisija on 1,0 mm, kuten OCAD 2020 piirtää ne.  
`todellinen:` niitä ei piirretä. B:n ympyrä ja E jäävät pois `GlyphPart.Draw`ssa, ja C on rengas, jonka säde on 0.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
`GlyphPart.Draw` piirtää ympyrän, kun sen viiva ja halkaisija ovat suurempia kuin 0, ja täyttää sen halkaisijan kokoiseksi, kun säde viivan keskelle on enintään 0. `Glyph.DrawSimple` täyttää ympyrän myös silloin, kun viiva on yhtä leveä kuin halkaisija, ja ympyrän rajat (`GlyphPart.BoundingBox`) kattavat täytetyn ympyrän. Sama koodi piirtää viivojen ja aluekuvioiden glyyfien ympyrät. Korjaus on haarassa `fix/thick-glyph-circles` (52d78a49). Sen testi `GlyphCircleTests.CircleIsFilledCircleOfItsDiameter` epäonnistuu ilman korjausta kolmessa tapauksessa neljästä: viiva yhtä leveä kuin halkaisija, glyyfissä toinen osa ja ympyrä origon ulkopuolella.

Vika löytyi tiketin [008](008-symbol-geometry.md) työssä. Tiketin 008 geometriametodi noudattaa nykyistä piirtoa (`GlyphPart.MakeClippingObject` ja `MakeClippingObjectOfSimpleGlyph`), joten kun korjaus yhdistetään haaraan `feature/palette-layers`, geometriametodi tarvitsee saman muutoksen, ja `SymbolGeometryTests` näyttää eron. Haaraa ei ole yhdistetty haaraan `feature/palette-layers`, koska vanha piirtokoodi pysyy siellä ennallaan tikettien 008–013 ajan (@Pauli, 18.11.).

Purple Penin OCAD 9+ -vienti kirjoittaa ympyrän halkaisijaksi halkaisijan miinus viivan leveyden (`OcadExport`), mikä tällaiselle ympyrälle on enintään 0. Tämä korjaus ei muuta vientiä.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
