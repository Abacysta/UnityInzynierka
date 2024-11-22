using NUnit.Framework;

public class HappinessTests
{
    [Test]
    public void ProvinceHappiness_ShouldNotExceedMax()
    {
        // Arrange
        Province province = new(
            "province1",
            "New Province",
            10,
            20,
            "land",
            Province.TerrainType.forest,
            "iron",
            500.0f,
            3000,
            1500,
            75,
            true,
            1
        );

        // Act
        province.Happiness = 105;

        // Assert
        Assert.AreEqual(100, province.Happiness);
    }

    [Test]
    public void ProvinceHappiness_ShouldNotExceedMin()
    {
        // Arrange
        Province province = new(
            "province1",
            "New Province",
            10,
            20,
            "land",
            Province.TerrainType.forest,
            "iron",
            500.0f,
            3000,
            1500,
            75,
            true,
            1
        );

        // Act
        province.Happiness = -5;

        // Assert
        Assert.AreEqual(0, province.Happiness);
    }
}