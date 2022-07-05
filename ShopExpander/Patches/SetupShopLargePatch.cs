using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace ShopExpander.Patches
{
	public sealed class SetupShopLargePatch : ILoadable
	{
		public static Mod Mod { get; private set; } = null!;

		private static Item[] cachedItems { get; set; }

		public void Load(Mod mod)
		{
			Mod = mod;

			IL.Terraria.Chest.SetupShop += ChestSetupShop;
		}

		public void Unload()
		{
			IL.Terraria.Chest.SetupShop -= ChestSetupShop;

			Mod = null!;
		}

		private void ChestSetupShop(ILContext il)
		{
			ILCursor c = new(il);

			/*
			 * IL_0000: call class Terraria.Player Terraria.Main::get_LocalPlayer()
			 * IL_0005: ldflda valuetype Terraria.ShoppingSettings Terraria.Player::currentShoppingSettings
			 * IL_000a: ldfld float64 Terraria.ShoppingSettings::PriceAdjustment
			 * IL_000f: ldc.r8 0.89999997615814209
			 * IL_0018: cgt.un
			 * IL_001a: ldc.i4.0
			 * IL_001b: ceq
			 * IL_001d: stloc.0
			 * IL_001e: ldarg.0
			 * IL_001f: ldfld class Terraria.Item[] Terraria.Chest::item
			 */

			// The types of instructions are unique in the entire method.
			// This allows very resistant IL matching
			Func<Instruction, bool>[] instrs1 =
			{
				i => i.MatchCall(out _),
				i => i.MatchLdflda(out _),
				i => i.MatchLdfld(out _),
				i => i.MatchLdcR8(out _),
				i => i.MatchCgtUn(),
				i => i.MatchLdcI4(out _),
				i => i.MatchCeq(),
				i => i.MatchStloc(out _),
				i => i.MatchLdarg(out _),
				i => i.MatchLdfld(out _),
			};

			if (!c.TryGotoNext(MoveType.After, instrs1))
			{
				Mod.Logger.Error("Failed to find IL instructions for shop array");
				return;
			}

			Mod.Logger.Info("Increasing vanilla shop size");

			c.EmitDelegate((Item[] items) =>
			{
				cachedItems = items;
				return new Item[ShopExpander.Instance.ProvisionOverrides.DefaultValue];
			});

			/*
			 * IL_0039: ldloc.s 4
			 * IL_003b: ldc.i4.s 40
			 */
			Func<Instruction, bool>[] instrs2 =
			{
				i => i.MatchLdloc(out _),
				i => i.MatchLdcI4(40),
			};

			if (!c.TryGotoNext(MoveType.After, instrs2))
			{
				Mod.Logger.Error("Failed to find IL instructions for shop array resetting");
				return;
			}

			Mod.Logger.Info("Fix vanilla shop iteration count");

			c.Emit(OpCodes.Pop);
			c.EmitDelegate(() => ShopExpander.Instance.ProvisionOverrides.DefaultValue);

			/*
			 * IL_20a1: ldloc.2
			 * IL_20a2: ldc.i4.s 39
			 */
			Func<Instruction, bool>[] instrs3 =
			{
				i => i.MatchLdloc(2),
				i => i.MatchLdcI4(39),
			};

			Mod.Logger.Info("Patching all 39 length tests to 80 length");

			while (c.TryGotoNext(MoveType.After, instrs3))
			{
				c.Emit(OpCodes.Pop);
				c.EmitDelegate(() => ShopExpander.Instance.ProvisionOverrides.DefaultValue - 1);
				Mod.Logger.Debug("Patched 39 length test");
			}
		}
	}
}
