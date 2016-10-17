using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace ModMyFactory.Helpers
{
    static class SecureStringHelper
    {
        /// <summary>
        /// Calculates the number of bytes a SecureString object will produce when encoded with a specified encoding.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="secureString">The SecureString object to measure.</param>
        /// <param name="encoding">The encoding used to create the bytes.</param>
        /// <returns>Returns the number of bytes the SecureString object will produce when encoded with the specified encoding.</returns>
        public static unsafe int GetSecureStringByteCount(SecureString secureString, Encoding encoding)
        {
            IntPtr bStr = IntPtr.Zero;

            try
            {
                bStr = Marshal.SecureStringToBSTR(secureString);

                char* chars = (char*)bStr.ToPointer();
                int length = encoding.GetByteCount(chars, secureString.Length);

                return length;
            }
            finally
            {
                if (bStr != IntPtr.Zero) Marshal.ZeroFreeBSTR(bStr);
            }
        }

        /// <summary>
        /// Calculates the number of bytes a SecureString object will produce when encoded.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="secureString">The SecureString object to measure.</param>
        /// <returns>Returns the number of bytes the SecureString object will produce when encoded.</returns>
        public static int GetSecureStringByteCount(SecureString secureString)
        {
            return GetSecureStringByteCount(secureString, Encoding.UTF8);
        }

        /// <summary>
        /// Fills a byte array from a SecureString object using the specified encoding.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="secureString">The SecureString object to fill the byte array from.</param>
        /// <param name="byteArray">The byte array to fill.</param>
        /// <param name="byteIndex">The index at which the byte array will be filled.</param>
        /// <param name="encoding">The encoding used to fill the byte array.</param>
        public static unsafe void SecureStringToBytes(SecureString secureString, byte[] byteArray, int byteIndex, Encoding encoding)
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

                for (int i = byteIndex; i < (byteIndex + length); i++)
                {
                    byteArray[i] = *bytes;
                    bytes++;
                }
            }
            finally
            {
                if (bytePointer != IntPtr.Zero) Marshal.FreeHGlobal(bytePointer);
                if (bStr != IntPtr.Zero) Marshal.ZeroFreeBSTR(bStr);
            }
        }

        /// <summary>
        /// Fills a byte array from a SecureString object.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="secureString">The SecureString object to fill the byte array from.</param>
        /// <param name="byteArray">The byte array to fill.</param>
        /// <param name="byteIndex">The index at which the byte array will be filled.</param>
        public static void SecureStringToBytes(SecureString secureString, byte[] byteArray, int byteIndex)
        {
            SecureStringToBytes(secureString, byteArray, byteIndex, Encoding.UTF8);
        }

        /// <summary>
        /// Destroys a byte array filled by the SecureStringToBytes method.
        /// </summary>
        /// <param name="byteArray">The byte array to destroy.</param>
        public static void DestroySecureByteArray(byte[] byteArray)
        {
            for (int i = 0; i < byteArray.Length; i++)
                byteArray[i] = 0;
        }
    }
}
