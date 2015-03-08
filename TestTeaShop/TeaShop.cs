using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTeaShop
{
	/// <summary>
	/// TeaShop är en simulation av en te affär med async/await metoder.
	/// TeaShop klassen startar programmet och skapar ägare (Owner) klassen.
	/// Det körs till async metoden EndProgram anropas och endSim blir
	/// sann.
	/// Tiden börjar 5 minuter till 15.
	/// Affären öppnar klockan 15:00.
	/// Last call (sista beställningar) händer klockan 15:30.
	/// Ägaren slutar tar emot beställningar klockan 16:45.
	/// Stängning är klockan 17:00 (fast ägaren väntar för att kunder att lämna).
	/// </summary>
	public class TeaShop
	{
		private bool endSim = false;

		private static void Main(string[] args)
		{
			TeaShop teaShop = new TeaShop();

			//Skapar owner objekt som börjar simulationen
			Owner owner = new Owner(teaShop);

			while (teaShop.endSim == false)
			{
			}

			Console.WriteLine("\nPress any key.");
			Console.ReadKey();
		}

		public async Task EndProgram()
		{
			endSim = true;
		}
	}
}
