/****************************************************
  Decoder for items.dat
  Copyright 2019 iProgramInCpp

  Permission is hereby granted, free of charge, to any person obtaining a 
  copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation 
  the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software 
  is furnished to do so, subject to the following conditions:
  
  The above copyright notice and this permission notice shall be included in all 
  copies or substantial portions of the Software.
  
  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
  INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
  PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
  SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
  
 ****************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ItemsDatDecoder {
    internal class Program {
        private static string DecodeName(IReadOnlyList<byte> bytes, int len, int id) {
            // MemorySerializeEncrypted from Proton SDK
            const string key = "PBG892FXX982ABC*";
            var result = "";
            for (var i = 0; i < len; i++)
                result += (char) (bytes[i] ^ key[(i + id) % 16]);

            return result;
        }

        private static void Main(string[] args) {
            var pause = true;
            Console.WriteLine("Growtopia items.dat decoder (C) 2019 iProgramInCpp");
            Console.WriteLine("This program is licensed under the MIT license.");
            Console.WriteLine("View https://opensource.org/licenses/MIT for more info.");
            switch (args.Length) {
                case 0:
                    Console.WriteLine("Usage: decoder.exe <items.dat> [-dont_pause] OR drag-and-drop your items.dat onto the decoder.");
                    return;
                case 2: {
                    if (args[1] == "-dont_pause")
                        pause = false;
                    break;
                }
            }

            Console.WriteLine("Decoding...");
            Stream stream = new FileStream(args[0], FileMode.Open);

            var useForIntReading = new byte[4];
            var useForShortReading = new byte[4];

            stream.Read(useForShortReading, 0, 2);
            var unused = BitConverter.ToInt16(useForShortReading, 0);

            stream.Read(useForIntReading, 0, 4);
            var itemCount = BitConverter.ToInt32(useForIntReading, 0);

            Stream streamOut = new FileStream("item_defs.txt", FileMode.OpenOrCreate);
            const string header1 = "// Formatting: \r\n// add_item\\id\\editType\\editCategory\\actionType\\hitSound\\itemName\\fileName\\texHash\\itemKind\\texX\\texY\\sprType\\isStripey\\" +
                                   "collType\\hitsTaken\\drops\\clothingType\\rarity\\toolKind\\audioFile\\audioHash\\audioVol\\seedBase\\seedOver\\treeBase\\treeOver\\" +
                                   "color1BGRA\\color2BGRA\\ingredient1\\ingredient2\\growTime\\petName\\petPrefix\\petSuffix\\petAbility\\extraUnkField1\\" +
                                   "\\extraOptions\\extraFilename\\extraOptions2\\extraUnknown1\\extraUnknown2\\extraUnkShort1\\extraUnkShort2\\extraUnkShort3\\isRayman\\val1\\val2\r\n" +
                                   "// NOTE: audio* can also be used for updating textures if it isn't already used for audio.\r\n" +
                                   "// Extracted with ItemsDatDecoder (C) 2019 iProgramInCpp\r\n" +
                                   "// Everything (C) 2013-2019 RTsoft/Hamumu/Ubisoft. All rights reserved.\r\n" +
                                   "// Unfortunately, I had to rely on \\ symbols instead of | because some data was not compatible. Sorry!\r\n" +
                                   "// Credits to Anybody for other fields' names!\r\n" +
                                   "// Credits to ness#8001 for finding item name decoding!\r\n" +
                                   "// Items.dat decoder (C) 2019 iProgramInCpp\r\n\r\n";

            var header1Bytes = Encoding.UTF8.GetBytes(header1);
            streamOut.Write(header1Bytes, 0, header1Bytes.Length);

            var header2 = $"item_db_ver\\{unused}\r\nitem_count\\{itemCount}\r\n\r\n";
            var header2Bytes = Encoding.UTF8.GetBytes(header2);
            streamOut.Write(header2Bytes, 0, header2Bytes.Length);

            for (var c = 0; c < itemCount; c++) {
                stream.Read(useForShortReading, 0, 2);
                var itemId = BitConverter.ToInt16(useForShortReading, 0);
                stream.Seek(2, SeekOrigin.Current);
                var editableType = (byte) stream.ReadByte();
                var editableCategory = (byte) stream.ReadByte();
                var actionType = (byte) stream.ReadByte();
                var hitSound = (byte) stream.ReadByte();

                stream.Read(useForShortReading, 0, 2);
                var itemNameLength = BitConverter.ToInt16(useForShortReading, 0);

                var itemNameEncoded = new byte[itemNameLength];
                stream.Read(itemNameEncoded, 0, itemNameLength);
                var decodedItemName = DecodeName(itemNameEncoded, itemNameLength, itemId);

                stream.Read(useForShortReading, 0, 2);
                var fileNameLength = BitConverter.ToInt16(useForShortReading, 0);

                var filenameBytes = new byte[fileNameLength];
                stream.Read(filenameBytes, 0, fileNameLength);
                var filename = Encoding.ASCII.GetString(filenameBytes);

                var texturehash1 = new byte[4];
                stream.Read(texturehash1, 0, 4);
                var texturehash = BitConverter.ToInt32(texturehash1, 0);

                var itemKind = (byte) stream.ReadByte();
                stream.Seek(4, SeekOrigin.Current);
                var textureX = (byte) stream.ReadByte();
                var textureY = (byte) stream.ReadByte();
                var spreadType = (byte) stream.ReadByte();
                var isStripey = (byte) stream.ReadByte();
                var collType = (byte) stream.ReadByte();
                var hitsToDestroy = (byte) stream.ReadByte();
                var dropChance = (byte) stream.ReadByte();
                // No
                stream.Seek(3, SeekOrigin.Current);
                var clothingType = stream.ReadByte();
                stream.Read(useForShortReading, 0, 2);
                var rarity = BitConverter.ToInt16(useForShortReading, 0);
                var toolKind = (byte) stream.ReadByte();
                stream.Read(useForShortReading, 0, 2);
                var audioFileLength = BitConverter.ToInt16(useForShortReading, 0);

                var audioFileBytes = new byte[audioFileLength];
                stream.Read(audioFileBytes, 0, audioFileLength);
                var audioFileStr69420 = Encoding.ASCII.GetString(audioFileBytes);

                var audioHash1 = new byte[4];
                stream.Read(audioHash1, 0, 4);
                var audioHash = BitConverter.ToInt32(audioHash1, 0);

                stream.Read(useForShortReading, 0, 2);
                var animLength = BitConverter.ToInt16(useForShortReading, 0);

                var petName = "";
                var petPrefix = "";
                var petSuffix = "";
                var petAbility = "";
                var extraFieldUnk5 = "";

                stream.Read(useForShortReading, 0, 2);
                var extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0) {
                    var bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    petName = Encoding.ASCII.GetString(bytesToWriteToStr);
                }

                stream.Read(useForShortReading, 0, 2);
                extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0) {
                    var bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    petPrefix = Encoding.ASCII.GetString(bytesToWriteToStr);
                }

                stream.Read(useForShortReading, 0, 2);
                extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0) {
                    var bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    petSuffix = Encoding.ASCII.GetString(bytesToWriteToStr);
                }

                stream.Read(useForShortReading, 0, 2);
                extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0) {
                    var bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    petAbility = Encoding.ASCII.GetString(bytesToWriteToStr);
                }

                stream.Read(useForShortReading, 0, 2);
                extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0) {
                    var bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    extraFieldUnk5 = Encoding.ASCII.GetString(bytesToWriteToStr);
                }
                // This should do the job of stream.Seek(+10)!

                var seedBase = (byte) stream.ReadByte();
                var seedOverlay = (byte) stream.ReadByte();
                var treeBase = (byte) stream.ReadByte();
                var treeLeaves = (byte) stream.ReadByte();

                // Color is ARGB.
                var color1 = new byte[4];
                stream.Read(color1, 0, 4);
                var color2 = new byte[4];
                stream.Read(color2, 0, 4);

                stream.Read(useForShortReading, 0, 2);
                var ingredient1 = BitConverter.ToInt16(useForShortReading, 0);
                stream.Read(useForShortReading, 0, 2);
                var ingredient2 = BitConverter.ToInt16(useForShortReading, 0);

                stream.Read(useForIntReading, 0, 4);
                int growTimeSec = BitConverter.ToInt16(useForIntReading, 0);

                stream.Read(useForShortReading, 0, 2);
                var extraFieldShort3 = BitConverter.ToInt16(useForShortReading, 0);

                stream.Read(useForShortReading, 0, 2);
                var isRayman = BitConverter.ToInt16(useForShortReading, 0);

                stream.Read(useForShortReading, 0, 2);
                var extraFieldLength = BitConverter.ToInt16(useForShortReading, 0);
                var bytesToWriteToStr234 = new byte[extraFieldLength];
                stream.Read(bytesToWriteToStr234, 0, extraFieldLength);
                var extraOptions = Encoding.ASCII.GetString(bytesToWriteToStr234);


                stream.Read(useForShortReading, 0, 2);
                var extraField2Length = BitConverter.ToInt16(useForShortReading, 0);
                bytesToWriteToStr234 = new byte[extraField2Length];
                stream.Read(bytesToWriteToStr234, 0, extraField2Length);
                var extraFilename = Encoding.ASCII.GetString(bytesToWriteToStr234);

                stream.Read(useForShortReading, 0, 2);
                var extraField3Length = BitConverter.ToInt16(useForShortReading, 0);
                bytesToWriteToStr234 = new byte[extraField3Length];
                stream.Read(bytesToWriteToStr234, 0, extraField3Length);
                var extraOptions2 = Encoding.ASCII.GetString(bytesToWriteToStr234);

                stream.Read(useForShortReading, 0, 2);
                var extraField4Length = BitConverter.ToInt16(useForShortReading, 0);
                bytesToWriteToStr234 = new byte[extraField4Length];
                stream.Read(bytesToWriteToStr234, 0, extraField4Length);
                var extraFieldUnk4 = Encoding.ASCII.GetString(bytesToWriteToStr234);

                stream.Seek(4, SeekOrigin.Current);
                stream.Read(useForShortReading, 0, 2);
                var value = BitConverter.ToInt16(useForShortReading, 0);
                stream.Read(useForShortReading, 0, 2);
                var value2 = BitConverter.ToInt16(useForShortReading, 0);

                stream.Read(useForShortReading, 0, 2);
                var unkValueShort1 = BitConverter.ToInt16(useForShortReading, 0);

                stream.Seek(16 - value, SeekOrigin.Current);

                stream.Read(useForShortReading, 0, 2);
                var unkValueShort2 = BitConverter.ToInt16(useForShortReading, 0);

                // we're done parsing, skip a bunch of bytes
                stream.Seek(50, SeekOrigin.Current);

                stream.Read(useForShortReading, 0, 2);
                var extraField5Length = BitConverter.ToInt16(useForShortReading, 0);
                bytesToWriteToStr234 = new byte[extraField5Length];
                stream.Read(bytesToWriteToStr234, 0, extraField5Length);
                var extraFieldUnk6 = Encoding.ASCII.GetString(bytesToWriteToStr234);

                // add the item info to the file
                var audiofilestring = audioFileStr69420;
                var file = $"add_item\\{itemId}\\{editableType}\\{editableCategory}\\{actionType}\\{hitSound}\\{decodedItemName}\\" +
                           $"{filename}\\{texturehash}\\{itemKind}\\{textureX}\\{textureY}\\{spreadType}\\{isStripey}\\" +
                           $"{collType}\\{hitsToDestroy}\\{dropChance}\\{clothingType}\\{rarity}\\{toolKind}\\{audiofilestring}\\" +
                           $"{audioHash}\\{animLength}\\{seedBase}\\{seedOverlay}\\{treeBase}\\{treeLeaves}" +
                           $"\\{color1[0]},{color1[1]},{color1[2]},{color1[3]}" +
                           $"\\{color2[0]},{color2[1]},{color2[2]},{color2[3]}" +
                           $"\\{ingredient1}\\{ingredient2}\\{growTimeSec}\\{petName}\\{petPrefix}\\{petSuffix}\\{petAbility}\\{extraFieldUnk5}" +
                           $"\\{extraOptions}\\{extraFilename}\\{extraOptions2}\\{extraFieldUnk4}\\{extraFieldUnk6}" +
                           $"\\{unkValueShort1}\\{unkValueShort2}\\{extraFieldShort3}\\{isRayman}\\{value}\\{value2}\r\n";
                var bs = Encoding.UTF8.GetBytes(file);
                streamOut.Write(bs, 0, bs.Length);
                // My way of displaying progress is kind of wonky, but it works fine.
                Console.Write($"\rProcessing items {(c + 1) / (float) itemCount * 100.0f:0.00}%   ");
            }

            Console.Write("\nDecoding successful.");

            streamOut.Close();
            stream.Close();
            if (!pause) return;
            Console.WriteLine(" Press any key to quit.");
            Console.ReadKey();
        }
    }
}