varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec4 v_color;
varying vec3 v_light_local_position;

uniform float u_ambient_value;
uniform float u_diffuse_value;
uniform float u_specular_value;
uniform vec4 u_light_color;
uniform vec4 u_color;

void main()
{
	vec3 light_vector = normalize(v_position - v_light_local_position);
    float light_dot_norm = dot(v_position - v_light_local_position, v_normal);
    // Diffuse component
    vec3 diffuse_componenet = vec3(1.0) * light_dot_norm * u_diffuse_value;

    // Specular compomenet
    // The specular power??
    //vec3 specular_component = reflect(light_eye, v_normal) * u_diffuse_value;
    gl_FragColor = vec4((v_color.xyz * diffuse_componenet), 1.0);

    //gl_FragColor = vec4((v_color * diffuse_componenet) + (u_light_color * specular_component), 1.0);
}
