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

		public Customer(int id, int numTeaCups, bool hasHurriedOrder, Owner owner)
		{
			this.id = id;
			this.numTeaCups = numTeaCups;
			cupsWanted = numTeaCups;
			this.hasHurriedOrder = hasHurriedOrder;
			this.owner = owner;

			Console.WriteLine("\nCustomer " + id + " enters and greets the Owner.\n");

			//owner.lastCall += ReceiveLastCall;
			//owner.stopServing += ReceiveStopServing;
			//goodbye += owner.ReceiveGoodbye;

			RunCustomer();
		}

		private async Task RunCustomer()
		{
			while(numTeaCups > 0 && !isLastOrders && !isClosed)
			{
				Pause();
				Task order = OrderTea();
				await order;
				Task.WaitAll(order);
			}

			if (numTeaCups <= 0 || isClosed)
				await SendGoodbye();
		}

		public void Pause()
		{
			Task.Delay(2000);
		}

		private async Task OrderTea()
		{
			isOrdering = true;
			Console.WriteLine("\nCustomer " + id + " orders tea.\n");
			Task receieveOrder = owner.TakeOrder(id);
			await receieveOrder;
			if (receieveOrder.IsCompleted)
			{
				numTeaCups--;
				Console.WriteLine("\nCustomer " + id + " receives tea.\n");
				Task drinkTea = DrinkTea();
				await drinkTea;
				if(drinkTea.IsCompleted)
				{
					cupsDrank++;
					Console.WriteLine("\nCustomer " + id + " has finished " + cupsDrank + " tea cups out of " + cupsWanted + " wanted.\n");
					isOrdering = false;
				}
			}
		}

		private async Task DrinkTea()
		{
			if (!isLastOrders)
			{
				Console.WriteLine("\nCustomer " + id + " is drinking tea.\n");
				await Task.Delay(normalDrinkDelay);
			}
			else if (isLastOrders && hasHurriedOrder)
			{
				Console.WriteLine("\nCustomer " + id + " is hurriedly gulping tea!\n");
				await Task.Delay(hurriedDrinkDelay);
			}
			else
			{
				Console.WriteLine("\nCustomer " + id + " is drinking tea.\n");
				await Task.Delay(normalDrinkDelay);
			}
		}

		private async Task CustomerLastOrders()
		{
			while (hasHurriedOrder && numTeaCups > 0 && isLastOrders && !isClosed)
			{
				Task order = OrderTea();
				await order;
				Task.WaitAll(order);
			}

			if (numTeaCups <= 0 || !hasHurriedOrder || isClosed)
				await SendGoodbye();
		}

		public async Task ReceiveLastCall()
		{
			isLastOrders = true;
			await CustomerLastOrders();
		}

		public async Task ReceiveStopServing()
		{
			isClosed = true;
			if(!isOrdering)
				await SendGoodbye();
		}

		private async Task SendGoodbye()
		{
			//owner.lastCall -= ReceiveLastCall;
			//owner.stopServing -= ReceiveStopServing;

			if(numTeaCups > 0)
				Console.WriteLine("\nCustomer " + id + " waves goodbye but still wanted " + numTeaCups + " cups.\n");
			else
				Console.WriteLine("\nCustomer " + id + " waves goodbye.\n");

			await owner.ReceiveGoodbye(this);

			//goodbye(this, new GoodbyeEventArgs(this));
		}
	}
}
