using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTeaShop
{
	public class Customer
	{
		private int id;
		private int numTeaCups;
		private bool hasHurriedOrder;
		private int normalDrinkDelay = 8000;
		private int hurriedDrinkDelay = 2000;
		private bool isLastOrders = false;
		private bool isClosed = false;
		private Owner owner;
		//public EventHandler goodbye;
		private int cupsWanted = 0;
		private int cupsDrank = 0;
		private bool isOrdering = false;

		public int ID
		{
			get { return id; }
		}

		/// <summary>
		/// Konstruktör för en Customer
		/// </summary>
		/// <param name="id">Customer ID</param>
		/// <param name="numTeaCups">mängd av önskade tekoppar</param>
		/// <param name="hasHurriedOrder">om kunden vill ta snabba beställningar i slutet</param>
		/// <param name="owner">Owner (ägare) objektet</param>
		public Customer(int id, int numTeaCups, bool hasHurriedOrder, Owner owner)
		{
			this.id = id;
			this.numTeaCups = numTeaCups;
			cupsWanted = numTeaCups;
			this.hasHurriedOrder = hasHurriedOrder;
			this.owner = owner;

			Console.WriteLine("\nCustomer " + id + " enters and greets the Owner.\n");

			RunCustomer();
		}

		/// <summary>
		/// RunCustomer kör kundens beteende. Om kunden vill ha fler koppar
		/// och affären är inte stängd eller nära stängningsdags, tar kunden
		/// lite tid på sig, beställer och sedan väntar på beställningen.
		/// Om affären är stängd eller kunden har fått alla koppar, lämnar
		/// kunden affären.
		/// </summary>
		/// <returns>Task (tom)</returns>
		private async Task RunCustomer()
		{
			while(numTeaCups > 0 && !isLastOrders && !isClosed)
			{
				Pause();
				Task order = OrderTea();
				await order;
				Task.WaitAll(order);
			}

			while (numTeaCups > 0 && isLastOrders && !isClosed && hasHurriedOrder)
			{
				Task hurriedOrder = CustomerLastOrders();
				await hurriedOrder;
				Task.WaitAll(hurriedOrder);
			}

			if (numTeaCups <= 0 || isClosed)
				await SendGoodbye();
		}

		/// <summary>
		/// En pause på 2 sekunder.
		/// </summary>
		public void Pause()
		{
			Task.Delay(2000);
		}

		/// <summary>
		/// OrderTea utför en tebeställning för kunden. 
		/// </summary>
		/// <returns>Task (tom)</returns>
		private async Task OrderTea()
		{
			isOrdering = true;
			Console.WriteLine("\nCustomer " + id + " orders tea.\n");

			Task receieveOrder = owner.TakeOrder(id);
			//Gör beställning
			await receieveOrder;

			//När beställningen är klar, kunden drycker te
			if (receieveOrder.IsCompleted)
			{
				numTeaCups--;
				Console.WriteLine("\nCustomer " + id + " receives tea.\n");

				//Kunden drycker te
				Task drinkTea = DrinkTea();
				await drinkTea;
				
				//När klar, rapporteras hur många fler kunden vill ha
				if(drinkTea.IsCompleted)
				{
					cupsDrank++;
					Console.WriteLine("\nCustomer " + id + " has finished " + cupsDrank + " tea cups out of " + cupsWanted + " wanted.\n");
					isOrdering = false;
				}
			}
		}

		/// <summary>
		/// DrinkTea drycker te snabbare eller långsammare beroende på om kunden är
		/// av den typen som vill drycker snabbt och beställer mer nära stängningsdags
		/// eller inte.
		/// </summary>
		/// <returns></returns>
		private async Task DrinkTea()
		{
			if (!isLastOrders)
			{
				Console.WriteLine("\nCustomer " + id + " is drinking tea.\n");
				await Task.Delay(normalDrinkDelay);
			}
			else if (isLastOrders && hasHurriedOrder)
			{
				Console.WriteLine("\n**Customer " + id + " is hurriedly gulping tea!**\n");
				await Task.Delay(hurriedDrinkDelay);
			}
			else
			{
				Console.WriteLine("\nCustomer " + id + " is drinking tea.\n");
				await Task.Delay(normalDrinkDelay);
			}
		}

		/// <summary>
		/// CustomerLastOrders körs när det är nära slutet av beställningstiden. Kunder
		/// som har hasHurriedOrder som falsk eller har fått alla koppar (eller om det är
		/// stängningdags) bara lämnar affären. Annars kunder med hasHurriedOrder som
		/// sann försöker ta en extra beställning så länge de fortfarande vill ha en
		/// kopp och det är inte slutet av beställningstiden.
		/// </summary>
		/// <returns></returns>
		private async Task CustomerLastOrders()
		{
			while (hasHurriedOrder && numTeaCups > 0 && isLastOrders && !isClosed)
			{
				Task hurriedOrder = OrderTea();
				await hurriedOrder;
				Task.WaitAll(hurriedOrder);
			}

			if (numTeaCups <= 0 || !hasHurriedOrder || isClosed)
				await SendGoodbye();
		}

		/// <summary>
		/// ReceiveLastCall tar emot anropning om sista tiden innan slutet
		/// av beställningstiden.
		/// </summary>
		/// <returns></returns>
		public async Task ReceiveLastCall()
		{
			isLastOrders = true;
			await CustomerLastOrders();
		}

		/// <summary>
		/// ReceiveStopServing tar emot slutet av beställningstiden.
		/// </summary>
		/// <returns></returns>
		public async Task ReceiveStopServing()
		{
			isClosed = true;
			if(!isOrdering)
				await SendGoodbye();
		}

		/// <summary>
		/// SendGoodbye körs när kunden lämnar affären och säger hej då. Kunden rapportera om
		/// den fick alla önskade koppar eller inte.
		/// </summary>
		/// <returns></returns>
		private async Task SendGoodbye()
		{
			if(numTeaCups > 0)
				Console.WriteLine("\nCustomer " + id + " waves goodbye but still wanted " + numTeaCups + " cups.\n");
			else
				Console.WriteLine("\nCustomer " + id + " waves goodbye.\n");

			await owner.ReceiveGoodbye(this);
		}
	}
}
