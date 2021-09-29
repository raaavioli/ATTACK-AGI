using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

public static class StaticNetworkUtilities
{

    static byte[] encodeLookupTable = makeEncodeLookupTable();
    static byte[] makeEncodeLookupTable()
    {
        byte[] b = new byte[256];
        for (int i = 0; i < 256; ++i)
        {
            int res;
            if ((i & 128) != 0)
            {
                res = i ^ 127;
                res <<= 1;
                res |= 1;
            }
            else
            {
                res = i << 1;
            }
            b[i] = (byte)res;
        }
        return b;
    }

    static byte[] decodeLookupTable = makeDecodeLookupTable();
    static byte[] makeDecodeLookupTable()
    {
        byte[] b = new byte[256];
        for (int i = 0; i < 256; ++i)
        {
            b[encodeLookupTable[i]] = (byte)i;
        }
        return b;
    }

    public static byte[] compressFrame_assumeNoZero(byte[] data)
    {
        byte[] buffer = new byte[960 * 540 * 2];//array for processing data
        byte[] spanBuffer = new byte[960 * 540];

        int outputCounter = 0;
        //count non-zero bytes, compress and append to output
        
        Parallel.For(0, 540, parloopi =>
        {
            parloopi *= 960;
            int end = parloopi + 960;
            int prevByte = parloopi == 0 ? 0 : data[parloopi-1];
            while (parloopi < end)
            {//ensure we don't compress something that would be more efficient in the case above
                //compression help
                int dat = data[parloopi];
                spanBuffer[parloopi] = encodeLookupTable[255 & (dat - prevByte)];
                prevByte = dat;
                //end compression help
                ++parloopi;
            }
        });
        int j = 960 * 540;
        /*int j = 0;
        for(j = 0; j < 540*960; ++j) 
        {
            int prevByte = j == 0 ? 0 : data[j-1];
            //ensure we don't compress something that would be more efficient in the case above
            //compression help
            int dat = data[j];
            spanBuffer[j] = encodeLookupTable[255 & (dat - prevByte)];
            prevByte = dat;
            //end compression help
            ++j;
        }*/
        
        //output number of bytes in compressed block
        //we need this so we know how much data we should decompress later
        buffer[outputCounter++] = 127;
        int jCopy = j;
        j -= 128;
        while (j >= 255)
        {
            buffer[outputCounter++] = 255;
            j -= 255;
        }
        buffer[outputCounter++] = (byte)(j);
        j = jCopy;
        //compress the span and write to output buffer
        byte[] tmp = rotateBits(spanBuffer, j);
        outputCounter += encodeBitstream(tmp, buffer, outputCounter);
        //copy to output buffer;
        //change to array copy function
        byte[] output = new byte[outputCounter];
        //Array.Copy (buffer, 0, output, 0, outputCounter);
        Buffer.BlockCopy(buffer, 0, output, 0, outputCounter);
        return output;
    }

    public static byte[] compressFrame(byte[] data, bool writeToSource = false)
    {
        byte[] buffer = new byte[960 * 540 * 2];//array for processing data
        byte[] spanBuffer = new byte[960 * 540];
        int i = 0;
        int outputCounter = 0;
        //theoretically it is possible for the new array to be longer
        while (i < data.Length)
        {

            if (data[i] == 0)
            {
                //count zero bytes and append to output
                int j = 1;
                while (j + i < data.Length && data[j + i] == 0)
                {
                    ++j;
                }
                i += j;
                while (j >= (16384 + 64))
                {//64*256
                    buffer[outputCounter++] = 255 - 64;
                    buffer[outputCounter++] = 255;
                    j -= (16384 + 64);
                }
                if (j > 64)
                {
                    j -= 65;
                    buffer[outputCounter++] = (byte)(128 | (j >> 8));
                    buffer[outputCounter++] = (byte)j;
                }
                else if (i != 0)
                {
                    buffer[outputCounter++] = (byte)(128 + 64 + j - 1);
                }
            }
            else
            {
                //count non-zero bytes, compress and append to output
                int j = 0;
                int prevByte = 0;
                int limit = data.Length - i < 960 * 540 ? data.Length - i : 960 * 540;//apparently having this in the previous case increases runtime?
                while (j < limit && (data[j + i] != 0 || ((j + 1 < limit) && data[j + 1 + i] != 0)))
                {//ensure we don't compress something that would be more efficient in the case above
                    //compression help
                    int dat = data[j + i];
                    spanBuffer[j] = encodeLookupTable[255 & (dat - prevByte)];
                    prevByte = dat;
                    //end compression help
                    ++j;
                }
                i += j;
                //output number of bytes in compressed block
                //we need this so we know how much data we should decompress later
                if (j >= 128)
                {
                    buffer[outputCounter++] = 127;
                    int jCopy = j;
                    j -= 128;
                    while (j >= 255)
                    {
                        buffer[outputCounter++] = 255;
                        j -= 255;
                    }
                    buffer[outputCounter++] = (byte)(j);
                    j = jCopy;
                }
                else
                {
                    buffer[outputCounter++] = (byte)(j - 1);
                }
                //compress the span and write to output buffer
                byte[] tmp = rotateBits(spanBuffer, j);
                outputCounter += encodeBitstream(tmp, buffer, outputCounter);

            }
        }
        //copy to output buffer;
        //change to array copy function
        byte[] output = writeToSource ? data : new byte[outputCounter];
        //Array.Copy (buffer, 0, output, 0, outputCounter);
        Buffer.BlockCopy(buffer, 0, output, 0, outputCounter);
        return output;
    }

    public static byte[] decompressFrame(byte[] data, byte[] outputBuffer = null)
    {
        byte[] output = outputBuffer == null ? new byte[960 * 540] : outputBuffer;
        int outputCounter = 0;
        int len = data.Length;
        for (int i = 0; i < len; )
        {
            byte b = data[i++];
            if ((b & 128) != 0)
            {
                if ((b & 64) != 0)
                {
                    outputCounter += (b & 63);
                    outputCounter += 1;
                }
                else
                {
                    int x = (b & 63) << 8;
                    x |= data[i++];
                    x += 65;
                    outputCounter += x;
                }
            }
            else
            {
                int spanLen = b + 1;
                if (spanLen == 128)
                {
                    do
                    {
                        spanLen += data[i];
                    } while (data[i++] == 255);
                }
                i += decompressBitSpan(data, i, output, spanLen, outputCounter);
                outputCounter += spanLen;
            }
        }
        return output;
    }

    static byte[] bitRotLookupTable = makeLookupTable();
    static byte[] makeLookupTable()
    {
        byte[] table = new byte[256];
        for (int i = 0; i < 256; ++i)
        {
            int tmp = 0;
            tmp |= (i & 1);
            tmp |= (i & 2) << 3;
            tmp |= (i & 4);
            tmp |= (i & 8) << 3;
            tmp |= (i & 16) >> 3;
            tmp |= (i & 32);
            tmp |= (i & 64) >> 3;
            tmp |= (i & 128);
            table[i] = (byte)tmp;
        }
        return table;
    }

    //re-order the bits of an array so that the most significant bits are stored first, instead of in order in the bytes
    static byte[] rotateBits(byte[] b, int length)
    {
        byte[] output = new byte[length];
        int outputCounter = 0;
        int l2 = length - (length & 7);
        int offset = l2 >> 3;

        //rotate longest possible part of the list with length evenly divisible by 8
        //for (int i = 0; i < l2; i+= 8) {
        Parallel.For (0, offset, j => {
        //for (int j = 0; j < offset; ++j)
        //{
            int i = j << 3;
            //i <<= 3;
            int x0, x1, x2, x3, x4, x5, x6, x7;
            int y0, y1, y2, y3, y4, y5, y6;
            y0 = b[i];
            y1 = b[i + 1];
            y2 = b[i + 2];
            y3 = b[i + 3];
            y4 = b[i + 4];
            y5 = b[i + 5];
            y6 = b[i + 6];
            x7 = b[i + 7];

            //quickly invert bits by using bitmasking
            x0 = (y0 & 15) | (y1 << 4);
            x1 = (y2 & 15) | (y3 << 4);
            x2 = (y4 & 15) | (y5 << 4);
            x3 = (y6 & 15) | (x7 << 4);
            x4 = (y1 & 240) | (y0 >> 4);
            x5 = (y3 & 240) | (y2 >> 4);
            x6 = (y5 & 240) | (y4 >> 4);
            x7 = (x7 & 240) | (y6 >> 4);

            y0 = x0;
            y1 = x1;
            y2 = x2;
            y3 = x3;
            y4 = x4;
            y5 = x5;
            y6 = x6;

            x0 = (y0 & 51) | ((y1 & 51) << 2);
            x1 = (y2 & 51) | ((y3 & 51) << 2);
            x2 = (y1 & 204) | ((y0 & 204) >> 2);
            y3 = (y3 & 204) | ((y2 & 204) >> 2);//OBS!
            x4 = (y4 & 51) | ((y5 & 51) << 2);
            x5 = (y6 & 51) | ((x7 & 51) << 2);
            x6 = (y5 & 204) | ((y4 & 204) >> 2);
            x7 = (x7 & 204) | ((y6 & 204) >> 2);

            y0 = x0;
            y1 = x1;
            y2 = x2;
            //y3 = x3;
            y4 = x4;
            y5 = x5;
            y6 = x6;


            x0 = (y0 & 85) | ((y1 & 85) << 1);
            x1 = (y1 & 170) | ((y0 & 170) >> 1);
            x2 = (y2 & 85) | ((y3 & 85) << 1);
            x3 = (y3 & 170) | ((y2 & 170) >> 1);
            x4 = (y4 & 85) | ((y5 & 85) << 1);
            x5 = (y5 & 170) | ((y4 & 170) >> 1);
            x6 = (y6 & 85) | ((x7 & 85) << 1);
            x7 = (x7 & 170) | ((y6 & 170) >> 1);


            //adjust bytes with a lookup table and output the bytes
            int offsetTracker = i >> 3;
            output[offsetTracker] = bitRotLookupTable[x0];
            offsetTracker += offset;
            output[offsetTracker] = bitRotLookupTable[x1];
            offsetTracker += offset;
            output[offsetTracker] = bitRotLookupTable[x2];
            offsetTracker += offset;
            output[offsetTracker] = bitRotLookupTable[x3];
            offsetTracker += offset;
            output[offsetTracker] = bitRotLookupTable[x4];
            offsetTracker += offset;
            output[offsetTracker] = bitRotLookupTable[x5];
            offsetTracker += offset;
            output[offsetTracker] = bitRotLookupTable[x6];
            offsetTracker += offset;
            output[offsetTracker] = bitRotLookupTable[x7];

        });
        //};
        outputCounter += l2;

        //rotate the last part that isn't
        //TODO: determine whether to keep this or not
        //best case scenario it's a miniscule tradeoff of time for a miniscule improvement to compression ratio
        //worst case scenario it's a moderate (20%-30%) increase in compression time for an equaly miniscule improvement to compression ratio
        if (l2 < length)
        {
            outputCounter <<= 3;
            for (int bit = 0; bit < 8; ++bit)
            {
                //Console.WriteLine ("I'M HERE");
                for (int i = l2; i < length; ++i)
                {
                    output[outputCounter >> 3] |= (byte)(((b[i] >> bit) & 1) << (outputCounter & 7));
                    ++outputCounter;
                }
            }
        }
        //for (int i = l2; i < length; ++i) {
        //	output [i] = b [i];
        //}

        return output;
    }
    //take an array where the bits are stored most significant first, and turn it into bytes
    static byte[] rerotateBits(byte[] b)
    {
        byte[] output = new byte[b.Length];
        int inputCounter = 0;
        int length = b.Length;
        int l2 = length - (length & 7);

        //WARNING! Bit shifting magic ahead!
        int offset = l2 >> 3;
        //Parallel.For (0, offset, j => {
        for (int j = 0; j < offset; ++j)
        {
            int i = j << 3;
            int x0, x1, x2, x3, x4, x5, x6, x7;
            int y0, y1, y2, y3, y4, y5, y6;
            int offsetTracker = j;
            y0 = bitRotLookupTable[b[offsetTracker]]; offsetTracker += offset;
            y1 = bitRotLookupTable[b[offsetTracker]]; offsetTracker += offset;
            y2 = bitRotLookupTable[b[offsetTracker]]; offsetTracker += offset;
            y3 = bitRotLookupTable[b[offsetTracker]]; offsetTracker += offset;
            y4 = bitRotLookupTable[b[offsetTracker]]; offsetTracker += offset;
            y5 = bitRotLookupTable[b[offsetTracker]]; offsetTracker += offset;
            y6 = bitRotLookupTable[b[offsetTracker]]; offsetTracker += offset;
            x7 = bitRotLookupTable[b[offsetTracker]];

            //********************
            x0 = (y0 & 85) | ((y1 & 85) << 1);
            x1 = (y1 & 170) | ((y0 & 170) >> 1);
            x2 = (y2 & 85) | ((y3 & 85) << 1);
            x3 = (y3 & 170) | ((y2 & 170) >> 1);
            x4 = (y4 & 85) | ((y5 & 85) << 1);
            x5 = (y5 & 170) | ((y4 & 170) >> 1);
            x6 = (y6 & 85) | ((x7 & 85) << 1);
            x7 = (x7 & 170) | ((y6 & 170) >> 1);

            y0 = x0;
            y1 = x1;
            y2 = x2;
            y3 = x3;
            y4 = x4;
            y5 = x5;
            y6 = x6;

            x0 = (y0 & 51) | ((y2 & 51) << 2);
            x1 = (y2 & 204) | ((y0 & 204) >> 2);
            x2 = (y1 & 51) | ((y3 & 51) << 2);
            x3 = (y3 & 204) | ((y1 & 204) >> 2);
            x4 = (y4 & 51) | ((y6 & 51) << 2);
            x5 = (y6 & 204) | ((y4 & 204) >> 2);
            x6 = (y5 & 51) | ((x7 & 51) << 2);
            x7 = (x7 & 204) | ((y5 & 204) >> 2);

            y0 = x0;
            y1 = x1;
            y2 = x2;
            y3 = x3;
            y4 = x4;
            y5 = x5;
            y6 = x6;

            x0 = (y0 & 15) | ((y4 & 15) << 4);
            x1 = (y4 & 240) | ((y0 & 240) >> 4);
            x2 = (y1 & 15) | ((y5 & 15) << 4);
            x3 = (y5 & 240) | ((y1 & 240) >> 4);
            x4 = (y2 & 15) | ((y6 & 15) << 4);
            x5 = (y6 & 240) | ((y2 & 240) >> 4);
            x6 = (y3 & 15) | ((x7 & 15) << 4);
            x7 = (x7 & 240) | ((y3 & 240) >> 4);


            //output the bytes
            output[i] = (byte)x0;
            output[i + 1] = (byte)x1;
            output[i + 2] = (byte)x2;
            output[i + 3] = (byte)x3;
            output[i + 4] = (byte)x4;
            output[i + 5] = (byte)x5;
            output[i + 6] = (byte)x6;
            output[i + 7] = (byte)x7;

            //});
        };
        inputCounter = l2 << 3;

        //ASDFGH

        for (int bit = 0; bit < 8; ++bit)
        {
            for (int i = l2; i < length; ++i)
            {
                output[i] |= (byte)((((b[inputCounter >> 3]) >> (inputCounter & 7)) & 1) << bit);
                ++inputCounter;
            }
        }
        //for (int i = l2; i < length; ++i) {
        //	output [i] = b [i];
        //}


        return output;
    }

    //take a compressed thing and uncompress it
    static int decompressBitSpan(byte[] data, int inOffset, byte[] output, int outLength, int outOffset)
    {
        int compressedDataSize;
        byte[] tmp = decodeBitstream(data, inOffset, outLength, out compressedDataSize);
        tmp = rerotateBits(tmp);
        byte prevByte = 0;
        int len = tmp.Length;
        for (int i = 0; i < len; ++i)
        {
            int db = decodeLookupTable[tmp[i]];
            prevByte = (byte)(db + prevByte);
            output[i + outOffset] = prevByte;
        }

        return compressedDataSize;
    }

    //compress a series of bytes
    static int encodeBitstream(byte[] data, byte[] outputBuffer, int outOffset)
    {
        int bufferCounter = outOffset;
        int len = data.Length;
        for (int i = 0; i < len; ++i)
        {
            int spanLen = 1;
            switch (data[i])
            {
                case 0:
                    while (i + 1 < len && data[i + 1] == 0)
                    {
                        ++i;
                        ++spanLen;
                    }
                    while (spanLen > 128)
                    {
                        outputBuffer[bufferCounter++] = 127;
                        spanLen -= 128;
                    }
                    outputBuffer[bufferCounter++] = (byte)(spanLen - 1);
                    break;
                default:
                    int headerIndex = bufferCounter++;
                    outputBuffer[bufferCounter++] = data[i];
                    while (i + 2 < len && (data[i + 1] != 0 || (data[i + 2] != 0)) && spanLen < 128)
                    {
                        ++spanLen;
                        outputBuffer[bufferCounter++] = data[++i];
                    }
                    outputBuffer[headerIndex] = (byte)(128 | (spanLen - 1));
                    break;
            }
        }

        return bufferCounter - outOffset;//return length of compressed data

    }

    static byte[] decodeBitstream(byte[] data, int offset, int outLength, out int compressedDataSize)
    {

        byte[] output = new byte[outLength];
        int outputCounter = 0;
        int i = offset;
        while (outputCounter < outLength)
        {
            if ((data[i] & 128) == 0)
            {
                outputCounter += 1 + (data[i] & 127);
            }
            else
            {
                int span = 1 + (data[i] & 127);
                //for (int j = 0; j < span; ++j) {
                //output [outputCounter++] = data [++i];
                //}
                Buffer.BlockCopy(data, i + 1, output, outputCounter, span);
                outputCounter += span;
                i += span;
            }
            ++i;
        }
        compressedDataSize = i - offset;
        return output;
    }
}