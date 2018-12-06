using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace ProcessServer
{
    class Program
    {

        private static double _width = 772; // ширина поля с фигурами
        private static double _height = 327; // высота поля с фигурами

        static void Main(string[] args)
        {

            if (args.Length == 2) // передача ширины и высоты поля с фигурами (если есть)
            {
                _width = double.Parse(args[0]);

                _height = double.Parse(args[1]);
            }

            Calculating();
        }

        private static void Calculating() // вычисление координат
        {

            Random random = new Random();

            int v = 2;
            double x = random.Next(20, 290);
            double y = random.Next(20, 240);
            double a = random.Next(0, 361);

            double vx = v * Math.Cos(a);
            double vy = v * Math.Sin(a);

            while (true)
            {
                x += vx;
                y += vy;

                if (x < 10)
                {
                    vx = -vx;
                }

                if (x > _width - 10)
                {
                    vx = -vx;
                }

                if (y < 10)
                {
                    vy = -vy;
                }

                if (y > _height - 10)
                {
                    vy = -vy;
                }

                char[] message = (x.ToString() + "|" + y.ToString() + "|" + vx.ToString() + "|" + vy.ToString()).ToCharArray();

                int size = message.Length;

                MemoryMappedFile sharedMemory = MemoryMappedFile.CreateOrOpen("MemoryFile", size * 2 + 4);

                using (MemoryMappedViewAccessor writer = sharedMemory.CreateViewAccessor(0, size * 2 + 4)) // запись в shared memory
                {
                    writer.Write(0, size);

                    writer.WriteArray(4, message, 0, message.Length);
                }

                Thread.Sleep(10);
            }
        }
    }
}
