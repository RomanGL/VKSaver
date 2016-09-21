using System.Linq;
using Windows.Data.Xml.Dom;

namespace VKSaver.Core.Services.Common
{
    internal static class ToastAudioHelper
    {
        public static void SetSuccessAudio(XmlDocument toastDoc)
        {
            SetToastAudio(toastDoc, AppConstants.SUCCESS_TOAST_SOUND);
        }

        public static void SetFailAudio(XmlDocument toastDoc)
        {
            SetToastAudio(toastDoc, AppConstants.FAIL_TOAST_SOUND);
        }

        private static void SetToastAudio(XmlDocument toastDoc, string audioPath)
        {
            var audioNode = toastDoc.GetElementsByTagName("audio").First();
            audioNode.Attributes[0].NodeValue = audioPath;
        }
    }
}
