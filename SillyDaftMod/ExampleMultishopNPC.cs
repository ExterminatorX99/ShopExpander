using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SillyDaftMod
{
    [AutoloadHead]
    internal class ExampleMultishopPerson : ModNPC
    {
        private int shopNum = 1;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Example Multishop Person");
            Main.npcFrameCount[NPC.type] = 25;
            NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
            NPCID.Sets.AttackFrameCount[NPC.type] = 4;
            NPCID.Sets.DangerDetectRange[NPC.type] = 700;
            NPCID.Sets.HatOffsetY[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Guide;

            Mod shopExpander = ModLoader.GetMod("ShopExpander");
            if (shopExpander != null)
            {
                shopExpander.Call("AddLegacyMultipageSetupMethods", this,
                    "First Shop", 1, (Action)(() => shopNum = 1),
                    "Second Shop", 2, (Action)(() => shopNum = 2),
                    "Third Shop", 3, (Action)(() => shopNum = 3)
                );
            }
        }

        public override string GetChat()
        {
            return "I offer multiple different pages of shop content.";
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            //If Shop Expander is loaded, don't need to set custom buttons.
            if (ModLoader.GetMod("ShopExpander") != null)
            {
                button = "Shop";
                return;
            }

            switch (ModContent.GetInstance<ExampleMultishopPerson>().shopNum)
            {
                case 1: button = "First Shop"; break;
                case 2: button = "Second Shop"; break;
                case 3: button = "Third Shop"; break;
            }

            button2 = "Change Shop";
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (firstButton)
            {
                shop = true;
            }
            else
            {
                shop = false;
                ExampleMultishopPerson npc = ModContent.GetInstance<ExampleMultishopPerson>();
                if (++npc.shopNum > 3)
                    npc.shopNum = 1;
            }
        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            switch (shopNum)
            {
                case 1:
                    shop.item[nextSlot++].SetDefaults(ItemID.DirtBlock);
                    shop.item[nextSlot++].SetDefaults(ItemID.StoneBlock);
                    shop.item[nextSlot++].SetDefaults(ItemID.Wood);
                    break;

                case 2:
                    shop.item[nextSlot++].SetDefaults(ItemID.LesserHealingPotion);
                    shop.item[nextSlot++].SetDefaults(ItemID.LesserManaPotion);
                    shop.item[nextSlot++].SetDefaults(ItemID.LesserRestorationPotion);
                    break;

                case 3:
                    shop.item[nextSlot++].SetDefaults(ItemID.CopperPickaxe);
                    shop.item[nextSlot++].SetDefaults(ItemID.CopperAxe);
                    shop.item[nextSlot++].SetDefaults(ItemID.CopperShortsword);
                    break;
            }
        }
    }
}