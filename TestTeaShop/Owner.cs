using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTeaShop
{
	/// <summary>
	/// Owner klassen tar emot en TeaShop objekt och skapar en klocka och
	/// en HashSet av kunder. Klockan 'startar' allting enligt öppningsdags.
	/// Owner simulera en ägare som fyller beställningar för kunder till
	/// det dags att sluta med nya beställningar. Affären stängs när det är
	/// stängningsdags och alla kunder har lämnat.
	/// </summary>
	public class Owner
	{
		//Händelse för slutet (styrs av klockan)
		public EventHandler endSimulation;
		//HashSet av kunder
		private HashSet<Customer> customers;
		//Klockan
		private Clock myClock;

		private bool isLastCall = false;
		private bool serving = false;
		private bool closingTime = false;
		private int tick = 0;
		private int customerId = 0;

		//Te affären (huvudprogrammet)
		private TeaShop teaShop;

		/// <summary>
		/// Tick räknar varje riktig sekund av klockan som blir .
		/// </summary>
		public int Tick
		{
			get { return tick; }
		}

		/// <summary>
		/// Konstruktörn tar emot en TeaShop och skapar en klock och en
		/// HashSet för kunder.
		/// </summary>
		/// <param name="teaShop"></param>
		public Owner(TeaShop teaShop)
		{
			this.customers = new HashSet<Customer>();
			this.myClock = new Clock(this);
			this.teaShop = teaShop;
		}

		/// <summary>
		/// RunOwner körs under tiden Owner tar emot beställningar och det
		/// är inte sista tiden innan stängning.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// En paus på 5 sekunder
		/// </summary>
		/// <returns>Task (tom)</returns>
		private async Task Pause()
		{
			await Task.Delay(5000);
		}

		/// <summary>
		/// createCustomer skapar en kund
		/// </summary>
		/// <returns>en Customer objekt</returns>
		public Customer createCustomer()
		{
			//Kunden vill ha en random mängd koppar
			Random rand = new Random();
			int cups = rand.Next(5);
			cups++;
			bool hurried = false;
			//Varandra kund vill försöker drycker extra snabbt just innan stängning för
			//att ta en extra kopp
			if (cups % 2 == 0) hurried = true;

			//Skapara Customer objektet
			Customer newCustomer = new Customer(customerId, cups, hurried, this);
			customerId++;

			return newCustomer;
		}

		/// <summary>
		/// TakeOrder tar emot en beställning från en kund.
		/// </summary>
		/// <param name="id">Customer id</param>
		/// <returns>Task (tom)</returns>
		public async Task TakeOrder(int id)
		{
			//Om ägaren fortfarande tar emot beställningar
			if (serving)
			{
				Console.WriteLine("\nOwner takes an order for customer " + id + ".\n");

				//Det tar 4 sekunder för ägaren att göra te åt en kund och han är
				//upptaget under tiden
				Task order = Task.Delay(4000);
				await order;
				Task.WaitAll(order);
			}
			//Annars blir det ingenting
			else
			{
				Console.WriteLine("\nOwner apologizes to customer " + id + " but he is no longer taking orders.\n");
			}
		}

		/// <summary>
		/// ReceiveAndStartServing anropas från klockan och börjar allting med RunOwner.
		/// </summary>
		/// <returns>Task (tom)</returns>
		public async Task ReceiveAndStartServing()
		{
			serving = true;
			Console.WriteLine("\n**Owner opens the store and starts serving orders.**\n");
			await RunOwner();
		}

		/// <summary>
		/// ReceiveAndSendLastCall anropas från klockan och startar perioden just innan
		/// stängning. Vissa kunder har ett speciellt uppförande nära stängningsdags och
		/// drycker snabbt för en kopp till.
		/// </summary>
		/// <returns>Task (tom)</returns>
		public async Task ReceiveAndSendLastCall()
		{
			isLastCall = true;
			Console.WriteLine("\n**Owner looks at the Clock and then gives last call.**\n");

			foreach (Customer c in customers)
				await c.ReceiveLastCall();
		}

		/// <summary>
		/// ReceiveAndStopServing anropas från klockan och slutar beställningar.
		/// </summary>
		/// <returns>Task (tom)</returns>
		public async Task ReceiveAndStopServing()
		{
			serving = false;
			Console.WriteLine("\n**Owner looks at the Clock and stops serving new orders.**\n");

			foreach (Customer c in customers)
				await c.ReceiveStopServing();
		}

		/// <summary>
		/// ReceiveClosingTime anropas från klockan och ger stängningsdags.
		/// </summary>
		/// <returns>Task (tom)</returns>
		public async Task ReceiveClosingTime()
		{
			serving = false;
			isLastCall = false;
			closingTime = true;
			Console.WriteLine("\nThere are currently " + customers.Count + " customers in the shop at closing time.\n");
			await CloseShop();
		}

		/// <summary>
		/// ReceiveGoodbye tar emot när en customer objekt är klar och tar bort den från
		/// affären.
		/// </summary>
		/// <param name="aCustomer">Customer objektet som är klar</param>
		/// <returns>Task (tom)</returns>
		public async Task ReceiveGoodbye(Customer aCustomer)
		{
			Console.WriteLine("\nOwner waves goodbye to customer " + aCustomer.ID + "\n");
			customers.Remove(aCustomer);
			Console.WriteLine("\nThere are currently " + customers.Count + " customers in the shop.\n");
			await CloseShop();
		}

		/// <summary>
		/// CloseShop anropar EndProgram för att slutar programmet men bara efter både
		/// stängningsdags och när alla kunder har lämnat affären.
		/// </summary>
		/// <returns>Task (tom)</returns>
		public async Task CloseShop()
		{
			if (closingTime && customers.Count <= 0)
			{
				Console.WriteLine("\n**Actual closing time at: " + myClock.ReportTime() + "**\n");
				myClock.receivedShutDown();
				Console.WriteLine("\n**The Owner closes up the shop and leaves.**\n");

				await teaShop.EndProgram();
			}
		}
	}
}
