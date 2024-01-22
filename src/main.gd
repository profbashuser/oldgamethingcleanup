extends Node


func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("fullscreen"):
		if (DisplayServer.window_get_mode() == DisplayServer.WINDOW_MODE_FULLSCREEN):
			DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_WINDOWED)
		else:
			DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN)
		
	if Input.is_action_just_pressed("restart"):
		get_tree().reload_current_scene()

func vertLen(vect: Vector3) -> float:
	var f:Vector3 = Vector3(vect.x, 0, vect.z);return f.length();

func fround(x, n = 0) -> float:
	n = pow(10, n)
	x = x * n
	if x >= 0: x = floor(x + 0.5)
	else: x = ceil(x - 0.5)
	return x/n

func toperc(val, mx):
	return floor((val / mx) * 100)
