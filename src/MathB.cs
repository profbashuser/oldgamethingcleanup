using System;
using Godot;

class MathB
{
    public static float Presicelerp(float v0, float v1, float t)
    {
        return (1 - t) * v0 + t * v1;
    }

    public static float Naivelerp(float v0, float v1, float t)
    {
        return (1 - t) * v0 + t * v1;
    }

    public static float VertLen(Vector3 vect) {
        Vector3 f = new(vect.X, 0f, vect.Z);return f.Length();
    }
}