shader_type spatial;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_nearest_mipmap;

uniform float distortion_size : hint_range(0.0, 1.0, 0.01) = 0.4;
uniform sampler2D glass;

void fragment()
{
	vec2 d = texture(glass, UV).xy - vec2(0.5);
	ALBEDO = texture(SCREEN_TEXTURE, SCREEN_UV + d * distortion_size).rgb;
}