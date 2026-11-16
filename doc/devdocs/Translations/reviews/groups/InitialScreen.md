# Key group InitialScreen

Nov 16, 2026. The window that Purple Pen opens at start: its title, the four choices (create, open, open the last
event, open the sample), the note that Purple Pen is free and lives on donations, and the donation link.
claude-opus-5-5 at effort max made every line below, and each entry says so in its comment.

- **Translated**: 18 lines, in 5 languages: nl and ru lacked most of the window, nn-NO, ro and sl the note and the link.
- **Not needed**: none.
- **Corrected**: 18 lines.
  - da called the event "hændelse", which is an incident, and wrote the verb Open as the adjective "Åben". The four
    choices now say "løb" and "Åbn", as the terms Event and Open.
  - nn-NO used Bokmål: "Åpne", "eksisterende", "Velkommen". Now "Opne", "eksisterande", "Velkomen".
  - fi held the English of the note and of the donation link.
  - Typos and split compounds: nb-NO "arrangenment", "Utvilking", "eksempel arrangement"; et "näidis võistlus"; ro
    "Deshide", "Bunvenit"; hu "Üdözöljük"; pt "Benvindo".
- **Left empty**: none.

Doubts:

- The choices are one line each in a window 335 px wide. ru "Open last viewed event" is "Открыть последнее
  соревнование" (open the last event), as uk does, because "последнее просмотренное соревнование" would not fit.
  Measured in the application's font, bg's existing "Отвори последно преглежданото състезание" is about 25 px too
  long; it stays, as it is not wrong.
- uk calls the event "захід" in this window, and terms.csv gives "змагання". "Захід" is a fair word for an event, so
  it stays.

#### da

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_openLastRadioButton_Text` | Open last viewed event | Åbn sidst sete løb<br>(corrects: Åben sidst sete hændelse:) | "hændelse" is an incident, an orienteering event is a "løb" (the term Event); "Åben" is the adjective, the verb is "Åbn" (the term Open); the colon of a label does not belong to a choice |  |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Åbn et eksempelløb<br>(corrects: Åben en eksempel hændelse) | "hændelse" is an incident, an orienteering event is a "løb" (the term Event); "Åben" is the adjective, the verb is "Åbn"; the compound is one word |  |
| `InitialScreen_createNewRadioButton_Text` | Create a new event... | Opret et nyt løb...<br>(corrects: Opret en ny hændelse...) | "hændelse" is an incident, an orienteering event is a "løb" (the terms Event and New Event) |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Åbn et eksisterende løb...<br>(corrects: Åben en eksisterende hændelse...) | "hændelse" is an incident, an orienteering event is a "løb" (the term Event); "Åben" is the adjective, the verb is "Åbn" |  |

#### et

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Ava näidisvõistlus<br>(corrects: Ava näidis võistlus) | the compound is one word in Estonian ("näidis võistlus") |  |

#### fi

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen on vapaa ohjelma, jota saa kopioida ja jakaa. Purple Penin kehitys on ainoastaan lahjoitustenne varassa.<br>(corrects: Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations.) | the satellite held the English; the first sentence as the language's About dialog (AboutForm_freeLabel_Text), "lahjoitus" as the term Make a donation | AboutForm_freeLabel_Text |
| `InitialScreen_donationLink_Text` | Make a donation | Lahjoita<br>(corrects: Make a donation) | the satellite held the English; the term Make a donation |  |

#### hu

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_Text` | Welcome to Purple Pen | Üdvözöljük a Purple Pen-ben<br>(corrects: Üdözöljük a Purple Pen-ben) | the typo "Üdözöljük" |  |

#### nb-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Åpne eksempelarrangement<br>(corrects: Åpne eksempel arrangement) | the compound is one word in Norwegian ("eksempel arrangement") |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Åpne et eksisterende arrangement...<br>(corrects: Åpne et eksisterende arrangenment) | the typo "arrangenment"; the ellipsis of the English, as the choice opens a file dialog |  |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen er fri programvare og kan kopieres og deles. Utvikling er kun støttet av frivillige donasjoner.<br>(corrects: Purple Pen er fri programvare og kan kopieres og deles. Utvilking er kun støttet av frivillige donasjoner.) | the typo "Utvilking"; nothing else changes |  |

#### nl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_openLastRadioButton_Text` | Open last viewed event | Open laatst bekeken evenement | third choice of the window that opens at start (InitialScreenWindow, 335 px wide, one line); the terms Open and Event | InitialScreen_createNewRadioButton_Text |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Open een voorbeeldevenement | fourth choice of the start window; the terms Open, Sample and Event |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Open een bestaand evenement... | second choice of the start window; the terms Open and Event, with the ellipsis of a choice that opens a file dialog |  |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen is gratis software en mag worden gekopieerd en gedeeld. De ontwikkeling van Purple Pen wordt uitsluitend gesteund door uw donaties. | the note above the donation link of the start window; its first sentence as the language's About dialog (AboutForm_freeLabel_Text) | AboutForm_freeLabel_Text |
| `InitialScreen_donationLink_Text` | Make a donation | Doneren | the link under the note of the start window, which opens the donation page; the term Make a donation |  |
| `InitialScreen_Text` | Welcome to Purple Pen | Welkom bij Purple Pen | title of the start window |  |

#### nn-NO

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen er fri programvare og kan kopierast og delast. Utviklinga av Purple Pen blir berre støtta av bidraga dykkar. | the note above the donation link of the start window; Nynorsk forms (kopierast, delast, utviklinga, berre), "bidrag" as the term Make a donation |  |
| `InitialScreen_donationLink_Text` | Make a donation | Gje eit bidrag | the link under the note of the start window, which opens the donation page; the term Make a donation |  |
| `InitialScreen_openLastRadioButton_Text` | Open last viewed event | Opne sist opna arrangement<br>(corrects: Åpne sist opna arrangement) | "Åpne" is Bokmål; the Nynorsk verb is "opne" (the term Open), as the text's own "opna" |  |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Opne eksempelarrangementet<br>(corrects: Åpne eksempelarrangementet) | "Åpne" is Bokmål; the Nynorsk verb is "opne" (the term Open) |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Opne eit eksisterande arrangement...<br>(corrects: Opne eit eksisterende arrangenment) | the typo "arrangenment", and the Bokmål participle "eksisterende" (Nynorsk "eksisterande"); the ellipsis of the English, as the choice opens a file dialog |  |
| `InitialScreen_Text` | Welcome to Purple Pen | Velkomen til Purple Pen<br>(corrects: Velkommen til Purple Pen) | "Velkommen" is Bokmål; Nynorsk writes "velkomen" |  |

#### pt

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_Text` | Welcome to Purple Pen | Bem-vindo ao Purple Pen<br>(corrects: Benvindo ao Purple Pen) | "Benvindo" is a misspelling of "Bem-vindo", as pt-BR |  |

#### ro

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen este software gratuit și poate fi copiat și distribuit. Dezvoltarea Purple Pen este susținută numai prin donațiile dumneavoastră. | the note above the donation link of the start window; its first sentence as the language's About dialog (AboutForm_freeLabel_Text) | AboutForm_freeLabel_Text |
| `InitialScreen_donationLink_Text` | Make a donation | Fă o donație | the link under the note of the start window, which opens the donation page; the term Make a donation |  |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Deschide un eveniment de exemplu<br>(corrects: Deshide un eveniment de exemplu) | the typo "Deshide", against "Deschide" of the choice above |  |
| `InitialScreen_Text` | Welcome to Purple Pen | Bun venit în Purple Pen<br>(corrects: Bunvenit în Purple Pen) | the greeting is written in two words, "Bun venit" |  |

#### ru

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_openLastRadioButton_Text` | Open last viewed event | Открыть последнее соревнование | third choice of the start window, one line of about 275 px; "последнее просмотренное соревнование" would not fit, and uk also says the last event; the terms Open and Event | uk |
| `InitialScreen_openSampleRadioButton_Text` | Open a sample event | Открыть образец соревнования | fourth choice of the start window; the terms Open, Sample and Event | uk |
| `InitialScreen_createNewRadioButton_Text` | Create a new event... | Создать новое соревнование... | first choice of the start window; the term Create Event, with the ellipsis of a choice that opens the wizard |  |
| `InitialScreen_openExistingRadioButton_Text` | Open an existing event... | Открыть существующее соревнование... | second choice of the start window; the terms Open and Event; about 267 px of the 275 that the line gives | uk |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen — бесплатная программа, её можно копировать и распространять. Разработка Purple Pen поддерживается только вашими пожертвованиями. | the note above the donation link of the start window; "пожертвование" as the term Make a donation | uk |
| `InitialScreen_donationLink_Text` | Make a donation | Сделать пожертвование | the link under the note of the start window, which opens the donation page; the term Make a donation |  |
| `InitialScreen_Text` | Welcome to Purple Pen | Добро пожаловать в Purple Pen | title of the start window |  |

#### sl

Made by claude-opus-5-5, effort max.

| Key | English | Translation | Context that decided it | Mirrors |
| --- | --- | --- | --- | --- |
| `InitialScreen_label1_Text` | Purple Pen is free software and may be copied and shared. Development of Purple Pen is supported only by your donations. | Purple Pen je brezplačen program, ki se lahko kopira in posreduje. Razvoj programa Purple Pen podpirajo le vaše donacije. | the note above the donation link of the start window; its first sentence as the language's About dialog (AboutForm_freeLabel_Text) | AboutForm_freeLabel_Text InitialScreen_donationLink_Text |
