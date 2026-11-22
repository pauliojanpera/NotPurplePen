# Matriisin yhtäsuuruus vertaa siirtymiä float-lukuina

**Konteksti**  
`Matrix` (`src/MapModel/Graphics2D/Matrix.cs`) säilyttää alkionsa double-lukuina, mutta `Equals` vertaa omia siirtymiään toisen matriisin `OffsetX`- ja `OffsetY`-ominaisuuksiin, jotka pyöristävät ne float-luvuiksi. Siksi matriisi ei ole yhtä kuin kopionsa, kun float ei esitä sen siirtymää tarkasti.

**Toistamisohjeet**  
1. Tee matriisi, joka kiertää pisteen ympäri: `Matrix m = new Matrix(); m.RotateAt(55, new PointF(4, 1));`.
2. Vertaa sitä kopioonsa: `m.Equals(m.Clone())`.

**Odotettu vs. Todellinen käytös**  
`odotettu:` `true`, koska kopiolla on samat alkiot.  
`todellinen:` `false`, koska kierron siirtymät ovat double-lukuja, joita float ei esitä.

**Versio / commit / järjestelmätiedot**  
`Purple Pen upstream d48f4456 / base, basewithfixes, dev / kaikki alustat`

(_valinnainen_) **Ehdotettu korjaus / hypoteesi**  
`Equals` vertaa siirtymiä double-lukuina, kuten se vertaa muita alkioita ja kuten `GetHashCode` laskee ne. Korjaus on haarassa `fix/matrix-equals-offsets` (b12084be), ja sen testi `MatrixTests.EqualsItsCopy` epäonnistuu ilman korjausta. `src/MapModel/Graphics2DStd/Matrix.cs` on sama luokka nimiavaruudessa `System.Drawing.Drawing2D`, ja sen `Equals` saa saman korjauksen (5e827f47). PPen.slnx ei käännä sitä, mutta MapModel.sln listaa sen. Upstreamin koodi ei kutsu `Matrix.Equals`ia, joten korjaus ei muuta piirtoa. Vika löytyi tiketin [005](005-layer-instances.md) testejä kirjoitettaessa. Ne vertaavat sijoitusten matriiseja alkioittain, joten ne eivät tarvitse korjausta.

(_valinnainen_) **Erotetut alatehtävät**  
<!-- Tästä tiketistä erotetut korjaukset ja ominaisuudet: tiketti, haara ja se, onko haara yhdistetty tämän tiketin haaraan -->
