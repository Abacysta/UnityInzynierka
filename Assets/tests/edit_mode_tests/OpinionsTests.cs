using NUnit.Framework;
using UnityEngine;

public class OpinionsTests
{
    [Test]
    public void OpinionValue_ShouldNotExceedMax()
    {
        // Arrange
        Map map = new();
        Country country = new(1, "Gauls", (5, 5), new Color(0.8392f, 0.7216f, 0.4706f), 1, map);

        // Act
        country.Opinions[2] = 205;

        // Assert
        Assert.AreEqual(200, country.Opinions[2]);
    }

    [Test]
    public void OpinionValue_ShouldNotExceedMin()
    {
        // Arrange
        Map map = new();
        Country country = new(1, "Gauls", (5, 5), new Color(0.8392f, 0.7216f, 0.4706f), 1, map);

        // Act
        country.Opinions[2] = -205;

        // Assert
        Assert.AreEqual(-200, country.Opinions[2]);
    }
}