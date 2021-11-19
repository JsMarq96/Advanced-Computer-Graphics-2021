attribute vec3 a_vertex;

uniform mat4 u_model;
uniform mat4 u_viewprojection;
uniform vec3 u_camera_position;

//this will store the color for the pixel shader
varying vec3 v_local_cam_pos;

void main()
{
	v_local_cam_pos = (inverse(u_model) * vec4( u_camera_position, 1.0) ).xyz;

	gl_Position = vec4(a_vertex, 1.0 );
}