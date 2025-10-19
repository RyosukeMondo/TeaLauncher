namespace TeaLauncher.Avalonia.Tests;

/// <summary>
/// Base class for all unit tests providing common setup and teardown functionality.
/// Inherit from this class to gain access to standard test initialization and cleanup.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Runs before each test method.
    /// Override this method in derived classes to add custom setup logic.
    /// </summary>
    [SetUp]
    public virtual void SetUp()
    {
        // Base setup - can be overridden by derived classes
    }

    /// <summary>
    /// Runs after each test method.
    /// Override this method in derived classes to add custom cleanup logic.
    /// </summary>
    [TearDown]
    public virtual void TearDown()
    {
        // Base cleanup - can be overridden by derived classes
    }
}
