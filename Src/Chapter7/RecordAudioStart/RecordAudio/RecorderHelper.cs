using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.IO;
using System.Diagnostics;


class RecorderHelper
{
    static byte[] buffer = new byte[4096];
    static bool _isRecording;


    public static bool IsRecording
    {
        get
        {
            return _isRecording;
        }
        set
        {
            _isRecording = value;
        }
    }

    public static void WriteWavFile(KinectAudioSource source, FileStream fileStream)
    {
        var size = 0; 
        //write wav header placeholder
        WriteWavHeader(fileStream, size);
        using (var audioStream = source.Start())
        {
            //chunk audio stream to file
            while (audioStream.Read(buffer, 0, buffer.Length) > 0 && _isRecording)
            {
                fileStream.Write(buffer, 0, buffer.Length);
                size += buffer.Length;

            }
        }

        //write real wav header
        long prePosition = fileStream.Position;
        fileStream.Seek(0, SeekOrigin.Begin);
        WriteWavHeader(fileStream, size);
        fileStream.Seek(prePosition, SeekOrigin.Begin);
        fileStream.Flush();
    }

    public static void WriteWavHeader(Stream stream, int dataLength)
    {
        using (MemoryStream memStream = new MemoryStream(64))
        {
            int cbFormat = 18;
            WAVEFORMATEX format = new WAVEFORMATEX()
            {
                wFormatTag = 1,
                nChannels = 1,
                nSamplesPerSec = 16000,
                nAvgBytesPerSec = 32000,
                nBlockAlign = 2,
                wBitsPerSample = 16,
                cbSize = 0
            };

            using (var bw = new BinaryWriter(memStream))
            {

                WriteString(memStream, "RIFF");
                bw.Write(dataLength + cbFormat + 4);
                WriteString(memStream, "WAVE");
                WriteString(memStream, "fmt ");
                bw.Write(cbFormat);

                bw.Write(format.wFormatTag);
                bw.Write(format.nChannels);
                bw.Write(format.nSamplesPerSec);
                bw.Write(format.nAvgBytesPerSec);
                bw.Write(format.nBlockAlign);
                bw.Write(format.wBitsPerSample);
                bw.Write(format.cbSize);

                WriteString(memStream, "data");
                bw.Write(dataLength);
                memStream.WriteTo(stream);
            }
        }
    }

    static void WriteString(Stream stream, string s)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(s);
        stream.Write(bytes, 0, bytes.Length);
    }

    struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }
}

