using System.Threading.Tasks;
using NUnit.Framework;

namespace Map_Svg.Tests
{
    // Closes the browser that the tests rendered with, once every test of the assembly has finished.
    [SetUpFixture]
    public class ChromiumSession
    {
        // Close the browser, if a test started it.
        [OneTimeTearDown]
        public async Task CloseBrowserAsync()
        {
            await ChromiumRenderer.CloseAsync();
        }
    }
}
