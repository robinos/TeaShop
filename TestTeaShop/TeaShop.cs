using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTeaShop
{
	public class TeaShop
	{
		private bool endSim = false;

		private static void Main(string[] args)
		{
			TeaShop teaShop = new TeaShop();
			Owner owner = new Owner(teaShop);

			while (teaShop.endSim == false)
			{
			}

			Console.WriteLine("\nPress any key.");
			Console.ReadKey();
		}

		public async Task ReceivedEnd()
		{
			endSim = true;
		}
	}
}
