varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;

uniform vec3 u_light_position;
uniform vec3 u_camera_position;

// Material maps
uniform sampler2D u_albedo_map;
uniform sampler2D u_metal_map;
uniform sampler2D u_rough_map;
uniform sampler2D u_brdf_LUT;

// HDRE textures
uniform sampler2D u_texture_prem_0; 
uniform sampler2D u_texture_prem_1; 
uniform sampler2D u_texture_prem_2; 
uniform sampler2D u_texture_prem_3; 
uniform sampler2D u_texture_prem_4; 

uniform int u_is_conductor_material;

uniform int output_mode;
// TODO: add the other maps

struct sVectors {
    vec3 normal;
    vec3 view;
    vec3 light;
    vec3 reflect;
    vec3 half;
    float n_dot_v;
};

struct sMaterial {
    float roughness;
    float metalness;
    vec3 diffuse_color;
    float alpha;
    vec3 specular_color;
};

// PROVIDED FUNCTIONS ===============
vec3 getReflectionColor(vec3 r, float roughness)
{
	float lod = roughness * 5.0;

	vec4 color;

	if(lod < 1.0) color = mix( textureCube(u_albedo_map, r), textureCube(u_texture_prem_0, r), lod );
	else if(lod < 2.0) color = mix( textureCube(u_texture_prem_0, r), textureCube(u_texture_prem_1, r), lod - 1.0 );
	else if(lod < 3.0) color = mix( textureCube(u_texture_prem_1, r), textureCube(u_texture_prem_2, r), lod - 2.0 );
	else if(lod < 4.0) color = mix( textureCube(u_texture_prem_2, r), textureCube(u_texture_prem_3, r), lod - 3.0 );
	else if(lod < 5.0) color = mix( textureCube(u_texture_prem_3, r), textureCube(u_texture_prem_4, r), lod - 4.0 );
	else color = textureCube(u_texture_prem_4, r);

	return color.rgb;
}

vec3 FresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

// CUSTOM FUNCTIONS ===============
sVectors computeVectors() {
    sVectors result;
    result.normal = normalize(v_normal); // TODO: change for normal map?
    result.view = normalize(v_world_position - u_camera_position);
    result.light = normalize(v_world_position - u_light_position);
    result.reflect = normalize(reflect(v_world_position, result.normal));
    result.half = normalize(result.view + result.light);
    result.n_dot_v = dot(result.normal, result.view);
}

sMaterial getMaterialProperties() {
    sMaterial mat_prop;
    mat_prop.roughness = texture2D(u_rough_map, v_uv).r;
    mat_prop.metalness = texture2D(u_metal_map, v_uv).r;
    vec4 alb_color = texture2D(u_albedo_map, v_uv);
    mat_prop.diffuse_color = alb_color.rgb;
    mat_prop.alpha = alb_color.a;

    if (u_is_conductor_material != 0) {
        mat_prop.specular_color = mat_prop.diffuse_color;
    } else {
        mat_prop.specular_color = vec3(0.04);
    }
}

vec3 getPixelColor(sVectors vects, sMaterial mat_props) {
    // IBL
    vec2 LUT_brdf = texture2D(u_brdf_LUT, vec2(vects.n_dot_v, mat_props.roughness));
    vec3 fresnel = FresnelSchlickRoughness(vects.n_dot_v, mat_props.specular_color, mat_props.roughness);
    vec3 specular_IBL = (fresnel * (LUT_brdf.r + LUT_brdf.g)) * getReflectionColor(vects.reflect, mat_props.roughness);

}

void main() {
    sVectors frag_vectors = computeVectors();
    sMaterial frag_material = computeVectors();

    vec3 color = getPixelColor(frag_vectors, frag_material);

    // Ouput other textures for debugging
    vec3 output_color;
    if (output_mode == 0) {
        output_color = color;
    } else if (output_mode == 1) {
        output_color = frag_material.diffuse_color;
    } else if (output_mode == 2) {
        output_color = vec3(frag_material.roughness);
    } else if (output_mode == 3) {
        output_color = vec3(frag_material.metalness);
    }
    gl_FragColor = vec4(output_color, 1.0);
}