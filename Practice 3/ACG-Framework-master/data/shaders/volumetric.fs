varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

uniform float step_vector;
uniform vec3 u_camera_position;
uniform vec4 u_color;
uniform sampler3D u_texture;
#define MAX_ITERATIONS 100

void main(){
    // Is the direction correct??
    vec3 ray_dir = normalize(u_camera_position - v_world_position);
    vec4 final_color = vec4(0.0);

    vec3 it_position = vec3(0.0);

	int steps = 100;
	float ray_step = 0.06;

	// Ray loop
	for(int i = 0; i < steps; i++){
		// v_position from (-1, -1, -1) to (1, 1, 1)
		vec3 sample_position = ((v_position - it_position) / 2.0) + 0.5;

		if (sample_position.x < 0.0 && sample_position.y < 0.0 && sample_position.z < 0.0) {
			break;
		}
		if (sample_position.x > 1.0 && sample_position.y > 1.0 && sample_position.z > 1.0) {
			break;
		}

		if (final_color.a == 1.0) {
			break;
		}
		
        float depth = texture(u_texture, sample_position).x;

		vec4 sample_color = vec4(depth, depth, depth, depth);
		//vec4 sample_color = vec4(depth, 1.0 - depth, 0.0, depth * depth);

		final_color = final_color + (ray_step * (1.0 - final_color.a) * sample_color);

        it_position = it_position + (ray_dir * ray_step);
	}
	
	gl_FragColor = final_color;
}