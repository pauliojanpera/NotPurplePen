# OCAD-viivan katkot sijoittuvat Bezier-käyrälle toisin kuin OCADissa

**Konteksti**  
Euklidisella metriikalla OCAD sijoittaa katkot Bezier-käyrälle käyrän neljänneksittäin ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md), kahdeksas ja yhdeksäs näyte):
- **Mitta.** OCAD mittaa käyrän neljänä jänteenä parametrin arvojen 0, ¼, ½, ¾ ja 1 pisteiden välillä. Siitä tulevat katkojen määrä (korjaus [017](017-ocad-dash-count.md)) ja myös katkojen pituus. Purple Pen sovittaa katkojen pituudet tasoitetun käyrän todelliseen pituuteen.
- **Sijoittelu.** Kunkin neljänneksen pituus mitassa on sen jänne, ja neljänneksen sisällä OCAD sijoittaa katkot lähes todellisen kaaren suhteessa. Purple Pen sijoittaa katkot tasaisesti todelliselle pituudelle.
- **Glyyfit.** Kolmannen näytteen PDF-vienti näyttää, että OCAD sijoittaa välimatkan glyyfit käyrälle samassa mitassa: neljänneksittäin sijoitetut glyyfit ovat enintään 0,032 mm:n päässä OCADin glyyfeistä (neliöllinen keskiarvo 0,013 mm), tasaisesti todelliselle pituudelle sijoitetut 0,090 mm:n päässä (0,050 mm).

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

Korjaus on haarassa `fix/ocad-dashes-on-curves` (df4e9975 ja 0c6ec007). `SymPath.GetOcadCurveMeasure` antaa Bezier-käyrällisen polun mitan (`OcadCurveMeasure`): suoran janan pituus on sen pituus ja käyrän neljänneksen pituus sen jänne. Mitta antaa kunkin kulma- ja katkopisteiden välisen osan pituuden ja muuntaa mitan etäisyydet tasoitetun polun pituuksiksi lineaarisesti neljännesten päiden välillä. Polku säilyttää mitan, kuten se säilyttää tasoitetut pisteensä. `ComputeDashDistances` sovittaa OCADin välistyksen katkot ja glyyfit euklidisella metriikalla kunkin osan mittaan ja muuntaa etäisyydet lopuksi, joten `DrawDashed` ja `FindPointsAlongLine` kävelevät polkua ennallaan, ja katkojen ja välien keskikohtien glyyfit pysyvät keskikohdissa. `Bezier.Flatten` antaa pisteidensä indeksit parametrin arvoissa ¼, ½ ja ¾, eikä tasoitus muutu. Missä käyrä on niin suora, ettei sillä ole pistettä neljänneksen päässä, mitta käyttää lähintä pistettä janalta, joka kulkee sen ohi. `SymPath.GetQuarterEnds` antaa jänteiden pisteet sekä mitalle että korjauksen 017 katkojen määrälle (`GetCornerAndDashPointLengthsForCounting`). Mapperin välistykset ja OCADin metriikka eivät muutu. `OcadDashesOnCurvesTests` vertaa yhdeksännen näytteen 17 käyrän katkoja ja kolmannen näytteen seitsemän S-käyrän glyyfejä OCADin PDF-vientiin: katkon päät ovat 0,06 mm:n, korkealla S-käyrällä 0,11 mm:n ja silmukalla 0,21 mm:n päässä OCADin päistä ja glyyfit 0,04 mm:n päässä OCADin glyyfeistä. Ilman korjausta 10 katkojen tapausta 17:stä ja kaikki 7 glyyfien tapausta epäonnistuvat. Korjaus siirtää keräävästä ajosta 44532214497 11 kuvabaselinea (998cf61f): viisi Skian, neljä Test_MapModelin GDI+-piirroksen ja kaksi PDF:n. Ne ovat OCAD 11 -karttoja, jotka mittaavat viivat euklidisesti. Basen omista baselineista korjaus ei siirrä muita kuin korjauksen 017 siirtämät, joten täysi ajo käyttää kansiota `prs/017-ocad-dash-count-base/`. Täysi ajo basen päällä meni läpi kohdassa dc76f6c6 (Tests 44535962486, Build 44535962339). Haara on yhdistetty haaraan `feature/palette-layers` (c53c9bebf, @Pauli, 19.11.). Yhdistämisessä kymmenen kuvabaselinea, joita myös aiemmat korjaukset siirtävät, oli ristiriidassa. Yhdistetyn haaran kerääjäajo 44538544336 piirsi kaikkien korjausten kanssa 145 baselinea, jotka d00c5c4e tallentaa, ja kolme basen route gadget -baselinea kansioon `prs/000-palette-layers-base/`. Sen täysi ajo basen päällä meni läpi (Tests 44543160008, Build 44543159900).

**Päätökset**  
- Neljännesmalli tehdään korjauksena euklidiselle metriikalle, ja OCADin metriikan kaarteet pysyvät ennallaan (@Pauli, 19.11.).
- Myöhempi päätös: myös OCADin metriikan kartoissa katkot ja glyyfit sijoitetaan Bezier-käyrälle tämän korjauksen mallilla: neljänneksen pituus mitassa on sen jänne OCADin metriikalla, ja neljänneksen sisällä pisteet ovat euklidisen kaaren suhteessa. Malli osuu OCADin metriikan PDF-vienteihin yhtä hyvin kuin euklidisiin (@Pauli, 19.11., tiketti [011](011-line-geometry.md), [OcadLines.md](../../OcadLines.md)).
- Haara juurtuu korjauksen 017 haaraan `fix/ocad-dash-count` (b99067b5), koska korjaus käyttää sen neljän jänteen mittaa. Upstreamille korjaus tarjotaan korjauksen 017 jälkeen (@Pauli, 19.11.).

**Haara**  
`fix/ocad-dashes-on-curves` juurtuu haaraan `fix/ocad-dash-count` (b99067b5), joka juurtuu kohtaan, jossa `base` kohtaa upstreamin (d48f4456).
