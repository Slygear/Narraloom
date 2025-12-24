#version 330 core

layout (location = 0) in vec2 aPosition;

uniform vec2 uResolution;

void main()
{
    vec2 zeroToOne = aPosition / uResolution;
    vec2 zeroToTwo = zeroToOne * 2.0;
    vec2 clipSpace = zeroToTwo - 1.0;

    gl_Position = vec4(clipSpace * vec2(1, -1), 0.0, 1.0);
}
