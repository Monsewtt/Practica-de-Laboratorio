using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Practica_de_Laboratorio
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Ciudadano
    {
        public int Id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string Nombre;

        public int Edad;

        // Constructor clásico
        public Ciudadano(int id, string nombre, int edad)
        {
            Id = id;
            Nombre = nombre;
            Edad = edad;
        }

        // Calculamos el tamaño exacto en bytes
        public static int Size => Marshal.SizeOf(typeof(Ciudadano));
    } 
    public class GestorArchivos
    {
        // Esta variable ya la pueden ver todos los que vivan en la Casita 2
        private readonly string _path = "datos_ciudadanos.dat";

        // Método para guardar calculando el Offset
        public void GuardarCiudadano(Ciudadano c, int posicion)
        {
            using (FileStream fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                long offset = (long)posicion * Ciudadano.Size;
                fs.Seek(offset, SeekOrigin.Begin);

                using (BinaryWriter writer = new BinaryWriter(fs, Encoding.UTF8, true))
                {
                    writer.Write(c.Id);
                    writer.Write(c.Nombre.PadRight(50).ToCharArray());
                    writer.Write(c.Edad);
                }
            }
        }
        public Ciudadano? LeerCiudadano(int posicion)
        {
            if (!File.Exists(_path)) return null;

            using (FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {
                long offset = (long)posicion * Ciudadano.Size;

                if (offset >= fs.Length) return null;

                fs.Seek(offset, SeekOrigin.Begin);
                using (BinaryReader reader = new BinaryReader(fs, Encoding.UTF8, true))
                {
                    int id = reader.ReadInt32();
                    string nombre = new string(reader.ReadChars(50)).TrimEnd();
                    int edad = reader.ReadInt32();

                    return new Ciudadano(id, nombre, edad);
                }
            }
        }
    }
}