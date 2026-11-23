## General description
The symbol key in `Map`. Each symbol of a map has a symbol key (`Symbol.Key`), which is its identity. The maker of
the symbol gives it, or the map gives it from its counter to a symbol that has none. The counter does not give a
value again. A symbol that replaces another gets the key of the replaced symbol, and the undo and the redo restore
the keys. The drawing does not use the keys, and it does not change.

## Changes in detail
* `SymbolKey.cs`: `SymbolKey`, a value that does not change, with a text form (`Text`). Two keys are the same when
  their text forms are the same, character by character. `SymbolKey.FromCounter` gives the key of a value of the
  counter: the value in base 62, with the digits 0–9, A–Z and a–z in this order. `SymbolKey.ToBase62` gives the text
  form of a number, and the course object keys of ticket 026 use it too.
* `Symbol.Key`: the maker of a symbol can set it before the map gets the symbol, as the other data of a symbol
  (`CheckModifiable`). The map gives a key with the internal `Symbol.AssignKey`. A copy of a symbol (`CloneDetached`)
  is a new symbol, without a key.
* `Map` keeps the counter and its symbols and holes by their keys:
  * `AddSymbol` checks that the keys of the symbol and of the holes that the map gets with it are free, before it
    changes the map or records an undo action, and a used key is a `MapUsageException`. Then it gives each symbol and
    hole without a key the next key of the counter. The holes of an area get their keys after the area.
  * The counter starts at 1. It skips a value whose text form a symbol of the map has as its key, and it does not
    give a value again, also after a removal or a `Clear`.
  * `RemoveSymbol` frees the keys of the symbol and its holes. The symbols keep their keys, so the undo actions of
    the addition and the removal, which keep the symbols, restore them with the same keys.
* `Map.ReplaceSymbol(Symbol symbol, Symbol replacement)` removes a symbol of the map, gives its key to the replacing
  symbol and adds the replacing symbol, in one write. It is a usage error when the replaced symbol is not in the map,
  when the replacing symbol is in a map, or when the replacing symbol has another key. The holes of a replacing area
  get keys from the counter, as the holes of the replaced area leave the map with it. Upstream has no edit that
  replaces a symbol.
* `SymbolKeyTests` (Test_MapModel, 15 tests) checks the keys of the counter and their text form, the keys that the
  maker gives, the uniqueness, the replacement, the undo and the redo, the copies and the holes. It reads
  `LordHill.ocd`, `LordHill OCAD11.ocd` and `LordHill.omap`, with 4,548 symbols and 80 holes each, and
  `HG_Mar28.omap`, with 1,563 symbols and 28 holes, and checks that their symbols and holes have the keys of the
  counter in the order of reading.

## Issue ticket number
[025-symbol-keys](../tickets/025-symbol-keys.md)

## Missing functionality
* The symbol keys of the course map, from the course object keys, are ticket 026.
* The encodings write the keys into SVG and PDF files. The epic does not open such a file as a map.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
