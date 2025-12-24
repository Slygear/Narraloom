#version 330 core

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

uniform vec2 uResolution;

void main()
{
    vec2 zeroToOne = aPosition / uResolution;
    vec2 clip = zeroToOne * 2.0 - 1.0;

    gl_Position = vec4(clip * vec2(1, -1), 0.0, 1.0);
    vTexCoord = aTexCoord;
}
