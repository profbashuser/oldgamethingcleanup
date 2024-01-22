extends Node3D

var mouse_mov: Vector2

@export var sway_threshhold:float = 2
@export var sway_lerp:float = 5

@export_group("Sway directions")
@export var sway_left : Vector3
@export var sway_right : Vector3
@export var sway_normal : Vector3

@export_group("Bob variables")
@export var bob_freq := 2.0
@export var bob_amp := 0.01

@onready var player:CharacterBody3D = get_parent().get_parent().get_parent()

var t_bob: float = 0
var b_pos : Vector3
var s_pos : Vector3

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		mouse_mov = event.relative

func _process(delta: float) -> void:
	#if mouse_mov:
	mouse_mov = lerp(mouse_mov, Vector2.ZERO, 10*delta)
	b_pos.x = lerp(b_pos.x, mouse_mov.x * 1, 10*delta)
	b_pos.y = lerp(b_pos.y, mouse_mov.y * 1, 10*delta)

	t_bob += delta * player._Velocity.length() * float(player.is_on_floor())
	b_pos = _bob(t_bob, bob_freq, bob_amp)

	position = s_pos + b_pos

func _bob(time:float, freq:float, amp:float) -> Vector3:
	var pos = Vector3.ZERO

	pos.y = sin(time * freq) * amp
	pos.x = cos(time * freq/2) * amp

	return pos

