#version 140

// sun.fs

#ifdef GL_ES
precision mediump float;
#endif

uniform vec4 Time;
uniform vec3 cameraPos;

uniform sampler2D NoiseTexture;
uniform sampler2D DistortTexture;

in vec3 v_Normal;
in vec3 v_Position;
in vec3 v_Position_ObjectSpace;

uniform vec4 Color;

out vec4 out_FragColor;

void main() 
{
    float noiseTime = mod(Time.x * 0.0125, 1.0);
    float noiseVal = 0.0;

    float noiseTexScale = 9000.0;
    float distortTexScale = 9000.0;
    float distortStrength = 0.05;

    float noiseTimeScaled = noiseTime * 0.25;

    vec4 noiseA1_raw = texture(DistortTexture, v_Position_ObjectSpace.xy / distortTexScale + noiseTime);
    vec4 noiseA2_raw = texture(NoiseTexture, v_Position_ObjectSpace.yx / noiseTexScale + (noiseA1_raw.rg - 0.5) * distortStrength - noiseTimeScaled);
    vec4 noiseB1_raw = texture(DistortTexture, v_Position_ObjectSpace.xz / distortTexScale + noiseTime + 0.33);
    vec4 noiseB2_raw = texture(NoiseTexture, v_Position_ObjectSpace.zx / noiseTexScale + (noiseB1_raw.rg - 0.5) * distortStrength - noiseTimeScaled + 0.33);
    vec4 noiseC1_raw = texture(DistortTexture, v_Position_ObjectSpace.yz / distortTexScale + noiseTime + 0.67);
    vec4 noiseC2_raw = texture(NoiseTexture, v_Position_ObjectSpace.zy / noiseTexScale + (noiseC1_raw.rg - 0.5) * distortStrength - noiseTimeScaled + 0.67);

    vec3 noiseA_raw = noiseA2_raw.rgb;
    vec3 noiseB_raw = noiseB2_raw.rgb;
    vec3 noiseC_raw = noiseC2_raw.rgb;

    vec3 texBlend = vec3(1.0);
    vec3 absNormal =  abs(normalize(v_Position_ObjectSpace));
    // Z is major axis
    if (absNormal.z > absNormal.x && absNormal.z > absNormal.y)
    {
        vec2 st = vec2(absNormal.x, absNormal.y) / absNormal.z;
        texBlend = clamp(vec3(
            st.s - 0.5,
            st.t - 0.5,
            1.5 - max(st.s, st.t)
            ), 0.0, 1.0);
    }
    // Y is major axis
    else if (absNormal.y > absNormal.x) // && absNormal.y > absNormal.z
    {
        vec2 st = vec2(absNormal.x, absNormal.z) / absNormal.y;
        texBlend = clamp(vec3(
            st.s - 0.5,
            1.5 - max(st.s, st.t),
            st.t - 0.5
            ), 0.0, 1.0);
    }
    // X is major axis
    else // absNormal.x > absNormal.y && absNormal.x > absNormal.z
    {
        vec2 st = vec2(absNormal.z, absNormal.y) / absNormal.x;
        texBlend = clamp(vec3(
            1.5 - max(st.s, st.t),
            st.t - 0.5,
            st.s - 0.5
            ), 0.0, 1.0);
    }

    // "renormalize" component sum == 1.0
    float component_total = (texBlend.x + texBlend.y + texBlend.z);
    texBlend /= component_total;

    vec3 noise_raw = noiseA_raw.rgb * texBlend.z + noiseB_raw.rgb * texBlend.y + noiseC_raw.rgb * texBlend.x;

    noiseVal = (noise_raw.x * 0.5 + noise_raw.y * 0.75 + noise_raw.z * 0.5) / 1.75;

    noiseVal = clamp(noiseVal * 1.25 - 0.25, 0.0, 1.0);

    // out_FragColor = vec4(vec3(noiseVal), -1.0);
    // return;

    vec3 camera_dir = normalize( cameraPos - v_Position );
    float ndotv = clamp( dot(camera_dir, v_Normal), 0.0, 1.0 );

    vec3 color = mix(vec3(1.0,1.0,0.3) * (2.0 + pow(ndotv, 2.0) * 10.0), vec3(1.0,0.24,0.05) * (1.0 + sqrt(ndotv) * 3.0), pow(noiseVal, 0.5 + pow(ndotv, 2.0) * 20.0));

    noiseVal = pow(noiseVal, 0.075);
    color = mix(vec3(1.0,1.0,0.3) * (2.0 + pow(ndotv, 2.0) * 10.0), vec3(1.0,0.24,0.05) * (1.0 + sqrt(ndotv) * 3.0), noiseVal);
    color = 0;
    out_FragColor = vec4(color * 1.5, 0.001);
}
