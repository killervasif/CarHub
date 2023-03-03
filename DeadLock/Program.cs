class Program
{
    static void Main()
    {
        object Ana = new object();
        object Ata = new object();



        Thread AtadanSorus = new Thread(() =>
        {
            lock (Ana)
            {
                Thread.Sleep(1000);
                lock (Ata)
                {

                }
            }
        });

        Thread AnadanSorus = new Thread(() =>
        {
            lock (Ata)
            {
                Thread.Sleep(1000);
                lock (Ana)
                {
                    
                }
            }
        });

        AnadanSorus.Start();
        AtadanSorus.Start();
    }
}


//Atadan nese isteyirsen anaya gonderir
//Anadan nese isteyirsen ataya gonderir

//Monitorla resurslara mueyyen vaxt limiti qoyaraq sonda lock-lari buraxmaga mecbur ede bilerik
//sonra da o biri thread ise duser (ana-ata numunesiyle nece cixis yolu gosterim aglima gelmedi)