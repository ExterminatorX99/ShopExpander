﻿using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ShopExpander
{
    public class ShopExpander : Mod
    {
        public static ShopExpander Instance { get; private set; }

        public ModItem ArrowLeft { get; private set; }
        public ModItem ArrowRight { get; private set; }

        public ShoppingList LastShopExpanded { get; set; }

        private HarmonyInstance harmonyInstance;

        private readonly Dictionary<object, int> provisionOverrides = new Dictionary<object, int>();
        private readonly HashSet<object> modifierOverrides = new HashSet<object>();
        private readonly HashSet<object> noDistinctOverrides = new HashSet<object>();

        public ShopExpander()
        {
            Properties = new ModProperties { Autoload = false };
        }

        public override void Load()
        {
            if (Instance != null)
                throw new InvalidOperationException("An instance of ShopExpander is already loaded.");

            harmonyInstance = HarmonyInstance.Create(Name);

            ArrowLeft = new ArrowItem();
            AddItem("ArrowLeft", ArrowLeft);

            ArrowRight = new ArrowItem();
            AddItem("ArrowRight", ArrowRight);

            Instance = this;
        }

        public override void PostSetupContent()
        {
            Patches.SetupShopPatch.Load();
            harmonyInstance.PatchAll();

            ArrowLeft.DisplayName.SetDefault("Previous page");
            ArrowRight.DisplayName.SetDefault("Next page");

            if (!Main.dedServ)
            {
                Main.itemTexture[ArrowLeft.item.type] = CropTexture(Main.textGlyphTexture[0], new Rectangle(4 * 28, 0, 28, 28));
                Main.itemTexture[ArrowRight.item.type] = CropTexture(Main.textGlyphTexture[0], new Rectangle(5 * 28, 0, 28, 28));
            }
        }

        public override void Unload()
        {
            Patches.SetupShopPatch.Unload();
            harmonyInstance.UnpatchAll();

            if (!Main.dedServ)
            {
                Main.itemTexture[ArrowLeft.item.type].Dispose();
                Main.itemTexture[ArrowRight.item.type].Dispose();
            }

            Instance = null;
        }

        public override object Call(params object[] args)
        {
            string command = args[0] as string;
            if (command == null)
                throw new ArgumentException("First argument must be string");

            switch (command)
            {
                case "SetProvisionSize":
                    if (!(args[2] is int))
                        throw new ArgumentException("Third argument must be int for SetProvisionSize");
                    SetProvisionSize(args[1], (int)args[2]);
                    break;

                case "SetModifier":
                    SetModifier(args[1]);
                    break;

                case "SetNoDistinct":
                    SetNoDistinct(args[1]);
                    break;

                case "GetLastShopExpanded":
                    if (LastShopExpanded != null)
                        return LastShopExpanded.ExtendedItems;
                    break;

                default:
                    throw new ArgumentException(string.Format("Unknown command: {0}", command));
            }
            return null;
        }

        public int GetProvisionSize(object obj)
        {
            int provSize;
            if (provisionOverrides.TryGetValue(obj, out provSize))
                return provSize;
            else
                return 40;
        }

        public void SetProvisionSize(object obj, int prov)
        {
            provisionOverrides[obj] = prov;
        }

        public bool IsModifier(object obj)
        {
            return modifierOverrides.Contains(obj);
        }

        public void SetModifier(object obj)
        {
            if (!modifierOverrides.Contains(obj))
                modifierOverrides.Add(obj);
        }

        public bool IsNoDistinct(object obj)
        {
            return noDistinctOverrides.Contains(obj);
        }

        public void SetNoDistinct(object obj)
        {
            if (!noDistinctOverrides.Contains(obj))
                noDistinctOverrides.Add(obj);
        }

        private Texture2D CropTexture(Texture2D texture, Rectangle newBounds)
        {
            Texture2D newTexture = new Texture2D(Main.graphics.GraphicsDevice, newBounds.Width, newBounds.Height);
            int area = newBounds.Width * newBounds.Height;
            Color[] data = new Color[area];
            texture.GetData(0, newBounds, data, 0, area);
            newTexture.SetData(data);
            return newTexture;
        }
    }
}