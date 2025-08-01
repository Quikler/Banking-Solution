using AutoFixture;

namespace Base.UnitTests;

public class BaseUnitTests
{
    protected static Fixture Fixture { get; }

    static BaseUnitTests()
    {
        Fixture = new Fixture();
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
