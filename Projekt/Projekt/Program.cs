using System;
using System.Collections.Generic;
using System.Linq;

namespace Projekt
{
    class Program
    {
        private static int rozmiar;
        private static int[,] dane;
        private static int szansaMutacjaZamiana;
        private static int szansaMutacjaInwersja;

        static void Main(string[] args)
        {
            //PARAMETRY DO TESTOWANIA
            int liczbaOsobnikow = 250; //Liczba osobników
            int liczbaGeneracji = 20000; //Liczba osobników
            int naciskSelektywny = (int)(200 * 0.03); //K - selekcja turniejowa - ilość osobników w turnieju
            szansaMutacjaZamiana = 4;
            szansaMutacjaInwersja = 15;

            string[] danePlik = System.IO.File.ReadAllLines(@"SCIEZKA\nrw1379.txt");
            rozmiar = Convert.ToInt32(danePlik[0]); //Rozmiar tablicy - liczba miast
            dane = przygotowanieTablicy(danePlik, rozmiar); //Dane połączeń pobrane z pliku
            int[][] populacjaOsobnicy = new int[liczbaOsobnikow][]; //tablicaOsobnikow
            int[] populacjaOceny = new int[liczbaOsobnikow]; //tablicaOsobnikow

            //Stworzenie populacji początkowej i ocena osobników
            for (int i = 0; i < liczbaOsobnikow; i++)
            {
                int[] osobnik = Osobnik(rozmiar);
                populacjaOsobnicy[i] = osobnik;
                populacjaOceny[i] = Ocena(osobnik);
            }

            List<int[]> najlepsi = new List<int[]>();
            int[] naj1 = new int[rozmiar];
            najlepszyOsobnik(populacjaOsobnicy, populacjaOceny).CopyTo(naj1, 0);
            najlepsi.Add(naj1);

            for(int i = 0; i < liczbaGeneracji; i++)
            {
                populacjaOsobnicy = SelekcjaTurniejowaOsobnicy(populacjaOsobnicy, populacjaOceny, naciskSelektywny);
                populacjaOsobnicy = KrzyzowaniePopulacji(populacjaOsobnicy);
                populacjaOsobnicy = MutowanieZamianaPopulacji(populacjaOsobnicy, szansaMutacjaZamiana);
                populacjaOsobnicy = MutowanieInwersjaPopulacji(populacjaOsobnicy, szansaMutacjaInwersja);
                populacjaOceny = Oceny(populacjaOsobnicy);

                int[] naj = new int[rozmiar];
                najlepszyOsobnik(populacjaOsobnicy, populacjaOceny).CopyTo(naj,0);

                najlepsi.Add(naj);
            }

            Console.WriteLine("WYNIK:");
            int[] zwyciezca = najlepszyOsobnik(najlepsi);

            wyswietlOsobnika(zwyciezca);
            //sprawdzPoprawnosc(zwyciezca);
        }

        public static int[,] przygotowanieTablicy(string[] danePlik, int rozmiar)
        {
            int[,] daneMiasta = new int[rozmiar, rozmiar];

            for (int i = 0; i < rozmiar; i++)
            {
                string[] daneLine = danePlik[i + 1].Split(" ");

                for (int j = 0; j < rozmiar; j++)
                {
                    if (j < daneLine.Length-1)
                    {
                        daneMiasta[i, j] = int.Parse(daneLine[j]);
                    }
                    else
                    {
                        string[] daneLineCol = danePlik[j + 1].Split(" ");
                        daneMiasta[i, j] = int.Parse(daneLineCol[i]);
                    }
                }
            }
            return daneMiasta;
        }

        public static void wyswietlDane(int[,] dane, int rozmiar)
        {
            for (int i = 0; i < rozmiar; i++)
            {
                for (int j = 0; j < rozmiar; j++)
                {
                    Console.Write(dane[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        public static void wyswietlPopulacje(int[][] populacjaOsobnicy, int[] populacjaOceny) 
        {
            for (int i = 0; i < populacjaOceny.Length; i++)
            {
                Console.Write("Osobnik: ");
                for (int j = 0; j < populacjaOsobnicy[i].Length; j++)
                {
                    Console.Write(populacjaOsobnicy[i][j] + " ");
                }
                Console.WriteLine();
                Console.WriteLine("Ocena: " + populacjaOceny[i]);
            }
        }

        public static void wyswietlOsobnika(int[] osobnik)
        {
            for (int i = 0; i < osobnik.Length; i++)
            {
                if(i != 0)
                    Console.Write("-" + osobnik[i]);
                else
                    Console.Write(osobnik[i]);    
            }
            Console.Write(" " + Ocena(osobnik));
        }

        public static int[] najlepszyOsobnik(int[][] populacja, int[] oceny)
        {
            int min = oceny.Min();
            int min_index = Array.IndexOf(oceny, min);

            return populacja[min_index];
        }

        public static int[] najlepszyOsobnik(List<int[]> najlepsi)
        {
            int[][] populacja = najlepsi.ToArray();
            int[] oceny = Oceny(populacja);

            int min = oceny.Min();
            int min_index = Array.IndexOf(oceny, min);

            return populacja[min_index];
        }

        public static int[] Osobnik(int rozmiar)
        {
            Random rnd = new Random();
            int[] osobnik = new int[rozmiar];

            for (int i = 0; i < osobnik.Length; i++)
            {
                osobnik[i] = i;
            }
            osobnik = osobnik.OrderBy(x => rnd.Next()).ToArray();

            return osobnik;
        }

        public static void sprawdzPoprawnosc(int[] osobnik)
        {
            int[] sprawdzany = new int[rozmiar];
            osobnik.CopyTo(sprawdzany, 0);
            Array.Sort(sprawdzany);

            bool czyDobrze = true;

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("SPRAWDZAM POPRAWNOŚĆ");
            for (int i = 0; i < rozmiar; i++)
            {
                if(i==0)
                    Console.Write(sprawdzany[i]);
                else
                    Console.Write("-" + sprawdzany[i]);

                if (i != sprawdzany[i])
                {
                    czyDobrze = false;
                }
            }
            Console.WriteLine("");
            Console.WriteLine("Poprawność: " + czyDobrze);
        }

        //OCENA
        public static int Ocena(int[] osobnik)
        {
            int ocena = 0;

            for (int j = 0; j < rozmiar; j++)
            {
                if (j + 1 < rozmiar)
                    ocena += dane[osobnik[j], osobnik[j + 1]];
                else
                    ocena += dane[osobnik[j], osobnik[0]];

            }
            return ocena;
        }

        public static int[] Oceny(int[][] osobnicy)
        {
            int[] oceny = new int[osobnicy.Length];

            for (int i = 0; i < osobnicy.Length; i++)
            {
                oceny[i] = Ocena(osobnicy[i]);
            }
            return oceny;
        }

        //SELEKCJA
        public static int[][] SelekcjaTurniejowaOsobnicy(int[][] osobnicy, int[] oceny, int naciskSelektywny)
        {
            int[][] populacjaTurniej = new int[osobnicy.Length][];

            for (int i = 0; i < populacjaTurniej.Length; i++)
            {
                populacjaTurniej[i] = Turniej(osobnicy, oceny, naciskSelektywny, i);
            }
            return populacjaTurniej;
        }

        public static int[] Turniej(int[][] osobnicy, int[] oceny, int naciskSelektywny, int od)
        {
            Random rnd = new Random();

            int[] tabIndex = new int[naciskSelektywny];
            int[] tabOcena = new int[naciskSelektywny];

            for (int i = 0; i < naciskSelektywny; i++)
            {
                int index = i + od;
                if (index >= osobnicy.Length)
                    index = i;

                tabIndex[i] = index;
                tabOcena[i] = oceny[tabIndex[i]];
            }

            int winner = tabIndex[0];
            int winnerValue = tabOcena[0];

            for (int i = 0; i < naciskSelektywny; i++)
            {
                if (tabOcena[i] < winnerValue)
                {
                    winner = tabIndex[i];
                    winnerValue = oceny[winner];
                }
            }
            return osobnicy[winner];
        }

        //KRZYŻOWANIE
        public static int[][] KrzyzowaniePopulacji(int[][] populacja)
        {
            Random rnd = new Random();
            int[][] potomkowie = new int[populacja.Length][];

            for (int i = 0; i < populacja.Length; i++)
            {
                if ((i + 1) == populacja.Length)
                {
                    potomkowie[i] = populacja[i];
                }
                else
                    potomkowie[i] = Krzyzowanie(populacja[i], populacja[i + 1]);
            }

            return potomkowie;
        }

        public static int[] Krzyzowanie(int[] osobnik1, int[] osobnik2)
        {
            Random rnd = new Random();

            int[] potomek = new int[rozmiar];
            osobnik1.CopyTo(potomek,0);

            int[] pomocniczy = new int[rozmiar];

            int granica1 = rnd.Next(1, rozmiar - 2);
            int granica2 = rnd.Next(1, rozmiar - 2);
           
            for (int i = 0; i < potomek.Length; i++)
            {
                pomocniczy[potomek[i]] = i;
            }
            
            if (granica1 > granica2)
            {
                int temp = granica1;
                granica1 = granica2;
                granica2 = temp;
            }
            
            for (int i = granica1; i <= granica2; i++)
            {
                int wartosc = osobnik2[i];
                int t = potomek[pomocniczy[wartosc]]; 
                potomek[pomocniczy[wartosc]] = potomek[i];
                potomek[i] = t;
                t = pomocniczy[potomek[pomocniczy[wartosc]]];
                pomocniczy[potomek[pomocniczy[wartosc]]] = pomocniczy[potomek[i]];
                pomocniczy[potomek[i]] = t;
            }

            return potomek;
        }

        //ZAMIANA
        public static int[][] MutowanieZamianaPopulacji(int[][] populacja, int szansaMutacjaZamiana)
        {
            Random rnd = new Random();
            int[][] potomkowie = new int[populacja.Length][];

            for (int i = 0; i < populacja.Length; i++)
            {
                if (rnd.Next(0, rozmiar) <= szansaMutacjaZamiana)
                    potomkowie[i] = ZamianaMutacja(populacja[i]);
                else
                    potomkowie[i] = populacja[i];
            }

            return potomkowie;
        }

        public static int[] ZamianaMutacja(int[] osobnik)
        {
            Random rnd = new Random();
            int index1 = rnd.Next(0, rozmiar);
            int index2 = rnd.Next(0, rozmiar);

            while (index1==index2)
            {
                index2 = rnd.Next(0, rozmiar);
            }

            int wartosc1 = osobnik[index1];
            int wartosc2 = osobnik[index2];

            osobnik[index1] = wartosc2;
            osobnik[index2] = wartosc1;

            if (rnd.Next(0, rozmiar) <= szansaMutacjaZamiana)
                return ZamianaMutacja(osobnik);
            else
                return osobnik;
        }

        //INWERSJA
        public static int[][] MutowanieInwersjaPopulacji(int[][] populacja, int szansaMutacjaInwersja)
        {
            Random rnd = new Random();
            int[][] potomkowie = new int[populacja.Length][];

            for (int i = 0; i < populacja.Length; i++)
            {
                if (rnd.Next(0, rozmiar) <= szansaMutacjaInwersja)
                    potomkowie[i] = InwersjaMutacja(populacja[i]);
                else
                    potomkowie[i] = populacja[i];
            }

            return potomkowie;
        }

        public static int [] InwersjaMutacja(int[] osobnik)
        {
            Random rnd = new Random();
            int granica1 = rnd.Next(1, rozmiar - 2);
            int granica2 = rnd.Next(1, rozmiar - 2);

            if (granica1 > granica2)
            {
                int temp = granica1;
                granica1 = granica2;
                granica2 = temp;
            }

            Array.Reverse(osobnik, granica1, granica2-granica1);

            if (rnd.Next(0, rozmiar) <= szansaMutacjaInwersja)
                return InwersjaMutacja(osobnik);
            else
                return osobnik;
        }
    }
}
