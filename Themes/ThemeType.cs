﻿using System;

namespace FramePFX.Themes {
    public enum ThemeType {
        SoftDark,
        RedBlackTheme,
        DeepDark,
        GreyTheme,
        DarkGreyTheme,
        LightTheme,
        PinkTheme
    }

    public static class ThemeTypeExtension {
        public static string GetName(this ThemeType type) {
            switch (type) {
                case ThemeType.SoftDark:        return "SoftDark";
                case ThemeType.RedBlackTheme:   return "RedBlackTheme";
                case ThemeType.DeepDark:        return "DeepDark";
                case ThemeType.GreyTheme:       return "GreyTheme";
                case ThemeType.DarkGreyTheme:   return "DarkGreyTheme";
                case ThemeType.LightTheme:   return "LightTheme";
                case ThemeType.PinkTheme:   return "PinkTheme";
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}