varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec3 v_light_local_position;
varying vec3 v_local_camera_pos;

uniform float u_material_ambient;
uniform float u_material_diffuse;
uniform float u_material_specular;
uniform float u_material_shininess;

uniform float u_light_ambient;
uniform float u_light_diffuse;
uniform float u_light_specular;
uniform vec4 u_light_color;
uniform vec3 u_light_position;

uniform vec4 u_color;
uniform vec3 u_camera_pos;

void main()
{
	vec3 light_vector = normalize(u_light_position - v_world_position);

    // Ambient component
    float ambient_component = u_material_ambient * u_light_ambient;

    // Diffuse component
    float light_dot_norm = max(dot(v_normal, light_vector), 0.0);
    float diffuse_componenet = light_dot_norm * u_material_diffuse * u_light_diffuse;

    // Specular compomenet
    vec3 r = normalize(reflect(light_vector, v_normal));
    vec3 v = normalize(u_camera_pos - v_world_position);
    float reflect_dot_view = max(dot(r, v), 0.0);

    float specular_component = pow(reflect_dot_view, u_material_shininess) * u_material_specular * u_light_specular;

    // Diffuse and ambient lights depend on the base color of the object
    // Specular deppedns on the light color
    gl_FragColor = vec4( (u_color.xyz * (ambient_component + diffuse_componenet)) + (u_light_color.xyz * specular_component), 1.0);
}