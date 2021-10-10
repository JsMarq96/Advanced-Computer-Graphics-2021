varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;

uniform vec4 u_color;
uniform vec3 u_camera_position;
uniform samplerCube u_texture;

void main()
{
	vec3 normal = normalize(v_normal);
    vec3 camera_vec = normalize(v_world_position - u_camera_position);
    // Tint the base texture
    gl_FragColor = u_color * (textureCube(u_texture, reflect(camera_vec, normal)));
}