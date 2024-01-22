extends Control

@onready var root = get_parent()
@onready var player = get_parent().get_parent().get_parent()


var gun = null

func _ready() -> void:
	gun = root.get_node("Hand").get_child(player.WeaponIndex)
	$HealthBar.max_value = player.Max_hp
	$HealthBar.value = player.Health

func _process(_delta: float) -> void:
	$WeaponLabel.text = gun.name
	$HealthBar.value = player.Health
	$HealthBar/HealthLabel.text = "%s%%" % [ ($HealthBar.value) ] #str($HealthBar.value) + "%"

	$SpeedLabel.text = str(Main.fround(Main.vertLen(player.velocity), 2))


func _on_character_body_3d_gun_switch(weaponIDX:int) -> void:
	gun = root.get_node("Hand").get_child(weaponIDX)
