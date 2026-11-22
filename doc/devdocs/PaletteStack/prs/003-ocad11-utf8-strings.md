## General description
OCAD 11 and later encode their parameter strings and the Pascal strings of their symbols, such as font names, in
UTF-8. OCAD 9 and 10 encode them in the 8-bit code page of the machine. The import decoded all versions in the 8-bit
code page, and the export encoded all versions in it. So a name with a character outside ASCII, such as "Vihreä",
read as "VihreÃ¤" from an OCAD 11 or later file, and the OCAD 11 or later files that Purple Pen wrote held such names
in the 8-bit code page.

Of the map files that OCAD itself wrote and whose strings hold characters outside ASCII, all OCAD 9 files (182) and
OCAD 10 files (4) use the 8-bit code page, and all OCAD 11 files (2), OCAD 12 files (13) and OCAD 2018 files (153) use
UTF-8. OpenOrienteering Mapper reads the strings of OCAD 11 and later in UTF-8 too.

## Changes in detail
* `Util.GetOcadEncoding` gives the encoding of a version: UTF-8 from OCAD 11 on, and the 8-bit code page before.
* The import and `OcadDump` switch their reader to that encoding after they read the file header.
* The export writes the strings in that encoding, and reserves for a parameter string the length of its bytes, not of
  its characters.
* `ssl_issprom2019.ocd` is the ISSprOM 2019 symbol set of the Finnish Orienteering Federation, as OCAD 2018 saved it.
  It names its colors in Finnish.
* `OcadImportTests.Ocad2018StringsAreUtf8` reads its color names. `WriteStringsInEncodingOfVersion` writes it as OCAD 9,
  10, 11, 12 and 2018, finds a name in the encoding of the version in the file, and reads the names again. Both fail
  without the change.

## Issue ticket number
[003-ocad11-utf8-strings](../tickets/003-ocad11-utf8-strings.md)

## Missing functionality
An OCAD 11 or later file that an earlier Purple Pen wrote, with a name outside ASCII, now reads with that name wrong.
Purple Pen writes such files as course files, and does not read them back.

## Changes to be logged
OCAD 11 -tiedostojen ja uudempien ääkköset luetaan oikein, esimerkiksi värien nimissä ja taustakuvien poluissa.
Purple Penin luomiin OCAD 11- ja uudempiin tiedostoihin ne tallentuvat niin, että OCAD lukee ne oikein.
