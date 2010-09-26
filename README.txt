GbcEmulator is an attempt to create a fully working GameBoy Color emulator, with a package that will read ROM metadata.

Current Status: PRE-PRE-ALPHA; NON-FUNCTIONAL; IN RAPID DEVELOPMENT

Current Jobs/Roadmap:

	* RomInfo
		- Virtually complete, reads metadata from ROM
		
	* Assembler
		- Supports all opcodes
		- Supports hexadecimal and labels
		- Outputs ROM files as far as the rest of the emulator supports them
		
	* Disassembler
		- Supports all opcodes that the Assembler supports
		- Outputs files in a pretty way
		- Only outputs flat files!

	* Opcodes
		- All opcodes added
		- Checking opcodes are correct
		- Correcting buggy opcodes indentified by // BUG: notifications once correct implementations are found
		- Flags mostly sorted out
			- Half-carry and DAA flags may be wrong
		- Sorting out timings for each operation
		
	* Memory Controller
		- Is almost completely implemented
			- Mmm01 is not supported (cannot find docs)
		- Does not support GPU VRAM or input or I/O
		- Needs another good checking against the spec(s)  
		
	* GPU
		- Research/Needs to be implemented
		
	* Sound
		- Research/Needs to be implemented
		
	* Video Output
		- XNA/WPF. WPF is preferable so there can be Silverlight version
		
	* Input
		- Research/Needs to be implemented
		
	* Timer
		- Research/Needs to be implemented