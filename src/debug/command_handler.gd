extends Node

enum {
	ARG_INT,
	ARG_STRING,
	ARG_BOOL,
	ARG_FLOAT
}

const valid_commands = [
	["test",
		[ARG_STRING] ],
	["trace",
		[ARG_STRING] ],
]

func output_txt(string):
	print(str(string))
	get_parent().get_parent().output_text(string);

func testit(string):
	print(str(string))
	output_txt("wow")

func trace(string):
	output_txt(string)


