extends MeshInstance3D

const ATTACKRANGE = 3;
const damage := 10;

func Shoot(RayCast:RayCast3D):
	if (Input.is_action_pressed("shoot")):
		if !$AnimationPlayer.is_playing():
			if get_can_hit(RayCast, "Enemy"):
				RayCast.get_collider().health -= damage
		$AnimationPlayer.play("ATK")


func get_can_hit(Ray:RayCast3D, group: String) -> bool:
	var v:bool = false
	if Ray.is_colliding():
		if Ray.get_collider() != null:
			if Ray.get_collider().is_in_group(group):
				if _target_in_range(Ray.get_collider()):
					v=true
	return v

func _target_in_range(target) -> bool:
	return (global_position.distance_to(target.global_position) < ATTACKRANGE)
