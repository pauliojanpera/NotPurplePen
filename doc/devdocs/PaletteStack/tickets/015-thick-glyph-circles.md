# OCAD-ympyrä, jonka viiva on vähintään halkaisijan levyinen, ei piirry kuten OCADissa

**Konteksti**  
OCAD ottaa ympyräelementin halkaisijan viivan keskeltä: OCAD 6–8 -tiedostossa se vähentää viivan leveyden tiedoston halkaisijasta. Tällä halkaisijalla ympyrä, jonka halkaisija on viivaa suurempi, on rengas, ympyrä, jonka viiva on vähintään halkaisijan levyinen, on halkaisijan kokoinen täytetty ympyrä, ja ympyrä, jonka halkaisija on enintään 0, jää piirtämättä ([oracles/ocad-thick-circles.md](../oracles/ocad-thick-circles.md)). Purple Pen lukee, piirtää ja kirjoittaa näistä kolme jälkimmäistä toisin:
- Tuonti (`OcadImport`) säilyttää OCAD 6–8 -ympyrän halkaisijan ulkoreunoista.
- `GlyphPart.Draw` piirtää ympyrän vain, kun sen halkaisija on viivaa suurempi, ja `Glyph.DrawSimple` täyttää yksinäisen ympyrän origossa vain, kun viiva on halkaisijaa leveämpi.
- Vienti (`OcadExport`) kirjoittaa tällaisen ympyrän renkaana: OCAD 9+:aan halkaisijan miinus viivan, joka on enintään 0.

**Toistamisohjeet**  
1. Avaa Purple Penissä tiedostot `src/MapModel/TestFiles/ocadimport/circle-line-wider-than-diameter-ocad12.ocd` ja `circle-line-wide-ocad8.ocd` (haarassa `fix/thick-glyph-circles`; samat tiedostot ovat oraakkelin kansiossa).
2. Vertaa niiden symboleita OCAD 2020:n piirtoon.

**Odotettu vs. Todellinen käytös**  
`odotettu:` OCAD 12 -tiedostossa A, B:n ympyrä, C ja E ovat 1,0 mm:n täytettyjä ympyröitä. OCAD 8 -tiedostossa F8 on 0,4 mm:n täytetty ympyrä, eikä A8:aa, B8:n ympyrää, C8:aa eikä E8:aa piirretä.  
`todellinen:` B:n ympyrää, C:tä ja E:tä ei piirretä. A8 on 1,0 mm:n täytetty ympyrä, ja F8 on noin 1,0 mm:n rengas.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Korjaus on haarassa `fix/thick-glyph-circles` (52d78a49 ja 5782a341):
- `GlyphPart.Draw` piirtää ympyrän, kun sen viiva ja halkaisija ovat suurempia kuin 0, ja täyttää sen halkaisijan kokoiseksi, kun säde viivan keskelle on enintään 0. `Glyph.DrawSimple` täyttää ympyrän myös silloin, kun viiva on yhtä leveä kuin halkaisija, ja ympyrän rajat (`GlyphPart.BoundingBox`) kattavat täytetyn ympyrän. Sama koodi piirtää viivojen ja aluekuvioiden glyyfien ympyrät.
- `OcadImport` ottaa OCAD 6–8 -ympyrän halkaisijan viivan keskeltä, kun se on enintään viivan levyinen, ja vähintään 0. OCAD 9+ -ympyrä ja OCAD 6–8 -rengas luetaan kuten ennen.
- `OcadExport` kirjoittaa ympyrän, jonka viiva on vähintään halkaisijan levyinen, OCAD 9+:aan halkaisijalla viivan keskeltä ja OCAD 6–8:aan viivan leveys lisättynä, joten edestakainen siirto säilyttää sen. Renkaat kirjoitetaan kuten ennen.

`GlyphCircleTests` tarkistaa neljä muodostettua glyyfiä sekä oraakkelin OCAD 12- ja OCAD 8 -tiedostojen 11 ympyrää OCAD 2020:n piirtoa vasten. Ilman piirron muutosta kolme muodostettua tapausta epäonnistuu, ja ilman tuonnin muutosta viisi OCAD 8 -tapausta. Korjaus siirtää neljä kuvabaselinea, jotka on tallennettu haaraan (3376cb04) keräävästä ajosta 44344592098: viivan bar scale (`multisymonedash`, Skia ja GDI+) saa 0,4 mm:n täytetyn ympyrän, ja symboli 418.2 (`LordHill` ja `LordHill OCAD11`, Skia) 0,01 mm:n ympyrät, jotka siirtävät 7 pikseliä enintään 16:lla. Täysi ajo basen päällä meni läpi (Tests 44346994914, Build 44346994936).

Vika löytyi tiketin [008](008-symbol-geometry.md) työssä. Tiketin 008 geometriametodi noudattaa nykyistä piirtoa (`GlyphPart.MakeClippingObject` ja `MakeClippingObjectOfSimpleGlyph`). Haara on yhdistetty haaraan `feature/palette-stack` (636d5f95) ennen tiketin 011 työtä (@Pauli, 19.11.), ja geometriametodi sai siellä saman muutoksen (6bc52ba1). `SymbolGeometryTests` saa testin mustista glyyfiympyröistä 7 leikkausobjektia neljän sijaan (47578e98). Yhdistetyn haaran täysi ajo basen päällä meni läpi (Tests 44429290119, Build 44429290123).

Mapper-vienti (`OpenMapperExport`) kirjoittaa ympyrän sisäsäteellä max(0, halkaisija/2 − viiva) ja ulkoleveydellä viiva. Ympyrästä, jonka viiva on vähintään halkaisijan levyinen, tulee siksi Mapperiin kahden viivan levyinen täytetty ympyrä. Tämä korjaus ei muuta Mapper-vientiä.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
