using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriftRacer
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new DriftRacer())
            {
                game.Parameters.TrackObjects = true;
                game.Run(args);
            }
        }
    }
}
