varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

uniform vec4 u_color;
uniform vec3 u_camera_pos;
uniform samplerCube u_texture;

void main()
{
	vec3 camera_vec = normalize(u_camera_pos - v_world_position);
    // Tint the base texture
    gl_FragColor = u_color * (textureCube(u_texture, reflect(camera_vec, v_normal)));
}