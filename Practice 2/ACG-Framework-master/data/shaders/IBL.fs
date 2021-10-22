varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;

uniform vec4 u_light_radiance;
uniform vec3 u_light_position;
uniform vec3 u_camera_position;

// Material maps
uniform sampler2D u_albedo_map;
uniform sampler2D u_metal_map;
uniform sampler2D u_rough_map;
uniform sampler2D u_brdf_LUT;

// HDRE textures
uniform samplerCube u_texture_enviorment; 
uniform samplerCube u_texture_prem_0; 
uniform samplerCube u_texture_prem_1; 
uniform samplerCube u_texture_prem_2; 
uniform samplerCube u_texture_prem_3; 
uniform samplerCube u_texture_prem_4; 

uniform float u_output_mode;
uniform float u_material_mode;
// TODO: add the other maps

#define PI 3.14159265359
#define RECIPROCAL_PI 0.3183098861837697

const float GAMMA = 2.2;
const float INV_GAMMA = 1.0 / GAMMA;

struct sVectors {
    vec3 normal;
    vec3 view;
    vec3 light;
    vec3 reflect;
    vec3 half_v;
    float n_dot_v;
    float n_dot_h;
    float l_dot_n;
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

	if(lod < 1.0) color = mix( textureCube(u_texture_enviorment, r), textureCube(u_texture_prem_0, r), lod );
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

// degamma
vec3 gamma_to_linear(vec3 color)
{
	return pow(color, vec3(GAMMA));
}

// gamma
vec3 linear_to_gamma(vec3 color)
{
	return pow(color, vec3(INV_GAMMA));
}

// CUSTOM FUNCTIONS ===============
sVectors computeVectors() {
    sVectors result;
    result.normal = normalize(v_normal);
    result.view = normalize(v_world_position - u_camera_position);
    result.light = normalize(u_light_position - v_world_position);
    result.reflect = normalize(reflect(result.view, result.normal));
    result.half_v = normalize(result.view + result.light);
    result.n_dot_v = max(dot(result.normal, result.view), 0.0) + 0.0001;
    result.n_dot_h = max(dot(result.normal, result.half_v), 0.0) + 0.0001;
    result.l_dot_n = max(dot(u_light_position, result.normal), 0.0) + 0.0001;

    return result;
}

sMaterial getMaterialProperties_v1() {
    sMaterial mat_prop;
    mat_prop.roughness = texture2D(u_rough_map, v_uv).r;
    mat_prop.metalness = texture2D(u_metal_map, v_uv).r;
    vec4 alb_color = texture2D(u_albedo_map, v_uv);
    mat_prop.diffuse_color = alb_color.rgb;
    mat_prop.alpha = alb_color.a;

    mat_prop.specular_color = mix(vec3(0.04), mat_prop.diffuse_color, mat_prop.metalness);
    mat_prop.diffuse_color = mix(mat_prop.diffuse_color, vec3(0.0), mat_prop.metalness);

    return mat_prop;
}

sMaterial getMaterialProperties_v2() {
    sMaterial mat_prop;
    mat_prop.roughness = texture2D(u_rough_map, v_uv).g;
    mat_prop.metalness = texture2D(u_rough_map, v_uv).b;
    vec4 alb_color = texture2D(u_albedo_map, v_uv);
    mat_prop.diffuse_color = alb_color.rgb;
    mat_prop.alpha = alb_color.a;

    mat_prop.specular_color = mix(vec3(0.04), mat_prop.diffuse_color, mat_prop.metalness);
    mat_prop.diffuse_color = mix(mat_prop.diffuse_color, vec3(0.0), mat_prop.metalness);

    return mat_prop;
}

sMaterial degamma(sMaterial mat) {
    mat.diffuse_color = gamma_to_linear(mat.diffuse_color);
    mat.specular_color = gamma_to_linear(mat.specular_color);

    return mat;
}

// PBR FUNCTIONS
float normal_Distribution_function(sVectors vects, sMaterial mat_props) {
    float roughness_squared = mat_props.roughness * mat_props.roughness;
    float denom = (vects.n_dot_h * vects.n_dot_h) * (roughness_squared - 1.0) + 1.0;
    float D = roughness_squared / (PI * denom * denom);
    return D;
}

float Geometry_atenuation_term(sVectors vects, sMaterial mat_props) {
    // Using Epic's aproximation
    float k = (mat_props.roughness + 1.0);
    k = k * k;
    k = k / 8.0;
    float G1 = vects.n_dot_v / (vects.n_dot_v *(1.0 - k) + k);
    float G2 = dot(vects.normal, vects.half_v) / (vects.l_dot_n * (1.0 - k) + k);
    return G1 * G2;
}

vec3 getPixelColor(sVectors vects, sMaterial mat_props) {
    // PBR
    // BRDF Diffuse: Lambertian
    vec3 diffuse_contribution = mat_props.diffuse_color / PI;

    // BRDF Specular: Cook-Torrance
    vec3 fresnel = FresnelSchlickRoughness(vects.l_dot_n, mat_props.specular_color, mat_props.roughness);
    float normalization_factor = 4.0 * vects.n_dot_v * vects.l_dot_n;
    
    vec3 specular_contribution = (fresnel * Geometry_atenuation_term(vects, mat_props) * normal_Distribution_function(vects, mat_props)) / normalization_factor;

    vec3 radiance = u_light_radiance.rgb;

    //return fresnel * Geometry_atenuation_term(vects, mat_props);
    return specular_contribution;
    return diffuse_contribution * specular_contribution * radiance * vects.l_dot_n;

    // IBL
    /*vec2 LUT_brdf = texture2D(u_brdf_LUT, vec2(vects.n_dot_v, mat_props.roughness)).rg;

    vec3 fresnel_IBL = FresnelSchlickRoughness(vects.n_dot_v, mat_props.specular_color, mat_props.roughness);

    vec3 specular_sample = getReflectionColor(vects.reflect, mat_props.roughness);

    vec3 specular_IBL = ((fresnel_IBL * LUT_brdf.x) + LUT_brdf.y) * specular_sample;
    
    vec3 diffuse_IBL = mat_props.diffuse_color * getReflectionColor(vects.normal, mat_props.roughness);
    //return diffuse_IBL;

    return specular_IBL + (diffuse_IBL);*/
}

void main() {
    sVectors frag_vectors = computeVectors();
    sMaterial frag_material;

    if (u_material_mode == 0.0) {
        frag_material = getMaterialProperties_v1();
    } else  {
        frag_material = getMaterialProperties_v2();
    }

    frag_material = degamma(frag_material);

    vec3 color = getPixelColor(frag_vectors, frag_material);

    color = linear_to_gamma(color);

    // Ouput other textures for debugging
    vec3 output_color;
    if (u_output_mode == 0.0) {
        output_color = color;
    } else if (u_output_mode == 1.0) {
        output_color = frag_material.diffuse_color;
    } else if (u_output_mode == 2.0) {
        output_color = vec3(frag_material.roughness, frag_material.roughness, frag_material.roughness);
    } else if (u_output_mode == 3.0) {
        output_color = vec3(frag_material.metalness, frag_material.metalness, frag_material.metalness);
    }
    gl_FragColor = vec4(output_color, 1.0);
}