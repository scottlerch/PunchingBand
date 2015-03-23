using SharpDX.IO;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace PunchingBand
{
    public class SoundEffect
    {
        private readonly AudioBuffer buffer;
        private readonly SoundStream stream;
        private readonly SourceVoice[] sourceVoices;
        private int index;

        public SoundEffect(string path, int poolSize = 5)
        {
            var xAudio = new XAudio2();
            var masteringVoice = new MasteringVoice(xAudio);
            var nativeFileStream = new NativeFileStream(path, NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);

            stream = new SoundStream(nativeFileStream);
            buffer = new AudioBuffer
            {
                Stream = stream.ToDataStream(),
                AudioBytes = (int)stream.Length,
                Flags = BufferFlags.EndOfStream
            };

            sourceVoices = new SourceVoice[poolSize];

            for (int i = 0; i < sourceVoices.Length; i++)
            {
                sourceVoices[i] = new SourceVoice(xAudio, stream.Format, true);
                sourceVoices[i].SetVolume(0);
            }
        }

        public void Play(double volume = 1.0)
        {
            var sourceVoice = sourceVoices[index];
            index = (index + 1)%sourceVoices.Length;

            sourceVoice.Stop();
            sourceVoice.FlushSourceBuffers();
            sourceVoice.SetVolume((float)volume);
            sourceVoice.SubmitSourceBuffer(buffer, stream.DecodedPacketsInfo);
            sourceVoice.Start();
        }
    }
}
