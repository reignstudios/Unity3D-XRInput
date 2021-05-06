using System.Text;

namespace VRstudios
{
	public static class OpenVR_Shared
    {
        // NOTE: capacity must match 'propertyText' for equals to work (in this case 256)
        public static StringBuilder propertyText = new StringBuilder(256);
        public static StringBuilder propertyText_Gamepad = new StringBuilder("gamepad", 256);
        public static StringBuilder propertyText_ViveController = new StringBuilder("vive_controller", 256);
        public static StringBuilder propertyText_IndexController = new StringBuilder("knuckles", 256);
        public static StringBuilder propertyText_Oculus = new StringBuilder("oculus", 256);// oculus_touch
        public static StringBuilder propertyText_WMR = new StringBuilder("holographic_controller", 256);
        public static StringBuilder propertyText_WMR_G2 = new StringBuilder("hpmotioncontroller", 256);
    }
}