using System.Threading;
using PurplePen;

namespace PurplePen.Tests
{
    // Reports the progress of a PDF conversion during a test run, where there is
    // no window to show and nobody to dismiss it.
    //
    // CoreMapUtil subscribes to the conversion's completion, then calls
    // ShowLoadingStatus and treats its return value as "did the conversion
    // succeed". The interactive implementation satisfies that contract by showing
    // a modal dialog, which blocks until the conversion closes it. This one
    // blocks on the completion signal directly. PdfLoadingUI cannot be used
    // unattended: on a failed conversion it shows an error and then waits for a
    // user to press Cancel, which never happens on a build machine.
    class HeadlessPdfLoadingStatus : IPdfLoadingStatus
    {
        private readonly ManualResetEventSlim completed = new ManualResetEventSlim(false);
        private readonly object sync = new object();
        private bool succeeded;

        // Waits for the conversion to finish and reports whether it succeeded.
        //
        // Parameters:
        //   fileName - the PDF being converted; unused, as nothing is displayed.
        public bool ShowLoadingStatus(string fileName)
        {
            completed.Wait();

            lock (sync) {
                bool result = succeeded;

                // The service is a singleton, so leave the signal clear for any
                // later conversion.
                completed.Reset();
                return result;
            }
        }

        // Records the outcome of the conversion and releases ShowLoadingStatus.
        // Called on the thread that ran the conversion, which may reach here
        // before ShowLoadingStatus is entered; the signal covers both orders.
        //
        // Parameters:
        //   success - true when the conversion produced a usable image.
        //   errorMessage - the converter's output when it did not; the caller
        //     reports this itself, so it is not needed here.
        public void LoadingComplete(bool success, string errorMessage)
        {
            lock (sync) {
                succeeded = success;
                completed.Set();
            }
        }
    }
}
