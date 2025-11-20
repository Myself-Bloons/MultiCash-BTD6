global using BTD_Mod_Helper;
global using System;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppTMPro;
using MelonLoader;
using MultiCash;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[assembly: MelonInfo(typeof(MultiCash.MultiCash), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace MultiCash;

public class MultiCash : BloonsTD6Mod
{
    public override void OnApplicationStart()
    {
        ModHelper.Msg<MultiCash>("MultiCash loaded!");
    }

    public static readonly ModSettingHotkey OpenGUI = new(KeyCode.F10)
    {
        displayName = "Global Multiplier"
    };

    public static readonly ModSettingDouble GlobalMultiplier = new(2.0);

    public static readonly ModSettingCategory SourceMultipliers = new("Cash Source Multipliers")
    {
        collapsed = false
    };

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (OpenGUI.JustPressed())
        {
            PopupScreen.instance.ShowSetNamePopup(
                "Global Cash Multiplier",
                "Enter multiplier (1 = 1x, 1.5 = 1.5x, 2 = 2x, etc.)",
                new Action<string>(value =>
                {
                    if (double.TryParse(value, out double multiplier))
                    {
                        GlobalMultiplier.SetValue(Math.Clamp(multiplier, 0.01, 100.0));
                    }
                }),
                ((double)GlobalMultiplier).ToString()
            );

            PopupScreen.instance.ModifyField(field =>
            {
                field.characterValidation = TMP_InputField.CharacterValidation.Decimal;
                field.characterLimit = 10;
            });
        }
    }

    public static readonly ModSettingDouble PopMultiplier = new(1.0)
    {
        displayName = "Pops & Round Rewards",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.PopIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble EcoMultiplier = new(1.0)
    {
        displayName = "Farms",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.BananaFarmIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble BankMultiplier = new(1.0)
    {
        displayName = "Bank Deposits",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.MonkeyBankUpgradeIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble CoopMultiplier = new(1.0)
    {
        displayName = "Coop Transfers",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.Coop2PlayerIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble SellingMultiplier = new(1.0)
    {
        displayName = "Tower Sales",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.SellingDisabledIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble BuyingMultiplier = new(1.0)
    {
        displayName = "Tower Purchases",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.BattleTowerPropIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble UpgradingMultiplier = new(1.0)
    {
        displayName = "Tower Upgrades",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.UpgradeBtn,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble GeraldoMultiplier = new(1.0)
    {
        displayName = "Geraldo Purchases",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.GeraldoIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble MapMultiplier = new(1.0)
    {
        displayName = "Map Interactables",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.GiftBoxIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble PropMultiplier = new(1.0)
    {
        displayName = "Prop Sales",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.PortableLakeIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble QuestMultiplier = new(1.0)
    {
        displayName = "Quest Rewards",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.QuestIcon,
        min = 0.0,
        max = 100.0
    };

    public static readonly ModSettingDouble CorvusMultiplier = new(1.0)
    {
        displayName = "Corvus Nourishment",
        slider = false,
        category = SourceMultipliers,
        icon = VanillaSprites.CorvusIcon,
        min = 0.0,
        max = 100.0
    };

    private static readonly Dictionary<Simulation.CashSource, ModSettingDouble> Multipliers = new()
    {
        { Simulation.CashSource.Normal, PopMultiplier },
        { Simulation.CashSource.EcoEarned, EcoMultiplier },
        { Simulation.CashSource.BankDeposit, BankMultiplier },
        { Simulation.CashSource.CoopTransferedCash, CoopMultiplier },
        { Simulation.CashSource.TowerSold, SellingMultiplier },
        { Simulation.CashSource.TowerBrought, BuyingMultiplier },
        { Simulation.CashSource.TowerUpgraded, UpgradingMultiplier },
        { Simulation.CashSource.GeraldoPurchase, GeraldoMultiplier },
        { Simulation.CashSource.MapInteractableUsed, MapMultiplier },
        { Simulation.CashSource.PropSold, PropMultiplier },
        { Simulation.CashSource.QuestAwarded, QuestMultiplier },
        { Simulation.CashSource.CorvusNourishment, CorvusMultiplier }
    };

    [HarmonyLib.HarmonyPatch(typeof(Simulation), nameof(Simulation.AddCash))]
    internal static class AddCash_Patch
    {
        [HarmonyLib.HarmonyPrefix]
        private static void Prefix(ref double c, Simulation.CashSource source)
        {
            if (Multipliers.TryGetValue(source, out var multiplier))
            {
                c *= GlobalMultiplier * multiplier;
            }
        }
    }
}
