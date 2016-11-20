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
        /// <param name="bytes">The byte array to fill.</param>
        /// <param name="offset">The offset at which the byte array will be filled.</param>
        /// <param name="encoding">The encoding used to fill the byte array.</param>
        public static unsafe void SecureStringToBytes(SecureString secureString, byte[] bytes, int offset, Encoding encoding)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            IntPtr bStr = IntPtr.Zero;
            try
            {
                bStr = Marshal.SecureStringToBSTR(secureString);
                char* charPointer = (char*)bStr.ToPointer();

                int byteCount = encoding.GetByteCount(charPointer, secureString.Length);
                if ((offset + byteCount) > bytes.Length) throw new IndexOutOfRangeException("The destination array is too small.");

                byte* bytePointer = (byte*)handle.AddrOfPinnedObject().ToPointer();
                bytePointer += offset;
                encoding.GetBytes(charPointer, secureString.Length, bytePointer, byteCount);
            }
            finally
            {
                handle.Free();
                if (bStr != IntPtr.Zero) Marshal.ZeroFreeBSTR(bStr);
            }
        }

        /// <summary>
        /// Fills a byte array from a SecureString object.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="secureString">The SecureString object to fill the byte array from.</param>
        /// <param name="bytes">The byte array to fill.</param>
        /// <param name="offset">The index at which the byte array will be filled.</param>
        public static void SecureStringToBytes(SecureString secureString, byte[] bytes, int offset)
        {
            SecureStringToBytes(secureString, bytes, offset, Encoding.UTF8);
        }

        /// <summary>
        /// Creates a byte array from a SecureString object using the specified encoding.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="secureString">The SecureString object to fill the byte array from.</param>
        /// <param name="encoding">The encoding used to fill the byte array.</param>
        public static unsafe byte[] SecureStringToBytes(SecureString secureString, Encoding encoding)
        {
            byte[] bytes;
            IntPtr bStr = IntPtr.Zero;
            try
            {
                bStr = Marshal.SecureStringToBSTR(secureString);
                char* charPointer = (char*)bStr.ToPointer();

                int byteCount = encoding.GetByteCount(charPointer, secureString.Length);
                bytes = new byte[byteCount];
                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

                try
                {
                    byte* bytePointer = (byte*)handle.AddrOfPinnedObject().ToPointer();
                    encoding.GetBytes(charPointer, secureString.Length, bytePointer, byteCount);
                }
                finally
                {
                    handle.Free();
                }
            }
            finally
            {
                if (bStr != IntPtr.Zero) Marshal.ZeroFreeBSTR(bStr);
            }

            return bytes;
        }

        /// <summary>
        /// Creates a byte array from a SecureString object.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="secureString">The SecureString object to fill the byte array from.</param>
        public static void SecureStringToBytes(SecureString secureString)
        {
            SecureStringToBytes(secureString, Encoding.UTF8);
        }

        /// <summary>
        /// Creates a SecureString from a byte array using the specified encoding.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="byteArray">The byte array the SecureString will be created from.</param>
        /// <param name="encoding">The encoding used to create the SecureString.</param>
        /// <returns>Returns the created SecureString.</returns>
        public static SecureString SecureStringFromBytes(byte[] byteArray, Encoding encoding)
        {
            char[] chars = encoding.GetChars(byteArray);

            try
            {
                var result = new SecureString();

                for (int i = 0; i < chars.Length; i++)
                    result.AppendChar(chars[i]);

                return result;
            }
            finally
            {
                DestroySecureCharArray(chars);
            }
        }

        /// <summary>
        /// Creates a SecureString from a byte array.
        /// This method does not create any managed string objects.
        /// </summary>
        /// <param name="byteArray">The byte array the SecureString will be created from.</param>
        /// <returns>Returns the created SecureString.</returns>
        public static SecureString SecureStringFromBytes(byte[] byteArray)
        {
            return SecureStringFromBytes(byteArray, Encoding.UTF8);
        }

        /// <summary>
        /// Destroys a byte array filled by the SecureStringToBytes method.
        /// </summary>
        /// <param name="charArray">The byte array to destroy.</param>
        public static void DestroySecureByteArray(byte[] charArray)
        {
            for (int i = 0; i < charArray.Length; i++)
                charArray[i] = 0;
        }

        /// <summary>
        /// Destroys a char array filled by the SecureStringToBytes method.
        /// </summary>
        /// <param name="charArray">The char array to destroy.</param>
        public static void DestroySecureCharArray(char[] charArray)
        {
            for (int i = 0; i < charArray.Length; i++)
                charArray[i] = char.MinValue;
        }
    }
}
