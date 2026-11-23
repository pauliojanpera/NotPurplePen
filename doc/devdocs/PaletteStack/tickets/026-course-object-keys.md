# Rataobjektin avain ja ratakartan symbolien avaimet
**Kuvaus**
`CourseFormatter` ja `TopologyFormatter` antavat jokaiselle rataobjektille, jonka ne lisäävät ratojen asetteluun (`CourseLayout`), rataobjektin avaimen (`CourseObj.Key`): kohdan roolin EventDB:n tiedoissa, kohdan käyttämät EventDB:n tunnisteet ja rataobjektin kerroksen (`CourseLayer`). `CourseLayout.RenderToMap` antaa ratakartan symboleille avaimet rataobjektin avaimesta ja symbolin indeksistä. Piirto ei muutu.

**Hyväksymiskriteerit**
- [x] Rataobjektin avaimella (`CourseObj.Key`) on kolme osaa: kohdan rooli EventDB:n tiedoissa (`CourseObjRole`), kohdan käyttämät EventDB:n tunnisteet ja rataobjektin kerros (`CourseLayer`). Avaimella on tekstimuoto.
- [x] Formatterit antavat avaimen jokaiselle rataobjektille, jonka ne lisäävät ratojen asetteluun (`CourseFormatter.FormatCourseToLayout` ja `TopologyFormatter.FormatCourseToLayout`). Jokaisella kohdalla on oma roolinsa, ja roolit ovat kohdan "Roolit" taulukossa.
- [x] Rataobjektin luokka ei ole osa avainta: koko rastiväli pitää avaimensa, kun se viitoitetaan, rasti pitää avaimensa, kun siitä tulee kartanvaihto, ja rastimerkintä pitää avaimensa, kun sen paikka muuttuu käyttäjän sijoittamaksi (`customNumberPlacement`).
- [x] Avain on erillään kentistä, joita valinta käyttää (`controlId`, `courseControlId`, `courseControlId2` ja `specialId`). Kentät pysyvät ennallaan.
- [x] Avaimet ovat yksikäsitteisiä ratojen asettelussa. Testit tarkistavat sen testitapahtumien jokaisen radan, sen osien, kaikkien rastien näkymän ja topologianäkymän asetteluilla.
- [x] `CourseLayout.RenderToMap` antaa ratakartan symbolille avaimen rataobjektin avaimesta ja symbolin indeksistä niiden symbolien joukossa, jotka rataobjekti lisää karttaan (`CourseObj.AddToMap`). Ratakartta, joka tehdään uudelleen samasta EventDB:n tilasta, antaa samat avaimet samoille rataobjekteille.
- [x] Avaimet pysyvät samoina muokkauksissa, jotka eivät muuta rataobjektin roolia, tunnisteita eivätkä kerrosta. Testit tarkistavat rastin siirron, komennon "Lisää kulmapiste" (`Leg.bends`), rastinumeron siirron ja koko rastivälin viitoituksen.
- [x] Piirto ei muutu, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot**
- **Päätökset:**
  - @Pauli hyväksyi tämän tiketin 23.11., myös sen luennan, että rooli kuvaa sen, mitä kohta tekee EventDB:n tiedoista (ks. "Roolit").
  - Neljä tikettiä korvaa tiketit 005, 007 ja 008, ja tämä on niistä neljäs. Sen haara juurtuu haaraan `feature/palette-stack` tiketin 025 jälkeen (@Pauli, 23.11., Progress.md: "The new tickets of pass 1").
  - Rataobjektin avain (@Pauli, 21.11., "The identity of a CourseObj"): `CourseFormatter` tai `TopologyFormatter` antaa sen kohdassa, jossa se tekee rataobjektin. Avaimessa ovat kohdan rooli EventDB:n tiedoissa, kohdan käyttämät EventDB:n tunnisteet ja `CourseLayer`. Jokaisella kohdalla on oma roolinsa. Avain on erillään valinnan kentistä, jotka ovat assosiaatioita eivätkä identiteettejä: rastiympyrällä ja sen numerolla tai osittain viitoitetun rastivälin kahdella osalla on samat assosiaatiot, ja rastivälin kartanottopaikalla ei tarkoituksella ole niitä. Rataobjektin luokka ei ole osa avainta, joten koko rastiväli pitää avaimensa, kun se viitoitetaan.
  - Ratakartassa symbolin avaimen tekee `CourseLayout.RenderToMap` EventDB:stä: rataobjektin avaimesta ja symbolin indeksistä rataobjektin symbolien joukossa (@Pauli, 21.11., "The symbol key").
  - Laskurin avaimissa on vain ASCII-kirjaimia ja -numeroita, joten välimerkit jäävät ratakartan avainten rakenteelle (@Pauli, 23.11., "The text form of the counter keys").
- **Toteutus** (haara `feature/course-object-keys`):
  - `CourseObjKey.cs` (PurplePenCore): `CourseObjRole` ja sen 13 roolia sekä `CourseObjKey`, jolla on rooli, tunnisteet, kerros ja tekstimuoto. Kaksi avainta ovat samat, kun niiden tekstimuodot ovat samat. Tunnisteet ja kerros ovat kannan 62 lukuja (`SymbolKey.ToBase62`).
  - Formatteri antaa roolin ja tunnisteet sisäisellä metodilla `CourseObj.SetKey`, ja `CourseObj.Key` tekee avaimen, kun se luetaan, kerroksen kentästä `layer`. Formatterien 11 kohtaa antavat roolit taulukon mukaan. Valinnan kentät eivät muutu, eikä avain ole osa vertailua `Equals` eikä tekstimuotoa `ToString`.
  - `Map.PrefixSymbolKeys(string prefix)` palauttaa jakson vapauttajan (`SymbolKeyPrefixReleaser`), joka lopettaa jakson, kun se vapautetaan. Indeksi ohittaa indeksin, jonka avain on jo kartan symbolilla, joten kaksi saman avaimen rataobjektia antaa symboleilleen eri avaimet ilman käyttövirhettä. Jakso on kirjoituksen sisällä, eivätkä jaksot ole sisäkkäisiä. Avaimeton rataobjekti, jollaisen vain muu koodi kuin formatterit voi tehdä, antaa symboleilleen laskurin avaimet (istunnon valinnat 23.11.).
  - `CourseLayout.RenderToMap` avaa jakson rataobjektin avaimen tekstimuodolla jokaisen `AddToMap`-kutsun ajaksi, joten `AddToMap`in 14 toteutusta eivät muutu.
  - Testit: PurplePen_Testsin `CourseObjKeyTests` (14 testiä) käyttää 11 testitapahtumaa: `marymoor.ppen`, `SpecialLegs.ppen`, `variations.ppen`, kansion `courserenderer` tapahtumat `Fake Relay.ppen`, `butterfly.ppen`, `GappedLegs.coursescribe`, `SpecialLegs.coursescribe` ja `MapIssueTest.ppen` sekä `courseformat/mapexchange1.ppen`, `courseformat/marymoor1.coursescribe` ja `relay/harper.ppen`. Niiden 584 asettelussa on 51 098 rataobjektia. Asettelut ovat kunkin radan, sen osien ja hajontojen asettelut, joissa ovat myös kaikkien rastien näkymä ja muut radat kuten `SelectionMgr` tekee ne, kaikkien rastien näkymä yksin ja topologianäkymät. Muokkausten testi käyttää tapahtumaa `changeevent/sampleevent1.coursescribe` ja tarkistaa myös kartanvaihdon. Test_MapModelin `SymbolKeyTests` saa avainjaksojen kolme testiä (18 testiä).
  - Täysi ajo meni läpi bc67cf2e:ssä (`wip/linux/course-object-keys`, basen päällä, valitsimella `--base-baselines prs/000-palette-stack-base`: Tests 45111279178, Build 45111279158) ilman keräävää ajoa. Sen PurplePen-ryhmä ajoi `CourseObjKeyTests`in 14 testiä ja MapModel-ryhmä avainjaksojen 3 testiä. Haara on yhdistetty haaraan `feature/palette-stack` (3e98e556).
- **Speksi:** [Specification.md](../Specification.md), luvun 02 kohta "The symbol keys and the course object keys". Nimet ovat [sanastossa](../Glossary.md): `CourseObj` · rataobjekti, `CourseObj.Key` · rataobjektin avain ja `Symbol.Key` · symbolin avain.
- **Roolit:** formatterit lisäävät rataobjekteja ratojen asetteluun näissä kohdissa. Rooli kuvaa sen, mitä kohta tekee EventDB:n tiedoista, joten kaksi koodin kohtaa, jotka tekevät saman asian, kuten rastimerkinnän käyttäjän sijoittamana tai automaattisesti, antavat saman roolin.

  | Rooli (`CourseObjRole`) | Kohta | Tunnisteet |
  |---|---|---|
  | `Special` | erityiskohteen rataobjekti (`CourseFormatter.CreateSpecial`) | `specialId` |
  | `Description` | rastinmääritteiden rataobjekti (`CreateDescriptionSpecial`) | `specialId` |
  | `Control` | rastinäkymän rasti, lähtö, maali, kartanvaihto, ylitys- tai alituspaikka tai kartanottopaikka (`CreateCourseObject`) | `controlId`, `courseControlId` |
  | `ExtraCourseControl` | radan maali, jonka radan osa näyttää (`CourseView.ExtraCourseControls`) | `controlId`, `courseControlId` |
  | `Label` | rastimerkintä eli numero tai koodi, käyttäjän sijoittamana tai automaattisesti (`CreateControlNumber`, `CreateCode`) | `controlId`, `courseControlId` |
  | `Leg` | koko rastiväli, viitoitettu tai viitoittamaton (`CreateLeg`) | rastivälin alun ja lopun `courseControlId` |
  | `LegBeginning` | osittain viitoitetun rastivälin alkuosa | samat |
  | `LegEnd` | osittain viitoitetun rastivälin loppuosa | samat |
  | `LegMapIssuePoint` | rastivälin kartanottopaikka (`FlaggingKind.IssuePointMiddle`) | samat |
  | `TopologyControl` | topologianäkymän rasti, lähtö, maali, ylitys- tai alituspaikka tai kartanottopaikka (`TopologyFormatter.CreateControlNumber`) | `controlId`, `courseControlId` |
  | `TopologyLeg` | topologianäkymän rastiväli (`CreateLegBetweenControls`) | rastivälin alun ja lopun `courseControlId` |
  | `DropTarget` | topologianäkymän rastivälin pudotuskohde (`TopologyDropTargetCourseObj`) | samat ja lisäyskohta (`LegInsertionLoc`) |
  | `VariationCode` | hajonnan koodi haaran alussa (`CreateVariationCode`) | `controlId` ja haaran `courseControlId` |
- **Nykytila** (`feature/palette-stack`):
  - Rataobjektilla ei ole avainta. Sen kentät `controlId`, `courseControlId`, `courseControlId2` ja `specialId` ovat valinnan assosiaatioita. EventDB:n tunniste `Id<T>` on kokonaisluku, ja `Id<T>.None` on 0.
  - Formatterit lisäävät rataobjektit ratojen asetteluun 11 kohdassa: `CourseFormatter.FormatCourseToLayout` seitsemässä ja `TopologyFormatter.FormatCourseToLayout` neljässä. Formatterit asettavat rataobjektin kerroksen sen jälkeen, kun ne ovat tehneet sen. Radan osan maali saa oletuskerroksen `MainCourse`.
  - Yhdessä asettelussa voi olla monta kerrosta: `SelectionMgr` lisää pääradan kerrokseen `MainCourse`, kaikkien rastien näkymän kerrokseen `AllControls` ja muut radat kerroksiin `OtherCourse1` + *i* mod 10. Yli kymmenen muuta rataa jakaa kerroksen, mutta niiden radan rastien tunnisteet ovat eri ratojen, eikä niillä ole erityiskohteita. Topologianäkymä käyttää kerroksia `AllVariations`, `MainCourse` ja `InvisibleObjects`.
  - `CourseLayout.RenderToMap` tekee ratakartan ja lisää jokaisen rataobjektin symbolit metodilla `CourseObj.AddToMap`, jolla on 14 toteutusta. Ne lisäävät symbolit metodilla `Map.AddSymbol` seitsemässä kohdassa.
  - `CourseObj.Equals` vertaa kenttiä. Formatterien testit vertaavat asetteluja tekstinä, jonka `CourseLayout.Dump` kirjoittaa rataobjektien tekstimuodoista (`ToString`).
- **Ratkaisut** (nimet valitsee istunto, @Pauli, 23.11.):
  - Avaimen tyyppi on `CourseObjKey`: rooli, tunnisteet ja kerros. Sen tekstimuoto on `<rooli>.<tunniste>….<kerros>`, esimerkiksi `leg.4.5.0`. Tunnisteet ja kerros ovat kannan 62 lukuja kuten laskurin avaimet, ja rooli on `CourseObjRole`n jäsenen nimi pienellä alkukirjaimella.
  - Formatteri antaa roolin ja tunnisteet kohdassa, jossa se tekee rataobjektin. Avain ottaa kerroksen rataobjektin kentästä `CourseObj.layer`, kun avain luetaan, koska formatterit asettavat kerroksen vasta tekemisen jälkeen.
  - `Map.PrefixSymbolKeys(string prefix)` (MapModel) aloittaa jakson, jonka aikana `Map.AddSymbol` antaa avaimettomalle symbolille avaimen `<prefix>/<indeksi>`. Indeksi on kannan 62 luku nollasta alkaen lisäysjärjestyksessä. `RenderToMap` avaa jakson rataobjektin avaimen tekstimuodolla jokaisen `AddToMap`-kutsun ajaksi, joten `AddToMap`in toteutukset eivät muutu. Esimerkiksi rastivälin ensimmäisen symbolin avain on `leg.4.5.0/0`.
  - Avain ei ole osa vertailua `CourseObj.Equals` eikä tekstimuotoa `ToString`, joten formatterien nykyiset testit pysyvät ennallaan. Uudet testit vertaavat avaimia erikseen.
- **Rajaus:** kartan laskurin avaimet ovat tiketti [025](025-symbol-keys.md). Avainten kirjoitus SVG- ja PDF-tiedostoihin kuuluu koodauksiin, ja tiedoston avaaminen kartaksi ei kuulu epiciin. Tiketit 023 ja 024 eivät käytä avaimia.
- **Testit:** rataobjektien avainten testit ovat PurplePen_Testsin tiedostossa `PurplePen/CourseObjKeyTests.cs`, esimerkiksi kansion `src/TestFiles` tapahtumilla, ja täysi ajo ajaa ne. Paikallisesti ne voi ajaa integraatioworktreessä projektilla `src/Testing/LinuxTestRun`, kun tiedosto lisätään paikallisesti sen projektiin. Avainjakson testit ovat Test_MapModelin tiedostossa `SymbolKeyTests.cs`. Piirto ei käytä avaimia, joten täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack` ilman keräävää ajoa: `tools/push-wip.sh --base-baselines prs/000-palette-stack-base <klooni> feature/course-object-keys`.
- **Haara:** `feature/course-object-keys` juurtuu haaraan `feature/palette-stack` sen jälkeen, kun tiketin 025 haara on yhdistetty siihen. Täysi ajo tehdään haarassa `wip/linux/course-object-keys` basen päällä valitsimella `--base-baselines prs/000-palette-stack-base`.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md), [../README.md](../README.md), [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja "Decisions", speksin esipuhe ja luvun 02 kohta "The symbol keys and the course object keys", [../Glossary.md](../Glossary.md), tiketti [025](025-symbol-keys.md) sekä tämä tiketti. [../tools/README.md](../tools/README.md) kertoo tiketin työkierron.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
