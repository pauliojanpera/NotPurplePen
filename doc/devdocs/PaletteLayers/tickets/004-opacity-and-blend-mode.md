# Palettimerkinnän peittävyys ja sekoitustila
**Kuvaus**
Palettimerkinnällä on speksin luvun 01 mukaiset peittävyys α ja sekoitustila B, ja OCAD- ja Mapper-tuonnit ja -viennit lukevat ja kirjoittavat ne, mutta piirto ei vielä käytä niitä.

**Hyväksymiskriteerit**
- [x] Palettimerkinnällä (`SymColor`) on peittävyys α väliltä 0–1 ja sekoitustila B. Oletuksena α on 1 ja B on normaali.
- [x] α ja B ovat erillään knockoutista (päällepainatuslipusta) ja CMYK-arvosta. Värin CMYK-arvo ja sen alfakanava (`CmykColor.Alpha`) eivät muutu, ja koodi, joka lukee värin CMYK-arvon, saa saman arvon kuin ennen.
- [x] Sekoitustilat ovat ne, jotka formaatit tuntevat: normaali, darken ja multiply. Tuonti lukee tuntemattoman arvon normaaliksi.
- [x] OCAD 9 ja uudemmat: tuonti lukee värimerkkijonon `t`-kentän peittävyydeksi prosentteina (100 on täysin peittävä, ja puuttuva kenttä on 100) sekä `b`-kentän sekoitustilaksi (`Normal`, `Darken` tai `Multiply`, ja puuttuva kenttä on normaali). Vienti kirjoittaa `t`:n jokaisen OCAD 9:n ja uudemman värin merkkijonoon ja `b`:n OCAD 12:n ja 2018:n väreihin, kuten OCAD itse kirjoittaa.
- [x] OCAD 6–8: muodossa ei ole peittävyyttä eikä sekoitustilaa. Tuonti antaa oletukset, eikä vienti kirjoita niitä.
- [x] Mapper: tuonti lukee värin `opacity`-määreen peittävyydeksi (puuttuva on 1), ja vienti kirjoittaa sen. Mapperissa ei ole sekoitustilaa, joten tuonti antaa normaalin, eikä vienti kirjoita sitä.
- [x] Palettimerkinnän kopio (`CopyFrom`, `Map.Clone` ja kumous) säilyttää α:n ja B:n.
- [x] Kun tiedosto tuodaan, viedään samaan muotoon ja tuodaan uudelleen, α ja B säilyvät samoina muodon rajoissa.
- [x] Piirto ei muutu, eikä yksikään kuvabaseline siirry. Piirto ei vielä käytä α:aa eikä B:tä, ja päällepainatusvaikutus toimii kuten ennen. Ratatiedostojen OCAD-dumppien väreihin tulee `t`-kenttä (ks. Tekstidumpit).
- [x] Testit kattavat LordHill-kartan peittävyyden (OCAD 9 ja 11), OCAD 12 -kartan ja SSL-pohjan (OCAD 2018) sekoitustilat, OCAD 6–8:n oletukset sekä Mapper-kierroksen värillä, jonka peittävyys on alle 1. Testit eivät piirrä.

**Lisätiedot**
- **Speksi:** luku 01 antaa palettikerroksen ominaisuudet C, α, B, G ja M. Luvun 05 mukaan α ja B kohdistuvat vaiheessa 2 kerran palettimerkinnän ryhmään, eivät jokaiseen muotoon erikseen (kuva 6). Luvun 06 mukaan knockout pidetään erillään α:sta ja sekoitustilasta.
- **Rajaus:** tämä on pelkkä malli, kuten tiketti 001. Piirto käyttää α:aa ja B:tä vasta vaiheessa 2. α:aa ei viedä `CmykColor.Alpha`-kanavaan, koska piirto käyttää sitä jokaisen muodon siveltimessä erikseen, ja juuri sen kuva 6 kieltää. B on palettimerkinnän ominaisuus. Se on eri asia kuin päällepainatusvaikutuksen darken, joka on piirtotila (luku 07).
- **Nykytila:** `SymColor` ei sisällä α:aa eikä B:tä. OCAD-tuonti ohittaa `t`- ja `b`-kentät. Mapper-tuonti lukee `opacity`-määreen `OpenMapperColor`-olioon mutta ei käytä sitä, ja Mapper-vienti kirjoittaa `opacity`:ksi `CmykColor.Alpha`n, joka on aina 1. Graphics2D:n `BlendMode`-luettelossa on vain `Darken`, jota piirto käyttää päällepainatusvaikutukseen.
- **OCADin kentät:** OCADin dokumentaatio kutsuu `t`-kenttää läpinäkyvyydeksi, mutta se on peittävyys. Tavalliset värit ovat `t100`, ja OOM lukee kentän peittävyytenä (0,01 · t). OCAD tuntee sekoitustilat Darken ja Multiply, ja sekoitustila vaikuttaa OCADissa vain PDF-vientiin ([Blend Mode](https://www.ocad.com/wiki/ocad/en/index.php?title=Blend_Mode)). OCADin itse kirjoittamissa kartoissa `b` on OCAD 12- ja 2018-tiedostoissa sekä kahdessa OCAD 9 -tiedostossa, ja sen arvot ovat `Normal` ja `Darken`. OCAD 10- ja 11-tiedostoissa sitä ei ole. `t` on lähes aina 100. OCAD kirjoittaa kentät järjestyksessä `o`, `t`, `b` ja sitten `s`- ja `p`-parit. OCAD ja OOM kirjoittavat `t`:n kokonaislukuna, ja niin kirjoittaa myös vienti: peittävyys 0,6 on `t60` eikä liukulukuna `t60.000004`.
- **Tekstidumpit:** `ControllerTests`in `CreateOcadFiles`-testit kirjoittavat ratoja OCAD 9- ja 11-tiedostoiksi ja vertaavat niiden `OcadDump`-dumppeja valmiisiin dumppeihin. Koska vienti kirjoittaa `t`:n jokaiseen väriin kuten OCAD, 30 dumppiin kansioissa `src/TestFiles/controller/ocad_create2`, `8`, `9` ja `10` tulee jokaisen värin `o`-rivin perään rivi `code=t, value=100`. Muu dumpeissa ei muutu. Kriteeri, jonka mukaan yksikään baseline ei siirry, tarkoitti kuvabaselineja, ja dumppien muutos on perusteltu korjaus (@Pauli, 18.11.).
- **Testikartat:**
  - `src/MapModel/TestFiles/rendering/LordHill.ocd` (OCAD 9) ja `rendering/LordHill OCAD11.ocd` (OCAD 11): väri "Purple" on `t80`.
  - `rendering/wholestructure.ocd` (OCAD 12): `bDarken` ja `bNormal`.
  - `ocadimport/ssl_issprom2019.ocd` (OCAD 2018, haarasta `fix/ocad11-utf8-strings`): `bDarken` ja `bNormal`.
  - Mapper: repossa ei ole Mapperin tallentamaa karttaa, jonka jonkin värin `opacity` on alle 1. Testi tekee sellaisen värin itse ja kierrättää sen. Mapperin tallentama kartta lisätään, jos @Pauli toimittaa sellaisen.
- **Haara:** `feature/opacity-and-blend-mode` juurtuu kokoavaan haaraan `feature/palette-layers`, koska se rakentuu tiketin 001 malliin ja käyttää tiketin 003 testikarttaa.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
