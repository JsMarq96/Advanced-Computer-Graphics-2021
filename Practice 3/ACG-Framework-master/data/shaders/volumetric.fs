varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

uniform float u_brightness;
uniform float u_step_size;
uniform vec3 u_camera_position;
uniform vec4 u_color;
uniform sampler3D u_texture;

uniform vec3 u_plane_origin;
uniform vec3 u_plane_direction;

// Jittering
uniform sampler2D u_noise_tex;
uniform float u_noise_size;

// IsoSurfaces
uniform vec4 u_light_color;
uniform float u_iso_threshold;
uniform float u_gradient_delta;

varying vec3 v_local_cam_pos;
varying vec3 v_local_light_position;

// ======================================
// COMONS
// ======================================

bool if_outside_of_plane(vec3 position) {
	vec3 plane_origin = vec3(0.0, 0.5, 0.0);
	
	float plane = dot(position - u_plane_origin, u_plane_direction);

	return plane > 0.0;
}

// ======================================
// RENDER VAINILLA VOLUMES
// ======================================
vec4 render_volume() {
	vec3 ray_dir = normalize(v_local_cam_pos - v_position);
    vec4 final_color = vec4(0.0);

    vec3 it_position = vec3(0.0);

	int MAX_ITERATIONS = 5000;

	// Add noise to aboud jitter
	float noise_sample = normalize(texture(u_noise_tex, gl_FragCoord.xy / u_noise_size)).r;
	it_position = it_position + (noise_sample * ray_dir * u_step_size);

	// Ray loop
	for(int i = 0; i < MAX_ITERATIONS; i++){
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
		
        float depth = texture3D(u_texture, sample_position).x;

		vec4 sample_color = vec4(depth, depth, depth, depth);

		if (!if_outside_of_plane(sample_position)) {
			final_color = final_color + (u_step_size * (1.0 - final_color.a) * sample_color);
		} else {
			final_color = vec4(0.0);
		}

        it_position = it_position + (ray_dir * u_step_size);
	}

	return final_color;
}

// ======================================
// RENDER ISOSURFACES
// ======================================

vec3 gradient(float h, vec3 coords) {
	vec3 r = vec3(0.0);
	float grad_x = texture(u_texture, vec3(coords.x + h, coords.y, coords.z)).x - 
				   texture(u_texture, vec3(coords.x - h, coords.y, coords.z)).x;

	float grad_y = texture(u_texture, vec3(coords.x, coords.y + h, coords.z)).x - 
				   texture(u_texture, vec3(coords.x, coords.y - h, coords.z)).x;
	
	float grad_z = texture(u_texture, vec3(coords.x, coords.y, coords.z + h)).x - 
				   texture(u_texture, vec3(coords.x, coords.y, coords.z - h)).x;
	
	return normalize(vec3(grad_x, grad_y, grad_z)  /  (h * 2));
}

vec3 phong(vec3 position, vec3 normal) {
	vec3 l = normalize(v_local_light_position - position);
	vec3 r = normalize(reflect(-l, normal));
    vec3 v = normalize(position - u_camera_position);
	float reflect_dot_view = clamp(dot(r, v), 0.0, 1.0);

	vec3 specular = pow(reflect_dot_view, 64.0) * vec3(1.0);

	vec3 diff = vec3(0.5) * clamp( dot(l, normal), 0.0, 1.0);

	return specular + diff + vec3(0.07);
}

vec4 render_isosurface() {
	vec3 ray_dir = normalize(v_local_cam_pos - v_position);
    vec4 final_color = vec4(0.0);

    vec3 it_position = vec3(0.0);

	int MAX_ITERATIONS = 500;

	// Add noise to aboud jitter
	float noise_sample = normalize(texture(u_noise_tex, gl_FragCoord.xy / u_noise_size)).r;
	it_position = it_position + (noise_sample * ray_dir * u_step_size);

	// Ray loop
	for(int i = 0; i < MAX_ITERATIONS; i++){
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
		
        vec4 d_vec = texture(u_texture, sample_position);

		float depth = d_vec.x;

		if (!if_outside_of_plane(sample_position) && depth >= u_iso_threshold) {
			vec3 grad = gradient(u_gradient_delta, sample_position);
			return vec4(phong(sample_position, grad), 1.0);
		}

        it_position = it_position + (ray_dir * u_step_size);
	}

	return vec4(0.0);
}

void main(){
    vec4 final_color = render_isosurface();
	final_color.rgb = final_color.rgb * u_brightness;

	if (final_color.a < 0.01) {
		discard;
	}

	gl_FragColor = final_color;
}