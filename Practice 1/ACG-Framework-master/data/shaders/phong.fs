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

uniform vec4 u_color;
uniform vec3 u_camera_pos;

void main()
{
	vec3 light_vector = normalize(v_position - v_light_local_position);

    // Ambient component
    float ambient_component = u_material_ambient * u_light_ambient;

    // Diffuse component
    float light_dot_norm = dot(light_vector, v_normal);
    float diffuse_componenet = light_dot_norm * u_material_diffuse * u_light_diffuse;

    // Specular compomenet
    float reflect_dot_view = dot(v_world_position - u_camera_pos , -normalize(reflect(light_vector, v_normal)));
    reflect_dot_view = clamp(reflect_dot_view, 0.0, 255.0);

    float specular_component = pow(reflect_dot_view, u_material_shininess) * u_material_specular * u_light_specular;

    // Diffuse and ambient lights depend on the base color of the object
    // Specular deppedns on the light color
    gl_FragColor = vec4( (u_color.xyz * (ambient_component + diffuse_componenet)) + (u_light_color.xyz * specular_component), 1.0);
}
