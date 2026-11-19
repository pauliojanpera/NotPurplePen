# OCAD-viivan katkot sijoittuvat Bezier-käyrälle toisin kuin OCADissa

**Konteksti**  
Euklidisella metriikalla OCAD sijoittaa katkot Bezier-käyrälle käyrän neljänneksittäin ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md), kahdeksas ja yhdeksäs näyte):
- **Mitta.** OCAD mittaa käyrän neljänä jänteenä parametrin arvojen 0, ¼, ½, ¾ ja 1 pisteiden välillä. Siitä tulevat katkojen määrä (korjaus [017](017-ocad-dash-count.md)) ja myös katkojen pituus. Purple Pen sovittaa katkojen pituudet tasoitetun käyrän todelliseen pituuteen.
- **Sijoittelu.** Kunkin neljänneksen pituus mitassa on sen jänne, ja neljänneksen sisällä OCAD sijoittaa katkot lähes todellisen kaaren suhteessa. Purple Pen sijoittaa katkot tasaisesti todelliselle pituudelle.

Neljännesmalli osuu yhdeksännen näytteen useimmilla käyrillä 0,05 mm:n ja jyrkimmillä (korkea S-käyrä ja silmukka) 0,1–0,2 mm:n tarkkuudella. Purple Penin tasainen sijoittelu poikkeaa taipuvilla käyrillä 0,1–0,3 mm ja silmukalla 0,8 mm. Neljänneksen sisällä jää järjestelmällinen ero, joka kasvaa neljänneksen kääntymiskulman neliönä, mutta sen sääntöä ei löytynyt. OCADin metriikalla mikään kokeiltu malli ei osu (0,05–0,19 mm), joten tämä korjaus koskee vain euklidista metriikkaa.

**Toistamisohjeet**  
1. Piirrä yhdeksännen näytteen tiedosto `dashes-9-ocad12-euclidean.ocd`.
2. Vertaa katkojen rajoja OCADin PDF-vientiin `ocad-viewer-dashes-9-ocad12-euclidean.pdf` (`measure-pdf-curves.py`).

**Odotettu vs. Todellinen käytös**  
`odotettu:` katkojen rajat ovat useimmilla käyrillä 0,05 mm:n ja jyrkimmillä 0,2 mm:n päässä OCADin rajoista, kuten neljännesmallissa.  
`todellinen:` Purple Penin rajat poikkeavat OCADin rajoista esimerkiksi käyrillä hook-end 0,13 mm, corner 0,12 mm, slow-start 0,21 mm, S-high 0,28 mm ja loop 0,84 mm.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
Euklidisella metriikalla ja OCADin katkojen sovituksella (`SpacingMethod.OCAD`) `ComputeDashDistances` sovittaa katkojen pituudet polun mittaan, jossa jokainen Bezier-käyrä on neljän jänteensä pituinen, ja polku sijoittaa pituuden pisteeksi samassa mitassa: neljänneksen sisällä todellisen kaaren suhteessa. Suorat osat mitataan ennallaan. Koostettu katkoviiva (tiketti [011](011-line-geometry.md)) tekee kaarteen ajot neljänneksittäin, ja ajon kuvio skaalataan neljänneksen kaaren ja jänteen suhteella. Testi vertaa yhdeksännen näytteen katkoja OCADin PDF-vientiin, kuten korjauksen 020 testi seitsemänteen näytteeseen.

**Päätökset**  
- Neljännesmalli tehdään korjauksena euklidiselle metriikalle, ja OCADin metriikan kaarteet pysyvät ennallaan (@Pauli, 19.11.).
- Haara juurtuu korjauksen 017 haaraan `fix/ocad-dash-count` (b99067b5), koska korjaus käyttää sen neljän jänteen mittaa. Upstreamille korjaus tarjotaan korjauksen 017 jälkeen (@Pauli, 19.11.).

**Haara**  
`fix/ocad-dashes-on-curves` juurtuu haaraan `fix/ocad-dash-count` (b99067b5), joka juurtuu kohtaan, jossa `base` kohtaa upstreamin (d48f4456).
