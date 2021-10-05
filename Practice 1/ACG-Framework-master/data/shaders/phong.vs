attribute vec3 a_vertex;
attribute vec3 a_normal;
attribute vec2 a_uv;
attribute vec4 a_color;

uniform vec3 u_camera_pos;

uniform mat4 u_model;
uniform mat4 u_viewprojection;
uniform vec3 u_light_position;

//this will store the color for the pixel shader
varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec3 v_light_local_position;
varying vec3 v_local_camera_pos;

void main()
{	
	//calcule the normal in camera space (the NormalMatrix is like ViewMatrix but without traslation)
	v_normal = (u_model * vec4( a_normal, 0.0) ).xyz;
	
	//calcule the vertex in object space
	v_position = a_vertex;
	v_world_position = (u_model * vec4( v_position, 1.0) ).xyz;
	
	mat4 inverse_model = inverse(u_model);

    // Get the local position of the light via inversing the model transform
    v_light_local_position = (inverse_model * vec4(u_light_position, 1.0)).xyz;

	v_local_camera_pos = (inverse_model * vec4(u_camera_pos, 1.0)).xyz;

	//calcule the position of the vertex using the matrices
	gl_Position = u_viewprojection * vec4( v_world_position, 1.0 );
}