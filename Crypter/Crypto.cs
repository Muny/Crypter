using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypter
{
    public enum CryptoActionType
    {
        Encrypt,
        Decrypt
    }

    public class CryptoAction
    {
        public CryptoActionType ActionType;

        public SecureString Password;

        public CryptoAction(CryptoActionType type, SecureString password)
        {
            this.ActionType = type;
            this.Password = password;
        }
    }


    public class Crypto
    {

        // Set your salt here, change it to meet your flavor:
        // The salt bytes must be at least 8 bytes.
        public static byte[] saltBytes = new byte[] { 66, 120, 77, 218, 203, 141, 138, 234, 221, 206, 196, 43, 177, 76, 118, 248, 105, 98, 252, 36, 147, 125, 116, 155, 39, 242, 98, 255, 52, 186, 6, 169, 3, 155, 90, 103, 47, 197, 238, 172, 158, 167, 159, 83, 20, 15, 3, 167, 87, 39, 143, 178, 228, 211, 153, 8, 20, 55, 135, 76, 111, 70, 211, 14 };

        public static byte[] GetSecureStringBytes(SecureString scstr)
        {
            byte[] secureStringBytes = null;
            // Convert System.SecureString to Pointer
            IntPtr unmanagedBytes = Marshal.SecureStringToGlobalAllocAnsi(scstr);
            try
            {
                unsafe
                {
                    byte* byteArray = (byte*)unmanagedBytes.ToPointer();
                    // Find the end of the string
                    byte* pEnd = byteArray;
                    while (*pEnd++ != 0) { }
                    // Length is effectively the difference here (note we're 1 past end) 
                    int length = (int)((pEnd - byteArray) - 1);
                    secureStringBytes = new byte[length];
                    for (int i = 0; i < length; ++i)
                    {
                        // Work with data in byte array as necessary, via pointers, here
                        secureStringBytes[i] = *(byteArray + i);
                    }
                }
            }
            finally
            {
                // This will completely remove the data from memory
                Marshal.ZeroFreeGlobalAllocAnsi(unmanagedBytes);
            }

            return secureStringBytes;
        }
    }
}
