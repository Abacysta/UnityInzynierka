using NUnit.Framework;

public class BuildingTests
{
    private Province province;

    // Arrange
    [SetUp]
    public void Setup()
    {
        // Arrange
        province = new(
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
    }

    [Test]
    public void GivenPopulationGreaterThanSchoolMinPop_WhenCreatingProvince_ThenSchoolBuildingShouldUpgradable()
    {
        // Act
        province.Population = 4000;
        province.Buildings = Province.defaultBuildings(province);

        // Assert
        Assert.AreEqual(0, province.Buildings[BuildingType.School]);
    }

    [Test]
    public void GivenPopulationLessThanOrEqualToSchoolMinPop_WhenCreatingProvince_ThenSchoolBuildingShouldBeLocked()
    {
        // Act
        province.Population = 2000;
        province.Buildings = Province.defaultBuildings(province);

        // Assert
        Assert.AreEqual(4, province.Buildings[BuildingType.School]);
    }

    [Test]
    public void GivenBuidlingAtLevel0_WhenUpgraded_ThenLevelShouldBe1()
    {
        // Assert
        province.ResourceType = Resource.Iron;

        // Act
        province.Buildings = Province.defaultBuildings(province);
        province.UpgradeBuilding(BuildingType.Mine);

        // Assert
        Assert.AreEqual(1, province.Buildings[BuildingType.Mine]);
    }

    [Test]
    public void GivenBuidlingAtLevel1_WhenDowngraded_ThenLevelShouldBe0()
    {
        // Assert
        province.ResourceType = Resource.Iron;

        // Act
        province.Buildings[BuildingType.Mine] = 1;
        province.DowngradeBuilding(BuildingType.Mine);

        // Assert
        Assert.AreEqual(0, province.Buildings[BuildingType.Mine]);
    }

    [Test]
    public void GivenGoldProvince_WhenCreatingProvince_ThenMineShouldBeUpgradable()
    {
        // Assert
        province.ResourceType = Resource.Gold;

        // Act
        province.Buildings = Province.defaultBuildings(province);
        province.DowngradeBuilding(BuildingType.Mine);

        // Assert
        Assert.AreEqual(0, province.Buildings[BuildingType.Mine]);
    }

    [Test]
    public void GivenIronProvince_WhenCreatingProvince_ThenMineShouldBeUpgradable()
    {
        // Assert
        province.ResourceType = Resource.Iron;

        // Act
        province.Buildings = Province.defaultBuildings(province);
        province.DowngradeBuilding(BuildingType.Mine);

        // Assert
        Assert.AreEqual(0, province.Buildings[BuildingType.Mine]);
    }

    [Test]
    public void GivenNotIronOrGoldProvince_WhenCreatingProvince_ThenMineShouldBeLocked()
    {
        // Assert
        province.ResourceType = Resource.SciencePoint;

        // Act
        province.Buildings = Province.defaultBuildings(province);

        // Assert
        Assert.AreEqual(4, province.Buildings[BuildingType.Mine]);
    }

    [Test]
    public void GivenLockedBuilding_WhenDowngradingIt_ThenItShouldBeStillLockedAt4()
    {
        // Assert
        province.ResourceType = Resource.Wood;

        // Act
        province.Buildings = Province.defaultBuildings(province);
        province.DowngradeBuilding(BuildingType.Mine);

        // Assert
        Assert.AreEqual(4, province.Buildings[BuildingType.Mine]);
    }

    [Test]
    public void GivenLockedBuilding_WhenUpgradingIt_ThenItShouldBeStillLockedAt4()
    {
        // Assert
        province.ResourceType = Resource.SciencePoint;

        // Act
        province.Buildings = Province.defaultBuildings(province);
        province.UpgradeBuilding(BuildingType.Mine);

        // Assert
        Assert.AreEqual(4, province.Buildings[BuildingType.Mine]);
    }

    [Test]
    public void GivenBuildingAtMaxLevel_WhenUpgradeIsAttempted_ThenLevelShouldStayAtMax()
    {
        // Assert
        province.ResourceType = Resource.Iron;

        // Act
        province.Buildings[BuildingType.Mine] = 3;
        province.UpgradeBuilding(BuildingType.Mine);

        // Assert
        Assert.AreEqual(3, province.Buildings[BuildingType.Mine]);
    }
}