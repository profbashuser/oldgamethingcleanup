extends Node3D

var damage = 20
var spread = 10

@onready var ray_container = $Container
@onready var head = get_parent().get_parent()


func _ready() -> void:
	randomize()
	for r in ray_container.get_children():
		r.target_position.x = randf_range(spread, -spread)
		r.target_position.y = randf_range(spread, -spread)

func Shoot(RayCast:RayCast3D):
	if Input.is_action_pressed("shoot"):
		if !$AnimationPlayer.is_playing():
			for r in ray_container.get_children():
				r.position = RayCast.position
				r.target_position.x = randf_range(spread, -spread)
				r.target_position.y = randf_range(spread, -spread)

				if get_can_shoot(r, "Enemy"):
					r.get_collider().health -= damage
			$AnimationPlayer.play("ATK")
			# head.rotate_x(deg_to_rad(10))

func get_can_shoot(Ray:RayCast3D, group: String) -> bool:
	var v:bool = false
	if Ray.is_colliding():
		if Ray.get_collider() != null:
			if Ray.get_collider().is_in_group(group):
				v=true
	return v
