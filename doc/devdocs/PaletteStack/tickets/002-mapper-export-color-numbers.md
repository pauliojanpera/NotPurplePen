# Mapper-viennin värinumerot eivät vastaa värien paikkoja tiedostossa

**Konteksti**  
Mapper hakee numeroa vastaavan värin siitä paikasta, jossa väri on tiedoston värilistassa, mutta Purple Penin Mapper-vienti numeroi värit niiden kartan paikan mukaan, joten kun ylin väri on rekisterimusta, jota vienti ei kirjoita, jokainen numero on yhtä suurempi kuin värin paikka.

**Toistamisohjeet**  
1. Tuo Mapper-kartta, esimerkiksi `src/MapModel/TestFiles/openmapper/teanaway.omap`. Tuonti lisää ylimmäksi väriksi rekisterimustan.
2. Vie kartta Mapper-muotoon (`InputOutput.WriteFile`, OpenMapper, versio 9).
3. Lue viedyn tiedoston `colors`-elementti.

**Odotettu vs. Todellinen käytös**  
`odotettu:` värien `priority`-arvot ovat tiedoston järjestyksessä 0, 1, 2 ja niin edelleen. Symbolien väriviittaukset ja koosteiden `spotcolor`-viittaukset osoittavat Mapperissa samoihin väreihin kuin Purple Penissä.  
`todellinen:` arvot alkavat ykkösestä. Mapper antaa jokaiselle viittaukselle seuraavan värin, ja viimeisen värin numero osoittaa listan ohi.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes, dev / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Vienti numeroi kirjoittamansa värit niiden tiedostopaikan mukaan. Korjaus on haarassa `fix/mapper-export-color-numbers` (95dc38ed), ja testi `ExportNumbersColorsByPlace` (9ae43910) epäonnistuu ilman sitä. Vika eriytettiin tiketistä 001-inks, jossa koosteet viittaavat spottiväreihin näillä numeroilla. Haara on yhdistetty feature/inksiin (a75c49aa).
