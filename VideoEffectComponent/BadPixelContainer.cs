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

        /// <summary>
        /// Записывает битые пиксели в динамический лист
        /// </summary>
        /// <param name="a">Номер битого пикселя</param>
        public static void RecordBadPixel(int a)
        {
            badPixelList.Add(a);
        }

        /// <summary>
        /// Возвращает количество битых пикселей
        /// </summary>
        /// <returns>количество битых пикселей</returns>
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
