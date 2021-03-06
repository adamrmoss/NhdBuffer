﻿using System;
using System.Linq;

namespace ScreenFlasher
{
  public static class MoreMath
  {
    public static int TrueMod(int x, int n)
    {
      return x - (int) ((double) x / n) * n;
    }

    public static int Max(params int[] ints)
    {
      return ints.Max();
    }

    public static double Max(params double[] doubles)
    {
      return doubles.Max();
    }

    public static int Clamp(int value, int min, int max)
    {
      return Math.Min(max, Math.Max(min, value));
    }

    public static double Clamp(double value, double min, double max)
    {
      return Math.Min(max, Math.Max(min, value));
    }
  }
}
