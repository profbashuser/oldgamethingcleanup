extends MeshInstance3D


var damage = 2

@onready var head = get_parent().get_parent()

func Shoot(RayCast:RayCast3D):
	if Input.is_action_pressed("shoot"):
		if !$AnimationPlayer.is_playing():
			# head.rotate_x(deg_to_rad(5))
			if get_can_shoot(RayCast, "Enemy"):
				RayCast.get_collider().health -= damage
		$AnimationPlayer.play("ATK")
	else:
		$AnimationPlayer.stop()
	

func get_can_shoot(Ray:RayCast3D, group: String) -> bool:
	var v:bool = false
	if Ray.is_colliding():
		if Ray.get_collider() != null:
			if Ray.get_collider().is_in_group(group):
				v=true
	return v
