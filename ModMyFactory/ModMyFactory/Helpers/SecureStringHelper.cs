using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace ModMyFactory.Helpers
{
    static class SecureStringHelper
    {
        /// <summary>
        /// Creates a byte array from a SecureString object using the specified encoding.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="secureString">The SecureString object to create the byte array from.</param>
        /// <param name="encoding">The encoding used to create the byte array.</param>
        /// <returns>Returns a byte array representing the specified SecureString object in the specified endcoding.</returns>
        public static unsafe byte[] SecureStringToBytes(SecureString secureString, Encoding encoding)
        {
            IntPtr bytePointer = IntPtr.Zero;
            IntPtr bStr = IntPtr.Zero;

            try
            {
                int maxByteCount = encoding.GetMaxByteCount(secureString.Length);

                bytePointer = Marshal.AllocHGlobal(maxByteCount);
                bStr = Marshal.SecureStringToBSTR(secureString);

                byte* bytes = (byte*)bytePointer.ToPointer();
                char* chars = (char*)bStr.ToPointer();
                int length = encoding.GetBytes(chars, secureString.Length, bytes, maxByteCount);

                byte[] result = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    result[i] = *bytes;
                    bytes++;
                }
                return result;
            }
            finally
            {
                if (bytePointer != IntPtr.Zero) Marshal.FreeHGlobal(bytePointer);
                if (bStr != IntPtr.Zero) Marshal.ZeroFreeBSTR(bStr);
            }
        }

        /// <summary>
        /// Creates a byte array from a SecureString object.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="secureString">The SecureString object to create the byte array from.</param>
        /// <returns>Returns a byte array representing the specified SecureString object.</returns>
        public static byte[] SecureStringToBytes(SecureString secureString)
        {
            return SecureStringToBytes(secureString, Encoding.UTF8);
        }

        /// <summary>
        /// Destroys a byte array created by the SecureStringToBytes method.
        /// </summary>
        /// <param name="byteArray">The byte array to destroy.</param>
        public static void DestroySecureByteArray(byte[] byteArray)
        {
            for (int i = 0; i < byteArray.Length; i++)
                byteArray[i] = 0;
        }
    }
}
