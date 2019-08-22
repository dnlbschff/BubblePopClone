using System;
using UnityEngine;

namespace BPC.Bubbles
{
    public static class NumberFormatter
    {
        public static string FormatBubbleValue(int value)
        {
            var exponent = Mathf.FloorToInt((Mathf.Log10(value)));
            if (exponent < 3)
            {
                return value.ToString("D");
            }

            value = Mathf.RoundToInt(value / Mathf.Pow(10, exponent));
            if (exponent < 6)
            {
                return $"{value:D}K"; 
            }

            if (exponent < 9)
            {
                return $"{value:D}M";
            }
            
            if (exponent < 12)
            {
                return $"{value:D}M";
            }

            throw new NotImplementedException();
        }
    }
}