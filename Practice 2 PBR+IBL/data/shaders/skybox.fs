varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;

uniform vec4 u_color;
uniform samplerCube u_texture;

uniform float u_tonemap_mode;

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

void main()
{
	// Tint the base texture
    vec3 color = (u_color * textureCube(u_texture, v_position)).rgb;

    if (u_tonemap_mode == 1.0) {
        color = toneMap(color);
    } else if (u_tonemap_mode == 2.0) {
        color = toneMapUncharted(color);
    }

    gl_FragColor =  vec4(color, 1.0);
}