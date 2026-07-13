using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class MercenaryPortraitProviderTests
{
    private GameObject imageObject;

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(imageObject);
    }

    [TestCase(MercenaryClass.Warrior, 0)]
    [TestCase(MercenaryClass.Knight, 0)]
    [TestCase(MercenaryClass.Archer, 1)]
    [TestCase(MercenaryClass.Ranger, 1)]
    [TestCase(MercenaryClass.Mage, 2)]
    [TestCase(MercenaryClass.Elementalist, 2)]
    [TestCase(MercenaryClass.Priest, 3)]
    [TestCase(MercenaryClass.Paladin, 3)]
    [TestCase(MercenaryClass.Rogue, 4)]
    [TestCase(MercenaryClass.Ninja, 4)]
    [TestCase(MercenaryClass.Lancer, 5)]
    [TestCase(MercenaryClass.GuardianLancer, 5)]
    public void TryApply_BaseAndAdvancedClassesUseBaseSheet(
        MercenaryClass mercenaryClass,
        int expectedIndex)
    {
        AssertPortrait(
            mercenaryClass,
            "MercenaryPortraitSheet",
            expectedIndex);
    }

    [TestCase(MercenaryClass.Warlord, 0)]
    [TestCase(MercenaryClass.Beastmaster, 1)]
    [TestCase(MercenaryClass.Chronomancer, 2)]
    [TestCase(MercenaryClass.Saint, 3)]
    [TestCase(MercenaryClass.Shadow, 4)]
    [TestCase(MercenaryClass.DragonKnight, 5)]
    public void TryApply_SpecialClassesUseSpecialSheet(
        MercenaryClass mercenaryClass,
        int expectedIndex)
    {
        AssertPortrait(
            mercenaryClass,
            "MercenarySpecialPortraitSheet",
            expectedIndex);
    }

    private void AssertPortrait(
        MercenaryClass mercenaryClass,
        string expectedTextureName,
        int expectedIndex)
    {
        imageObject = new GameObject(
            "Portrait",
            typeof(RectTransform),
            typeof(RawImage));
        RawImage image = imageObject.GetComponent<RawImage>();

        Assert.That(
            MercenaryPortraitProvider.TryApply(image, mercenaryClass),
            Is.True);
        Assert.That(image.texture, Is.Not.Null);
        Assert.That(image.texture.name, Is.EqualTo(expectedTextureName));

        float expectedX = (expectedIndex % 3) / 3f + (0.38f / 6f);
        float expectedY = expectedIndex / 3 == 0 ? 0.5f : 0f;
        Assert.That(image.uvRect.x, Is.EqualTo(expectedX).Within(0.0001f));
        Assert.That(image.uvRect.y, Is.EqualTo(expectedY).Within(0.0001f));
        Assert.That(image.uvRect.width, Is.EqualTo(0.62f / 3f).Within(0.0001f));
        Assert.That(image.uvRect.height, Is.EqualTo(0.5f).Within(0.0001f));
    }
}
