using System;
using System.Collections.Generic;
using System.Text;
using System.Speech;
using System.Runtime.InteropServices;

namespace Speech_SDK
{
    public class Speech
    {
        [DllImport("winmm.dll")]   //引用winmm.dll     
        private static extern long waveOutSetVolume(UInt32 deviceID, UInt32 Volume);         
        /// <summary>
        /// 
        /// </summary>
        private static System.Speech.Synthesis.SpeechSynthesizer speech = null;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="language"></param>
        public static void Init(string language)
        {
            speech = new System.Speech.Synthesis.SpeechSynthesizer();
            if (language == null || language == "")
                //speech.SelectVoice("Microsoft Huihui Desktop");
                ;
            else
                ;
                //speech.SelectVoice(language);
                speech.SetOutputToDefaultAudioDevice();
        }
        /// <summary>
        /// 同步播放，在播放完成后返回
        /// 一旦执行不可终止
        /// </summary>
        /// <param name="text"></param>
        public static void Speak(string text)
        {
            if (speech == null)
                return;
            try
            {
                speech.Speak(text);
            }
            catch
            {
            }

        }
        /// <summary>
        /// 异步播放，立刻返回
        /// </summary>
        /// <param name="text"></param>
        public static void SpeakSyn(string text)
        {
            if (speech == null)
                return;
            try
            {
                speech.SpeakAsync(text);
            }
            catch
            {
            }
        }
        /// <summary>
        /// 清除正在播放的语音
        /// </summary>
        public static void ClearSpeakSyn()
        {
            if (speech == null)
                return;
            try
            {
                speech.SpeakAsyncCancelAll();
            }
            catch
            {
            }
        }

        public static void ChangeVolume(byte a)
        {
            if (speech == null)
                Init("");
            int v = a * 256 + a;
            try
            {
                    waveOutSetVolume((UInt32) 0, (UInt16)v);
            }
            catch
            {
            }
        }
        public static void Release() //释放
        {
            if (speech == null)
                return;
            speech.Dispose();
        }

        public static void Pause() //暂停
        {
            if (speech == null)
                Init("");
            speech.Pause();
        }
        public static void Resume()  //恢复 初始化
        {
            if (speech == null)
                Init("");
            
            speech.Resume();

        }
       
    }
}
