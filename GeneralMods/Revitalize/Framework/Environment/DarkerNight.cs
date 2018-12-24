﻿using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revitalize.Framework.Illuminate;
using System.IO;
using Microsoft.Xna.Framework;

namespace Revitalize.Framework.Environment
{
    /// <summary>
    /// Deals with making night time darker in Stardew.
    /// </summary>
    public class DarkerNight
    {
        
        /// <summary>
        /// Darkness intensity.
        /// </summary>
        public static float IncrediblyDark = 0.9f;
        /// <summary>
        /// Darkness intensity.
        /// </summary>
        public static float VeryDark = 0.75f;
        /// <summary>
        /// Darkness intensity.
        /// </summary>
        public static float SomewhatDark = 0.50f;

        /// <summary>
        /// The config file.
        /// </summary>
        public static DarkerNightConfig Config;

        /// <summary>
        /// The calculated night color.
        /// </summary>
        private static Color CalculatedColor;

        /// <summary>
        /// Initializes the config for DarkerNight.
        /// </summary>
        public static void InitializeConfig()
        {
            if (File.Exists(Path.Combine(Revitalize.ModCore.ModHelper.DirectoryPath, "Configs", "DarkerNightConfig.json")))
            {
                Config = Revitalize.ModCore.ModHelper.Data.ReadJsonFile<DarkerNightConfig>(Path.Combine("Configs", "DarkerNightConfig.json"));
            }
            else
            {
                Config = new DarkerNightConfig();
                Revitalize.ModCore.ModHelper.Data.WriteJsonFile<DarkerNightConfig>(Path.Combine("Configs", "DarkerNightConfig.json"),Config);
            }
        }

        /// <summary>
        /// Sets the color of darkness at night.
        /// </summary>
        public static void SetDarkerColor()
        {
            if (Config.Enabled == false) return;
            if (Game1.player.currentLocation.IsOutdoors && Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
            {
                Game1.outdoorLight = CalculatedColor;
            }
        }

        /// <summary>
        /// Calculates how dark it should be a night.
        /// </summary>
        public static void CalculateDarkerNightColor()
        {
            if (Config.Enabled == false) return;
            if (Game1.player == null) return;

            //Calculate original lighting.
            if (Game1.timeOfDay >= Game1.getTrulyDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.75 + ((double)((int)((double)(Game1.timeOfDay - Game1.timeOfDay % 100) + (double)(Game1.timeOfDay % 100 / 10) * 16.6599998474121) - Game1.getTrulyDarkTime()) + (double)Game1.gameTimeInterval / 7000.0 * 16.6000003814697) * 0.000624999986030161));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }
            else if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.300000011920929 + ((double)((int)((double)(Game1.timeOfDay - Game1.timeOfDay % 100) + (double)(Game1.timeOfDay % 100 / 10) * 16.6599998474121) - Game1.getStartingToGetDarkTime()) + (double)Game1.gameTimeInterval / 7000.0 * 16.6000003814697) * 0.00224999990314245));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }

            Revitalize.ModCore.log("OUT: " + Game1.outdoorLight);

            int red = Game1.outdoorLight.R;

            if (Game1.player.currentLocation.IsOutdoors && Game1.timeOfDay>= Game1.getStartingToGetDarkTime())
            {
                //Game1.ambientLight = Game1.ambientLight.GreyScaleAverage();
                CalculatedColor = Game1.ambientLight* ( (red+30) / 255f) * Config.DarknessIntensity;

                Revitalize.ModCore.log("OUT: " + CalculatedColor);   
                Revitalize.ModCore.log("Ambient"+Game1.ambientLight);
            }
        }


    }
}
