varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;

// Light & Scene uniforms
uniform vec4 u_light_radiance;
uniform vec3 u_light_position;
uniform vec3 u_camera_position;

// Material maps
uniform sampler2D u_albedo_map;
uniform sampler2D u_metal_map;
uniform sampler2D u_rough_map;
uniform sampler2D u_opacity_map;
uniform sampler2D u_normal_map;
uniform sampler2D u_height_map;

uniform sampler2D u_brdf_LUT;

// Material multipliyers
uniform float u_metalness_mult;
uniform float u_roughness_mult;

// HDRE textures
uniform samplerCube u_texture_enviorment; 
uniform samplerCube u_texture_prem_0; 
uniform samplerCube u_texture_prem_1; 
uniform samplerCube u_texture_prem_2; 
uniform samplerCube u_texture_prem_3; 
uniform samplerCube u_texture_prem_4; 

uniform float u_tonemap_mode;
uniform float u_output_mode;
uniform float u_material_mode;

uniform float u_opacity_enable;

// POM uniforms
uniform float u_POM_enable;
uniform float u_POM_resolution;
uniform float u_POM_depth;

#define PI 3.14159265359
#define RECIPROCAL_PI 0.3183098861837697

const float GAMMA = 2.2;
const float INV_GAMMA = 1.0 / GAMMA;


// ===================================================
// DATA STRUCTS
// ===================================================

struct sVectors {
    vec3 normal;
    vec3 view;
    vec3 light;
    vec3 reflect;
    vec3 half_v;
    vec3 tangent_view;
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

    vec3 base_color;
    vec3 normal;

    float n_dot_v;
    float n_dot_h;
    float l_dot_n;
};

// ===================================================
// PROVIDED FUNCTIONS 
// ===================================================

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

mat3 cotangent_frame(vec3 N, vec3 p, vec2 uv){
	// get edge vectors of the pixel triangle
	vec3 dp1 = dFdx( p );
	vec3 dp2 = dFdy( p );
	vec2 duv1 = dFdx( uv );
	vec2 duv2 = dFdy( uv );

	// solve the linear system
	vec3 dp2perp = cross( dp2, N );
	vec3 dp1perp = cross( N, dp1 );
	vec3 T = dp2perp * duv1.x + dp1perp * duv2.x;
	vec3 B = dp2perp * duv1.y + dp1perp * duv2.y;

	// construct a scale-invariant frame
	float invmax = inversesqrt( max( dot(T,T), dot(B,B) ) );
	return mat3( T * invmax, B * invmax, N );
}

vec3 perturbNormal( vec3 N, vec3 V, vec2 texcoord, vec3 normal_pixel ) {
	#ifdef USE_POINTS
	return N;
	#endif

	// assume N, the interpolated vertex normal and
	// V, the view vector (vertex to eye)
	//vec3 normal_pixel = texture2D(normalmap, texcoord ).xyz;
	normal_pixel = normal_pixel * 255./127. - 128./127.;
	mat3 TBN = cotangent_frame(N, V, texcoord);
	return normalize(TBN * normal_pixel);
}

/* 
	Convert 0-Inf range to 0-1 range so we can
	display info on screen
*/
vec3 toneMap(vec3 color)
{
    return color / (color + vec3(1.0));
}

// Uncharted 2 tone map
// see: http://filmicworlds.com/blog/filmic-tonemapping-operators/
vec3 toneMapUncharted2Impl(vec3 color)
{
    const float A = 0.15;
    const float B = 0.50;
    const float C = 0.10;
    const float D = 0.20;
    const float E = 0.02;
    const float F = 0.30;
    return ((color*(A*color+C*B)+D*E)/(color*(A*color+B)+D*F))-E/F;
}

vec3 toneMapUncharted(vec3 color)
{
    const float W = 11.2;
    color = toneMapUncharted2Impl(color * 2.0);
    vec3 whiteScale = 1.0 / toneMapUncharted2Impl(vec3(W));
    return color * whiteScale;
}

// ===================================================
// CUSTOM FUNCTIONS 
// ===================================================

sVectors computeVectors() {
    sVectors result;
    result.normal = normalize(v_normal);
    result.view = normalize(u_camera_position - v_world_position);
    result.light = normalize(u_light_position - v_world_position);
    result.half_v = normalize(result.view + result.light);
    result.reflect = normalize(reflect(-result.view, result.normal));

    // Clamped dots
    result.n_dot_v = clamp(dot(result.normal, result.view), 0.001, 1.0);
    result.n_dot_h = clamp(dot(result.normal, result.half_v), 0.001, 1.0);
    result.l_dot_n = clamp(dot(result.normal, result.light), 0.001, 1.0);

    // From world to tangent space
    mat3 inv_TBN = transpose(cotangent_frame(result.normal, result.view, v_uv));
    result.tangent_view = inv_TBN * result.view;

    return result;
}

// ===================================================
// MATERIAL PROPERTIES FUNCTIONS
// ===================================================

/**
    Each one of these funcions loads the data based on the texture format:
    We currently support 3 formats:
        -V1: each data is stored on the corresponding texture file
        -V2: each data is in a different texture, with the exception of metalness that is the the G channel of the roughness map
        -V3: is stored on the Minecraft OldPBR format
*/
sMaterial getMaterialProperties_v1(sVectors vects, vec2 uv) {
    // Each property is a separed texture
    sMaterial mat_prop;
    // Fetch the materials and added the multpilers
    mat_prop.roughness = texture2D(u_rough_map, uv).r * u_roughness_mult;
    mat_prop.metalness = texture2D(u_metal_map, uv).r * u_metalness_mult;

    mat_prop.normal = normalize(perturbNormal(vects.normal, vects.view, uv, texture2D(u_normal_map, uv).rgb));

    // Get the new dot products
    mat_prop.n_dot_v = clamp(dot(mat_prop.normal, vects.view), 0.001, 1.0);
    mat_prop.n_dot_h = clamp(dot(mat_prop.normal, vects.half_v), 0.001, 1.0);
    mat_prop.l_dot_n = clamp(dot(mat_prop.normal, vects.light), 0.001, 1.0);

    if (u_opacity_enable == 1.0) {
        mat_prop.alpha = texture2D(u_opacity_map, uv).r;
    } else {
        mat_prop.alpha = 1.0;
    }

    vec4 alb_color = texture2D(u_albedo_map, uv);

    // Gamma to linear
    mat_prop.base_color = gamma_to_linear(alb_color.rgb);
    mat_prop.diffuse_color = mat_prop.base_color;

    // Clamping the values for avoid stinky division/multiplication by 0
    mat_prop.roughness = clamp(mat_prop.roughness, 0.01, 1.0);
    mat_prop.metalness = clamp(mat_prop.metalness, 0.01, 1.0);

    mat_prop.specular_color = mix(vec3(0.04), mat_prop.diffuse_color, mat_prop.metalness);
    mat_prop.diffuse_color = mix(mat_prop.diffuse_color, vec3(0.0), mat_prop.metalness);

    return mat_prop;
}

sMaterial getMaterialProperties_v2(sVectors vects, vec2 uv) {
    // Metalness and roughness are defined on the same texture file
    sMaterial mat_prop;
    // Fetch the materials and added the multpilers
    mat_prop.roughness = texture2D(u_rough_map, uv).g * u_roughness_mult;
    mat_prop.metalness = texture2D(u_rough_map, uv).b * u_metalness_mult;

    mat_prop.normal = normalize(perturbNormal(vects.normal, vects.view, uv, texture2D(u_normal_map, uv).rgb));

    // Get the new dot products
    mat_prop.n_dot_v = clamp(dot(mat_prop.normal, vects.view), 0.001, 1.0);
    mat_prop.n_dot_h = clamp(dot(mat_prop.normal, vects.half_v), 0.001, 1.0);
    mat_prop.l_dot_n = clamp(dot(mat_prop.normal, vects.light), 0.001, 1.0);

    if (u_opacity_enable == 1.0) {
        mat_prop.alpha = texture2D(u_opacity_map, uv).r;
    } else {
        mat_prop.alpha = 1.0;
    }

    vec4 alb_color = texture2D(u_albedo_map, uv);

    // Gamma to linear
    mat_prop.base_color = gamma_to_linear(alb_color.rgb);
    mat_prop.diffuse_color = mat_prop.base_color;

    // Clamping the values for avoid stinky division/multiplication by 0
    mat_prop.roughness = clamp(mat_prop.roughness, 0.01, 1.0);
    mat_prop.metalness = clamp(mat_prop.metalness, 0.01, 1.0);

    mat_prop.specular_color = mix(vec3(0.04), mat_prop.diffuse_color, mat_prop.metalness);
    mat_prop.diffuse_color = mix(mat_prop.diffuse_color, vec3(0.0), mat_prop.metalness);

    return mat_prop;
}

sMaterial getMaterialProperties_v3(sVectors vects, vec2 uv) {
    // Minecraft OldPBR format
    sMaterial mat_prop;
    // Convert the smoothness to roughness
    mat_prop.roughness = (1.0 - texture2D(u_rough_map, uv).r);
    mat_prop.roughness = mat_prop.roughness * mat_prop.roughness * u_roughness_mult;
    mat_prop.metalness = texture2D(u_rough_map, uv).g * u_metalness_mult;
    // mat_prop.emisiveness = texture2D(u_rough_map, v_uv).b;

    mat_prop.normal = normalize(perturbNormal(vects.normal, vects.view, uv, texture2D(u_normal_map, uv).rgb));

    // Get the new dot products
    mat_prop.n_dot_v = clamp(dot(mat_prop.normal, vects.view), 0.001, 1.0);
    mat_prop.n_dot_h = clamp(dot(mat_prop.normal, vects.half_v), 0.001, 1.0);
    mat_prop.l_dot_n = clamp(dot(mat_prop.normal, vects.light), 0.001, 1.0);

    vec4 alb_color = texture2D(u_albedo_map, uv);

    // Gamma to linear
    mat_prop.base_color = gamma_to_linear(alb_color.rgb);
    mat_prop.diffuse_color = mat_prop.base_color;
    mat_prop.alpha = alb_color.a;

    // Clamping the values for avoid stinky division/multiplication by 0
    mat_prop.roughness = clamp(mat_prop.roughness, 0.01, 1.0);
    mat_prop.metalness = clamp(mat_prop.metalness, 0.01, 1.0);

    mat_prop.specular_color = mix(vec3(0.04), mat_prop.diffuse_color, mat_prop.metalness);
    mat_prop.diffuse_color = mix(mat_prop.diffuse_color, vec3(0.0), mat_prop.metalness);

    return mat_prop;
}

// ===================================================
// PBR FUNCTIONS
// ===================================================

float normal_Distribution_function(sVectors vects, sMaterial mat_props) {
    //return ((mat_props.roughness + 2.0) / (2.0 * PI)) * pow(vects.n_dot_h, mat_props.roughness);
    float alpha = mat_props.roughness * mat_props.roughness;
    float alpha_squared = alpha * alpha;

    float denom = ((mat_props.n_dot_h * mat_props.n_dot_h) * (alpha_squared - 1.0)) + 1.0;
    return alpha_squared / (PI * denom * denom);
}

float Geometry_atenuation_term(sVectors vects, sMaterial mat_props) {
    // Using Epic's aproximation
    float r = mat_props.roughness + 1.0;
    float k = (r * r) / 8.0;
    float G1 = mat_props.n_dot_v / (mat_props.n_dot_v * (1.0 - k) + k);
    float G2 = mat_props.l_dot_n / (mat_props.l_dot_n * (1.0 - k) + k);
    return G1 * G2;
}

vec3 getPixelColor(sVectors vects, sMaterial mat_props) {
    // PBR =======
    // BRDF Diffuse: Lambertian
    vec3 diffuse_contribution = mat_props.diffuse_color;

    // BRDF Specular: Cook-Torrance
    vec3 fresnel = FresnelSchlickRoughness(mat_props.n_dot_h, mat_props.specular_color, mat_props.roughness);
    float normalization_factor = (4.0 * mat_props.n_dot_v * mat_props.l_dot_n) + 0.0001;
    vec3 specular_contribution = (fresnel * Geometry_atenuation_term(vects, mat_props) * normal_Distribution_function(vects, mat_props)) / normalization_factor;

    vec3 BRDF = diffuse_contribution + specular_contribution;

    vec3 radiance = u_light_radiance.rgb;
    vec3 direct_light_result = BRDF * radiance * mat_props.l_dot_n;

    // IBL =======
    vec2 uv_LUT = vec2(mat_props.n_dot_v, mat_props.roughness);

    // Specular IBL
    vec2 LUT_brdf = texture2D(u_brdf_LUT, uv_LUT).rg;
    vec3 fresnel_IBL = FresnelSchlickRoughness(mat_props.n_dot_v, mat_props.specular_color, mat_props.roughness);
    vec3 specular_sample = getReflectionColor(vects.reflect, mat_props.roughness);
    vec3 specular_IBL = ((fresnel_IBL * LUT_brdf.x) + LUT_brdf.y) * specular_sample;
    
    // Setting a high roughness value so there are softer reflections on the diffuse IBL
    vec3 diffuse_IBL = mat_props.diffuse_color * getReflectionColor(mat_props.normal, 1.0);

    // Energy conservation substraction
    diffuse_IBL = diffuse_IBL * (1.0 - fresnel_IBL);

    vec3 IBL_light_result = specular_IBL + diffuse_IBL;

    // Result =======
    return direct_light_result + IBL_light_result;
}

// ===================================================
// POM FUNCTIONS
// ===================================================

float get_height(vec2 uv_coords) {
    if (u_material_mode == 2.0) {
        return 1.0 - (texture2D(u_normal_map, uv_coords).a * 2.0 - 1.0);
    }
    // TODO: Alternative heightmap location
    return 1.0 - (texture2D(u_normal_map, uv_coords).a * 2.0 - 1.0);
}

/**
* Iterate through the heighmap with the direction of the tangential view vector
* NOTE: There is some artifacts on some extrems parts that a simple smoothing could not solve
*       But increasing the resolution of the POM effect makes it a bit better
*/
vec2 get_POM_coords(vec2 base_coords, vec3 view_vector) {
    float map_depth = get_height(base_coords);
    float layer_depth = 0.0;
    float prev_layer_depth = 0.0;
    // Step depth size
    float layer_step = 1.0 / u_POM_resolution;
    // Starting point
    vec2 it_coords = base_coords;
    vec2 prev_coords = vec2(0);
    // Direction for the layer look up
    vec2 step_vector = ((-view_vector.xy) * u_POM_depth) / u_POM_resolution;

    // Early stop
    if (map_depth == 0.0) {
        return it_coords;
    }

    // Traverse the layers until you find that you went too low
    for(; layer_depth < 1.0 && map_depth > layer_depth; layer_depth += layer_step) {
        prev_coords = it_coords;
        it_coords -= step_vector;
        map_depth = get_height(it_coords);
    }

    // Smooth between the current and previus layer's depths based on the actual depth
    return mix(it_coords, prev_coords, (layer_depth - map_depth) / layer_step );
}

// ===================================================
// MAIN
// ===================================================

void main() {
    sVectors frag_vectors = computeVectors();
    sMaterial frag_material;
    vec2 frag_coords;

    if (u_POM_enable == 1.0) {
        frag_coords = get_POM_coords(v_uv, frag_vectors.tangent_view);
    } else {
        frag_coords = v_uv;
    }

    // Load the material properties based on the source
    if (u_material_mode == 0.0) {
        frag_material = getMaterialProperties_v1(frag_vectors, frag_coords);
    } else if (u_material_mode == 1.0) {
        frag_material = getMaterialProperties_v2(frag_vectors, frag_coords);
    } else if (u_material_mode == 2.0) {
        frag_material = getMaterialProperties_v3(frag_vectors, frag_coords);
    }

    // Opacity
    if (frag_material.alpha < 0.1) {
        discard;
    }

    vec3 color = getPixelColor(frag_vectors, frag_material);

    // Tonemapping modes
    if (u_tonemap_mode == 1.0) {
        color = toneMap(color);
    } else if (u_tonemap_mode == 2.0) {
        color = toneMapUncharted(color);
    }

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
    } else if (u_output_mode == 4.0) {
        output_color = vec3(frag_material.normal);
    }
    gl_FragColor = vec4(output_color, frag_material.alpha);
}