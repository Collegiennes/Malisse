using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class GainBias
{
    public static float Bias(double time, double bias)
    {
        return (float)(time / ((((1.0 / bias) - 2.0) * (1.0 - time)) + 1.0));
    }

    public static float Gain(double time, double gain)
    {
        if (time < 0.5)
            return (float) (Bias(time * 2.0, gain) / 2.0);
        else
            return (float) (Bias(time * 2.0 - 1.0, 1.0 - gain) / 2.0 + 0.5);
    }
}
