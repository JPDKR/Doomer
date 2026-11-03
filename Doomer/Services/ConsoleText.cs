using System.Runtime.InteropServices;
using System.Text;

namespace Doomer.Services
{
    // Text cmd reads or writes: UTF-8 once a batch switched the console to it, and otherwise the system's OEM
    // code page (the one the console starts in, which can differ from the user's format settings).
    public static class ConsoleText
    {
        private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        static ConsoleText() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        public static Encoding Oem => Encoding.GetEncoding((int)GetOEMCP());

        // UTF-8 when the bytes are valid UTF-8 (BOM or not), the OEM code page when they aren't.
        public static string Decode(byte[] bytes)
        {
            var preamble = Encoding.UTF8.Preamble;
            var start = bytes.AsSpan().StartsWith(preamble) ? preamble.Length : 0;

            try
            {
                return StrictUtf8.GetString(bytes, start, bytes.Length - start);
            }
            catch (DecoderFallbackException)
            {
                return Oem.GetString(bytes);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern uint GetOEMCP();
    }
}
