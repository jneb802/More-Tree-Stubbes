﻿using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using Jotunn.Configs;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace moreTreeStubbes
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class moreTreeStubbes : BaseUnityPlugin
    {
        public const string PluginGUID = "com.bepinex.moreTreeStubbes";
        public const string PluginName = "More Tree Stubbes";
        public const string PluginVersion = "0.0.1";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private AssetBundle embeddedResourceBundle;
        private GameObject oakStumpPrefab;

        private void Awake()
        {
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("More Tree Stubbes has Loaded");

            // Subscribe to the OnVanillaPrefabsAvailable event
            PrefabManager.OnVanillaPrefabsAvailable += OnPrefabsAvailable;

        }

        private void OnPrefabsAvailable()
        {

            // Load assets and add vegetation here
            LoadAssets();
            AddOakStumpVegetation();

            // Unsubscribe if you only want to execute this once
            PrefabManager.OnVanillaPrefabsAvailable -= OnPrefabsAvailable;
        }

        private void LoadAssets()
        {
            // string resourcePath = "oakStubbe.assets.birch_stubbe_bundle"; // Replace with your actual resource path
            embeddedResourceBundle = AssetUtils.LoadAssetBundleFromResources("oak_stubbe_bundle");
            oakStumpPrefab = embeddedResourceBundle.LoadAsset<GameObject>("oak_stubbe"); // Replace with your actual asset name

            // Print Embedded Resources
            Jotunn.Logger.LogInfo($"Embedded resources: {string.Join(", ", typeof(moreTreeStubbes).Assembly.GetManifestResourceNames())}");

            if (embeddedResourceBundle == null)
            {
                Jotunn.Logger.LogError("Failed to load the asset bundle.");
                return;
            }

            foreach (string resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Jotunn.Logger.LogInfo(resourceName);
            }
        }

        private void AddOakStumpVegetation()
        {
            // Ensure oakStumpPrefab is loaded
            if (oakStumpPrefab == null)
            {
                Jotunn.Logger.LogError("oakStumpPrefab is not loaded.");
                return;
            }

            // Configure the Destructible component
            var destructible = oakStumpPrefab.GetComponent<Destructible>() ?? oakStumpPrefab.AddComponent<Destructible>();
            destructible.m_minToolTier = 2;
            destructible.m_health = 120f;

            // Set up destroyed and hit effects
            GameObject destroyedEffectPrefab = PrefabManager.Cache.GetPrefab<GameObject>("vfx_stubbe");
            GameObject destroyedSoundPrefab = PrefabManager.Cache.GetPrefab<GameObject>("sfx_wood_break");
            GameObject hitEffectPrefab = PrefabManager.Cache.GetPrefab<GameObject>("vfx_SawDust");
            GameObject hitSoundPrefab = PrefabManager.Cache.GetPrefab<GameObject>("sfx_tree_hit");

            destructible.m_destroyedEffect.m_effectPrefabs = new EffectList.EffectData[]
            {
                new EffectList.EffectData { m_prefab = destroyedEffectPrefab },
                new EffectList.EffectData { m_prefab = destroyedSoundPrefab }
            };

            destructible.m_hitEffect.m_effectPrefabs = new EffectList.EffectData[]
            {
                new EffectList.EffectData { m_prefab = hitEffectPrefab },
                new EffectList.EffectData { m_prefab = hitSoundPrefab }
            };

            // Set up the DropOnDestroyed component
            var dropOnDestroyed = oakStumpPrefab.GetComponent<DropOnDestroyed>() ?? oakStumpPrefab.AddComponent<DropOnDestroyed>();
            dropOnDestroyed.m_dropWhenDestroyed.m_drops = new List<DropTable.DropData>
            {
                new DropTable.DropData
                {
                    m_item = PrefabManager.Instance.GetPrefab("Wood"),
                    m_stackMin = 4,
                    m_stackMax = 6,
                    m_weight = 0.75f
                },

                new DropTable.DropData
                {
                    m_item = PrefabManager.Instance.GetPrefab("FineWood"),
                    m_stackMin = 6,
                    m_stackMax = 10,
                    m_weight = 1f
                }

            };

            dropOnDestroyed.m_dropWhenDestroyed.m_dropMin = 2;

            // Define the vegetation configuration
            VegetationConfig oakStumpConfig = new VegetationConfig
            {
                Biome = Heightmap.Biome.Meadows | Heightmap.Biome.Swamp | Heightmap.Biome.BlackForest | Heightmap.Biome.Mistlands,
                BlockCheck = true,
                Min = 0,
                Max = 2,
                InForest = true,
                ForestThresholdMin = 0,
                ForestThresholdMax = 2,
                MaxTilt = 20,
                MaxTerrainDelta = 3,
                TerrainDeltaRadius = 6,
                GroupSizeMax = 1,
                GroundOffset = -0.4f,
                
            };

            // Create and add the custom vegetation
            CustomVegetation customVegetation = new CustomVegetation(oakStumpPrefab, false, oakStumpConfig);
            ZoneManager.Instance.AddCustomVegetation(customVegetation);
        }
    }
}