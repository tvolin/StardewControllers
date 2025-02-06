﻿using System.Text;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Enchantments;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StarControl.Menus;

/// <summary>
/// A menu item that corresponds to an item in the player's inventory.
/// </summary>
internal class InventoryMenuItem : IRadialMenuItem
{
    public string Id => Item.QualifiedItemId;

    /// <summary>
    /// The underlying inventory item.
    /// </summary>
    public Item Item { get; }

    public string Title { get; }

    public string Description { get; }

    public int? StackSize => Item.maximumStackSize() > 1 ? Item.Stack : null;

    public int? Quality => Item.Quality;

    public Texture2D? Texture { get; }

    public Rectangle? SourceRectangle { get; }

    public Rectangle? TintRectangle { get; }

    public Color? TintColor { get; }

    public InventoryMenuItem(Item item)
    {
        Logger.Log(LogCategory.Menus, "Starting refresh of inventory menu.");
        Item = item;
        Title = item.DisplayName;
        Description = UnparseText(item.getDescription());

        var data = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
        var textureData = GetTextureRedirect(item);
        Texture = textureData?.GetTexture() ?? data.GetTexture();
        SourceRectangle = textureData?.GetSourceRect() ?? data.GetSourceRect();
        (TintRectangle, TintColor) = GetTinting(item, textureData ?? data);
    }

    public ItemActivationResult Activate(
        Farmer who,
        DelayedActions delayedActions,
        ItemActivationType activationType
    )
    {
        if (Item is Tool tool && activationType == ItemActivationType.Instant)
        {
            var isActivating = IsActivating();
            if (!isActivating && (!Context.CanPlayerMove || who.canOnlyWalk || who.UsingTool))
            {
                return ItemActivationResult.Ignored;
            }
            if (who.CurrentTool != tool)
            {
                var toolIndex = who.Items.IndexOf(tool);
                if (toolIndex < 0)
                {
                    return ItemActivationResult.Ignored;
                }
                who.CurrentToolIndex = who.Items.IndexOf(tool);
            }
            if (tool is not MeleeWeapon)
            {
                who.FireTool();
            }
            Game1.pressUseToolButton();
            return isActivating ? ItemActivationResult.Used : ItemActivationResult.ToolUseStarted;
        }
        return FuzzyActivation.ConsumeOrSelect(
            who,
            Item,
            delayedActions,
            activationType == ItemActivationType.Secondary
                ? InventoryAction.Select
                : InventoryAction.Use
        );
    }

    public void ContinueActivation()
    {
        var who = Game1.player;
        if (Item is not Tool tool || who.CurrentTool != tool)
        {
            return;
        }
        if (!who.canReleaseTool || who.Stamina < 1 || tool is FishingRod)
        {
            return;
        }
        var maxPowerModifier = tool.hasEnchantmentOfType<ReachingToolEnchantment>() ? 1 : 0;
        var maxPower = tool.UpgradeLevel + maxPowerModifier;
        if (who.toolPower.Value >= maxPower)
        {
            return;
        }
        if (who.toolHold.Value <= 0)
        {
            who.toolHold.Value = (int)(tool.AnimationSpeedModifier * 600);
        }
        else
        {
            who.toolHold.Value -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            if (who.toolHold.Value <= 0)
            {
                who.toolPowerIncrease();
            }
        }
    }

    public bool EndActivation()
    {
        var who = Game1.player;
        if (Item is Tool tool && who.CurrentTool == tool && who.UsingTool && who.canReleaseTool)
        {
            who.EndUsingTool();
            return true;
        }
        // This isn't equivalent to vanilla logic, but if we detect that the player is no longer
        // using ANY tool (which is what UsingTool tells us) then any button that the controller is
        // "holding" should be released anyway, so that it can be pressed again.
        if (!who.UsingTool)
        {
            return true;
        }
        return false;
    }

    public string? GetActivationSound(
        Farmer who,
        ItemActivationType activationType,
        string defaultSound
    )
    {
        return Item is Tool && activationType == ItemActivationType.Instant ? null : defaultSound;
    }

    public bool IsActivating()
    {
        return Item is Tool tool && Game1.player.CurrentTool == tool && Game1.player.UsingTool;
    }

    private static ParsedItemData? GetTextureRedirect(Item item)
    {
        return item is SObject obj && item.ItemId == "SmokedFish"
            ? ItemRegistry.GetData(obj.preservedParentSheetIndex.Value)
            : null;
    }

    private static (Rectangle? tintRect, Color? tintColor) GetTinting(
        Item item,
        ParsedItemData data
    )
    {
        if (item is not ColoredObject coloredObject)
        {
            return default;
        }
        if (item.ItemId == "SmokedFish")
        {
            // Smoked fish implementation is unique (and private) in ColoredObject.
            // We don't care about the animation here, but should draw it darkened; the quirky
            // way this is implemented is to draw a tinted version of the original item sprite
            // (not an overlay) sprite over top of the original sprite.
            return (data.GetSourceRect(), new Color(80, 30, 10) * 0.6f);
        }
        return !coloredObject.ColorSameIndexAsParentSheetIndex
            ? (data.GetSourceRect(1), coloredObject.color.Value)
            : (null, coloredObject.color.Value);
    }

    // When we call Item.getDescription(), most implementations go through `Game1.parseText`
    // which splits the string itself onto multiple lines. This tries to remove that, so that we
    // can do our own wrapping using our own width.
    //
    // N.B. The reason we don't just use `ParsedItemData.Description` is that, at least in the
    // current version, it's often only a "base description" and includes format placeholders,
    // or is missing suffixes.
    private static string UnparseText(string text)
    {
        var sb = new StringBuilder();
        var isWhitespace = false;
        var newlineCount = 0;
        foreach (var c in text)
        {
            if (c == ' ' || c == '\r' || c == '\n')
            {
                if (!isWhitespace)
                {
                    sb.Append(' ');
                }
                isWhitespace = true;
                if (c == '\n')
                {
                    newlineCount++;
                }
            }
            else
            {
                // If the original text has a "paragraph", the formatted text will often look
                // strange if that is also collapsed into a space. So preserve _multiple_
                // newlines somewhat as a single "paragraph break".
                if (newlineCount > 1)
                {
                    // From implementation above, newlines are counted as whitespace so we know
                    // that the last character will always be a space when hitting here.
                    sb.Length--;
                    sb.Append("\r\n\r\n");
                }
                sb.Append(c);
                isWhitespace = false;
                newlineCount = 0;
            }
        }
        if (isWhitespace)
        {
            sb.Length--;
        }
        return sb.ToString();
    }
}
