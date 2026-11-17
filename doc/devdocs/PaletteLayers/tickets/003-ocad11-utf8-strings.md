# OCAD 11:n ja uudempien tiedostojen merkkijonot luetaan ja kirjoitetaan väärällä koodauksella

**Konteksti**  
OCAD 11 ja uudemmat koodaavat parametrimerkkijonot ja symbolien Pascal-merkkijonot UTF-8:na, mutta Purple Pen lukee ja kirjoittaa ne koneen 8-bittisellä koodisivulla, joten muut kuin ASCII-merkit menevät rikki molempiin suuntiin.

**Toistamisohjeet**  
1. Tuo OCADin tallentama OCAD 11-, 12- tai 2018-kartta, jonka värien nimissä on ääkkösiä, esimerkiksi SSL:n ISSprOM 2019 -pohja (`src/MapModel/TestFiles/ocadimport/ssl_issprom2019.ocd` haarassa `fix/ocad11-utf8-strings`).
2. Lue värien nimet (`Map.AllColors`).
3. Vie kartta OCAD 12 -muotoon ja etsi tiedostosta värin nimen tavut.

**Odotettu vs. Todellinen käytös**  
`odotettu:` värin nimi on "Vihreä - Pistekohde", ja Purple Penin kirjoittamassa OCAD 11:n tai uudemman tiedostossa nimi on UTF-8:aa.  
`todellinen:` värin nimi on "VihreÃ¤ - Pistekohde". Kirjoitetussa tiedostossa nimi on 8-bittisellä koodisivulla, joten OCAD ja Mapper lukevat sen rikki.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes, dev / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Koodauksen ratkaisee tiedoston versio. OCADin itse kirjoittamista kartoista, joiden merkkijonoissa on muita kuin ASCII-merkkejä, OCAD 9 -tiedostot (182) ja OCAD 10 -tiedostot (4) ovat 8-bittisiä. OCAD 11 -tiedostot (2), OCAD 12 -tiedostot (13) ja OCAD 2018 -tiedostot (153) ovat UTF-8:aa. Mapper lukee merkkijonot samoin. Korjaus on haarassa `fix/ocad11-utf8-strings`: tuonti, vienti ja `OcadDump` käyttävät version koodausta, ja vienti varaa parametrimerkkijonolle tilaa tavujen mukaan (7ef5174a). Testit ovat commitissa 5b76cd35. Purple Penin aiemmin kirjoittamat OCAD 11:n tai uudemmat ratatiedostot, joissa on muita kuin ASCII-merkkejä, luetaan nyt rikki, mutta Purple Pen ei lue ratatiedostojaan takaisin.
