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
- [x] Näytteet: Purple Pen kirjoittaa OCAD 12 -näytetiedostot (`InputOutput.WriteFile`) väliaikaisesta testistä,
  jota ei commitoida, kuten katkoviivanäytteet ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md)). Vienti
  kirjoittaa kunkin palettivärin peittävyyden (`t`) ja sekoitustilan (`OcadBlendMode`). Tiedosto `*-cells.json` antaa
  kunkin solun palettivärit ja muodot millimetreinä paperilla, ja kulmien neljä rekisteröintimerkkiä antavat
  piirroksen mittakaavan ja paikan. Näytteet ja niiden solujen tiedostot ovat kansiossa
  `oracles/ocad-blend-modes/`.
- [x] Näytteiden tapaukset kattavat ainakin nämä:
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
- [x] Viennit: @Pauli antaa kustakin näytteestä OCADin näkymän kuvakaappauksen, PDF-viennin ja PNG-viennin 600 dpi:n
  tarkkuudella ilman reunanpehmennystä, sekä OCADin version ja PDF-viennin asetukset. Istunto antaa hänelle
  tiedostot ja kertoo, mitä raportoida, kuten oraakkelit [ocad-thick-circles.md](../oracles/ocad-thick-circles.md) ja
  [ocad-dashes.md](../oracles/ocad-dashes.md) näyttävät.
- [x] PDF:n mittaus: skripti `oracles/ocad-blend-modes/measure-pdf.py` lukee PDF-viennin rakenteen suoraan, ei
  katselimen kuvaa:
  - kunkin muodon värin (CMYK tai spottiväri)
  - grafiikkatilojen sekoitustilat (`/BM`) ja peittävyydet (`/CA`, `/ca`)
  - läpinäkyvyysryhmät (`/Group`, `/S /Transparency`) ja niiden sekoitusväriavaruudet (`/CS`), myös sivun ryhmän.

  Näin tulos ei riipu katselimen värimuunnoksesta.
- [x] Näkymän mittaus: skripti mittaa näkymän kuvista ja PNG-vienneistä kunkin solun värit rekisteröintimerkkien
  avulla. Värejä verrataan vain saman kuvan sisällä, kuten `ocad-darken.md` tekee.
- [x] Oraakkeli `oracles/ocad-blend-modes.md` kertoo näytteet, viennit, mittaukset ja tulokset. Se vastaa näihin:
  - Soveltaako OCADin näkymä B:tä ja α:aa?
  - Miten PDF-vienti kirjoittaa ne, ja missä sekoitusväriavaruudessa?
  - Ovatko Darken ja Multiply PDF:ssä musteittaisia (kunkin musteen suurempi osuus ja komplementtien tulo)?
  - Sovelletaanko α palettiväriin kerran vai kuhunkin muotoon?

  Se kertoo myös, mitkä kohdat oraakkelista `ocad-darken.md` pitävät ja mitkä eivät.
- [x] Johtopäätökset kirjataan Record.md:hen: mitä PDF:n koodaus ja piirtotilat tarvitsevat, ja pitääkö
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
- **Toteutus** (haara `docs/process-coordination`):
  - Näyte on yksi OCAD 12 -tiedosto, `oracles/ocad-blend-modes/blend-modes-ocad12.ocd`, ja sen solutiedosto
    `blend-modes-cells.json`: 33 palettiväriä, 76 solua ja 336 aluetta, joiden värit mittaus lukee. Purple Pen
    kirjoitti sen väliaikaisesta testistä haaran `feature/palette-stack` koodilla (0d2148b3), ja tuonti lukee
    kunkin palettivärin `t`:n, `b`:n, `o`:n ja spottivärierottelujen osuudet sellaisina kuin ne tehtiin.
    Tapauksen 4 spottivärit ovat samassa tiedostossa: OCADin CMYK-vienti jättää spottivärit huomiotta, ja
    spottivärierottelujen vienti näyttää vain värit, joilla on spottiosuus.
  - `measure-pdf.py` lukee PDF-viennin rakenteen ja antaa CMYK-sekoitusväriavaruudessa kunkin DeviceCMYK-alueen
    musteet PDF:n mallin mukaan. `measure-colors.py` mittaa kuvan alueiden värit ja vertaa niitä kuuteen tapaan
    koostaa palettivärit kuvan omista väreistä. Ne on tarkistettu katkoviivanäytteen OCAD-viennillä, istunnon
    skriptin kirjoittamalla PDF:llä, Purple Penin piirroksilla ja kuvakaappausta jäljittelevällä kuvalla.
  - Oraakkeli `oracles/ocad-blend-modes.md` kertoo näytteen, OCADin dokumentaation, OCADin PDF-viennin rakenteen
    katkoviivanäytteiden vienneistä, viennit, jotka @Pauli antoi, mittaukset, tulokset ja vastaukset sekä Purple Penin
    piirrokset.
  - @Pauli antoi viennit 24.11. OCAD 2020.5.18 Viewerista, samasta versiosta kuin katkoviivanäytteet: näkymän
    kuvakaappaukset normaali-, luonnos- ja spottivärinäkymässä, Colors-dialogin, PNG-viennit normaali- ja
    spottivärinäkymästä, CMYK-PDF:n sekä spottivärierottelujen PDF:t yhdistettyinä ja Spot Violetista erikseen.
    Viewerissä ei ole sekoitustiladialogia. Vientiasetukset näkyvät tiedostoista: värit PDF:n väriavaruuksista, koko
    kartta MediaBoxista ja kartan mittakaava merkeistä.
  - Tulokset: OCADin näkymä, luonnosnäkymä ja PNG-vienti soveltavat α:aa kerran kuhunkin palettiväriin eivätkä
    sovella B:tä (`measure-colors.py`: kaikki 133 aluetta, virhe enintään 6 tasoa). PDF-vienti kirjoittaa kunkin
    palettivärin läpinäkyvyysryhmäksi, jonka `Do`:n grafiikkatila antaa B:n (`/BM`) ja α:n (`/ca`, `/CA`). Sivulla
    ei ole ryhmää eikä ryhmillä `/CS`:ää, joten katselin valitsee sekoitusväriavaruuden. CMYK:ssa Darken ja Multiply
    ovat musteittaisia: Ghostscriptin mustelevyt vastaavat PDF:n mallia kaikilla 336 alueella 0,3 prosenttiyksikön
    tarkkuudella (`compare-plates.py`). RGB-laitteelle Ghostscript sekoittaa samat ryhmät RGB:ssä. Päällepainatusvärin
    muodoilla on `/OP true /OPM 1`. Spottivärierottelujen vienti ei kirjoita B:tä, ja se kirjoittaa osuuden 0
    täyttönä, joka pyyhkii erottelun.
  - Istunnon valinnat ovat Record.md:n kohdassa "The choices of the session in the samples of ticket 027".
- **Rajaus:** tiketti ei muuta koodia eikä piirtoa. B:n ja α:n toteutus on tiketti 010 ja PDF:n koodauksen tiketti.
  Mapperilla ei ole sekoitustiloja, ja sen α on oraakkelissa [mapper-opacity.md](../oracles/mapper-opacity.md).
- **Haara:** ei koodihaaraa. Näytteet, skriptit ja oraakkeli tulevat haaraan `docs/process-coordination`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md),
  [../README.md](../README.md), [../Handover.md](../Handover.md), speksin esipuhe ja luvut 02 ja 04, [../Glossary.md](../Glossary.md), oraakkelit
  [ocad-darken.md](../oracles/ocad-darken.md) ja [ocad-dashes.md](../oracles/ocad-dashes.md), tiketti
  [004](004-opacity-and-blend-mode.md) sekä tämä tiketti.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
* [029](029-ocad-float-strings.md) OCAD-vienti kirjoittaa float-laskun virheen värien osuuksiin ja asettelutekstien
  fonttikokoihin. Värien osuuksien vika löytyi tämän tiketin näytteestä (@Pauli pyysi tiketin 24.11.): haara
  `fix/ocad-float-strings`, @Pauli hyväksyi sen 24.11., ei toteuteta nyt (@Pauli, 24.11.). Tällä tiketillä ei ole
  koodihaaraa, johon sen voisi yhdistää, eikä tämä tiketti odota sitä.
