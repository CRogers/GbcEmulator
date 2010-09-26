
Assembler
=========

To use the assembler, simply use the exact same instructions from http://imrannazar.com/Gameboy-Z80-Opcode-Map,
subject to these constraints:

- Instructions are one per line
- Whitespace can be put inbetween instructions, registers, constants and commas
- Everything after after the _instruction + a space (or other whitespace character)_ is a comment
- There must not be whitespace between registers/constants and brackets: `( HL )` -NO, `(HL)` -YES
- Mnemonics are case-insensitive
- Similar to NASM syntax

Sections:
---------

* Sections that can be used are: `metadata`, `data`, `bss`, `rst00`, `rst08`, `rst10`, `rst18`, `rst20`, `rst28`, `rst30`, `rst38`, `text`

* Sections are used like:
<pre>.section text
    ; code or data here
.end</pre>

Metadata:
---------

* Describes the info-header (bytes $104 - $14F in the ROM) of the ROM.

* Example:

<pre>.section metadata
	name = Rom Test
	cartType = 1B
	color = true
	licenseeCode = BEEF
	superGb = false
	romSize = 1
	ramSize = 3
	japanese = false
.end</pre>

* The values for `cartType` (Catridge Type) are:
<pre>Cartridge type:
    0 - ROM ONLY                12 - ROM+MBC3+RAM
    1 - ROM+MBC1                13 - ROM+MBC3+RAM+BATT
    2 - ROM+MBC1+RAM            19 - ROM+MBC5
    3 - ROM+MBC1+RAM+BATT       1A - ROM+MBC5+RAM
    5 - ROM+MBC2                1B - ROM+MBC5+RAM+BATT
    6 - ROM+MBC2+BATTERY        1C - ROM+MBC5+RUMBLE
    8 - ROM+RAM                 1D - ROM+MBC5+RUMBLE+SRAM
    9 - ROM+RAM+BATTERY         1E - ROM+MBC5+RUMBLE+SRAM+BATT
    B - ROM+MMM01               1F - Pocket Camera
    C - ROM+MMM01+SRAM          FD - Bandai TAMA5
    D - ROM+MMM01+SRAM+BATT     FE - Hudson HuC-3
    F - ROM+MBC3+TIMER+BATT     FF - Hudson HuC-1
    10 - ROM+MBC3+TIMER+RAM+BATT
    11 - ROM+MBC3</pre>

* The values for `romSize` are:

<pre>ROM size:
    0 - 256Kbit =  32KByte =   2 banks
    1 - 512Kbit =  64KByte =   4 banks
    2 -   1Mbit = 128KByte =   8 banks
    3 -   2Mbit = 256KByte =  16 banks
    4 -   4Mbit = 512KByte =  32 banks
    5 -   8Mbit =   1MByte =  64 banks
    6 -  16Mbit =   2MByte = 128 banks
  $52 -   9Mbit = 1.1MByte =  72 banks
  $53 -  10Mbit = 1.2MByte =  80 banks
  $54 -  12Mbit = 1.5MByte =  96 banks</pre>

* The values for `ramSize` are:

<pre>RAM size:
    0 - None
    1 -  16kBit =  2kB = 1 bank
    2 -  64kBit =  8kB = 1 bank
    3 - 256kBit = 32kB = 4 banks
    4 -   1MBit =128kB =16 banks</pre>

Data:
-----

* Defines `data` that is stored in the ROM and all metions to it in code will be changed to a pointer of its ROM location

* Example `data` section:

<pre>.section data
    helloWorld	db	"Hello, World", 1 times 5, "quote\"test\"" times 2, 2
    meow		dw	FFFFh,ABCDh, "lol", 0
    end			eq	500

.end</pre>

* `db` will save as a series of `byte`s, `bw` as a series of `word`s

* `eq` will define a constant that will be inserted into any code at compile time (like a C `#DEFINE` directive)

* You can use `string` and decimal or hex numbers (with a `h` on the end)

* The `times x` directive will repeat the last piece of data x times (in dec or hex)

Bss:
----

* Adds "uninitialised" data, storing a pointer for the data offset if all data reserved in this section was laid out sequentially

* Example `bss` section:

<pre>.section bss
	array		resb	200
	wordArray	resw	50

.end</pre>

* `resb` reserves an array of bytes, `resw` reserves an array of words

RstXX:
------

* Stores up to 8 bytes of code starting from address `$XX`

* Code can be called using the `rst XX` opcode.

* Example `rst08` section:

<pre>.section rst08

	ld A, L
	xor C
	ret

.end</pre>

Text:
-----

* The main bulk of the code. Use opcodes as described in the link at the top of the page.

* Labels can be used like `label:	ld	A, L` and **must always start on the same line as the opcode** 
	* Labels can be defined in any code section and can be used to jump into any code section

* Comments are placed after a semicolon at the end of the code, e.g: `ld A, 6   ; this is a comment`

---

* Complicated test program:

<pre>; for(int i = 0; i < 512; i++)
;     eram[i] = (byte)(i + 6);

.section metadata
	name = Rom Test
	cartType = 1B
	color = true
	licenseeCode = BEEF
	superGb = false
	romSize = 1
	ramSize = 3
	japanese = false
.end


.section data
	helloWorld	db	"Hello, World", 1 times 5, "quote\"test\"" times 2, 2
	meow		dw	FFFFh,ABCDh, "lol", 0
	end			eq	500

.end

.section bss
	array		resb	200
	wordArray	resw	50

.end

.section rst00

	ld A, H
	xor B	
	ret NZ

.end

.section rst08

	ld A, L
	xor C
	ret

.end

.section rst20

	ld A, L
	add A, 6
	ret

.end

.section rst28

	rst28:	ld (HL), A
			inc HL
			jp rst28_lol
	rst28_lol: ret

.end


.section text

	ld	BC, A200h

	ld	HL, A000h

	loop:	call doIt
			jp	NZ, loop

			ld A, 111
			stop

	doIt:	ld	A, L
			rst 20
			rst 28
			rst 0
			ret

	stop

.end</pre>