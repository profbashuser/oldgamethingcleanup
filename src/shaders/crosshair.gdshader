shader_type canvas_item;

uniform bool isEnabled = true;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_nearest_mipmap;

void fragment() {
	if (isEnabled) {
		COLOR.rgb = 1.0 - textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0).rgb;
		COLOR.a = texture(TEXTURE , UV).a;
	}
}