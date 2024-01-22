extends CharacterBody3D

var timer := 0.0;

const MAX_HEALTH := 10;
var health := MAX_HEALTH;

var player = null;

const SPEED := 3.0;
const ATTACKRANGE := 2.5;

var run := false
var attack := false

@export var player_path : NodePath;

@onready var nav_agent := $NavigationAgent3D

func _ready() -> void:
	player = get_node(player_path)

var state = states.STATE_WALKING

enum states {
	STATE_ATTACKING,
	STATE_WALKING
}

func _process(delta: float) -> void:
	velocity = Vector3.ZERO

	match state:
		states.STATE_WALKING: # Navigation
			nav_agent.target_position = player.global_transform.origin
			var next_nav_point = nav_agent.get_next_path_position()
			velocity = (next_nav_point - global_transform.origin).normalized() * SPEED
			look_at(Vector3(global_position.x + velocity.x, global_position.y,
							global_position.z + velocity.z), Vector3.UP)
		states.STATE_ATTACKING:
			look_at(Vector3(player.global_position.x, global_position.y, player.global_position.z), Vector3.UP)
			timer += delta
			if timer > .8:
				timer = 0
				if _target_in_range(): _attack()

			if !timer > 0:
				state = states.STATE_WALKING

	# Attack state
	if (_target_in_range()):
		state = states.STATE_ATTACKING

	if health <= 0:
		queue_free()

	move_and_slide()

func _target_in_range() -> bool:
	return (global_position.distance_to(player.global_position) < ATTACKRANGE)

func _attack():
	player.Health -= 10;
