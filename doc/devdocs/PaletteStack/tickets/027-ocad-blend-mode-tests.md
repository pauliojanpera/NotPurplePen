# OCADin sekoitustilojen ja peittävyyden testit
**Kuvaus**
Selvitetään näytteillä, miten OCAD piirtää palettivärin sekoitustilan B (`b`: Normal, Darken ja Multiply) ja
peittävyyden α (`t`) omassa näkymässään ja PDF-viennissään. Nykyinen tieto
([oracles/ocad-darken.md](../oracles/ocad-darken.md)) perustuu yhden OCADin ohjesivun kuviin, joiden jälkikäsittelyä ei
tunneta, eikä siinä ole Multiplyä eikä alle 100:n peittävyyttä (@Pauli, 23.11.). Purple Pen kirjoittaa näytteet,
@Pauli antaa niistä OCADin näkymän kuvat ja PDF-viennit, ja istunto mittaa ne. Tulos kirjataan oraakkeliksi
`oracles/ocad-blend-modes.md`. Se ratkaisee PDF:n koodauksen sekoitusväriavaruuden ja antaa piirtotiloille
musteittaisen B:n ulkoasun. Koodi ei muutu.

**Hyväksymiskriteerit**
- [ ] Näytteet: Purple Pen kirjoittaa OCAD 12 -näytetiedostot (`InputOutput.WriteFile`) väliaikaisesta testistä,
  jota ei commitoida, kuten katkoviivanäytteet ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md)). Vienti
  kirjoittaa kunkin palettivärin peittävyyden (`t`) ja sekoitustilan (`OcadBlendMode`). Tiedosto `*-cells.json` antaa
  kunkin solun palettivärit ja muodot millimetreinä paperilla, ja kulmien neljä rekisteröintimerkkiä antavat
  piirroksen mittakaavan ja paikan. Näytteet ja niiden solujen tiedostot ovat kansiossa
  `oracles/ocad-blend-modes/`.
- [ ] Näytteiden tapaukset kattavat ainakin nämä:
  1. Darken ja Multiply kunkin alla olevan palettivärin päällä. Alla olevia värejä ovat paperi, prosessivärit
     ilman mustaa ja mustan kanssa (esimerkiksi C87 M18 ja C0 M56 Y100 K18) sekä pelkkä musta K50. Päällä olevat värit
     ovat C0 M100, C76 Y91 ja K50.
  2. Peittävyys 50 sekoitustiloilla Normal, Darken ja Multiply.
  3. Kaksi saman palettivärin päällekkäistä muotoa peittävyydellä 50: saako päällekkäinen osa α:n kerran vai kahdesti
     (speksin kuva 6).
  4. Päällepainatusväri (`SymColor.OverPrint`) sekoitustiloilla Normal ja Darken sekä spottivärierottelun väri
     Darkenilla.
  5. Sama Darken-väri palettijärjestyksessä kahden Normal-värin välissä: sekoittuuko se kaikkien alla olevien vai
     vain viereisen palettivärin kanssa.
- [ ] Viennit: @Pauli antaa kustakin näytteestä OCADin näkymän kuvakaappauksen, PDF-viennin ja PNG-viennin 600 dpi:n
  tarkkuudella ilman reunanpehmennystä, sekä OCADin version ja PDF-viennin asetukset. Istunto antaa hänelle
  tiedostot ja kertoo, mitä raportoida, kuten oraakkelit [ocad-thick-circles.md](../oracles/ocad-thick-circles.md) ja
  [ocad-dashes.md](../oracles/ocad-dashes.md) näyttävät.
- [ ] PDF:n mittaus: skripti `oracles/ocad-blend-modes/measure-pdf.py` lukee PDF-viennin rakenteen suoraan, ei
  katselimen kuvaa:
  - kunkin muodon värin (CMYK tai spottiväri)
  - grafiikkatilojen sekoitustilat (`/BM`) ja peittävyydet (`/CA`, `/ca`)
  - läpinäkyvyysryhmät (`/Group`, `/S /Transparency`) ja niiden sekoitusväriavaruudet (`/CS`), myös sivun ryhmän.

  Näin tulos ei riipu katselimen värimuunnoksesta.
- [ ] Näkymän mittaus: skripti mittaa näkymän kuvista ja PNG-vienneistä kunkin solun värit rekisteröintimerkkien
  avulla. Värejä verrataan vain saman kuvan sisällä, kuten `ocad-darken.md` tekee.
- [ ] Oraakkeli `oracles/ocad-blend-modes.md` kertoo näytteet, viennit, mittaukset ja tulokset. Se vastaa näihin:
  - Soveltaako OCADin näkymä B:tä ja α:aa?
  - Miten PDF-vienti kirjoittaa ne, ja missä sekoitusväriavaruudessa?
  - Ovatko Darken ja Multiply PDF:ssä musteittaisia (kunkin musteen suurempi osuus ja komplementtien tulo)?
  - Sovelletaanko α palettiväriin kerran vai kuhunkin muotoon?

  Se kertoo myös, mitkä kohdat oraakkelista `ocad-darken.md` pitävät ja mitkä eivät.
- [ ] Johtopäätökset kirjataan Record.md:hen: mitä PDF:n koodaus ja piirtotilat tarvitsevat, ja pitääkö
  tiketin 004 tulkinta (`t` on α ja `b` on B) paikkansa. Speksin muutoksen päättää @Pauli.

**Lisätiedot**
- **Päätökset:**
  - @Pauli hyväksyi tämän tiketin 24.11. Viennit hän antaa, kun on OCADin ääressä.
  - OCADin sekoitustiloja on testattava lisää, koska nykyinen tieto perustuu yhden ohjesivun kuviin (@Pauli, 23.11.,
    "More tests of OCAD's blend modes"). Testit ovat oma tikettinsä ennen PDF:n koodausta, eikä tiketti 010 riipu niistä
    (istunnon valinta 23.11.).
  - OCAD on omien karttojensa piirron auktoriteetti (speksin esipuhe). @Pauli antaa OCADin näkymän kuvat ja viennit,
    kun työ tarvitsee niitä (@Pauli, 18.11.).
  - Skian kanava sekoittaa RGB:ssä, ja musteittainen B kuuluu PDF:n koodaukseen ja piirtotiloihin (päätökset 23.11.,
    "The blend mode of the drawing").
- **Nykytila:**
  - `oracles/ocad-darken.md` perustuu OCADin wikisivun "Blend Mode" kuviin. Niiden mukaan OCADin näkymä ei sovella
    B:tä, ja PDF:ssä Darken vastaa kunkin musteen suurempaa osuutta FOGRA45L-profiililla 36:n tarkkuudella. Adobe
    Readerin profiilia ei tunneta. Oraakkelissa ei ole Multiplyä eikä alle 100:n peittävyyttä.
  - OCADin tuonti lukee `t`:n peittävyydeksi (`t` / 100, `SymColor.BoundOpacity`) ja `b`:n sekoitustilaksi
    (`OcadBlendMode.FromName`). Vienti kirjoittaa `t`:n ja sekoitustilan (`OcadExport`, tiketti 004).
- **Rajaus:** tiketti ei muuta koodia eikä piirtoa. B:n ja α:n toteutus on tiketti 010 ja PDF:n koodauksen tiketti.
  Mapperilla ei ole sekoitustiloja, ja sen α on oraakkelissa [mapper-opacity.md](../oracles/mapper-opacity.md).
- **Haara:** ei koodihaaraa. Näytteet, skriptit ja oraakkeli tulevat haaraan `docs/process-coordination`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md),
  [../README.md](../README.md), [../Handover.md](../Handover.md), speksin esipuhe ja luvut 02 ja 04, [../Glossary.md](../Glossary.md), oraakkelit
  [ocad-darken.md](../oracles/ocad-darken.md) ja [ocad-dashes.md](../oracles/ocad-dashes.md), tiketti
  [004](004-opacity-and-blend-mode.md) sekä tämä tiketti.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
