using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace L2.Models
{
    public class Drink : IComparable
    {
        public string Name { get; set; }
        public string Flavor { get; set; }
        public int Volume { get; set; }
        public float Price { get; set; }
        public string Manufacturer { get; set; }

        public int CompareTo(object obj)
        { return Name.CompareTo(((Drink)obj).Name); }

        public static Comparison<Drink> CompareByName = delegate (Drink d1, Drink d2)
        { return d1.CompareTo(d2); };

        public static Comparison<Drink> CompareByFlavor = delegate (Drink d1, Drink d2)
        { return d1.CompareTo(d2); };

        public static Comparison<Drink> CompareByVolume = delegate (Drink d1, Drink d2)
        { return d1.Volume < d2.Volume ? -1 : d1.Volume > d2.Volume ? 1 : 0; };

        public static Comparison<Drink> CompareByPrice = delegate (Drink d1, Drink d2)
        { return d1.Price < d2.Price ? -1 : d1.Price > d2.Price ? 1 : 0; };

        public static Comparison<Drink> CompareByManufacturer = delegate (Drink d1, Drink d2)
        { return d1.CompareTo(d2); };
    }
}
