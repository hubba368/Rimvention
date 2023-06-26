using System;
using UnityEngine;
using Verse;


namespace Rimvention
{
    [StaticConstructorOnStartup]
    public static class RimventionTextures
    {
        public static readonly Texture2D TestIcon = ContentFinder<Texture2D>.Get("UI/Icons/testicon");
        public static readonly Texture2D TestIcon2 = ContentFinder<Texture2D>.Get("UI/Icons/testicon2");
    }
}
