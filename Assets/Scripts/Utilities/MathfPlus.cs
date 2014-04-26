using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MathfPlus 
{
	public static Vector3 GetClosestAngle(Vector3 oldRotation, Vector3 newRotation)
	{
		float x = GetClosestAngle(oldRotation.x, newRotation.x);
		float y = GetClosestAngle(oldRotation.y, newRotation.y);
		float z = GetClosestAngle(oldRotation.z, newRotation.z);
		
		return new Vector3(x, y, z);
	}
	
	public static float GetClosestAngle(float oldRotation, float newRotation)
	{
		float newValue = newRotation;
		if (oldRotation - newRotation > 180.0f)
		{
			newValue += 360.0f;
		}
		else if (oldRotation - newRotation < -180.0f)
		{
			newValue -= 360.0f;
		}
		
		return newValue;
	}
	
	public static float GetUniversalAngle(float angle)
	{
		// TODO: It can be better without a while.
		while (angle < 0.0f || angle >= 360.0f)
		{
			if (angle < 0.0f)
			{
				angle += 360.0f;
			}
			else if (angle >= 360.0f)
			{
				angle -= 360.0f;
			}
		}
		
		return angle;
	}
	
	public static float QuartEaseOut(float t, float b, float c, float d)
    {
        return -c * ( ( t = t / d - 1 ) * t * t * t - 1 ) + b;
    }

    public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
    {
        // Using formula from http://www.mvps.org/directx/articles/catmull/
        // Internally using doubles not to lose precission
        double amountSquared = amount * amount;
        double amountCubed = amountSquared * amount;
        return (float)(0.5 * (2.0 * value2 +
            (value3 - value1) * amount +
            (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
            (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
    }
    public static Vector3 CatmullRom(Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount)
    {
        return new Vector3(CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
            CatmullRom(value1.y, value2.y, value3.y, value4.y, amount),
            CatmullRom(value1.z, value2.z, value3.z, value4.z, amount));
    }

    public static Vector3 CatmullRom(IList<Vector3> values, float step)
    {
        float totalRatio = step;
        var zeroBasedCount = values.Count - 1;

        var i0 = (int)Mathf.Clamp((zeroBasedCount * totalRatio) - 1, 0, zeroBasedCount);
        var i1 = (int)Mathf.Clamp((zeroBasedCount * totalRatio), 0, zeroBasedCount);
        var i2 = (int)Mathf.Clamp((zeroBasedCount * totalRatio) + 1, 0, zeroBasedCount);
        var i3 = (int)Mathf.Clamp((zeroBasedCount * totalRatio) + 2, 0, zeroBasedCount);

        var tMinRatio = (double)i1 / zeroBasedCount;
        var tMaxRatio = (double)i2 / zeroBasedCount;
        var tBracketDuration = tMaxRatio - tMinRatio;
        var s = Mathf.Clamp01((float)((totalRatio - tMinRatio) / (tBracketDuration == 0 ? 1 : tBracketDuration)));

        return CatmullRom(values[i0], values[i1], values[i2], values[i3], s);
    }

    public static float BSpline(float p1, float p2, float p3, float p4, float s)
    {
        var a0 = (-p1 + 3 * p2 - 3 * p3 + p4) / 6.0;
        var a1 = (3 * p1 - 6 * p2 + 3 * p3) / 6.0;
        var a2 = (-3 * p1 + 3 * p3) / 6.0;
        var a3 = (p1 + 4 * p2 + p3) / 6.0;

        return (float) ((a2 + s * (a1 + s * a0)) * s + a3);
    }

    public static Vector3 BSpline(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float s)
    {
        return new Vector3(BSpline(p1.x, p2.x, p3.x, p4.x, s),
            BSpline(p1.y, p2.y, p3.y, p4.y, s),
            BSpline(p1.z, p2.z, p3.z, p4.z, s));
    }

    public static Vector3 BSpline(IList<Vector3> values, float step)
    {
        float totalRatio = step;
        var zeroBasedCount = values.Count - 1;

        var i0 = (int)Mathf.Clamp((zeroBasedCount * totalRatio) - 1, 0, zeroBasedCount);
        var i1 = (int)Mathf.Clamp((zeroBasedCount * totalRatio), 0, zeroBasedCount);
        var i2 = (int)Mathf.Clamp((zeroBasedCount * totalRatio) + 1, 0, zeroBasedCount);
        var i3 = (int)Mathf.Clamp((zeroBasedCount * totalRatio) + 2, 0, zeroBasedCount);

        var tMinRatio = (double)i1 / zeroBasedCount;
        var tMaxRatio = (double)i2 / zeroBasedCount;
        var tBracketDuration = tMaxRatio - tMinRatio;
        var s = Mathf.Clamp01((float)((totalRatio - tMinRatio) / (tBracketDuration == 0 ? 1 : tBracketDuration)));

        return BSpline(values[i0], values[i1], values[i2], values[i3], s);
    }

    public static Vector4 PadVector3(Vector3 v)
    {
        return new Vector4(v.x, v.y, v.z, 1.0f);
    }
}
