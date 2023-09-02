using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using AquaModelLibrary.AquaStructs;
using FalseIdola.Audio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Reloaded.Memory.Streams;
using SonicAudioLib.Archives;
using VGAudio.Containers.Wave;
using Zamboni;

namespace FalseIdola
{
    public class AudioPlayer
    {
        public WaveOutEvent outputDevice;
        public IWaveProvider audioFile;
        public FileType type = 0;

        public AudioPlayer()
        {
            //Test
            var strm = new MemoryStream(File.ReadAllBytes(@"A:\PS4_NGS_PreRetem\datareboot\sound\music\adaptive\mu_11_bgm02_defense_1.ice"));
            var fVarIce = IceFile.LoadIceFile(strm);
            MusicFileReboot mus;
            strm.Dispose();

            List<byte[]> files = new List<byte[]>(fVarIce.groupOneFiles);
            files.AddRange(fVarIce.groupTwoFiles);

            //Loop through files to get what we need
            foreach (byte[] ffile in files)
            {
                var name = IceFile.getFileName(ffile).ToLower();
                if (name.EndsWith(".mus"))
                {
                    using (var stream = new MemoryStream(ffile))
                    using (var sr = new BufferedStreamReader(stream, 8192))
                    {
                        File.WriteAllBytes("C:\\testfile.mus", ffile);
                        mus = new MusicFileReboot(sr);
                    }
                }
            }
            files.Clear();
            files = null;


            var cpkStream = new FileStream(@"A:\PS4_NGS_PreRetem\datareboot\sound\music\adaptive\mu_11_bgm02_defense_1.cpk", FileMode.Open);
            var cpk = new CriCpkArchive();
            cpk.Read(cpkStream);
            var file = cpk.GetFileByName(cpkStream, "B1-001_O-1_1M_B.hca");

            //var testHca = File.ReadAllBytes(@"C:\Users\Shadi\Downloads\PSO2_Audio_Tools\sonicaudiotools\04_player_rod\00000.hca");
            type = AudioInfo.GetFileTypeFromName("test.hca");
            var hcaFile = Audio.IO.OpenFile(file, type);
            //var bytes = AudioInfo.Containers[type].GetWriter().GetFile(audioFile, GetConfiguration(SelectedFileType));
            var bytes = AudioInfo.Containers[FileType.Wave].GetWriter().GetFile(hcaFile, new WaveConfiguration() { Codec = WaveCodec.Pcm16Bit });
            //File.WriteAllBytes(@"C:\Users\Shadi\Downloads\PSO2_Audio_Tools\sonicaudiotools\04_player_rod\00000.wav", bytes);
            //audioFile = new AudioFileReader(@"A:\AC6 OST\ACVI_FOR_SOUNDTRACK\47_Stargazer.mp3");
            //IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(bytes), new WaveFormat(48000, 16, 1));
            IWaveProvider provider = new WaveFileReader(new MemoryStream(bytes));
            audioFile = provider;
            outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();
            cpkStream.Dispose();
        }
    }
}
