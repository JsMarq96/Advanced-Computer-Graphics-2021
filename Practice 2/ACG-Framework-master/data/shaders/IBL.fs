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
    vec3 base_color;
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

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
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
    result.half_v = normalize(result.view + result.light);
    result.reflect = normalize(reflect(result.view, result.normal));
    result.n_dot_v = max(dot(result.normal, result.view), 0.0);
    result.n_dot_h = max(dot(result.normal, result.half_v), 0.0);
    result.l_dot_n = max(dot(result.normal, result.light), 0.0) ;

    return result;
}

sMaterial getMaterialProperties_v1() {
    sMaterial mat_prop;
    mat_prop.roughness = texture2D(u_rough_map, v_uv).r;
    mat_prop.metalness = texture2D(u_metal_map, v_uv).r;
    vec4 alb_color = texture2D(u_albedo_map, v_uv);
    mat_prop.base_color = alb_color.rgb;
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
    mat_prop.base_color = alb_color.rgb;
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
    //return ((mat_props.roughness + 2.0) / (2.0 * PI)) * pow(vects.n_dot_h, mat_props.roughness);
    float alpha = mat_props.roughness * mat_props.roughness;
    float alpha_squared = alpha * alpha;

    float denom = ((vects.n_dot_h * vects.n_dot_h) * (alpha_squared - 1.0)) + 1.0;
    return alpha_squared / (PI * denom * denom);
}

float Geometry_atenuation_term(sVectors vects, sMaterial mat_props) {
    // Using Epic's aproximation
    float r = mat_props.roughness + 1.0;
    float k = (r * r) / 8.0;
    float G1 = vects.n_dot_v / (vects.n_dot_v * (1.0 - k) + k);
    float G2 = vects.l_dot_n / (vects.l_dot_n * (1.0 - k) + k);
    return G1 * G2;
}

vec3 getPixelColor(sVectors vects, sMaterial mat_props) {
    // PBR
    // BRDF Diffuse: Lambertian
    vec3 diffuse_contribution = mat_props.diffuse_color;

    // BRDF Specular: Cook-Torrance
    //vec3 fresnel = FresnelSchlickRoughness(vects.n_dot_v, vec3(1.0), mat_props.roughness);
    vec3 fresnel = fresnelSchlick(vects.n_dot_h, mat_props.specular_color);
    float normalization_factor = 4.0 * vects.n_dot_v * vects.l_dot_n + 0.0001;
    
    vec3 specular_contribution = (fresnel * Geometry_atenuation_term(vects, mat_props) * normal_Distribution_function(vects, mat_props)) / normalization_factor;

    vec3 BRDF = diffuse_contribution + specular_contribution;

    vec3 radiance = u_light_radiance.rgb;

    //return fresnel;
    //return vec3(1.0) * normal_Distribution_function(vects, mat_props); //Geometry_atenuation_term(vects, mat_props);
    return BRDF * vects.l_dot_n;
    //return diffuse_contribution * vects.l_dot_n;
    //return fresnel * Geometry_atenuation_term(vects, mat_props);
    //return specular_contribution;
    return BRDF * radiance * vects.l_dot_n;

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
        output_color = frag_material.base_color;
    } else if (u_output_mode == 2.0) {
        output_color = vec3(frag_material.roughness, frag_material.roughness, frag_material.roughness);
    } else if (u_output_mode == 3.0) {
        output_color = vec3(frag_material.metalness, frag_material.metalness, frag_material.metalness);
    }
    gl_FragColor = vec4(output_color, 1.0);
}