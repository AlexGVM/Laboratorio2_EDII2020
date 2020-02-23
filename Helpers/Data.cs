using System;
using System.Collections.Generic;
using L2.Models;

namespace L2.Helpers
{
    public class Data
    {
        private static Data _instance = null;

        public static Data Instance
        {
            get
            {
                if (_instance == null) _instance = new Data();
                return _instance;
            }
        }
        
        public List<Drink> Items = new List<Drink>();
        public BStarTree<Drink> Tree = new BStarTree<Drink>(5);
    }
}
