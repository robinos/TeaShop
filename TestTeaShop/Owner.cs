using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTeaShop
{
	public class Owner
	{
		//public EventHandler lastCall;
		//public EventHandler stopServing;
		public EventHandler endSimulation;
		//public EventHandler<EventArgs> orders;
		private HashSet<Customer> customers;
		private Clock myClock;
		private bool isLastCall = false;
		private bool serving = false;
		private bool closingTime = false;
		private int tick = 0;
		private int customerId = 0;
		private TeaShop teaShop;

		public int Tick
		{
			get { return tick; }
		}

		public Owner(TeaShop teaShop)
		{
			this.customers = new HashSet<Customer>();
			this.myClock = new Clock(this);
			this.teaShop = teaShop;
			/*myClock.startServing += ReceiveAndStartServing;
			myClock.lastCall += ReceiveAndSendLastCall;
			myClock.stopServing += ReceiveAndStopServing;
			myClock.closingTime += ReceiveClosingTime;*/
			//endSimulation += teaShop.ReceivedEnd;
		}

		private async Task RunOwner()
		{
			while (!isLastCall && serving)
			{
				customers.Add(createCustomer());
				Task customerPause = Pause();
				await customerPause;
				Task.WaitAll(customerPause);
			}
		}

		private async Task Pause()
		{
			await Task.Delay(5000);
		}

		public Customer createCustomer()
		{
			Random rand = new Random();
			int cups = rand.Next(5);
			cups++;
			bool hurried = false;
			if (cups % 2 == 0) hurried = true;

			Customer newCustomer = new Customer(customerId, cups, hurried, this);
			customerId++;

			return newCustomer;
		}

		public async Task TakeOrder(int id)
		{
			if (serving)
			{
				Console.WriteLine("\nOwner takes an order for customer " + id + ".\n");
				Task order = Task.Delay(4000);
				await order;
				Task.WaitAll(order);
			}
			else
			{
				Console.WriteLine("\nOwner apologizes to customer " + id + " but he is no longer taking orders.\n");
			}
		}

		public async Task ReceiveAndStartServing()
		{
			serving = true;
			Console.WriteLine("\n**Owner opens the store and starts serving orders.**\n");
			await RunOwner();
		}

		public async Task ReceiveAndSendLastCall()
		{
			isLastCall = true;
			Console.WriteLine("\n**Owner looks at the Clock and then gives last call.**\n");

			foreach (Customer c in customers)
				await c.ReceiveLastCall();
		}

		public async Task ReceiveAndStopServing()
		{
			serving = false;
			Console.WriteLine("\n**Owner looks at the Clock and stops serving new orders.**\n");

			foreach (Customer c in customers)
				await c.ReceiveStopServing();
		}

		public async Task ReceiveClosingTime()
		{
			serving = false;
			isLastCall = false;
			closingTime = true;
			Console.WriteLine("\nThere are currently " + customers.Count + " customers in the shop at closing time.\n");
			await TestClosing();
		}

		/*public void ReceiveGoodbye(object sender, EventArgs e)
		{
			GoodbyeEventArgs goodbyeArgs = (GoodbyeEventArgs)e;
			goodbyeArgs.ACustomer.goodbye -= ReceiveGoodbye;
			Console.WriteLine("\nOwner waves goodbye to customer " + goodbyeArgs.ACustomer.ID + "\n");
			customers.Remove(goodbyeArgs.ACustomer);
			Console.WriteLine("\nThere are currently " + customers.Count + " customers in the shop.\n");
			TestClosing();
		}*/

		public async Task ReceiveGoodbye(Customer aCustomer)
		{
			Console.WriteLine("\nOwner waves goodbye to customer " + aCustomer.ID + "\n");
			customers.Remove(aCustomer);
			Console.WriteLine("\nThere are currently " + customers.Count + " customers in the shop.\n");
			await TestClosing();
		}

		public async Task TestClosing()
		{
			if (closingTime && customers.Count <= 0)
			{
				Console.WriteLine("\n**Actual closing time at: " + myClock.ReportTime() + "**\n");
				myClock.receivedShutDown();
				Console.WriteLine("\n**The Owner closes up the shop and leaves.**\n");

				//endSimulation(this, new EventArgs());
				await teaShop.ReceivedEnd();
			}
		}
	}
}
