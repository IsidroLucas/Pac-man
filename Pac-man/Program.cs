using System;
using System.IO;

namespace Pac_man
{
    class Coor
    {
        // fila y columna (como propiedades)
        public int fil { get; set; }
        public int col { get; set; }
        public Coor(int _fil = 0, int _col = 0) { fil = _fil; col = _col; }

        // sobrecarga de + y - para hacer "desplazamientos" con coordenadas
        public static Coor operator +(Coor c1, Coor c2)
        {
            return new Coor(c1.fil + c2.fil, c1.col + c2.col);
        }

        public static Coor operator -(Coor c)
        {
            return new Coor(-c.fil, -c.col);
        }

        // sobrecarga de los operadores == y != para comparar coordenadas mediante fil y col
        public static bool operator ==(Coor c1, Coor c2)
        {
            return c1.fil == c2.fil && c1.col == c2.col;
        }
        public static bool operator !=(Coor c1, Coor c2)
        {
            //public bool Equals(Coor c){
            return !(c1 == c2);
        }

        public override bool Equals(object c)
        {
            return (c is Coor) && this == (Coor)c;
        }
    }

    class Tablero
    {
        // contenido de las casillas
        public enum Casilla { Libre, Muro, Comida, Vitamina, MuroCelda };
        // matriz de casillas (tablero)
        public Casilla[,] cas;
        // representacion de los personajes (pacman y fantasmas)
        public struct Personaje
        {
            public Coor pos, dir, // posicion y direccion actual
            ini; // posicion inicial (para fantasmas)
        }
        // vector de personajes, 0 es pacman y el resto fantasmas
        public Personaje[] pers;
        // colores para los personajes
        ConsoleColor[] colors = { ConsoleColor.DarkYellow, ConsoleColor.Red, ConsoleColor.Magenta, ConsoleColor.Cyan, ConsoleColor.DarkBlue };
        const int lapCarcelFantasmas = 3000; // retardo para quitar el muro a los fantasmas
        int lapFantasmas; // tiempo restante para quitar el muro
        int numComida; // numero de casillas restantes con comida o vitamina
        Random rnd; // generador de aleatorios
                    // flag para mensajes de depuracion en consola
        private bool Debug = true;

        public Tablero(string file) //Constructor
        {
            char[] c = LecturaDeFichero(file);

            int columnas = Columnas(c);

            int filas = Filas(c);


            //Creación del tablero
            cas = new Tablero.Casilla[filas, columnas];

            //Creacion del array de personajes
            pers = new Tablero.Personaje[5];

            for (int j = 0; j < pers.Length; j++)
            {
                pers[j].dir = new Coor();
                pers[j].pos = new Coor();
                pers[j].ini = new Coor();
            }

            //Direccion inicial de pac-man
            pers[0].dir.fil = 1;
            pers[0].dir.col = 0;

            //Este bucle recorre el array "Tablero" y lo rellena dependiendo del número del array auxiliar "c"
            //Si encuentra ";" disminuye "col" para hacer como si no existiera
            int i = 0;
            int fantasmas = 1;
            for (int row = 0; row < cas.GetLength(0); row++)
            {
                for (int col = 0; col < cas.GetLength(1); col++)
                {                                                               //Pasarlo a un método
                    if (c[i] == ';') { i++; col--; }
                    else if (c[i] == '0') { cas[row, col] = (Casilla)0; i++; }
                    else if (c[i] == '1') { cas[row, col] = (Casilla)1; i++; }
                    else if (c[i] == '2') { cas[row, col] = (Casilla)2; i++; }
                    else if (c[i] == '3') { cas[row, col] = (Casilla)3; i++; }
                    else if (c[i] == '4') { cas[row, col] = (Casilla)4; i++; }
                    else if (c[i] == '5' || c[i] == '6' || c[i] == '7' || c[i] == '8')
                    {
                        cas[row, col] = (Casilla)0;
                        pers[fantasmas].ini.fil = row;
                        pers[fantasmas].ini.col = col;
                        pers[fantasmas].pos.fil = pers[fantasmas].ini.fil;
                        pers[fantasmas].pos.col = pers[fantasmas].ini.col;
                        fantasmas++;
                        i++;
                    }
                    else if (c[i] == '9')
                    {
                        cas[row, col] = (Casilla)0;
                        pers[0].pos.fil = row;
                        pers[0].pos.col = col;
                        i++;
                    }
                }
            }

            //Inicialización del timer
            lapFantasmas = lapCarcelFantasmas;

            //Inicialización del random
            rnd = new Random(100);
        }

        static char[] LecturaDeFichero(string file)
        {
            StreamReader levels;

            levels = new StreamReader(file);

            //Variables del tamaño del nivel para la creación de "Tablero"
            int filas = 0;
            int columnas;

            //String auxiliar para la lectura del nive
            string end = "";
            file = "";
            //Booleano para el control del bucle
            bool finish = false;

            //El bucle va leyendo cada fila del archivo en "end" y la suma a "file", cuando end encuentre "null" querra decir que no hay más lineas en el archivo
            while (!finish)
            {
                end = levels.ReadLine();

                file += end;
                file += ";";

                if (end == null) { finish = true; }
            }

            levels.Close();

            //Creo un array auxiliar para calcular las columnas
            char[] c;

            c = file.ToCharArray();

            //Limpio el array de espacios vacios
            string limpio = "";
            int j = 0;
            while (j < c.Length)
            {
                if (c[j] == ' ') { j++; }
                else { limpio += c[j]; j++; }
            }

            c = limpio.ToCharArray();

            return c;
        }

        static int Filas(char[] c)
        {
            int filas = 0;

            int i = 0;
            bool tamaño = false;
            while (!tamaño)
            {
                if (c[i] == ';' && c[i + 1] == ';') { tamaño = true; filas++; }
                else if (c[i] == ';') { filas++; i++; }
                else { i++; }
            }
            return filas;
        }

        static int Columnas(char[] c)
        {
            //Este bucle recorre el array auxiliar "c" y suma uno a "columnas" y 
            //cuando encuentra el primer ";" significa que ese es el tamaño de columnas para crear "Tablero"
            int columnas = 0;

            bool tamaño = false;
            while (!tamaño)
            {
                if (c[columnas] == ';') { tamaño = true; }
                else { columnas++; }
            }


            return columnas;
        }

        public Casilla[,] GetTab()
        {
            return cas;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            string file = "level00.dat";
            Tablero tab;

            tab = new Tablero(file);

            while (true)
            {
                Dibuja(tab);

            }
            
            
        }

        static void Dibuja(Tablero tab)
        {
            for (int row = 0; row < tab.cas.GetLength(0); row++)
            {
                for (int col = 0; col < tab.cas.GetLength(1); col++)
                {
                    if (tab.cas[row, col] == (Tablero.Casilla)1)
                    {
                        Console.BackgroundColor = ConsoleColor.White; 
                        Console.SetCursorPosition(2 * col, row); 
                        Console.Write("  "); 
                    }
                    else if (tab.cas[row, col] == (Tablero.Casilla)2) 
                    { 
                        Console.BackgroundColor = ConsoleColor.DarkGray; 
                        Console.ForegroundColor = ConsoleColor.White; 
                        Console.SetCursorPosition(2 * col, row); 
                        Console.Write(".."); 
                    }
                    else if (tab.cas[row, col] == (Tablero.Casilla)3) 
                    { 
                        Console.BackgroundColor = ConsoleColor.DarkGray; 
                        Console.ForegroundColor = ConsoleColor.Green; 
                        Console.SetCursorPosition(2 * col, row); 
                        Console.Write("**"); 
                    }
                    else if (tab.cas[row, col] == (Tablero.Casilla)4) 
                    { 
                        Console.BackgroundColor = ConsoleColor.Blue; 
                        Console.SetCursorPosition(2 * col, row); 
                        Console.Write("  "); 
                    }
                    else if (tab.cas[row, col] == (Tablero.Casilla)0 && row == tab.pers[0].pos.fil && col == tab.pers[0].pos.col) 
                    { 
                        Console.BackgroundColor = ConsoleColor.Yellow; 
                        Console.SetCursorPosition(2 * col, row); 
                        Console.Write(">>"); 
                    }
                    else if (tab.cas[row, col] == (Tablero.Casilla)0 && row == tab.pers[1].pos.fil && col == tab.pers[1].pos.col)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.SetCursorPosition(2 * col, row);
                        Console.Write("ºº");
                    }
                    else if (tab.cas[row, col] == (Tablero.Casilla)0 && row == tab.pers[2].pos.fil && col == tab.pers[2].pos.col)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.SetCursorPosition(2 * col, row);
                        Console.Write("ºº");
                    }
                    else if (tab.cas[row, col] == (Tablero.Casilla)0 && row == tab.pers[3].pos.fil && col == tab.pers[3].pos.col)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.SetCursorPosition(2 * col, row);
                        Console.Write("ºº");
                    }
                    else if (tab.cas[row, col] == (Tablero.Casilla)0 && row == tab.pers[4].pos.fil && col == tab.pers[4].pos.col)
                    {
                        Console.BackgroundColor = ConsoleColor.Magenta;
                        Console.SetCursorPosition(2 * col, row);
                        Console.Write("ºº");
                    }
                    else if (tab.cas[row, col] == (Tablero.Casilla)0)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.SetCursorPosition(2 * col, row);
                        Console.Write("  ");
                    }
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }
        }




       
    }
}
