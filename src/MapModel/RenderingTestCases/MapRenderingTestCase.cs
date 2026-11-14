namespace RenderingTestCases
{
    // A test of the map rendering tests: its name, whether it may run beside other tests, and the checks it makes,
    // in order.
    public sealed class MapRenderingTestCase
    {
        // The name of the test.
        public readonly string Name;

        // Whether the test must not run beside other tests.
        public readonly bool NonParallelizable;

        // The checks the test makes, in order.
        public readonly MapRenderingCheck[] Checks;

        // Create a test.
        // name: the name of the test.
        // nonParallelizable: whether the test must not run beside other tests.
        // checks: the checks the test makes, in order.
        public MapRenderingTestCase(string name, bool nonParallelizable, MapRenderingCheck[] checks)
        {
            Name = name;
            NonParallelizable = nonParallelizable;
            Checks = checks;
        }

        // Return the name of the test.
        public override string ToString()
        {
            return Name;
        }
    }
}
