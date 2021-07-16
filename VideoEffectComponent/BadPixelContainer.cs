using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoEffectComponent
{
    public static class BadPixelContainer
    {
        static List<int> badPixelList;
        static BadPixelContainer()
        {
            badPixelList = new List<int>();
        }
        public static void RecordBadPixel(int a)
        {
            badPixelList.Add(a);
        }
        public static int CountBadPixel()
        {
            return badPixelList.Count;
        }
        public static IList<int> GetBadPixels()
        {
            return badPixelList;
        }
    }
}
