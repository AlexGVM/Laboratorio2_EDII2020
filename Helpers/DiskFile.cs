using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using L2.Helpers;
using L2.Models;
using System.Text;


namespace L2.Helpers
{
    public class DiskFile
    {
        public static int FixedSizeText
        {
            get { return FixedSize; }

        }

        public static int FixedSize { get { return 127; } }
        public static void Archivo(int Root)
        {
            List<Drink> bebidas = new List<Drink>();


            string path = Environment.CurrentDirectory + "\\Archivo.txt";


            using (var archivo = new FileStream(path, FileMode.OpenOrCreate))
            {
                foreach (var item in bebidas)
                {
                    archivo.Write(ByteGenerator.ConvertToBytes(item.ToFixedLengthString()), 0, FixedSize);

                }
            }

            var buffer = new byte[FixedSize];
            using (var archivo1 = new FileStream(path, FileMode.OpenOrCreate))
            {

                archivo1.Seek((Root - 1) * FixedSize, SeekOrigin.Begin);
                archivo1.Read(buffer, 0, FixedSize);
            }

            var nodeString = ByteGenerator.ConvertToBytes(buffer);
            //Se divide para llenar el nodo
            var values = nodeString.Split('|');
            var bebidas2 = new Drink
            {

                Name = values[0].Trim(),
                Flavor = values[1].Trim(),
                Volume = int.Parse(values[2]),
                Price = float.Parse(values[3]),
                Manufacturer = values[4].Trim(),

            };

        }
    }
}
