using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


delegate bool Wybierz<T>(T t);

namespace projekt
{
    [Flags]
    enum RodzajSilnikaFlag { BENZYNOWY = 1, DIESEL = 2, ELEKTRYCZNY = 4 }
    [Serializable]
    abstract class Samochod : ICloneable, IComparable<Samochod>
    {


        public string Marka { get; set; }
        public string Model { get; set; }
        public RodzajSilnikaFlag RodzajSilnika { get; set; }
        public int RokProdukcji { get; set; }

        protected Samochod(string marka, string model, int rokProdukcji)
        {
            Marka = marka;
            Model = model;
            RokProdukcji = rokProdukcji;
        }

        public abstract object Clone();

        public virtual void WyswietlInformacje()
        {
            Console.WriteLine($"Marka:{Marka}, Model:{Model}, RokProdukcji: {RokProdukcji} RodzajSilnika: {RodzajSilnika}");
        }
        public int CompareTo([AllowNull] Samochod other)
        {
            return this.CompareTo(other);
        }
        public class SamochodPoModeluComparer : IComparer<Samochod>
        {
            public int Compare([AllowNull] Samochod x, [AllowNull] Samochod y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }
                return x.Model.CompareTo(y.Model);
            }
        }
    }





    class SalonSamochodowy
    {
        private List<Samochod> samochody;
        public SalonSamochodowy()
        {
            samochody = new List<Samochod>();
        }

        public void DodajSamochod(Samochod samochod)
        {
            samochody.Add(samochod);
        }
        public void UsunSamochod(Samochod samochod)
        {
            samochody.Remove(samochod);
        }
        public void ZapiszSalonDoPliku(string nazwaPliku)
        {
            FileStream fs = null;

            BinaryFormatter formatter = new BinaryFormatter(); //serializacja binarna
            try
            {
                fs = new FileStream(nazwaPliku, FileMode.OpenOrCreate, FileAccess.Write);
                formatter.Serialize(fs, samochody);
                Console.WriteLine("\tSalon samochodowy został zapisany do pliku.");
            }
            catch
            {
                throw new ArgumentException("Nie udało się zapisać salonu do pliku");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }
        public static SalonSamochodowy WczytajSalonZPliku(string nazwaPliku)
        {
            FileStream fs = null;
            BinaryFormatter formatter = new BinaryFormatter(); //deserializacja binarna
            try
            {
                fs = new FileStream(nazwaPliku, FileMode.OpenOrCreate, FileAccess.Read);
                var e = (List<Samochod>)formatter.Deserialize(fs);
                SalonSamochodowy nowy = new SalonSamochodowy();
                for (int i = 0; i < e.Count; i++)
                {
                    nowy.DodajSamochod(e[i]);
                }
                Console.WriteLine("\tSalon samochodowy został wczytany z pliku.");
                return nowy;
            }
            catch
            {
                throw new ArgumentException("Nie udało się wczytać salonu z pliku");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }

        }
        public void WyswietlPosortowane()
        {
            List<Samochod> kopia = new List<Samochod>();
            for (int i = 0; i < samochody.Count; i++)
            {
                if (samochody[i] != null) //klonowanie listy samochody aby pozniej wyswietlic posortowane
                {
                    if (samochody[i] is SamochodElektryczny)
                    {
                        kopia.Add((SamochodElektryczny)samochody[i].Clone());
                    }
                    else if (samochody[i] is SamochodSpalinowy)
                    {
                        kopia.Add((SamochodSpalinowy)samochody[i].Clone());
                    }
                    else if (samochody[i] is SamochodHybrydowy)
                    {
                        kopia.Add((SamochodHybrydowy)samochody[i].Clone());
                    }
                }
            }
            kopia.Sort((x, y) =>
            {
                int ret = x.RokProdukcji.CompareTo(y.RokProdukcji);
                if (ret == 0 && x is SamochodElektryczny && y is SamochodElektryczny)
                {
                    ret = (y as SamochodElektryczny).PojemnoscBaterii.CompareTo((x as SamochodElektryczny).PojemnoscBaterii);
                }
                return ret;
            });   //sortowanie najpierw po roku produkcji potem dla elektrycznych po pojemosci baterii
            WyswietlSamochody(kopia);

        }
        public void WyswietlPosortowane(IComparer<Samochod> comparer)
        {
            List<Samochod> kopia = new List<Samochod>();
            for (int i = 0; i < samochody.Count; i++) //klonowanie listy samochody aby pozniej wyswietlic posortowane
            {
                if (samochody[i] != null)
                {
                    if (samochody[i] is SamochodElektryczny)
                    {
                        kopia.Add((SamochodElektryczny)samochody[i].Clone());
                    }
                    else if (samochody[i] is SamochodSpalinowy)
                    {
                        kopia.Add((SamochodSpalinowy)samochody[i].Clone());
                    }
                    else if (samochody[i] is SamochodHybrydowy)
                    {
                        kopia.Add((SamochodHybrydowy)samochody[i].Clone());
                    }
                }
            }
            kopia.Sort(comparer); //korzystam z comparera dzieki czemu sortuje po modelu samochodu
            WyswietlSamochody(kopia);
        }
        public void WyswietlSamochody()
        {
            Console.WriteLine("");
            for (int i = 0; i < samochody.Count; i++)
            {
                if (samochody[i] != null)
                {
                    samochody[i].WyswietlInformacje();
                }
            }
        }
        private void WyswietlSamochody(List<Samochod> lista)
        {
            Console.WriteLine("");
            for (int i = 0; i < lista.Count; i++)
            {
                if (lista[i] != null)
                {
                    lista[i].WyswietlInformacje();
                }
            }
        }
        public void WyswietlSamochody(Wybierz<Samochod> w)
        {

            for (int i = 0; i < samochody.Count; i++)
            {
                if (w.Invoke(samochody[i]) == true)
                {
                    Console.WriteLine("");
                    samochody[i].WyswietlInformacje();
                }
            }
        }

    }



    [Serializable]
    class SamochodElektryczny : Samochod
    {


        public int PojemnoscBaterii { get; set; }
        public SamochodElektryczny(string marka, string model, int rokProdukcji, int pojemnoscBaterii) : base(marka, model, rokProdukcji)
        {
            this.PojemnoscBaterii = pojemnoscBaterii;

        }

        public new int CompareTo(Samochod other)
        {
            return this.CompareTo(other);
        }
        public override void WyswietlInformacje()
        {
            Console.WriteLine($"Marka: {Marka}, Model: {Model}, RokProdukcji: {RokProdukcji} Pojemność baterii: {PojemnoscBaterii} kWh");
        }

        public override object Clone()
        {
            return new SamochodElektryczny(this.Marka, this.Model, this.RokProdukcji, this.PojemnoscBaterii);
        }
    }
    [Serializable]
    class SamochodHybrydowy : Samochod
    {
        public int PojemnoscBaterii { get; set; }
        public SamochodHybrydowy(string marka, string model, int rokProdukcji, RodzajSilnikaFlag rodzajSilnika, int pojemnoscBaterii) : base(marka, model, rokProdukcji)
        {
            this.RodzajSilnika = rodzajSilnika;
            this.PojemnoscBaterii = pojemnoscBaterii;
            if (rodzajSilnika != (RodzajSilnikaFlag.ELEKTRYCZNY | RodzajSilnikaFlag.DIESEL) && rodzajSilnika != (RodzajSilnikaFlag.ELEKTRYCZNY | RodzajSilnikaFlag.BENZYNOWY))
            {
                throw new ArgumentException("Samochód hybrydowy składa się z silnika elektrycznego oraz spalinowego!");
            }
        }
        public override object Clone()
        {
            return new SamochodHybrydowy(this.Marka, this.Model, this.RokProdukcji, this.RodzajSilnika, this.PojemnoscBaterii);
        }
        public override void WyswietlInformacje()
        {
            Console.WriteLine($"Marka: {Marka}, Model: {Model}, RokProdukcji: {RokProdukcji} Rodzaj silnika: {RodzajSilnika} Pojemność baterii: {PojemnoscBaterii} kWh");
        }
    }
    [Serializable]
    class SamochodSpalinowy : Samochod
    {
        public SamochodSpalinowy(string marka, string model, int rokProdukcji, RodzajSilnikaFlag rodzajSilnika) : base(marka, model, rokProdukcji)
        {
            this.RodzajSilnika = rodzajSilnika;
            if (rodzajSilnika == RodzajSilnikaFlag.ELEKTRYCZNY)
            {
                throw new ArgumentException("Samochody hybrydowe tworzymy jako hybrydowe, a nie spalinowe!");
            }
        }
        public override object Clone()
        {
            return new SamochodSpalinowy(this.Marka, this.Model, this.RokProdukcji, this.RodzajSilnika);
        }
        public override void WyswietlInformacje()
        {
            Console.WriteLine($"Marka: {Marka}, Model: {Model}, Rok produkcji: {RokProdukcji} Rodzaj silnika: {RodzajSilnika}");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            FileStream filestream = new FileStream("Wynik.txt", FileMode.Create);
            StreamWriter streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);

            Console.WriteLine("\t\t\t\tBBBBB     M     M    W       W");
            Console.WriteLine("\t\t\t\tB     B    MM   MM    W       W");
            Console.WriteLine("\t\t\t\tB     B    M M M M    W   W   W");
            Console.WriteLine("\t\t\t\tBBBBB      M  M  M    W W W W");
            Console.WriteLine("\t\t\t\tB     B    M     M    WW   WW");
            Console.WriteLine("\t\t\t\tB     B    M     M    W     W");
            Console.WriteLine("\t\t\t\tBBBBB      M     M    W     W");
            Console.WriteLine("\t\t\t\t\tSalon sprzedaży");

            SalonSamochodowy salon = new SalonSamochodowy();

            SamochodSpalinowy spalinowy1 = new SamochodSpalinowy("BMW", "M3", 2022, RodzajSilnikaFlag.BENZYNOWY);
            // Tworzenie i dodawanie 50 instancji samochodów do salonu
            salon.DodajSamochod(spalinowy1);
            salon.DodajSamochod(new SamochodSpalinowy("BMW", "M5", 2023, RodzajSilnikaFlag.DIESEL));
            salon.DodajSamochod(new SamochodElektryczny("BMW", "i3", 2022, 60));
            salon.DodajSamochod(new SamochodElektryczny("BMW", "i3", 2022, 70));
            salon.DodajSamochod(new SamochodElektryczny("BMW", "i3", 2022, 93));
            salon.DodajSamochod(new SamochodHybrydowy("BMW", "330e", 2023, RodzajSilnikaFlag.BENZYNOWY | RodzajSilnikaFlag.ELEKTRYCZNY, 40));
            salon.DodajSamochod(new SamochodSpalinowy("BMW", "X5", 2022, RodzajSilnikaFlag.DIESEL));
            salon.DodajSamochod(new SamochodSpalinowy("BMW", "X7", 2023, RodzajSilnikaFlag.BENZYNOWY));
            salon.DodajSamochod(new SamochodElektryczny("BMW", "i4", 2022, 70));
            salon.DodajSamochod(new SamochodHybrydowy("BMW", "530e", 2023, RodzajSilnikaFlag.BENZYNOWY | RodzajSilnikaFlag.ELEKTRYCZNY, 50));
            salon.DodajSamochod(new SamochodSpalinowy("BMW", "M2", 2022, RodzajSilnikaFlag.BENZYNOWY));
            salon.DodajSamochod(new SamochodSpalinowy("BMW", "M8", 2023, RodzajSilnikaFlag.DIESEL));
            salon.DodajSamochod(new SamochodElektryczny("BMW", "iX3", 2022, 65));
            salon.DodajSamochod(new SamochodHybrydowy("BMW", "745e", 2023, RodzajSilnikaFlag.BENZYNOWY | RodzajSilnikaFlag.ELEKTRYCZNY, 45));
            salon.DodajSamochod(new SamochodSpalinowy("BMW", "X3", 2022, RodzajSilnikaFlag.DIESEL));
            salon.DodajSamochod(new SamochodSpalinowy("BMW", "X6", 2023, RodzajSilnikaFlag.BENZYNOWY));
            salon.DodajSamochod(new SamochodElektryczny("BMW", "i8", 2022, 75));
            salon.DodajSamochod(new SamochodHybrydowy("BMW", "X1 xDrive25e", 2023, RodzajSilnikaFlag.BENZYNOWY | RodzajSilnikaFlag.ELEKTRYCZNY, 55));
            salon.DodajSamochod(new SamochodSpalinowy("BMW", "330i", 2022, RodzajSilnikaFlag.BENZYNOWY));
            salon.DodajSamochod(new SamochodSpalinowy("BMW", "430i", 2023, RodzajSilnikaFlag.DIESEL));
            salon.DodajSamochod(new SamochodElektryczny("BMW", "iX5", 2022, 80));
            salon.DodajSamochod(new SamochodHybrydowy("BMW", "X2 xDrive25e", 2023, RodzajSilnikaFlag.BENZYNOWY | RodzajSilnikaFlag.ELEKTRYCZNY, 60));

            Console.WriteLine("\tWszystkie samochody w salonie:");
            salon.WyswietlSamochody();

            // Testowanie serializacji i deserializacji
            string nazwaPliku = "salon.bin";
            salon.ZapiszSalonDoPliku(nazwaPliku);

            Console.WriteLine("\n\tUsuwanie samochodu z salonu...");
            salon.UsunSamochod(spalinowy1);

            Console.WriteLine("\n\tWszystkie samochody po usunięciu:");
            salon.WyswietlSamochody();

            SalonSamochodowy wczytanySalon = SalonSamochodowy.WczytajSalonZPliku(nazwaPliku);

            Console.WriteLine("\n\tWczytane samochody z pliku:");
            wczytanySalon.WyswietlSamochody();

            Console.WriteLine("\n\tWyświetlanie samochodów według poszukiwanego wzorca (delegat i funkcja anonimowa):");
            salon.WyswietlSamochody((s) => s.Model == "M3");
            salon.WyswietlSamochody((s) => s.Model == "M2");

            Console.WriteLine("\n\tWyświetlanie posortowanych po roku produkcji, a elektryczne następnie po pojemności baterii malejąco:");
            salon.WyswietlPosortowane();

            Console.WriteLine("\n\tWyświetlanie po modelu:");
            salon.WyswietlPosortowane(new Samochod.SamochodPoModeluComparer());

            Console.WriteLine("\n\tTest obsługi wyjątków:");
            ExceptionsTester(() => { new SamochodSpalinowy("BMW", "M5", 2023, RodzajSilnikaFlag.ELEKTRYCZNY); });
            ExceptionsTester(() => { new SamochodHybrydowy("BMW", "745e", 2023, RodzajSilnikaFlag.BENZYNOWY, 45); });
            ExceptionsTester(() => { new SamochodHybrydowy("BMW", "745e", 2023, RodzajSilnikaFlag.DIESEL, 45); });
            ExceptionsTester(() => { new SamochodHybrydowy("BMW", "745e", 2023, RodzajSilnikaFlag.ELEKTRYCZNY, 45); });

            Console.SetOut(System.IO.TextWriter.Null);
        }

        public delegate void TesterDelegate();

        public static void ExceptionsTester(TesterDelegate test)
        {
            try
            {
                test();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
