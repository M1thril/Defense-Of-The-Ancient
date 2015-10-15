
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace TowerDef
{
    /// <summary>
    /// 
    /// </summary>
    public class Audio
    {
        [DllImport("winmm.dll")]
        private static extern int mciSendString
            (
                string lpstrCommand,
                string lpstrReturnString,
                int uReturnLength,
                int hwndCallback
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName
            (
                [MarshalAs(UnmanagedType.LPTStr)]    string path,
                 [MarshalAs(UnmanagedType.LPTStr)]    StringBuilder shortPath,
                 int shortPathLength
            );

        public Audio()
        {

        }

        public void Play(string FileName)
        {
            StringBuilder shortPathTemp = new StringBuilder(255);
            int result = GetShortPathName(FileName, shortPathTemp, shortPathTemp.Capacity);
            string ShortPath = shortPathTemp.ToString();

            mciSendString("open " + ShortPath + " alias song", "", 0, 0);
            mciSendString("play song", "", 0, 0);
        }

        public void Stop()
        {
            mciSendString("stop song", "", 0, 0);
        }

        public void Pause()
        {
            mciSendString("pause song", "", 0, 0);
        }

        public void Close()
        {
            mciSendString("close song", "", 0, 0);
        }
    }
}
