using System;
using System.Reflection;
using NUnit.Framework;

public sealed class SaveManagerEquipmentTests
{
    [Test]
    public void CreateSavedEquipment_InvalidLegacyInstance_IsSkipped()
    {
        EquipmentInstance invalidEquipment =
            new EquipmentInstance(null, EquipmentQuality.Normal, Array.Empty<EquipmentModifier>());
        MethodInfo method = typeof(SaveManager).GetMethod(
            "CreateSavedEquipment",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(method, Is.Not.Null);
        object result = method.Invoke(null, new object[] { invalidEquipment });

        Assert.That(result, Is.Null);
    }
}
