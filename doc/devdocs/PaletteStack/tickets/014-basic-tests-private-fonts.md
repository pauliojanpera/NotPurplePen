# BasicTestsin fonttitestit poistavat renderöintitestien yksityiset fontit

**Konteksti**  
`BasicTests`in fontinvalintatestit (`FontMatch_*`) tyhjentävät yksityiset fontit (`SkiaFontManager.ClearPrivateFonts`) testin alussa ja lopussa. `RenderingTests` rekisteröi Roboto-tiedostonsa yksityisinä fontteina staattisessa konstruktorissaan kerran. Siksi Roboto-kartat piirtyvät Robotolla vain, kun konstruktori ajetaan `BasicTests`in jälkeen.

**Toistamisohjeet**  
1. Aja `RenderingTests`in staattinen konstruktori ennen `BasicTests`iä, esimerkiksi testitapausten lähteellä, jonka NUnit kutsuu testejä tehdessään. Tiketin [008](008-symbol-geometry.md) haaran `feature/symbol-geometry` commit c38217a6 tekee näin: `SymbolGeometryTests`in lähde on staattinen metodi, ja luokan staattinen konstruktori ajaa `RenderingTests`in konstruktorin.
2. Aja Map_Skia.Tests koneella, jolle Robotoa ei ole asennettu, esimerkiksi CI:n Windows-koneella.

**Odotettu vs. Todellinen käytös**  
`odotettu:` `RenderingTests.Roboto` piirtää tekstit Robotolla ja vastaa baselinea `RobotoTest.png`.  
`todellinen:` tekstit piirtyvät Arialilla, joka on `SkiaFontManager`in oletusfontti, ja 10 655 pikseliä eroaa baselinesta (Tests-ajo 44318693192). Paikallisessa ajossa väliaikainen kirjaus näytti, että `RenderingTests`in konstruktori rekisteröi 8 fonttia testejä tehtäessä, `BasicTests` tyhjensi ne, ja `RenderingTests.Roboto` alkoi ilman yksityisiä fontteja. Paikallisesti tekstit piirtyivät silti Robotolla, koska tälle koneelle Roboto on asennettu.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes / koneet, joille Robotoa ei ole asennettu`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
`RenderingTests.AddFontFiles` rekisteröi Roboto-tiedostot, ja `RenderingTests`in staattinen konstruktori kutsuu sitä. `BasicTests`in purku (`[TearDown]`) kutsuu sitä jokaisen testin jälkeen, joten renderöintitestien fontit ovat yksityisissä fonteissa testien järjestyksestä riippumatta. `SkiaFontManager.AddFontFile` jättää jo rekisteröidyn fontin ennalleen. Korjaus on haarassa `fix/basic-tests-private-fonts` (5638588f), ja sen täysi ajo basen päällä meni läpi (Tests 44325347234, Build 44325347956). Kokeiluajossa commitista dbe7de2a, jossa on base, korjaus ja tiketin 008 commit c38217a6, `RenderingTests.Roboto` meni läpi (Tests 44325323415, ryhmä Skia). Sen haara `experiment/basic-tests-private-fonts` on poistettu. Vika löytyi tiketin 008 täydestä ajosta. Tiketin 008 testit antavat `RenderingTests`in lisätä fontit kiinnittimen alussa (`[OneTimeSetUp]`) eivätkä tarvitse korjausta, joten haaraa ei ole yhdistetty haaraan `feature/palette-layers`.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
