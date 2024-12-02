using NUnit.Framework;
using static Assets.classes.subclasses.Constants.Province;
public class HappinessTests
{
    [Test]
    public void ProvinceHappiness_ShouldNotExceedMax()
    {
        // Arrange
        Province province = new(
            "New Province",
            10,
            20,
            "land",
            Province.TerrainType.forest,
            Resource.Iron,
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
        Assert.AreEqual(MAX_HAPP, province.Happiness);
    }

    [Test]
    public void ProvinceHappiness_ShouldNotExceedMin()
    {
        // Arrange
        Province province = new(
            "New Province",
            10,
            20,
            "land",
            Province.TerrainType.forest,
            Resource.Iron,
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
        Assert.AreEqual(MIN_HAPP, province.Happiness);
    }
}