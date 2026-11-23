# Tekstien ja suorakulmioiden geometriametodit
**Kuvaus**
Tekstin symbolimäärittely (`TextSymDef`) ja suorakulmion symbolimäärittely (`RectangleSymDef`) antavat symboliensa
täyden peiton palettiväripeittojen leikkausobjektit omalla geometriametodillaan ilman piirtoa: tekstisymbolin
(`TextSymbol`) tekstijuosteet tekstin kehyksineen, alleviivaukset, tekstin keskipisteen ja kehyssuorakulmion, viivan
varren tekstin (`LineTextSymbol`) grafeemit tekstijuosteina sekä suorakulmiosymbolin (`RectangleSymbol`) viivan,
ruudukon viivat ja ruudukon tekstit. Tekstin leikkausobjektien sijoituksen sisin muunnos on tekstin asema, ja Y-akselin
peilaus, jolla piirto asettaa tekstin, siirtyy muotoihin. Tämän tiketin jälkeen jokaisella symbolimäärittelyllä on
geometriametodi, eikä vaihe 1 käytä kerääjää. Kerääjä on testien oraakkeli tikettiin 009 asti. Piirto ei muutu.

**Hyväksymiskriteerit**
- [ ] Geometriametodien rajapinta: `TextSymDef` ja `RectangleSymDef` korvaavat tiketin
  [024](024-point-symbol-geometry.md) rajapinnan `SymDef.MakeClippingObjects(Symbol, SymColor, CoveragePrototype)`.
  `TextSymDef` antaa sekä tekstisymbolin että viivan varren tekstin leikkausobjektit. Rajapinta on abstrakti, joten
  jokaisella symbolimäärittelyllä on geometriametodi: `HoleSymDef` ja `ImageSymDef` eivät anna leikkausobjekteja, koska
  ne eivät piirrä palettiväriä, eikä `GraphicsSymDef` anna kerääjän leikkausobjekteja millekään symbolille. Vaihe 1
  tekee jokaisen palettiväripeiton leikkausobjektit geometriametodilla ilman piirtokohdetta (`IGraphicsTarget`) ja ilman
  piirtoasetuksia (`RenderOptions`), eikä se käytä kerääjää (`ClippingObjectCollector`). Kerääjä
  (`SymDef.CollectClippingObjects`) jää testien oraakkeliksi.
- [ ] Tekstisymboli: geometriametodi antaa palettivärissä ne leikkausobjektit, jotka `TextSymDef.Draw` piirtää siinä,
  samassa järjestyksessä: rivi kerrallaan kunkin sanan, tai kunkin grafeemin, kun merkkiväli on suurempi kuin 0,
  tekstijuosteena omassa paikassaan fontin värissä ja tekstin kehyksen värissä kehysviivana tai varjona, kunkin rivin
  jälkeen sen alleviivaukset, sitten tekstin keskipisteen ja viimeisenä kehyssuorakulmion. Tyhjä tekstijuoste ei peitä
  mitään, eikä siitä tule leikkausobjektia.
- [ ] Tekstijuosteen fontti on tekstin symbolimäärittelyn fontti ilman fontin alleviivausefektiä
  (`TextEffects.Underline`), kuten `CreateObjects` luo sen. Alleviivaus ja fontin alleviivausefekti ovat viivamuotoja,
  kuten `DrawUnderline` piirtää ne: alleviivaus kappaleen viimeisen rivin alla omassa värissään ja fontin
  alleviivausefekti jokaisen rivin alla fontin värissä.
- [ ] Tekstin kehys: kehysviiva on tekstijuosteen täytetyn muodon jälkeen kehyksen värissä yksi viivamuoto,
  tekstijuosteen ääriviiva täydellä leveydellä, joka on kaksi kertaa kehyksen viivanleveys, ja kehyksen viivatyylillä
  (päätös 19.11.). Varjo on tekstijuoste kehyksen värissä siirrettynä `shadowX`:n ja `shadowY`:n verran.
  Kehyssuorakulmio on täytetty muoto rivien ympärillä marginaaleineen, ja sen alareuna on alleviivauksen alla, kun
  alleviivaus on päällä, kuten `DrawFramingRectangle`.
- [ ] Tekstin asema: tekstisymbolin leikkausobjektin yksittäisen sijoituksen sisin affiininen muunnos on tekstin asema,
  sen kohta (`TextSymbol.Location`) ja kulma (`Rotation`). Muodot ovat aseman koordinaateissa, joiden Y-akseli osoittaa
  ylöspäin kuten kartan: alleviivausten ja kehyssuorakulmion pisteiden ja tekstijuosteiden vasempien yläkulmien
  y-koordinaatit ovat vastakkaiset piirron koordinaatteihin nähden, ja tekstijuoste asettaa glyyfinsä vasemmasta
  yläkulmastaan alaspäin X-akselin suuntaan. Piirto asettaa rivit Y-akseli alaspäin: sen muunnos on
  `Translate(Location)`, `Scale(1, -1)` ja `RotateAt(-Rotation)`.
- [ ] Tekstin keskipiste: muotoilemattoman tekstin keskipisteen leikkausobjektit tulevat pistesymbolin
  symbolimäärittelyn geometriametodilla (tiketti 024) tekstin sijoituksen sisällä: tekstin asema, siirto keskipisteeseen
  ja glyyfin asema. Muotoillulla tekstillä ei ole keskipistettä, kuten `DrawCenterPoint`issa.
- [ ] Viivan varren teksti: geometriametodi antaa kunkin grafeemin tekstijuosteena kehysviivoineen tai varjoineen omassa
  yksittäisessä sijoituksessaan, jonka asema on grafeemin alkupiste polulla ja suunta alkupisteestä grafeemin
  loppupisteeseen, kuten `DrawTextOnPath`: grafeemit asettuvat polun varrelle OCADin metriikalla
  (`GetLineTextGraphemePlacement`). Tekstijuosteen vasen yläkulma on aseman koordinaateissa, kuten tekstisymbolin.
  Viivan varren tekstillä ei ole alleviivausta, keskipistettä eikä kehyssuorakulmiota, joten niiden väreissä sen
  palettiväripeitoilla ei ole leikkausobjekteja.
- [ ] Suorakulmiosymboli: geometriametodi antaa viivan värissä suorakulmion viivan viivamuotona, joka pitää
  pyöristettyjen kulmien Bezier-käyrät, sekä ruudukon viivat ja ruudukon tekstit ruudukon sijoituksessa (`Location` ja
  `Rotation`), siinä järjestyksessä kuin `RectangleSymDef.Draw` piirtää ne. Ruudukon tekstin asema on tekstin paikka
  solussa kulmalla 0, ja tekstijuosteen vasen yläkulma on aseman origossa.
- [ ] Jokaisen geometriametodin leikkausobjektin sijoitus antaa asemansa (`Placement.Poses`).
- [ ] Kerääjä on oraakkeli: testit vertaavat geometriametodien leikkausobjekteja kerääjän leikkausobjekteihin
  (`SymDef.CollectClippingObjects`) rakenteena jokaisessa tekstisymbolin, viivan varren tekstin ja suorakulmiosymbolin
  palettiväripeitossa. Vertailu siirtää kerääjän Y-akselin peilauksen muotoihin ja ohittaa kehysviivan kapeammat viivat
  (ks. "Sallitut erot"). Muut leikkausobjektit ovat täsmälleen samat. Vertailu kattaa kansion `skia_render` jokaisen
  OCAD- ja Mapper-kartan sekä katkoviivanäytteiden tiedostot `dashes-*.ocd`.
- [ ] Rakennetestit tarkistavat ilman piirtoa tekstijuosteet ja niiden paikat, merkkivälin, kehysviivan, varjon,
  kehyssuorakulmion, alleviivauksen ja fontin alleviivausefektin, tekstin keskipisteen, tekstin aseman ja Y-akselin
  suunnan, viivan varren tekstin grafeemit asemineen, suorakulmion viivan, ruudukon viivat ja tekstit sekä
  numeroimattomat solut. Testit rakentavat kartan tapauksille, joita vertailun kartoissa ei ole (ks. "Testit").
- [ ] Tikettien 023, 024, 011 ja 012 testit menevät läpi, myös peiton vertailu, jossa kaikki leikkausobjektit tulevat
  geometriametodeilta.
- [ ] Tämän tiketin haara ei muuta piirtoa, eikä yksikään kuva- tai tekstibaseline siirry.

**Lisätiedot**
- **Päätökset:**
  - @Pauli hyväksyi tämän tiketin ja sen istunnon valinnat (ks. "Ratkaisut") 23.11., myös tekstin aseman: tekstin
    sijoituksen sisin muunnos on tekstin asema, ja Y-akselin peilaus, jolla piirto asettaa tekstin, siirtyy muotoihin.
  - Geometriametodit tehdään neljänä osana symbolilajeittain, ja tämä on tekstien ja suorakulmioiden osa, erotettu
    tiketistä 008 (@Pauli, 18.11.). Tiketit 023–026 korvaavat tiketit 005, 007 ja 008 (@Pauli, 23.11.), joten tämä
    tiketti käyttää tiketin 024 rajapintaa ja oraakkelia. @Pauli hyväksyy luonnoksen ensin tiketiksi, joka ei ole enää
    luonnos, ja vasta sitten se toteutetaan (@Pauli, 23.11.).
  - Vanha piirtokoodi pysyy ennallaan geometriametodien ajan, jotta kerääjä pysyy riippumattomana oraakkelina.
    Geometriametodit kirjoitetaan sen rinnalle, ja symbolin kokoava logiikka saa olla kahdessa paikassa (@Pauli,
    18.11.). Kun muutos ei voi muuttaa piirtoa, täysi ajo tehdään ilman keräävää ajoa (@Pauli, 18.11.).
  - Kerääjä on testien oraakkeli tikettien 008–013 ajan, ja tiketti 009 poistaa sen ja asetuksen
    `omitHatchingsAndPatterns` (@Pauli, 18.11.).
  - Hyödyn mittari: välikerroksen rakenteesta on hyötyä, kun se antaa kanavien ratkaista perustapaukset abstraktiolla,
    jonka kanava tarjoaa (@Pauli, 19.11.).
  - Tekstin rakenne: jokainen sana, tai grafeemi, kun merkkiväli on suurempi kuin 0, on tekstijuoste omassa paikassaan,
    kuten piirrossa ja kerääjässä. Kanavat ottavat paikoitetut tekstijuosteet sellaisinaan (SVG:n `tspan` x ja y, PDF:n
    `Td` ja `TJ`, Skian paikoitettu tekstiblobi), ja paikat pysyvät Purple Penin mittojen mukaisina (kysymys 1, @Pauli,
    19.11.).
  - Kehysviiva: tekstijuosteen kehys on yksi viiva täydellä leveydellä ja symbolin liitoksilla, kuten OCADin
    PDF-viennissä. OCADin oma piirros jättää kehykseen samoja reikiä kuin Skia ja Cairo yhdellä kynällä, joten Purple
    Penin välikynien paikkaamat reiät eivät ole virheitä OCADiin verrattuna. Vertailu kerääjään sallii eron, ja tiketti
    kertoo sen syyn (kysymys 2, @Pauli, 19.11., [oracles/ocad-framing.md](../oracles/ocad-framing.md)).
  - Sijoitus antaa asemansa, ja asema on sijoituksen sisäkkäisistä affiinisista muunnoksista sisin (@Pauli, 20.11.).
    Tiketti 011 jätti tämän tiketin ratkaistavaksi kerääjän tekstien sijoituksen, jonka sisin muunnos peilaa Y-akselin
    (@Pauli hyväksyi tiketin 011 23.11.).
- **Speksi:** [Specification.md](../Specification.md), luvut 02 ja 03. Luvun 02 kohta "The symbol definitions and the
  symbols" antaa symbolimäärittelyt ja symbolit, ja kohta "The lines" viivan varren tekstin OCADin metriikan. Luvun 03
  kohta "The forms of the derived values" antaa tekstijuosteen (`TextShape`) ja kehysviivan, ja kohta "The placements"
  sijoituksen, aseman ja sisäkkäiset affiiniset muunnokset. Nimet ovat [sanastossa](../Glossary.md).
- **Nykytila** (`feature/palette-stack`, 903e6bde, tiketin 012 jälkeen):
  - `TextSymbol.Draw` kutsuu metodia `TextSymDef.Draw` rivitetyllä tekstillä (`WrappedText`). `Draw` palaa, kun
    palettiväri ei ole fontin väri, tekstin kehyksen väri, alleviivauksen väri eikä keskipisteen väri. Se asettaa
    muunnoksen `Translate(Location)`, `Scale(1, -1)` ja `RotateAt(-Rotation)` ja käy rivit läpi Y-akseli alaspäin:
    sanat, tai grafeemit, kun merkkiväli on suurempi kuin 0, piirtää `DrawStringWithEffects`, välilyönnit ja sarkaimet
    siirtävät paikkaa, ja jokaisen rivin jälkeen `DrawUnderline` piirtää fontin alleviivausefektin ja kappaleen
    viimeisen rivin jälkeen alleviivauksen. Lopuksi se piirtää keskipisteen (`DrawCenterPoint`) ja kehyssuorakulmion
    (`DrawFramingRectangle`).
  - `DrawStringWithEffects` täyttää tekstijuosteen fontin värissä. Kehyksen värissä se täyttää kehysviivan
    tekstijuosteen ja piirtää sen ääriviivan ensin kapeammilla kynillä, 0,33 kertaa fonttikoko ja sen monikerrat
    viisteliitoksin, ja viimeisenä täyden levyisellä kynällä kehyksen viivatyylillä, tai se täyttää varjon siirretyn
    tekstijuosteen.
  - `DrawCenterPoint` piirtää keskipisteen vain muotoilemattomalle tekstille (`fullWidth == 0`): se asettaa tekstin
    muunnoksen sisälle muunnoksen `Translate(keskipiste)` ja `Scale(1, -1)`, ja pistesymbolin symbolimäärittely piirtää
    glyyfinsä kohtaan (0, 0) kulmalla 0.
  - `LineTextSymbol.Draw` kutsuu metodia `DrawTextOnPath`, joka piirtää vain fontin värin ja kehyksen värin, kehysviivan
    tai varjon. Kullekin grafeemille se asettaa muunnoksen `Translate(alkupiste)`, `Scale(1, -1)` ja `RotateAt(-kulma)`
    ja piirtää tekstijuosteen kohtaan (0, `-LineTextVertOffset`). `GetLineTextGraphemePlacement` mittaa polun OCADin
    metriikalla (`BizzarroLength`, `PointAtLengthBizzarro`).
  - `RectangleSymDef.Draw` piirtää viivan värissä suorakulmion viivan polkuna (`GetPath`,
    `SymPath.CreateRoundedRectangle`) kynällä, jonka leveys on `LineThickness` ja viivatyyli `Rounded`, kun kulmat ovat
    pyöristetyt, ja muuten `Mitered`. Ruudukon se piirtää muunnoksessa `Translate(Location)` ja `Rotate(Rotation)`:
    ruudukon viivat 0,15 mm:n kynällä viisteliitoksin ja ruudukon tekstit Arial-fontilla lihavoituna muunnoksessa
    `Translate(paikka)` ja `Scale(1, -1)` kohtaan (0, 0).
  - Kerääjä tekee tekstistä täytetyn tekstijuosteen (`DrawText`) tai tekstijuosteen viivamuodon (`DrawTextOutline`) sen
    vasemmalla yläkulmalla nykyisessä sijoituksessa, ja suorakulmion täytöstä polun sen neljästä kulmasta. Yksittäinen
    sijoitus heittää poikkeuksen, kun sen asemia kysytään ja sen sisin muunnos ei ole kierto ja siirto: niin on kaikissa
    kerääjän tekstien sijoituksissa.
  - Tiketin 012 jälkeen kerääjä tekee vaiheessa 1 vain tekstisymbolien, viivan varren tekstien ja suorakulmiosymbolien
    leikkausobjektit, perusluokan `SymDef.MakeClippingObjects` kautta. `GraphicsSymDef` antaa muun lajin symbolille
    kerääjän leikkausobjektit, mutta vain `GraphicsAreaSymbol` ja `GraphicsLineSymbol` saavat `GraphicsSymDef`in.
    `HoleSymDef` ja `ImageSymDef` eivät piirrä palettiväriä, joten vaihe 1 ei kutsu niitä.
- **Vanha työ** (`feature/text-geometry`, 2ad8c6f3, juurtuu vanhaan haaraan `feature/area-geometry`):
  - Sopii: `TextSymDef.AddClippingObjects`, joka käy rivit, sanat ja grafeemit läpi `Draw`in rinnalla,
    `AddStringWithEffects`, jossa kehysviiva on yksi viiva, `AddUnderline`, `AddCenterPoint`, `AddFramingRectangle`,
    `AddClippingObjectsOfTextOnPath`, `RectangleSymDef.AddClippingObjects` ja `AddGridText`, `PathShape.FromRectangle`,
    joka antaa suorakulmion kulmat kerääjän järjestyksessä, vertailun sääntö kehysviivan kapeammista viivoista sekä
    testit `SymbolGeometryTests`issä. Ne sopivat, kun geometriametodi antaa yhden palettivärin leikkausobjektit tiketin
    024 rajapinnalla ja tekstin sijoituksen sisin muunnos on tekstin asema.
  - Ei sovi: rakentaja (`LayerInstanceBuilder`), joka lisää kaikkien palettivärien leikkausobjektit,
    `Symbol.AddClippingObjects`, koska rajapinta on `SymDef.MakeClippingObjects`, tekstin sijoitus, jonka sisin muunnos
    peilaa Y-akselin, ja `Map.CreateLayerInstances`.
- **Ratkaisut** (istunnon valinnat, @Pauli hyväksyi ne tiketin kanssa 23.11.):
  - `TextSymDef` ja `RectangleSymDef` korvaavat metodin `SymDef.MakeClippingObjects`. `TextSymDef` antaa tekstisymbolin
    leikkausobjektit sisäisellä metodilla, joka seuraa `Draw`ia, ja viivan varren tekstin leikkausobjektit sisäisellä
    metodilla, joka seuraa `DrawTextOnPath`ia.
  - `SymDef.MakeClippingObjects` muuttuu abstraktiksi, jotta vaiheella 1 ei ole tietä kerääjään. `HoleSymDef` ja
    `ImageSymDef` antavat tyhjän luettelon, ja `GraphicsSymDef` heittää muun lajin symbolille poikkeuksen
    `ArgumentException`, koska vain `GraphicsAreaSymbol`in ja `GraphicsLineSymbol`in rakentaja ottaa `GraphicsSymDef`in.
  - Y-akselin peilaus siirtyy muotoihin: tekstin sijoituksen sisin muunnos on tekstin asema `Translate(Location)` ja
    `RotateAt(Rotation)`, ja muotojen y-koordinaatit ovat vastakkaiset piirron koordinaatteihin nähden. Speksin luvun 03
    mukaan asema on sisin muunnos ja kierto ja siirto, ja piirron muunnos `Translate(Location)`, `Scale(1, -1)` ja
    `RotateAt(-Rotation)` on sama kuin `Translate(Location)`, `RotateAt(Rotation)` ja `Scale(1, -1)`. Vertailun
    kartoissa tämä on mitattu: kun kerääjän 7 122 tekstisymbolin leikkausobjektin sisin muunnos kerrotaan peilauksella
    `Scale(1, -1)`, se on bitilleen tekstin asema, ja kolmen keskipisteen kahden ulomman muunnoksen yhdistelmä on
    bitilleen tekstin aseman ja keskipisteeseen siirron yhdistelmä. Samoin 1 957 viivan varren tekstin leikkausobjektin
    muunnos on peilauksen jälkeen tarkasti kierto ja siirto, ja 235 ruudukon tekstin sisin muunnos tarkasti siirto.
    Peilaus vaihtaa vain lukujen etumerkkejä, joten vertailu pysyy tarkkana.
  - Tämä tarkentaa speksin luvun 03 kohtaa "The forms of the derived values" ja sanaston merkintää *tekstijuoste*:
    tekstijuosteen vasen yläkulma on sijoituksen koordinaateissa, joiden Y-akseli osoittaa ylöspäin, ja glyyfit ovat sen
    alapuolella. Speksi ja sanasto saivat tarkennuksen, kun @Pauli hyväksyi tiketin (23.11.).
  - Peiton vertailun apufunktio (tiketti 023) piirtää tekstijuosteen peilaamalla Y-akselin ja vasemman yläkulman
    y-koordinaatin, jolloin piirtokohde saa saman muunnoksen ja saman kohdan kuin piirrossa.
  - Tekstin keskipisteen leikkausobjektit tulevat glyyfin geometriametodilla (`Glyph.MakeClippingObjects`), joka saa
    ulomman sijoituksen: tekstin aseman ja siirron keskipisteeseen, jonka y-koordinaatti on vastakkainen piirron
    koordinaattiin nähden.
  - Ruudukon tekstin asema on piirron muunnos `Translate(paikka)` ilman peilausta, ja viivan varren tekstin grafeemin
    asema on `Translate(alkupiste)` ja `RotateAt(kulma)`, ja sen tekstijuosteen vasen yläkulma on (0,
    `LineTextVertOffset`).
- **Sallitut erot** vertailussa kerääjään:
  - Y-akselin peilaus: kerääjän sijoituksen muunnos, joka peilaa Y-akselin, kerrotaan peilauksella `Scale(1, -1)`, ja
    peilaus siirtyy seuraavaan sisempään muunnokseen tai sisimmästä muunnoksesta muotoihin: muotojen pisteiden ja
    tekstijuosteiden vasempien yläkulmien y-koordinaatit vaihtavat etumerkkiä. Sen jälkeen luvut ovat samat tarkasti.
  - Kehysviiva: piirto vetää tekstijuosteen ääriviivan ensin kapeammilla kynillä ja viimeisenä täyden levyisellä
    kynällä. Kapeammat viivat ovat leveimmän sisällä, ja leikkausobjektin kehysviiva on se yksi viiva (päätös 19.11.).
    Siksi vertailu säilyttää kerääjän peräkkäisistä saman tekstijuosteen viivamuodoista samassa sijoituksessa vain
    viimeisen.
- **Rajaus:**
  - Kerääjä on testien oraakkeli, ja asetus `omitHatchingsAndPatterns` jää sen mukana. Viivoitukset ja aluekuviot
    peittoprototyyppeinä ja kerääjän oraakkelikäytön loppu ovat luonnos [009](009-pattern-instances.md).
  - Vaihe 2 on luonnos [010](010-palette-composition.md). SVG-koodaus ja litistys eivät kuulu tähän, eivätkä
    korostukset.
  - Kuvatason ja asettelutason objekteilla (`ImageSymDef`, esimerkiksi `ImageTextSymbol`) ja rei'illä (`HoleSymDef`) ei
    ole palettiväripeittoja.
- **Testit:** Paikallinen Map_Skia.Testsin ajo on `tools/linux-renderings.sh` integraatioworktreessa
  (`tools/int-worktree.sh`) Windows-koneen fonteilla (`tools/windows-fonts.sh`), ja sen jokainen testi menee läpi
  (Progress.md, "Local tests"). Vanha piirtokoodi ei muutu, joten `tools/compare-renderings.py` ei löydä yhtään
  siirtynyttä kuvaa, kun se vertaa ajon renderöintejä haaran `feature/palette-stack` renderöinteihin.
  - Vertailun kartoissa (kansion `skia_render` 162 karttaa) on 1 671 tekstisymbolia 67 kartassa ja niillä 1 179
    palettiväripeittoa, 86 viivan varren tekstiä 15 kartassa ja niillä 93 palettiväripeittoa sekä 175
    suorakulmiosymbolia 18 kartassa ja niillä 147 palettiväripeittoa. Kerääjä tekee tekstisymboleista 6 973 täytettyä
    tekstijuostetta, 116 kehysviivaa, 22 alleviivausta, 11 kehyssuorakulmiota ja kolmen keskipisteen leikkausobjektit,
    viivan varren teksteistä 1 940 täytettyä tekstijuostetta ja 17 kehysviivaa sekä suorakulmioista 147 suorakulmion
    viivaa, 91 ruudukon viivaa ja 235 ruudukon tekstiä.
  - Kartoissa ovat kehysviiva viivatyyleillä `Beveled`, `Mitered` ja `Rounded` ja kapeammilla kynillä (frametext1),
    varjo myös viivan varren tekstissä (frametext3), kehyssuorakulmio (frametext2), alleviivaus, keskipiste
    muotoilemattomassa ja muotoillussa tekstissä (textpoint10), merkkiväli, tasattu, muotoiltu, kierretty ja tyhjä
    teksti, sarkaimet, viivan varren tekstin tasaukset vasen, keski, oikea ja tasattu sekä ruudukot, ruudukon tekstit
    alhaalta numeroituina, pyöristetyt kulmat ja kierretyt suorakulmiot.
  - Kartoissa ei ole fontin alleviivausefektiä, viivan varren tekstiä, jonka symbolimäärittelyllä on alleviivaus,
    keskipiste tai kehyssuorakulmio, suorakulmiota ilman viivaa eikä numeroimattomia soluja. Rakennetestit rakentavat
    ne.
  - Täysi ajo tehdään kansiosta `doc/devdocs/PaletteStack` ilman keräävää ajoa: `tools/push-wip.sh --base-baselines
    prs/000-palette-stack-base <klooni> feature/text-symbol-geometry`.
- **Haara:** `feature/text-symbol-geometry` juurtuu haaraan `feature/palette-stack` (903e6bde, tiketin 012 jälkeen).
  Täysi ajo tehdään haarassa `wip/linux/text-symbol-geometry` basen päällä valitsimella `--base-baselines
  prs/000-palette-stack-base`. Vanha haara `feature/text-geometry` jää vanhan vaiheen 1 tallenteeksi.
- **Aloitus kylmässä sessiossa:** lue järjestyksessä [../../Introduction.md](../../Introduction.md),
  [../README.md](../README.md), [../Progress.md](../Progress.md) ja sen kohdat "How to continue the work" ja
  "Decisions", speksin esipuhe ja luvut 01–03, [../Glossary.md](../Glossary.md), tiketit
  [023](023-palette-separations.md), [024](024-point-symbol-geometry.md), [011](011-line-geometry.md) ja
  [012](012-area-geometry.md) sekä tämä tiketti. [../tools/README.md](../tools/README.md) kertoo tiketin työkierron.

**Erotetut alatehtävät**
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
