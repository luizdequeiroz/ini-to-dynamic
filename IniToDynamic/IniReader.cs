using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IniToDynamic
{
    class IniReader
    {
        /// <summary>
        ///     Método que realiza a leitura de arquivo .ini e retorna a estrutura em objeto dinâmico.
        /// </summary>
        /// <autor>Luiz de Queiroz</autor>
        /// <param name="fileName">String com o caminho e nome do arquivo.</param>
        /// <returns>Objeto dinâmico</returns>
        public static dynamic GetIniAsDynamic(string fileName)
        {
            StreamReader reader = new StreamReader(fileName);

            var content = Crypt.Decrypt(reader.ReadToEnd(), "csharpbetter");
            var listOSection = Conversor.ConvertToList(content);
            var htParents = Conversor.ConvertToHashtables(listOSection);
            var keyValuePairs = Conversor.ConvertToKeyValuePairs(htParents);
            var dynamic = Conversor.ConvertToDynamic(keyValuePairs);

            return dynamic;
        }

        class Crypt
        {
            public static string Encrypt(string content, string key)
            {
                if (content == null)
                {
                    return null;
                }

                if (key == null)
                {
                    key = string.Empty;
                }

                var bytesToBeEncrypted = Encoding.UTF8.GetBytes(content);
                var keyBytes = Encoding.UTF8.GetBytes(key);

                keyBytes = SHA1.Create().ComputeHash(keyBytes);

                var bytesEncrypted = Encrypt(bytesToBeEncrypted, keyBytes);

                return Convert.ToBase64String(bytesEncrypted);
            }

            public static string Decrypt(string encryptedText, string password)
            {
                if (encryptedText == null)
                {
                    return null;
                }

                if (password == null)
                {
                    password = string.Empty;
                }

                var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
                var passwordBytes = Encoding.UTF8.GetBytes(password);

                passwordBytes = SHA1.Create().ComputeHash(passwordBytes);

                var bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

                return Encoding.UTF8.GetString(bytesDecrypted);
            }

            private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] keyBytes)
            {
                byte[] encryptedBytes = null;

                var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        var key = new Rfc2898DeriveBytes(keyBytes, saltBytes, 1000);

                        AES.KeySize = 256;
                        AES.BlockSize = 128;
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.Close();
                        }

                        encryptedBytes = ms.ToArray();
                    }
                }

                return encryptedBytes;
            }

            private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] keyBytes)
            {
                byte[] decryptedBytes = null;

                var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        var key = new Rfc2898DeriveBytes(keyBytes, saltBytes, 1000);

                        AES.KeySize = 256;
                        AES.BlockSize = 128;
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);
                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }

                        decryptedBytes = ms.ToArray();
                    }
                }

                return decryptedBytes;
            }
        }

        class Conversor
        {
            public static string[] ConvertToList(string content)
            {
                return content.Split('[', ']', '\r', '\n', '\t')
                              .Select(os => os.TrimStart().TrimEnd())
                              .Where(os => !string.IsNullOrEmpty(os))
                              .ToArray();
            }

            public static Hashtable ConvertToHashtables(string[] listOSection)
            {
                var htParents = new Hashtable();
                var htChildren = new Hashtable();
                var section = "";
                foreach (var oSection in listOSection)
                {
                    if (oSection.Contains('='))
                    {
                        var kVArray = oSection.Split('=');
                        htChildren[kVArray[0]] = kVArray[1];
                        htParents[section] = htChildren;
                    }
                    else
                    {
                        section = oSection;
                        htChildren = new Hashtable();
                        htParents[oSection] = new Hashtable();
                    }
                }
                return htParents;
            }

            public static IOrderedEnumerable<KeyValuePair<string, IOrderedEnumerable<KeyValuePair<string, string>>>> ConvertToKeyValuePairs(Hashtable htParents)
            {
                return htParents.Cast<DictionaryEntry>()
                                .ToDictionary(parent => (string)parent.Key, parent => (parent.Value as Hashtable)
                                     .Cast<DictionaryEntry>()
                                     .ToDictionary(child => (string)child.Key, child => (string)child.Value)
                                     .OrderByDescending(child => child.Key))
                                .OrderByDescending(parent => parent.Key);
            }

            public static dynamic ConvertToDynamic(IOrderedEnumerable<KeyValuePair<string, IOrderedEnumerable<KeyValuePair<string, string>>>> keyValuePairs)
            {
                dynamic dynamic = new ExpandoObject();
                var dictionary = dynamic as IDictionary<string, object>;
                foreach (var kvp in keyValuePairs)
                {
                    dynamic dynamicChild = new ExpandoObject();
                    var dictionaryChild = dynamicChild as IDictionary<string, object>;
                    foreach (var kvpChild in kvp.Value)
                    {
                        dictionaryChild.Add(kvpChild.Key, kvpChild.Value);
                    }

                    dictionary.Add(kvp.Key, dynamicChild);
                }
                return dynamic;
            }
        }
    }
}
