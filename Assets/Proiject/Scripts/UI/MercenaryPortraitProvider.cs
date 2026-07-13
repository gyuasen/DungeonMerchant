using UnityEngine;
using UnityEngine.UI;

public static class MercenaryPortraitProvider
{
    private const string PortraitSheetPath = "UI/MercenaryPortraitSheet";
    private const string SpecialPortraitSheetPath =
        "UI/MercenarySpecialPortraitSheet";
    private const float CellWidth = 1f / 3f;
    private const float CellHeight = 0.5f;

    // The source portraits are square, while the resume frame is tall.
    // Crop both sides instead of stretching the character horizontally.
    private const float PortraitCropWidth = CellWidth * 0.62f;
    private static Texture2D portraitSheet;
    private static Texture2D specialPortraitSheet;

    public static bool TryApply(
        RawImage target,
        MercenaryClass mercenaryClass)
    {
        if (target == null)
        {
            return false;
        }

        bool useSpecialPortrait =
            MercenaryClassProgression.IsSpecialClass(mercenaryClass);
        Texture2D sheet = useSpecialPortrait
            ? LoadSpecialPortraitSheet()
            : LoadPortraitSheet();

        // Keep the existing base-class portrait as a fallback if the optional
        // special sheet cannot be loaded.
        if (sheet == null && useSpecialPortrait)
        {
            sheet = LoadPortraitSheet();
            useSpecialPortrait = false;
        }

        if (sheet == null)
        {
            target.texture = null;
            return false;
        }

        int index = useSpecialPortrait
            ? GetSpecialPortraitIndex(mercenaryClass)
            : GetPortraitIndex(mercenaryClass);
        int column = index % 3;
        int topRow = index / 3;
        float cellLeft = column * CellWidth;
        float cropLeft = cellLeft + ((CellWidth - PortraitCropWidth) * 0.5f);
        target.texture = sheet;
        target.uvRect = new Rect(
            cropLeft,
            topRow == 0 ? CellHeight : 0f,
            PortraitCropWidth,
            CellHeight);
        target.color = Color.white;
        return true;
    }

    private static Texture2D LoadPortraitSheet()
    {
        portraitSheet ??= Resources.Load<Texture2D>(PortraitSheetPath);
        return portraitSheet;
    }

    private static Texture2D LoadSpecialPortraitSheet()
    {
        specialPortraitSheet ??=
            Resources.Load<Texture2D>(SpecialPortraitSheetPath);
        return specialPortraitSheet;
    }

    private static int GetPortraitIndex(MercenaryClass mercenaryClass)
    {
        switch (MercenaryClassProgression.GetBaseClass(mercenaryClass))
        {
            case MercenaryClass.Warrior: return 0;
            case MercenaryClass.Archer: return 1;
            case MercenaryClass.Mage: return 2;
            case MercenaryClass.Priest: return 3;
            case MercenaryClass.Rogue: return 4;
            default: return 5;
        }
    }

    private static int GetSpecialPortraitIndex(MercenaryClass mercenaryClass)
    {
        switch (mercenaryClass)
        {
            case MercenaryClass.Warlord: return 0;
            case MercenaryClass.Beastmaster: return 1;
            case MercenaryClass.Chronomancer: return 2;
            case MercenaryClass.Saint: return 3;
            case MercenaryClass.Shadow: return 4;
            case MercenaryClass.DragonKnight: return 5;
            default: return GetPortraitIndex(mercenaryClass);
        }
    }
}
