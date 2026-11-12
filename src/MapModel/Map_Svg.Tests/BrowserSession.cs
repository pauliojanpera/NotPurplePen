using System.Threading.Tasks;
using NUnit.Framework;

namespace Map_Svg.Tests
{
    // Closes the browsers that the tests rendered with, once every test of the assembly has finished.
    [SetUpFixture]
    public class BrowserSession
    {
        // Close the browsers that tests started.
        [OneTimeTearDown]
        public async Task CloseBrowsersAsync()
        {
            await BrowserRenderer.CloseAsync();
        }
    }
}
