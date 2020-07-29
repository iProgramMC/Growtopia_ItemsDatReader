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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ItemsDatDecoder
{
    class Program
    {
        static string DecodeName(byte[] bytes, int len, int id)
        {
	    // MemorySerializeEncrypted from Proton SDK
            string key = "PBG892FXX982ABC*";
            string result = "";
            for (int i = 0; i < len; i++)
            {
                result += (char)(bytes[i] ^ key[(i + id) % 16]);
            }
            return result;
        }
        static void Main(string[] args)
        {
            bool pause = true;
            Console.WriteLine("Growtopia items.dat decoder (C) 2019 iProgramInCpp");
            Console.WriteLine("This program is licensed under the MIT license.");
            Console.WriteLine("View https://opensource.org/licenses/MIT for more info.");
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: decoder.exe <items.dat> [-dont_pause] OR drag-and-drop your items.dat onto the decoder.");
                return;
            }
            if (args.Length == 2)
            {
                if (args[1] == "-dont_pause")
                    pause = false;
            }
            Console.WriteLine("Decoding...");
            Stream stream = new FileStream(args[0], FileMode.Open);

            byte[] useForIntReading = new byte[4];
            byte[] useForShortReading = new byte[4];

            int itemCount = 0;
            short unused = 0;

            stream.Read(useForShortReading, 0, 2);
            unused = BitConverter.ToInt16(useForShortReading, 0);

            stream.Read(useForIntReading, 0, 4);
            itemCount = BitConverter.ToInt32(useForIntReading, 0);

            Stream streamOut = new FileStream("item_defs.txt", FileMode.OpenOrCreate);
            // NOTE (July 29th): The variable names are not changed, but the notation is. Neither is the order, so it should still be readable by ItemsDatEncoder.
            string header1 = "// Formatting: \r\n// add_item\\id\\editType\\editCategory\\actionType\\hitSound\\itemName\\fileName\\texHash\\itemKind\\texX\\texY\\sprType\\isStripey\\" +
                "collType\\hitsTaken\\drops\\clothingType\\rarity\\maxItems\\audioFile\\audioHash\\animLengthMs\\seedBase\\seedOver\\treeBase\\treeOver\\" +
                "color1BGRA\\color2BGRA\\ingredient1\\ingredient2\\growTime\\petName\\petPrefix\\petSuffix\\petAbility\\extraUnkField1\\" +
                "\\extraOptions\\extraFilename\\extraOptions2\\extraUnknown1\\extraUnknown2\\extraUnkShort1\\extraUnkShort2\\extraUnkShort3\\isRayman\\val1\\val2\r\n" +
                "// NOTE: audio* can also be used for updating textures if it isn't already used for audio.\r\n" +
                "// Extracted with ItemsDatDecoder (C) 2019 iProgramInCpp\r\n" +
                "// Everything (C) 2013-2019 RTsoft/Hamumu/Ubisoft. All rights reserved.\r\n"+
                "// Unfortunately, I had to rely on \\ symbols instead of | because some data was not compatible. Sorry!\r\n"+
                "// Credits to Anybody for other fields' names!\r\n"+
                "// Credits to ness#8001 for finding item name decoding!\r\n"+
                "// Items.dat decoder (C) 2019 iProgramInCpp\r\n\r\n";

            byte[] header1Bytes = Encoding.UTF8.GetBytes(header1);
            streamOut.Write(header1Bytes, 0, header1Bytes.Length);

            string header2 = $"item_db_ver\\{unused}\r\nitem_count\\{itemCount}\r\n\r\n";
            byte[] header2Bytes = Encoding.UTF8.GetBytes(header2);
            streamOut.Write(header2Bytes, 0, header2Bytes.Length);

            for (int c = 0; c < itemCount; c++)
            {
                short itemID = 0;
                stream.Read(useForShortReading, 0, 2);
                itemID = BitConverter.ToInt16(useForShortReading, 0);
                stream.Seek(2, SeekOrigin.Current);
                byte editableType = (byte)stream.ReadByte();
                byte editableCategory = (byte)stream.ReadByte();
                byte actionType = (byte)stream.ReadByte();
                byte hitSound = (byte)stream.ReadByte();

                short itemNameLength;
                stream.Read(useForShortReading, 0, 2);
                itemNameLength = BitConverter.ToInt16(useForShortReading, 0);

                byte[] itemNameEncoded = new byte[itemNameLength];
                stream.Read(itemNameEncoded, 0, itemNameLength);
                string decodedItemName = DecodeName(itemNameEncoded, itemNameLength, itemID);

                short fileNameLength;
                stream.Read(useForShortReading, 0, 2);
                fileNameLength = BitConverter.ToInt16(useForShortReading, 0);

                byte[] filenameBytes = new byte[fileNameLength];
                stream.Read(filenameBytes, 0, fileNameLength);
                string filename = Encoding.ASCII.GetString(filenameBytes);

                byte[] texturehash_ = new byte[4];
                stream.Read(texturehash_, 0, 4);
                int texturehash = BitConverter.ToInt32(texturehash_, 0);

                byte itemKind = (byte)stream.ReadByte();
                stream.Seek(4, SeekOrigin.Current);
                byte textureX = (byte)stream.ReadByte();
                byte textureY = (byte)stream.ReadByte();
                byte spreadType = (byte)stream.ReadByte();
                byte isStripey = (byte)stream.ReadByte();
                byte collType = (byte)stream.ReadByte();
                byte hitsToDestroy = (byte)stream.ReadByte();
                byte dropChance = (byte)stream.ReadByte();
                int clothingType = 0;
                // No
                stream.Seek(3, SeekOrigin.Current);
                clothingType = stream.ReadByte();
                short rarity = 0;
                stream.Read(useForShortReading, 0, 2);
                rarity = BitConverter.ToInt16(useForShortReading, 0);
                byte toolKind = (byte)stream.ReadByte();
                short audioFileLength = 0;
                stream.Read(useForShortReading, 0, 2);
                audioFileLength = BitConverter.ToInt16(useForShortReading, 0);

                byte[] audioFileBytes = new byte[audioFileLength];
                stream.Read(audioFileBytes, 0, audioFileLength);
                string audioFileStr69420 = Encoding.ASCII.GetString(audioFileBytes);

                byte[] audioHash_ = new byte[4];
                stream.Read(audioHash_, 0, 4);
                int audioHash = BitConverter.ToInt32(audioHash_, 0);

                short audioVolume = 0;
                stream.Read(useForShortReading, 0, 2);
                audioVolume = BitConverter.ToInt16(useForShortReading, 0);
                
                string petName = "";
                string petPrefix = "";
                string petSuffix = "";
                string petAbility = "";
                string extraFieldUnk5 = "";
				
                stream.Read(useForShortReading, 0, 2);
                short extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0)
                {
                    byte[] bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    petName = Encoding.ASCII.GetString(bytesToWriteToStr);
                }
                stream.Read(useForShortReading, 0, 2);
                extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0)
                {
                    byte[] bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    petPrefix = Encoding.ASCII.GetString(bytesToWriteToStr);
                }
                stream.Read(useForShortReading, 0, 2);
                extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0)
                {
                    byte[] bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    petSuffix = Encoding.ASCII.GetString(bytesToWriteToStr);
                }
                stream.Read(useForShortReading, 0, 2);
                extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0)
                {
                    byte[] bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    petAbility = Encoding.ASCII.GetString(bytesToWriteToStr);
                }
                stream.Read(useForShortReading, 0, 2);
                extraFieldLenCommon = BitConverter.ToInt16(useForShortReading, 0);
                if (extraFieldLenCommon < 30 && extraFieldLenCommon >= 0)
                {
                    byte[] bytesToWriteToStr = new byte[extraFieldLenCommon];
                    stream.Read(bytesToWriteToStr, 0, extraFieldLenCommon);
                    extraFieldUnk5 = Encoding.ASCII.GetString(bytesToWriteToStr);
                }
                // This should do the job of stream.Seek(+10)!

                byte seedBase = (byte)stream.ReadByte();
                byte seedOverlay = (byte)stream.ReadByte();
                byte treeBase = (byte)stream.ReadByte();
                byte treeLeaves = (byte)stream.ReadByte();
                
                // Color is ARGB.
                byte[] color1 = new byte[4];
                stream.Read(color1, 0, 4);
                byte[] color2 = new byte[4];
                stream.Read(color2, 0, 4);

                short ingredient1 = 0;
                stream.Read(useForShortReading, 0, 2);
                ingredient1 = BitConverter.ToInt16(useForShortReading, 0);
                short ingredient2 = 0;
                stream.Read(useForShortReading, 0, 2);
                ingredient2 = BitConverter.ToInt16(useForShortReading, 0);

                int growTimeSec = 0;
                stream.Read(useForIntReading, 0, 4);
                growTimeSec = BitConverter.ToInt16(useForIntReading, 0);

                short extraFieldShort3 = 0;
                stream.Read(useForShortReading, 0, 2);
                extraFieldShort3 = BitConverter.ToInt16(useForShortReading, 0);

                short isRayman = 0;
                stream.Read(useForShortReading, 0, 2);
                isRayman = BitConverter.ToInt16(useForShortReading, 0);

                short extraFieldLength = 0;
                stream.Read(useForShortReading, 0, 2);
                extraFieldLength = BitConverter.ToInt16(useForShortReading, 0);
                string extraOptions = "";
                byte[] bytesToWriteToStr234 = new byte[extraFieldLength];
                stream.Read(bytesToWriteToStr234, 0, extraFieldLength);
                extraOptions = Encoding.ASCII.GetString(bytesToWriteToStr234);


                short extraField2Length = 0;
                stream.Read(useForShortReading, 0, 2);
                extraField2Length = BitConverter.ToInt16(useForShortReading, 0);
                string extraFilename = "";
                bytesToWriteToStr234 = new byte[extraField2Length];
                stream.Read(bytesToWriteToStr234, 0, extraField2Length);
                extraFilename = Encoding.ASCII.GetString(bytesToWriteToStr234);

                short extraField3Length = 0;
                stream.Read(useForShortReading, 0, 2);
                extraField3Length = BitConverter.ToInt16(useForShortReading, 0);
                string extraOptions2 = "";
                bytesToWriteToStr234 = new byte[extraField3Length];
                stream.Read(bytesToWriteToStr234, 0, extraField3Length);
                extraOptions2 = Encoding.ASCII.GetString(bytesToWriteToStr234);

                short extraField4Length = 0;
                stream.Read(useForShortReading, 0, 2);
                extraField4Length = BitConverter.ToInt16(useForShortReading, 0);
                string extraFieldUnk_4 = "";
                bytesToWriteToStr234 = new byte[extraField4Length];
                stream.Read(bytesToWriteToStr234, 0, extraField4Length);
                extraFieldUnk_4 = Encoding.ASCII.GetString(bytesToWriteToStr234);

                stream.Seek(4, SeekOrigin.Current);
                short value = 0;
                stream.Read(useForShortReading, 0, 2);
                value = BitConverter.ToInt16(useForShortReading, 0);
                short value2 = 0;
                stream.Read(useForShortReading, 0, 2);
                value2 = BitConverter.ToInt16(useForShortReading, 0);
                
                short unkValueShort1 = 0;
                stream.Read(useForShortReading, 0, 2);
                unkValueShort1 = BitConverter.ToInt16(useForShortReading, 0);

                stream.Seek(16 - value, SeekOrigin.Current);

                short unkValueShort2 = 0;
                stream.Read(useForShortReading, 0, 2);
                unkValueShort2 = BitConverter.ToInt16(useForShortReading, 0);
                
                // we're done parsing, skip a bunch of bytes
                stream.Seek(50, SeekOrigin.Current);

                short extraField5Length = 0;
                stream.Read(useForShortReading, 0, 2);
                extraField5Length = BitConverter.ToInt16(useForShortReading, 0);
                string extraFieldUnk_5 = "";
                bytesToWriteToStr234 = new byte[extraField5Length];
                stream.Read(bytesToWriteToStr234, 0, extraField5Length);
                extraFieldUnk_5 = Encoding.ASCII.GetString(bytesToWriteToStr234);

                // add the item info to the file
                string audiofilestring = audioFileStr69420;
                string file_ = $"add_item\\{itemID}\\{editableType}\\{editableCategory}\\{actionType}\\{hitSound}\\{decodedItemName}\\" +
                                $"{filename}\\{texturehash}\\{itemKind}\\{textureX}\\{textureY}\\{spreadType}\\{isStripey}\\" +
                                $"{collType}\\{hitsToDestroy}\\{dropChance}\\{clothingType}\\{rarity}\\{toolKind}\\{audiofilestring}\\" +
                                $"{audioHash}\\{audioVolume}\\{seedBase}\\{seedOverlay}\\{treeBase}\\{treeLeaves}" +
                                $"\\{color1[0]},{color1[1]},{color1[2]},{color1[3]}" +
                                $"\\{color2[0]},{color2[1]},{color2[2]},{color2[3]}" +
                                $"\\{ingredient1}\\{ingredient2}\\{growTimeSec}\\{petName}\\{petPrefix}\\{petSuffix}\\{petAbility}\\{extraFieldUnk5}" +
                                $"\\{extraOptions}\\{extraFilename}\\{extraOptions2}\\{extraFieldUnk_4}\\{extraFieldUnk_5}" +
                                $"\\{unkValueShort1}\\{unkValueShort2}\\{extraFieldShort3}\\{isRayman}\\{value}\\{value2}\r\n";
                byte[] bs = Encoding.UTF8.GetBytes(file_);
                streamOut.Write(bs, 0, bs.Length);
                // My way of displaying progress is kind of wonky, but it works fine.
                Console.Write($"\rProcessing items {(int)((float)((float)(c + 1) / (float)itemCount) * 100.0f)}%   ");
            }
            Console.Write("\nDecoding successful.");

            streamOut.Close();
            stream.Close();
            if (pause)
            {
                Console.WriteLine(" Press any key to quit.");
                Console.ReadKey();
            }
        }
    }
}
